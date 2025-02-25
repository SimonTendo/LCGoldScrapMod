using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;

public class TagVent : MonoBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GameObject tagContainer;

    private EnemyVent attachedToVent;
    public ScanNodeProperties scanNode;
    public GameObject scanNodeObject;

    private void Start()
    {
        scanNodeObject.SetActive(false);
    }

    public void AssignName(string startingText)
    {
        if (attachedToVent == null)
        {
            return;
        }
        scanNode.subText = $"{startingText} {GetEnemyName()}";
    }

    private string GetEnemyName()
    {
        EnemyType givenType = attachedToVent.enemyType;
        if (givenType == null || givenType.enemyName == null || givenType.enemyName == "")
        {
            return "???";
        }
        switch (givenType.enemyName)
        {
            case "Flowerman":
                return "Bracken";
            case "Crawler":
                return "Thumper";
            case "Centipede":
                return "Snare flea";
            case "Puffer":
                return "Spore lizard";
            case "Blob":
                return "Hygrodere";
            case "Girl":
                return "Ghost girl";
            case "Spring":
                return "Coil-head";
            case "Clay Surgeon":
                return "Barber";
            case "Bunker Spider":
                return "Bunker spider";
            default:
                return givenType.enemyName;
        }
    }

    [HarmonyPatch(typeof(EnemyVent))]
    public class NewEnemyVent
    {
        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void SpawnVentTag(EnemyVent __instance)
        {
            GameObject spawnedTag = Instantiate(tagContainer, __instance.transform);
            TagVent tag = spawnedTag.GetComponent<TagVent>();
            tag.attachedToVent = __instance;
            Logger.LogDebug($"spawned {spawnedTag.name} on {tag.attachedToVent.gameObject.name} #{tag.attachedToVent.NetworkObjectId}");
        }

        [HarmonyPostfix, HarmonyPatch("SyncVentSpawnTimeClientRpc")]
        public static void VentTagDisplaySpawning(EnemyVent __instance)
        {
            TagVent childTag = __instance.GetComponentInChildren<TagVent>(true);
            if (childTag != null)
            {
                Logger.LogDebug($"#{__instance.NetworkObjectId} setting name");
                childTag.AssignName("Will spawn:");
                Logger.LogDebug($"set name of {childTag.name} to {childTag.scanNode.subText}");
            }
        }

        [HarmonyPostfix, HarmonyPatch("OpenVentClientRpc")]
        public static void VentTagDisplaySpawned(EnemyVent __instance)
        {
            TagVent childTag = __instance.GetComponentInChildren<TagVent>(true);
            if (childTag != null)
            {
                childTag.AssignName("Spawned:");
            }
        }
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!Config.hostToolRebalance)
        {
            scanNodeObject.SetActive(true);
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        scanNodeObject.SetActive(false);
    }
}
