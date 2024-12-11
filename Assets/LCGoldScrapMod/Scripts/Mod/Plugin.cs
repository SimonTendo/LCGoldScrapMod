using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using BepInEx.Bootstrap;
using LethalConfig;
using LethalConfig.ConfigItems;

[BepInPlugin("LCGoldScrapMod", "LCGoldScrapMod", "2.0.0")]
[BepInDependency("LCSimonTendoPlaylistsMod", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("LethalConfig", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;
    public static AssetBundle CustomGoldScrapAssets;
    public static GoldScrapSaveData.SaveData saveData;
    public static string sAssemblyLocation;
    public static new Config myConfig { get; internal set; }

    public static bool alreadyAddedItems = false;
    public static bool alreadyAddedUnlockables = false;

    public static bool appliedHostConfigs = false;

    //Miscellaneous objects
    public static ItemDataList allGoldScrap;
    public static UnlockableItem[] allGoldUnlockables;
    public static GameObject[] allMiscNetworkPrefabs;
    public static bool goldScrapSpawnEnabled = true;
    public static int suspectedLevelListLength = -1;

    //Compatibility
    public static bool v50Compatible = false;
    public static bool v60Compatible = false;
    public static bool playlistsModCompatible = false;
    public static bool lethalConfigCompatible = false;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin LCGoldScrapMod is loaded!");

        sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        CustomGoldScrapAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "goldscrapassetbundle"));
        if (CustomGoldScrapAssets == null)
        {
            Logger.LogError("Failed to load LCGoldScrapMod AssetBundle");
            return;
        }
        else
        {
            Logger.LogInfo("Loaded LCGoldScrapMod AssetBundle");
        }

        Harmony harmony = new Harmony("LCGoldScrapMod");
        harmony.PatchAll();
        HarmonyFileLog.Enabled = false;
        myConfig = new(Config); 

        LoadMiscellaneous();
        LoadSavedData();
        GetCompatability();
        global::Config.LoadAndDisplayConfigs();
        UnityNetcodePatcher();
    }

    public static void LoadMiscellaneous()
    {
        allGoldScrap = CustomGoldScrapAssets.LoadAsset<ItemDataList>("Assets/LCGoldScrapMod/GoldScrapNetcode/GoldScrapItemList.asset");
        allGoldUnlockables = CustomGoldScrapAssets.LoadAsset<UnlockablesList>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopUnlockablesList.asset").unlockables.ToArray();
        allMiscNetworkPrefabs = CustomGoldScrapAssets.LoadAsset<GameObjectList>("Assets/LCGoldScrapMod/GoldScrapNetcode/MiscNetworkPrefabs.asset").allPrefabs;
        TagPlayer.tagContainer = CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapMisc/GoldenGlassSecrets/ScanNodes/Players/PlayerTagScanNode.prefab");
        TagVent.tagContainer = CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapMisc/GoldenGlassSecrets/ScanNodes/Vents/VentTagScanNode.prefab");
        TagDoor.tagContainer = CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapMisc/GoldenGlassSecrets/ScanNodes/Doors/DoorTagScanNode.prefab");
        LightSwitchRevealer.lightRevealerPrefab = CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapMisc/GoldenGlassSecrets/LightSwitchRevealer/LightSwitchRevealer.prefab");
        LogManager.managerPrefab = CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapMisc/StoryLogs/LogManager.prefab");
        LogManager.allLogPrefabs = CustomGoldScrapAssets.LoadAsset<GameObjectList>("Assets/LCGoldScrapMod/GoldScrapMisc/StoryLogs/AllLogs.asset");
        AssetsCollection.LoadEditorAssets();
        StoreAndTerminal.LoadEditorAssets();
    }

    public static void LoadSavedData()
    {
        saveData = new GoldScrapSaveData.SaveData();
        saveData.Load();
    }

    private static void GetCompatability()
    {
        //v50
        if (typeof(UnlockableItem).GetField("jumpAudio") != null)
        {
            v50Compatible = true;
            suspectedLevelListLength = 12;
        }
        else
        {
            suspectedLevelListLength = 8;
        }
        Logger.LogDebug($"v50 compatibility is {v50Compatible}");

        //v60
        if (typeof(GrabbableObject).GetMethod("ActivatePhysicsTrigger") != null)
        {
            v60Compatible = true;
        }
        Logger.LogDebug($"v60 compatibility is {v60Compatible}");

        //LCSimonTendoPlaylistsMod
        playlistsModCompatible = CheckForPlugin("LCSimonTendoPlaylistsMod");

        //LethalConfig
        lethalConfigCompatible = CheckForPlugin("LethalConfig");
        if (lethalConfigCompatible)
        {
            AddLethalConfig();
        }
    }

    public static GameObject SearchForObject(string name, Transform parentTransform = null)
    {
        Logger.LogDebug($"Starting search for {name}");
        GameObject objectToSearch;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name)
            {
                objectToSearch = obj;
                Logger.LogDebug($"Found existing {obj.name}, using as parent.");
                return objectToSearch;
            }
        }
        objectToSearch = new GameObject(name);
        if (parentTransform != null)
        {
            objectToSearch.transform.SetParent(parentTransform, false);
        }
        Logger.LogDebug($"Did not find existing {name}, instantiating new {objectToSearch.name}");
        return objectToSearch;
    }

    private static bool CheckForPlugin(string pluginName, bool printDebug = true)
    {
        foreach (var plugin in Chainloader.PluginInfos.Values)
        {
            if (plugin.Metadata.Name == pluginName)
            {
                if (printDebug) Logger.LogDebug($"Successfully found {pluginName}");
                return true;
            }
        }
        if (printDebug) Logger.LogDebug($"Failed to find {pluginName}");
        return false;
    }

    private static void UnityNetcodePatcher()
    {
        //Method courtesy of Evaisa and the Lethal Company Modding Wiki
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    private static void AddLethalConfig()
    {
        LethalConfigManager.SetModIcon(CustomGoldScrapAssets.LoadAsset<Sprite>("Assets/LCGoldScrapMod/GoldScrapVisuals/Sprites/icon.png"));
        LethalConfigManager.SetModDescription("Add 50 gold versions of scrap and 20 store unlockables!");
        LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(global::Config.selectedLevels, true));
        LethalConfigManager.AddConfigItem(new FloatInputFieldConfigItem(global::Config.minValueMultiplier, true));
        LethalConfigManager.AddConfigItem(new FloatInputFieldConfigItem(global::Config.maxValueMultiplier, true));
        LethalConfigManager.AddConfigItem(new FloatInputFieldConfigItem(global::Config.rarityMultiplier, true));
        LethalConfigManager.AddConfigItem(new FloatInputFieldConfigItem(global::Config.weightMultiplier, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.moddedMoonSpawn, true));
        LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(global::Config.moddedMoonRarity, true));
        LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(global::Config.moddedMoonCost, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.newSFX, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.replaceSFX, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.replaceEnemySFX, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.fixScan, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.toolsRebalance, true));
        LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(global::Config.sillyScrap, true));
    }
}

//GitHub test comment
