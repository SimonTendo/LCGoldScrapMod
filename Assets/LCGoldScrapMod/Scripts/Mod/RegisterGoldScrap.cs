using UnityEngine;
using static Plugin;
using static AssetsCollection;

public class RegisterGoldScrap
{
    public static bool alreadyAddedGoldScrapOnAwake = false;
    public static bool alreadyAddedGoldScrapOnModded = false;

    public static void SetAllItemsAwake()
    {
        if (alreadyAddedGoldScrapOnAwake) return;

        //GoldScrap
        //v1
        GoldBolt.SetUp();
        GoldenAirhorn.SetUp();
        GoldenEggbeater.SetUp();
        TalkativeGoldBar.SetUp();
        GoldRegister.SetUp();
        GoldenBoots.SetUp();
        GoldenHorn.SetUp();
        PurifiedMask.SetUp();
        GoldAxle.SetUp();
        GoldJug.SetUp();
        GoldenBell.SetUp();
        GoldenGlass.SetUp();
        GoldMug.SetUp();
        GoldenFlask.SetUp();
        DuckOfGold.SetUp();
        GoldSign.SetUp();
        GoldPuzzle.SetUp();
        ComedyGold.SetUp();
        CookieGoldPan.SetUp();
        GolderBar.SetUp();
        Marigold.SetUp();
        GoldenGrunt.SetUp();
        CuddlyGold.SetUp();
        Goldkeeper.SetUp();
        GoldSpring.SetUp();
        GoldenGuardian.SetUp();
        GoldTypeEngine.SetUp();
        TiltControls.SetUp();
        JacobsLadder.SetUp();
        GoldToyRobot.SetUp();

        //v2
        TatteredGoldSheet.SetUp();
        GoldenGirl.SetUp();
        GoldPan.SetUp();
        ArtOfGold.SetUp();
        GoldPerfume.SetUp();
        EarlGold.SetUp();
        ExtremelyGoldenCup.SetUp();
        GoldFishProp.SetUp();
        Goldfish.SetUp();
        GoldRemote.SetUp();
        JarOfGoldPickles.SetUp();
        GoldenVision.SetUp();
        GoldRing.SetUp();
        GoldenRetriever.SetUp();
        JackInTheGold.SetUp();
        GoldBird.SetUp();
        GoldenClock.SetUp();
        Goldmine.SetUp();
        GoldenGrenade.SetUp();
        GoldBeacon.SetUp();

        Config.SetCustomGoldScrapValues();

        //GoldStore
        GoldNugget.SetUp();
        GoldOre.SetUp();
        CreditsCard.SetUp();
        GoldenHourglass.SetUp();
        GoldenPickaxe.SetUp();
        GoldToilet.SetUp();
        GoldenTicket.SetUp();
        GoldCrown.SetUp();
        SafeBox.SetUp();
        GoldfatherClock.SetUp();
        GoldenGlove.SetUp();

        alreadyAddedGoldScrapOnAwake = true;
    }

    private static void RegisterGoldScrapVanilla(ItemData itemData, float customRarity)
    {
        float minusLoss = -0.34f / Config.rarityMultiplier.Value;
        float plusGain = 0.67f * Config.rarityMultiplier.Value;
        float customChange = itemData.customChange > 0 ? itemData.customChange / 3f * Config.rarityMultiplier.Value : itemData.customChange * 3f / Config.rarityMultiplier.Value;

        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddMinus)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, customRarity, level, selectedLevel, minusLoss);
            }

            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddDefault)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, customRarity, level, selectedLevel);
            }

            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddPlus)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, customRarity, level, selectedLevel, plusGain);
            }

            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddCustom)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, customRarity, level, selectedLevel, customChange);
            }
        }
    }

    private static void AddSpawnableScrapToLevel(Item itemName, float itemRarity, SelectableLevel level, GoldScrapLevels selectedLevel, float difference = 0)
    {
        string selectedLevelName = selectedLevel.ToString();
        if (!Config.IsLevelSelected(selectedLevelName)) return;
        string selectedLevelFull = selectedLevelName + "Level";
        if (level.name == selectedLevelFull)
        {
            SpawnableItemWithRarity GoldScrapWithRarity = new SpawnableItemWithRarity();
            GoldScrapWithRarity.spawnableItem = itemName;
            GoldScrapWithRarity.rarity = (int)(itemRarity + difference);
            if (GoldScrapWithRarity.rarity < 1) GoldScrapWithRarity.rarity = 1;
            level.spawnableScrap.Add(GoldScrapWithRarity);
        }
    }

    public static void RegisterGoldScrapModded()
    {
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            foreach (ItemData itemName in allGoldScrap.allItemData)
            {
                if (level.levelID > suspectedLevelListLength && StoreAndTerminal.GetMoonTravelCost(level.levelID) >= Config.moddedMoonCost.Value)
                {
                    SpawnableItemWithRarity GoldScrapWithRarity = new SpawnableItemWithRarity();
                    GoldScrapWithRarity.spawnableItem = itemName.itemProperties;
                    GoldScrapWithRarity.rarity = Config.moddedMoonRarity.Value;
                    level.spawnableScrap.Add(GoldScrapWithRarity);
                }
            }
        }
        alreadyAddedGoldScrapOnModded = true;
    }

    



    //Gold Scrap
    //GoldBolt
    public class GoldBolt
    {
        public static string itemFolder = "GoldBolt";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            GameObject itemPrefab = itemName.spawnPrefab;
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldBoltMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldBoltMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldenAirhorn
    public class GoldenAirhorn
    {
        public static string itemFolder = "GoldenAirhorn";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static NoisemakerProp itemScript = itemPrefab.GetComponent<NoisemakerProp>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenAirhornMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenAirhornMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight1");
                itemScript.noiseSFX[0] = LoadSillySFX("Airhorn");
                itemScript.noiseSFXFar[0] = LoadSillySFX("AirhornFar");
                itemScript.useCooldown = 2f;
                itemScript.maxLoudness = 1f;
                itemScript.minLoudness = 0.95f;
                itemScript.maxPitch = 1f;
                itemScript.minPitch = 0.8f;
            }
            else
            {
                itemName.grabSFX = sharedSFXflashlightGrab;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXairhornNoise == null || sharedSFXairhornFarNoise == null)
                {
                    itemScript.noiseSFX[0] = LoadReplaceSFX("AirhornSFX");
                    itemScript.noiseSFXFar[0] = LoadReplaceSFX("AirhornFarSFX");
                    itemScript.useCooldown = 0.1f;
                    itemScript.maxLoudness = 0.7f;
                    itemScript.minLoudness = 0.6f;
                    itemScript.maxPitch = 1f;
                    itemScript.minPitch = 1f;
                }
                else
                {
                    itemScript.noiseSFX[0] = sharedSFXairhornNoise;
                    itemScript.noiseSFXFar[0] = sharedSFXairhornFarNoise;
                    itemScript.useCooldown = 2f;
                    itemScript.maxLoudness = 1f;
                    itemScript.minLoudness = 0.95f;
                    itemScript.maxPitch = 1f;
                    itemScript.minPitch = 0.8f;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldenEggbeater
    public class GoldenEggbeater
    {
        public static string itemFolder = "GoldenEggbeater";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenEggbeaterMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenEggbeaterMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid3");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //TalkativeGoldBar
    public class TalkativeGoldBar
    {
        public static string itemFolder = "TalkativeGoldBar";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || talkativeGoldBarMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(120.0f, -4.2f, -462.81f);
                itemName.positionOffset.Set(0.15f, 0.16f, -0.08f);
            }
            else
            {
                itemMeshFilter.mesh = talkativeGoldBarMesh;
                itemName.rotationOffset.Set(145.0f, -4.2f, -462.81f);
                itemName.positionOffset.Set(0.11f, 0.15f, -0.04f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight2");
                itemScript.grabAudio = LoadSillySFX("OldPhone");
                itemScript.noiseLoudness = 0.5f;
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXoldPhoneNoise == null)
                {
                    itemScript.grabAudio = LoadReplaceSFX("OldPhoneSFX");
                    itemScript.noiseLoudness = 0.5f;
                }
                else
                {
                    itemScript.grabAudio = sharedSFXoldPhoneNoise;
                    itemScript.noiseLoudness = 0.3f;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldRegister
    public class GoldRegister
    {
        public static string itemFolder = "GoldRegister";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static NoisemakerProp itemScript = itemPrefab.GetComponent<NoisemakerProp>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            GameObject goldRegisterMain = itemPrefab.transform.GetChild(1).gameObject;
            GameObject goldRegisterCrank = itemPrefab.transform.GetChild(2).GetChild(0).gameObject;
            GameObject goldRegisterDrawer = itemPrefab.transform.GetChild(2).GetChild(1).gameObject;
            MeshFilter mainMeshFilter = goldRegisterMain.GetComponent<MeshFilter>();
            MeshFilter crankMeshFilter = goldRegisterCrank.GetComponent<MeshFilter>();
            MeshFilter drawerMeshFilter = goldRegisterDrawer.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldRegisterMainMesh == null || goldRegisterCrankMesh == null || goldRegisterDrawerMesh == null)
            {
                mainMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                mainMeshFilter.mesh = goldRegisterMainMesh;
                crankMeshFilter.mesh = goldRegisterCrankMesh;
                drawerMeshFilter.mesh = goldRegisterDrawerMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectHeavy2");
                itemScript.noiseSFX[0] = LoadSillySFX("CashRegister");
                itemScript.useCooldown = 1.5f;
                itemScript.maxLoudness = 1f;
                itemScript.minLoudness = 0.9f;
                itemScript.maxPitch = 1f;
                itemScript.minPitch = 0.95f;
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
                if (Config.replaceSFX.Value || sharedSFXcashRegisterNoise == null)
                {
                    itemScript.noiseSFX[0] = LoadReplaceSFX("CashRegisterSFX");
                    itemScript.useCooldown = 1f;
                    itemScript.maxLoudness = 1f;
                    itemScript.minLoudness = 0.9f;
                    itemScript.maxPitch = 1f;
                    itemScript.minPitch = 1f;
                }
                else
                {
                    itemScript.noiseSFX[0] = sharedSFXcashRegisterNoise;
                    itemScript.useCooldown = 2.2f;
                    itemScript.maxLoudness = 1f;
                    itemScript.minLoudness = 0.9f;
                    itemScript.maxPitch = 1f;
                    itemScript.minPitch = 0.95f;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldenBoots
    public class GoldenBoots
    {
        public static string itemFolder = "GoldenBoots";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenBootsMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.restingRotation.Set(-90.0f, 0.0f, 90.0f);
                itemName.positionOffset.Set(-0.2f, -0.2f, 0.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldenBootsMesh;
                itemName.restingRotation.Set(-90f, 0.0f, 0.0f);
                itemName.positionOffset.Set(0.1f, -0.1f, 0.15f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldenHorn
    public class GoldenHorn
    {
        public static string itemFolder = "GoldenHorn";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static NoisemakerProp itemScript = itemPrefab.GetComponent<NoisemakerProp>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenHornMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenHornMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid3");
                itemScript.noiseSFX[0] = LoadSillySFX("Clownhorn");
                itemScript.noiseSFXFar[0] = LoadSillySFX("ClownhornFar");
                itemScript.useCooldown = 0.75f;
                itemScript.maxLoudness = 1f;
                itemScript.minLoudness = 0.6f;
                itemScript.maxPitch = 1f;
                itemScript.minPitch = 0.93f;
            }
            else
            {
                itemName.grabSFX = sharedSFXflashlightGrab;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXclownhornNoise == null || sharedSFXclownhornFarNoise == null)
                {
                    itemScript.noiseSFX[0] = LoadReplaceSFX("ClownhornSFX");
                    itemScript.noiseSFXFar[0] = LoadReplaceSFX("ClownhornFarSFX");
                    itemScript.useCooldown = 0.25f;
                    itemScript.maxLoudness = 1f;
                    itemScript.minLoudness = 0.9f;
                    itemScript.maxPitch = 1f;
                    itemScript.minPitch = 1f;
                }
                else
                {
                    itemScript.noiseSFX[0] = sharedSFXclownhornNoise;
                    itemScript.noiseSFXFar[0] = sharedSFXclownhornFarNoise;
                    itemScript.useCooldown = 0.3f;
                    itemScript.maxLoudness = 1f;
                    itemScript.minLoudness = 0.6f;
                    itemScript.maxPitch = 1f;
                    itemScript.minPitch = 0.93f;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //PurifiedMask
    public class PurifiedMask
    {
        public static string itemFolder = "PurifiedMask";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static HauntedMaskItem itemScript = itemPrefab.GetComponent<HauntedMaskItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            itemScript.maskIsHaunted = false;
            //Mesh
            if (Config.sillyScrap.Value || purifiedMaskMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = purifiedMaskMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldAxle
    public class GoldAxle
    {
        public static string itemFolder = "GoldAxle";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldAxleMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldAxleMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectHeavy1");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldJug
    public class GoldJug
    {
        public static string itemFolder = "GoldJug";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldJugMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(190f, 20f, 0.0f);
                itemName.positionOffset.Set(0.2f, 0.08f, 0.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldJugMesh;
                itemName.rotationOffset.Set(180f, 17.52f, 0.0f);
                itemName.positionOffset.Set(-0.1f, 0.08f, 0.21f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldenBell
    public class GoldenBell
    {
        public static string itemFolder = "GoldenBell";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenBellMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(0.0f, 100.0f, -29.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldenBellMesh;
                itemName.rotationOffset.Set(29.5f, 98f, -29.2f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropBell");
                itemScript.dropAudio = LoadSillySFX("DropBell");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                if (Config.replaceSFX.Value || sharedSFXdropBell == null)
                {
                    itemName.dropSFX = LoadReplaceSFX("BellSFX");
                    itemScript.dropAudio = LoadReplaceSFX("BellSFX");
                }
                else
                {
                    itemName.dropSFX = sharedSFXdropBell;
                    itemScript.dropAudio = sharedSFXdropBell;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                itemScript.noiseRange = 128;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Bell...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                itemScript.noiseRange = 64;
            }
        }
    }
        

    //GoldenGlass
    public class GoldenGlass
    {
        public static string itemFolder = "GoldenGlass";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenGlassScript itemScript = itemPrefab.GetComponent<GoldenGlassScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenGlassMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGlassMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemScript.beginRevealClip = LoadSillySFX($"{itemFolder}BeginReveal");
                itemScript.endRevealClip = LoadSillySFX($"{itemFolder}EndReveal");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXmenuConfirm == null ||  sharedSFXmenuCancel == null)
                {
                    itemScript.beginRevealClip = LoadReplaceSFX($"{itemFolder}BeginSFX");   
                    itemScript.endRevealClip = LoadReplaceSFX($"{itemFolder}EndSFX");   
                }
                else
                {
                    itemScript.beginRevealClip = sharedSFXmenuConfirm;
                    itemScript.endRevealClip = sharedSFXmenuCancel;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                Material[] newMats = { defaultMaterialGold, defaultMaterialGold };
                itemPrefab.GetComponent<MeshRenderer>().materials = newMats;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Glass...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                Material[] newMatsAlt = { defaultMaterialGold, defaultMaterialGoldTransparent };
                itemPrefab.GetComponent<MeshRenderer>().materials = newMatsAlt;
            }
        }
    }
        

    //GoldMug
    public class GoldMug
    {
        public static string itemFolder = "GoldMug";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldMugMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldMugMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldenFlask
    public class GoldenFlask
    {
        public static string itemFolder = "GoldenFlask";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenFlaskMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenFlaskMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("GrabBottle");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemName.grabSFX = sharedSFXgrabBottle;
                itemName.dropSFX = sharedSFXdropGlass1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //DuckOfGold
    public class DuckOfGold
    {
        public static string itemFolder = "DuckOfGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || duckOfGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(0.0f, 90.0f, 17.16f);
                itemName.positionOffset.Set(0.06f, 0.1f, -0.15f);
            }
            else
            {
                itemMeshFilter.mesh = duckOfGoldMesh;
                itemName.rotationOffset.Set(0.0f, 90.0f, 17.16f);
                itemName.positionOffset.Set(0.06f, 0.19f, -0.03f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("GrabDuck");
                itemName.dropSFX = LoadSillySFX("DropDuck");
                itemScript.dropAudio = LoadSillySFX("DropDuck");
            }
            else if (Config.replaceSFX.Value || sharedSFXgrabDuck == null || sharedSFXdropDuck == null)
            {
                itemName.grabSFX = LoadReplaceSFX("DuckSFX");
                itemName.dropSFX = LoadReplaceSFX("DropSFX");
                itemScript.dropAudio = LoadReplaceSFX("DropSFX");
            }
            else
            {
                itemName.grabSFX = sharedSFXgrabDuck;
                itemName.dropSFX = sharedSFXdropDuck;
                itemScript.dropAudio = sharedSFXgrabDuck;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldSign
    public class GoldSign
    {
        public static string itemFolder = "GoldSign";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static Shovel itemScript = itemPrefab.GetComponent<Shovel>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldSignMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.restingRotation.Set(0.0f, 0.0f, 168.0f);
                itemName.positionOffset.Set(-0.07f, -0.05f, 0.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldSignMesh;
                itemName.restingRotation.Set(0.0f, 90.0f, -90.0f);
                itemName.positionOffset.Set(-0.04f, -0.1f, 0.22f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid1");
                itemScript.hitSFX[0] = LoadSillySFX("ShovelHit1");
                itemScript.hitSFX[1] = LoadSillySFX("ShovelHit2");
                itemScript.reelUp = LoadSillySFX("ShovelReel");
                itemScript.swing = LoadSillySFX("ShovelSwing");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceSFX.Value || sharedSFXshovelHit0 == null || sharedSFXshovelHit1 == null || sharedSFXshovelReel == null || sharedSFXshovelSwing == null)
                {
                    itemScript.hitSFX[0] = LoadReplaceSFX("SignHit0");
                    itemScript.hitSFX[1] = LoadReplaceSFX("SignHit1");
                    itemScript.reelUp = LoadReplaceSFX("SignReel");
                    itemScript.swing = LoadReplaceSFX("SignSwing");
                }
                else
                {
                    itemScript.hitSFX[0] = sharedSFXshovelHit0;
                    itemScript.hitSFX[1] = sharedSFXshovelHit1;
                    itemScript.reelUp = sharedSFXshovelReel;
                    itemScript.swing = sharedSFXshovelSwing;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool(float hostWeightMultiplier)
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                itemName.weight = Config.CalculateWeightCustom(1.4f, hostWeightMultiplier);
                if (!Config.sillyScrap.Value && goldSignMeshAlt != null)
                {
                    itemPrefab.GetComponent<MeshFilter>().mesh = goldSignMeshAlt;
                }
                else
                {
                    itemPrefab.GetComponent<MeshFilter>().mesh = LoadSillyMesh(itemFolder);
                }
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Gold Sign...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                itemName.weight = Config.CalculateWeightCustom(itemData.defaultWeight, hostWeightMultiplier);
                if (!Config.sillyScrap.Value && goldSignMesh != null)
                {
                    itemPrefab.GetComponent<MeshFilter>().mesh = goldSignMesh;
                }
                else
                {
                    itemPrefab.GetComponent<MeshFilter>().mesh = LoadSillyMesh(itemFolder);
                }
            }
        }
    }
        

    //GoldPuzzle
    public class GoldPuzzle
    {
        public static string itemFolder = "GoldPuzzle";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldPuzzleMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.positionOffset.Set(0.0f, 0.1f, -0.33f);
            }
            else
            {
                itemMeshFilter.mesh = goldPuzzleMesh;
                itemName.positionOffset.Set(0.0f, 0.0f, 0.0f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
                itemName.pocketSFX = LoadSillySFX($"Pocket{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                itemName.pocketSFX = null;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //ComedyGold
    public class ComedyGold
    {
        public static string itemFolder = "ComedyGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static WhoopieCushionItem itemScript = itemPrefab.GetComponent<WhoopieCushionItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || comedyGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = comedyGoldMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight1");
                itemScript.fartAudios[0] = LoadSillySFX("Fart1");
                itemScript.fartAudios[1] = LoadSillySFX("Fart2");
                itemScript.fartAudios[2] = LoadSillySFX("Fart3");
                itemScript.fartAudios[3] = LoadSillySFX("Fart4");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXwhoopieCushionNoise0 == null || sharedSFXwhoopieCushionNoise1 == null || sharedSFXwhoopieCushionNoise2 == null || sharedSFXwhoopieCushionNoise3 == null)
                {
                    itemScript.fartAudios[0] = LoadReplaceSFX("FartSFX1");
                    itemScript.fartAudios[1] = LoadReplaceSFX("FartSFX2");
                    itemScript.fartAudios[2] = LoadReplaceSFX("FartSFX3");
                    itemScript.fartAudios[3] = LoadReplaceSFX("FartSFX4");
                }
                else
                {
                    itemScript.fartAudios[0] = sharedSFXwhoopieCushionNoise0;
                    itemScript.fartAudios[1] = sharedSFXwhoopieCushionNoise1;
                    itemScript.fartAudios[2] = sharedSFXwhoopieCushionNoise2;
                    itemScript.fartAudios[3] = sharedSFXwhoopieCushionNoise3;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
            //v60 Compatibility
            GameObject TriggerPrefab = itemPrefab.transform.Find("Trigger").gameObject;
            if (v60Compatible)
            {
                SetWhoopieCushionV60(TriggerPrefab, itemPrefab);
            }
            else
            {
                SetWhoopieCushionV56(TriggerPrefab, itemPrefab);
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        private static void SetWhoopieCushionV60(GameObject triggerPrefab, GameObject itemPrefab)
        {
            triggerPrefab.AddComponent<GrabbableObjectPhysicsTrigger>().itemScript = itemPrefab.GetComponent<GrabbableObject>();
        }

        private static void SetWhoopieCushionV56(GameObject triggerPrefab, GameObject itemPrefab)
        {
            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.name == "WhoopieCushion")
                {
                    GameObject child = item.spawnPrefab.transform.Find("Trigger").gameObject;
                    MonoBehaviour[] monoBehaviours = child.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour monoBehaviour in monoBehaviours)
                    {
                        if (monoBehaviour.GetType().Name == "WhoopieCushionTrigger")
                        {
                            object type = triggerPrefab.AddComponent(monoBehaviour.GetType());
                            type.GetType().GetField("itemScript").SetValue(type, itemPrefab.GetComponent<WhoopieCushionItem>());
                            Plugin.Logger.LogInfo($"implemented component {monoBehaviour.GetType()} onto {triggerPrefab} with value {type.GetType().GetField("itemScript").GetValue(type)} during runtime since v60 compatibility is {v60Compatible}");
                            return;
                        }
                    }
                }
            }
        }
    }
        

    //CookieGoldPan
    public class CookieGoldPan
    {
        public static string itemFolder = "CookieGoldPan";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || cookieGoldPanMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = cookieGoldPanMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid3");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropThinMetal;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GolderBar
    public class GolderBar
    {
        public static string itemFolder = "GolderBar";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || golderBarMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(0.0f, 0.0f, 280.0f);
                itemName.positionOffset.Set(0.15f, 0.09f, 0.025f);
            }
            else
            {
                itemMeshFilter.mesh = golderBarMesh;
                itemName.rotationOffset.Set(-2.43f, 128.71f, -330.4f);
                itemName.positionOffset.Set(0.04f, 0.15f, -0.09f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //CuddlyGold
    public class CuddlyGold
    {
        public static string itemFolder = "CuddlyGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || cuddlyGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = cuddlyGoldMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
                itemName.pocketSFX = LoadSillySFX($"{itemFolder}Pocket");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                itemName.pocketSFX = null;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldenGrunt
    public class GoldenGrunt
    {
        public static string itemFolder = "GoldenGrunt";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenGruntMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGruntMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //Goldkeeper
    public class Goldkeeper
    {
        public static string itemFolder = "Goldkeeper";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldkeeperMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldkeeperMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldSpring
    public class GoldSpring
    {
        public static string itemFolder = "GoldSpring";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldSpringScript itemScript = itemPrefab.GetComponent<GoldSpringScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter HeadMeshFilter = itemPrefab.transform.GetChild(1).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldSpringMesh == null || goldSpringHeadMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldSpringMesh;
                HeadMeshFilter.mesh = goldSpringHeadMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
                itemScript.stopClip = LoadSillySFX($"{itemFolder}Stop");
                itemScript.goClip = LoadSillySFX($"{itemFolder}Go");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceEnemySFX.Value || sharedSFXspringHardClip == null || sharedSFXspringSoftClip == null)
                {
                    itemScript.stopClip = LoadReplaceSFX($"{itemFolder}Stop");
                    itemScript.goClip = LoadReplaceSFX($"{itemFolder}Go");
                }
                else
                {
                    itemScript.stopClip = sharedSFXspringHardClip;
                    itemScript.goClip = sharedSFXspringSoftClip;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //Marigold
    public class Marigold
    {
        public static string itemFolder = "Marigold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || marigoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(-40.0f, 160.0f, 285.0f);
                itemName.positionOffset.Set(-0.5f, -0.1f, 0.25f);
            }
            else
            {
                itemMeshFilter.mesh = marigoldMesh;
                itemName.rotationOffset.Set(-30.0f, 210.0f, 250.0f);
                itemName.positionOffset.Set(0.2f, -0.15f, 0.525f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //GoldenGuardian
    public class GoldenGuardian
    {
        public static string itemFolder = "GoldenGuardian";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenGuardianScript itemScript = itemPrefab.GetComponent<GoldenGuardianScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenGuardianMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGuardianMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
                itemScript.buildUpClip = LoadSillySFX($"{itemFolder}BuildUp");
                itemScript.explodeClip = LoadSillySFX($"{itemFolder}Explode");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceEnemySFX.Value || sharedSFXnutcrackerAim == null || sharedSFXgunShoot == null)
                {
                    itemScript.buildUpClip = LoadReplaceSFX($"{itemFolder}BuildUp");
                    itemScript.explodeClip = LoadReplaceSFX($"{itemFolder}Explode");
                }
                else
                {
                    itemScript.buildUpClip = sharedSFXnutcrackerAim;
                    itemScript.explodeClip = sharedSFXgunShoot;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
            //Misc
            itemScript.stunGrenadeExplosion = flashbangParticle;
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Guardian...");
            }
            else
            {
                itemName.isConductiveMetal = true;
            }
        }
    }
        

    //GoldTypeEngine
    public class GoldTypeEngine
    {
        public static string itemFolder = "GoldTypeEngine";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldTypeEngineMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldTypeEngineMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //TiltControls
    public class TiltControls
    {
        public static string itemFolder = "TiltControls";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || tiltControlsMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = tiltControlsMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemName.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }
        

    //JacobsLadder
    public class JacobsLadder
    {
        public static string itemFolder = "JacobsLadder";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static FlashlightItem itemScript = itemPrefab.GetComponent<FlashlightItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.transform.GetChild(1).GetComponent<MeshFilter>();
            GameObject JacobsLadderLight = itemPrefab.transform.GetChild(2).gameObject;
            GameObject JacobsLadderGlow = itemPrefab.transform.GetChild(3).gameObject;
            itemScript.flashlightBulb = JacobsLadderLight.GetComponent<Light>();
            itemScript.flashlightBulbGlow = JacobsLadderGlow.GetComponent<Light>();
            itemScript.flashlightBulb.intensity = 400;
            itemScript.flashlightBulb.lightShadowCasterMode = LightShadowCasterMode.Everything;
            itemScript.flashlightBulb.shadows = LightShadows.Hard;
            itemScript.flashlightBulb.enabled = false;
            itemScript.flashlightBulbGlow.lightShadowCasterMode = LightShadowCasterMode.Everything;
            itemScript.flashlightBulbGlow.shadows = LightShadows.Hard;
            itemScript.flashlightBulbGlow.enabled = false;
            itemScript.flashlightTypeID = jacobsLadderFlashlightID;
            itemScript.flashlightBulb.intensity = 400;
            //Mesh
            if (Config.sillyScrap.Value || jacobsLadderMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = jacobsLadderMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemName.pocketSFX = LoadSillySFX("FlashlightPocket");
                itemScript.outOfBatteriesClip = LoadSillySFX("FlashlightOut");
                itemScript.flashlightFlicker = LoadSillySFX("FlashlightFlicker");
                itemScript.flashlightClips[0] = LoadSillySFX("FlashlightClick1");
                itemScript.flashlightClips[1] = LoadSillySFX("FlashlightClick2");
            }
            else
            {
                itemName.grabSFX = sharedSFXflashlightGrab;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                itemName.pocketSFX = sharedSFXflashlightPocket;
                if (Config.replaceSFX.Value || sharedSFXflashlightOut == null || sharedSFXflashlightFlicker == null || sharedSFXflashlightClip == null)
                {
                    itemScript.outOfBatteriesClip = LoadReplaceSFX("FlashlightOutSFX");
                    itemScript.flashlightFlicker = LoadReplaceSFX("FlashlightFlickerSFX");
                    itemScript.flashlightClips[0] = LoadReplaceSFX("FlashlightClick0SFX");
                    itemScript.flashlightClips[1] = LoadReplaceSFX("FlashlightClick1SFX");
                }
                else
                {
                    itemScript.outOfBatteriesClip = sharedSFXflashlightOut;
                    itemScript.flashlightFlicker = sharedSFXflashlightFlicker;
                    itemScript.flashlightClips[0] = sharedSFXflashlightClip;
                    itemScript.flashlightClips[1] = sharedSFXflashlightClip;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconFlashlight == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconFlashlight;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                itemName.batteryUsage = 150;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Jacob's Ladder...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                itemName.batteryUsage = 210;
            }
        }
    }
        

    //GoldToyRobot
    public class GoldToyRobot
    {
        public static string itemFolder = "GoldToyRobot";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GameObject RightArm = itemPrefab.transform.GetChild(1).gameObject;
        public static GameObject LeftArm = itemPrefab.transform.GetChild(2).gameObject;
        public static AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        private static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter RightArmMeshFilter = RightArm.GetComponent<MeshFilter>();
            MeshFilter LeftArmMeshFilter = LeftArm.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldToyRobotMainMesh == null || goldToyRobotMainMesh == null || goldToyRobotLeftMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.rotationOffset.Set(-10.0f, 89.46f, 110.0f);
                itemName.positionOffset.Set(-0.15f, 0.12f, -0.4f);
            }
            else
            {
                itemMeshFilter.mesh = goldToyRobotMainMesh;
                RightArmMeshFilter.mesh = goldToyRobotRightMesh;
                LeftArmMeshFilter.mesh = goldToyRobotLeftMesh;
                itemName.rotationOffset.Set(8.0f, 89.46f, 110.0f);
                itemName.positionOffset.Set(0.0f, 0.09f, -0.15f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid2");
                itemScript.grabAudio = LoadSillySFX("RobotToyCheer");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceSFX.Value || sharedSFXrobotToyCheer == null)
                {
                    itemScript.grabAudio = LoadReplaceSFX("ToyRobotSFX");
                }
                else
                {
                    itemScript.grabAudio = sharedSFXrobotToyCheer;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //TatteredGoldSheet
    public class TatteredGoldSheet
    {
        public static string itemFolder = "TatteredGoldSheet";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || tatteredGoldSheetMesh == null || tatteredGoldSheetAltMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemName.meshVariants[0] = LoadSillyMesh(itemFolder);
                itemName.meshVariants[1] = LoadSillyMesh($"{itemFolder}Alt");
            }
            else
            {
                itemMeshFilter.mesh = tatteredGoldSheetMesh;
                itemName.meshVariants[0] = tatteredGoldSheetMesh;
                itemName.meshVariants[1] = tatteredGoldSheetAltMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid3");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropThinMetal;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldenGirl
    public class GoldenGirl
    {
        public static string itemFolder = "GoldenGirl";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenGirlScript itemScript = itemPrefab.GetComponent<GoldenGirlScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenGirlMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGirlMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight2");
                itemScript.reappearClip = LoadSillySFX($"{itemFolder}Appear");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceEnemySFX.Value || sharedSFXgirlLaugh == null)
                {
                    itemScript.reappearClip = LoadReplaceSFX($"{itemFolder}SFX");
                }
                else
                {
                    itemScript.reappearClip = sharedSFXgirlLaugh;
                }
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldPan
    public class GoldPan
    {
        public static string itemFolder = "GoldPan";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldPanMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldPanMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid1");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //ArtOfGold
    public class ArtOfGold
    {
        public static string itemFolder = "ArtOfGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter frameMeshFilter = itemPrefab.transform.GetChild(1).gameObject.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || artOfGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                frameMeshFilter.mesh = LoadSillyMesh($"{itemFolder}Frame");
            }
            else
            {
                itemMeshFilter.mesh = LoadSillyMesh($"{itemFolder}Artwork");
                frameMeshFilter.mesh = artOfGoldMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectHeavy1");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
            //Artwork
            if (Config.sillyScrap.Value)
            {
                itemName.materialVariants = artOfGoldMaterials.allSillyArtwork.ToArray();
            }
            else
            {
                itemName.materialVariants = artOfGoldMaterials.allArtwork.ToArray();
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldPerfume
    public class GoldPerfume
    {
        public static string itemFolder = "GoldPerfume";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldPerfumeMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldPerfumeMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //EarlGold
    public class EarlGold
    {
        public static string itemFolder = "EarlGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || earlGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = earlGoldMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropThinMetal;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity); 
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //ExtremelyGoldenCup
    public class ExtremelyGoldenCup
    {
        public static string itemFolder = "ExtremelyGoldenCup";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || extremelyGoldenCupMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = extremelyGoldenCupMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldFishProp
    public class GoldFishProp
    {
        public static string itemFolder = "GoldFishProp";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldFishPropMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldFishPropMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight2");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //Goldfish
    public class Goldfish
    {
        public static string itemFolder = "Goldfish";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldfishMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldfishMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldRemote
    public class GoldRemote
    {
        public static string itemFolder = "GoldRemote";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldRemoteScript itemScript = itemPrefab.GetComponent<GoldRemoteScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldRemoteMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldRemoteMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemScript.remoteAudio.clip = LoadSillySFX($"{itemFolder}Click");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXremoteClick == null)
                {
                    itemScript.remoteAudio.clip = LoadReplaceSFX($"{itemFolder}SFX");
                }
                else
                {
                    itemScript.remoteAudio.clip = sharedSFXremoteClick;
                }
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                itemScript.useCooldown = 1.5f;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Gold Remote...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                itemScript.useCooldown = 0f;
            }
        }
    }


    //JarOfGoldPickles
    public class JarOfGoldPickles
    {
        public static string itemFolder = "JarOfGoldPickles";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter jarMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter pickleMeshFilter = itemPrefab.transform.GetChild(0).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldPicklesJarMesh == null || goldPicklesPickleMesh == null)
            {
                jarMeshFilter.mesh = LoadSillyMesh(itemFolder);
                pickleMeshFilter.mesh = LoadSillyMesh($"{itemFolder}Pickle");
            }
            else
            {
                jarMeshFilter.mesh = goldPicklesJarMesh;
                pickleMeshFilter.mesh = goldPicklesPickleMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("GrabBottle");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropGlass1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldenVision
    public class GoldenVision
    {
        public static string itemFolder = "GoldenVision";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenVisionMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenVisionMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldRing
    public class GoldRing
    {
        public static string itemFolder = "GoldRing";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldRingMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldRingMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldenRetriever
    public class GoldenRetriever
    {
        public static string itemFolder = "GoldenRetriever";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter bodyMeshFilter = itemPrefab.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter topMeshFilter = itemPrefab.transform.GetChild(1).GetChild(1).GetComponent<MeshFilter>();
            MeshFilter bottomMeshFilter = itemPrefab.transform.GetChild(1).GetChild(2).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenRetrieverBodyMesh == null || goldenRetrieverTopMesh == null || goldenRetrieverBottomMesh == null)
            {
                bodyMeshFilter.mesh = LoadSillyMesh(itemFolder);
                topMeshFilter.mesh = null;
                bottomMeshFilter.mesh = null;
            }
            else
            {
                bodyMeshFilter.mesh = goldenRetrieverBodyMesh;
                topMeshFilter.mesh = goldenRetrieverTopMesh;
                bottomMeshFilter.mesh = goldenRetrieverBottomMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemName.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //JackInTheGold
    public class JackInTheGold
    {
        public static string itemFolder = "JackInTheGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static JackInTheGoldScript itemScript = itemPrefab.GetComponent<JackInTheGoldScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter bodyMeshFilter = itemPrefab.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter lidMeshFilter = itemPrefab.transform.GetChild(1).GetChild(1).GetComponent<MeshFilter>();
            MeshFilter crankMeshFilter = itemPrefab.transform.GetChild(1).GetChild(2).GetComponent<MeshFilter>();
            MeshFilter upperJawMeshFilter = itemPrefab.transform.GetChild(1).GetChild(3).GetComponent<MeshFilter>();
            MeshFilter lowerJawMeshFilter = itemPrefab.transform.GetChild(1).GetChild(4).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || jackInTheGoldBodyMesh == null || jackInTheGoldLidMesh == null || jackInTheGoldCrankMesh == null || jackInTheGoldUpperJawMesh == null || jackInTheGoldLowerJawMesh == null)
            {
                bodyMeshFilter.mesh = LoadSillyMesh(itemFolder);
                lidMeshFilter.mesh = null;
                crankMeshFilter.mesh = null;
                upperJawMeshFilter.mesh = null;
                lowerJawMeshFilter.mesh = null;
            }
            else
            {
                bodyMeshFilter.mesh = jackInTheGoldBodyMesh;
                lidMeshFilter.mesh = jackInTheGoldLidMesh;
                crankMeshFilter.mesh = jackInTheGoldCrankMesh;
                upperJawMeshFilter.mesh = jackInTheGoldUpperJawMesh;
                lowerJawMeshFilter.mesh = jackInTheGoldLowerJawMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid2");
                itemScript.pocketedThemeClip = LoadSillySFX($"{itemFolder}Song");
                itemScript.explodeClip = LoadSillySFX($"{itemFolder}Pop");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceEnemySFX.Value || sharedSFXjesterTheme == null || sharedSFXjesterPop == null)
                {
                    itemScript.pocketedThemeClip = LoadReplaceSFX($"{itemFolder}Theme");
                    itemScript.explodeClip = LoadReplaceSFX($"{itemFolder}Explode");
                }
                else
                {
                    itemScript.pocketedThemeClip = sharedSFXjesterTheme;
                    itemScript.explodeClip = sharedSFXjesterPop;
                }
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //GoldBird
    public class GoldBird
    {
        public static string itemFolder = "GoldBird";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldBirdScript itemScript = itemPrefab.GetComponent<GoldBirdScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.transform.GetChild(1).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldBirdMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldBirdMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectHeavy3");
                itemScript.alarmClip = LoadSillySFX($"{itemFolder}Alarm");
                itemScript.awakeClip = LoadSillySFX($"{itemFolder}Awake");
                itemScript.lightOnClip = LoadSillySFX($"{itemFolder}On");
                itemScript.lightOffClip = LoadSillySFX($"{itemFolder}Off");
                itemScript.dieClip = LoadSillySFX($"{itemFolder}Die");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject3;
                if (Config.replaceEnemySFX.Value || sharedSFXoldBirdAlarm == null || sharedSFXoldBirdWake == null || sharedSFXoldBirdOn == null || sharedSFXoldBirdOff == null || sharedSFXgunShoot == null)
                {
                    itemScript.alarmClip = LoadReplaceSFX($"{itemFolder}Alarm");
                    itemScript.awakeClip = LoadReplaceSFX($"{itemFolder}Awake");
                    itemScript.lightOnClip = LoadReplaceSFX($"{itemFolder}On");
                    itemScript.lightOffClip = LoadReplaceSFX($"{itemFolder}Off");
                    itemScript.dieClip = LoadReplaceSFX($"{itemFolder}Die");
                }
                else
                {
                    itemScript.alarmClip = sharedSFXoldBirdAlarm;
                    itemScript.awakeClip = sharedSFXoldBirdWake;
                    itemScript.lightOnClip = sharedSFXoldBirdOn;
                    itemScript.lightOffClip = sharedSFXoldBirdOff;
                    itemScript.dieClip = sharedSFXgunShoot;
                }
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
            //Lights
            itemScript.headlight.enabled = false;
            itemScript.headlight.shadows = LightShadows.Hard;
            itemScript.headlight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }

    
    //GoldenClock
    public class GoldenClock
    {
        public static string itemFolder = "GoldenClock";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenClockScript itemScript = itemPrefab.GetComponent<GoldenClockScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter bodyMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter secondMeshFilter = itemPrefab.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter minuteMeshFilter = itemPrefab.transform.GetChild(3).GetChild(1).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldenClockBodyMesh == null || goldenClockMinuteMesh == null)
            {
                bodyMeshFilter.mesh = LoadSillyMesh(itemFolder);
                secondMeshFilter.mesh = LoadSillyMesh($"{itemFolder}Hand");
                minuteMeshFilter.mesh = LoadSillyMesh($"{itemFolder}Hand");
            }
            else
            {
                bodyMeshFilter.mesh = goldenClockBodyMesh;
                secondMeshFilter.mesh = goldenClockMinuteMesh;
                minuteMeshFilter.mesh = goldenClockMinuteMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid1");
                itemScript.tickSFX = LoadSillySFX($"{itemFolder}Beep");
                itemScript.tockSFX = LoadSillySFX($"{itemFolder}Boop");
                itemScript.failClip = LoadSillySFX($"{itemFolder}Off");
                itemScript.intervalClip = LoadSillySFX($"{itemFolder}Minute");
                itemScript.closeCallApproach = LoadSillySFX($"{itemFolder}CloseAnticipation");
                itemScript.closeCallSuccess = LoadSillySFX($"{itemFolder}CloseSuccess");
                itemScript.closeCallFail = LoadSillySFX($"{itemFolder}CloseFail");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceSFX.Value || sharedSFXclockTick == null || sharedSFXclockTock == null || sharedSFXapplause == null || sharedSFXoldBirdOff == null || sharedSFXremoteClick == null || sharedSFXsnareBuildUp == null || sharedSFXmenuCancel == null)
                {
                    itemScript.tickSFX = LoadReplaceSFX("BeepSFX");
                    itemScript.tockSFX = LoadReplaceSFX("BoopSFX");
                    itemScript.failClip = LoadReplaceSFX("FailSFX");
                    itemScript.intervalClip = LoadReplaceSFX("ClockSFX");
                    itemScript.closeCallApproach = LoadReplaceSFX("AudienceBuildUpSFX");
                    itemScript.closeCallSuccess = LoadReplaceSFX("AudienceApplauseSFX");
                    itemScript.closeCallFail = LoadReplaceSFX("AudienceSadSFX");
                }
                else
                {
                    itemScript.tickSFX = sharedSFXclockTick;
                    itemScript.tockSFX = sharedSFXclockTock;
                    itemScript.failClip = sharedSFXoldBirdOff;
                    itemScript.intervalClip = sharedSFXremoteClick;
                    itemScript.closeCallApproach = sharedSFXsnareBuildUp;
                    itemScript.closeCallSuccess = sharedSFXapplause;
                    itemScript.closeCallFail = sharedSFXmenuCancel;
                }
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }


    //Goldmine
    public class Goldmine
    {
        public static string itemFolder = "Goldmine";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldmineScript itemScript = itemPrefab.GetComponent<GoldmineScript>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldmineMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldmineMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectMid1");
                itemScript.triggerClip = LoadSillySFX($"{itemFolder}Trigger");
                itemScript.beepClip = LoadSillySFX($"{itemFolder}Beep");
                itemScript.onClip = LoadSillySFX($"{itemFolder}On");
                itemScript.offClip = LoadSillySFX($"{itemFolder}Off");
            }
            else
            {
                itemName.grabSFX = sharedSFXshovelPickUp;
                itemName.dropSFX = sharedSFXdropMetalObject2;
                if (Config.replaceEnemySFX.Value || sharedSFXlandmineTrigger == null || sharedSFXlandmineBeep == null || sharedSFXlandmineOn == null || sharedSFXlandmineOff == null)
                {
                    itemScript.triggerClip = LoadReplaceSFX($"{itemFolder}TriggerSFX");
                    itemScript.beepClip = LoadReplaceSFX($"{itemFolder}BeepSFX");
                    itemScript.onClip = LoadReplaceSFX($"{itemFolder}OnSFX");
                    itemScript.offClip = LoadReplaceSFX($"{itemFolder}OffSFX"); 
                }
                else
                {
                    itemScript.triggerClip = sharedSFXlandmineTrigger;
                    itemScript.beepClip = sharedSFXlandmineBeep;
                    itemScript.onClip = sharedSFXlandmineOn;
                    itemScript.offClip = sharedSFXlandmineOff;
                }
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
            //Miscellaneous
            itemScript.triggerLight.shadows = LightShadows.Hard;
            itemScript.triggerLight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                itemName.toolTips[2] = "";
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Goldmine...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                itemName.toolTips[2] = "Toggle : [E]";
            }
        }
    }


    //GoldenGrenade
    public class GoldenGrenade
    {
        public static string itemFolder = "GoldenGrenade";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static StunGrenadeItem itemScript = itemPrefab.GetComponent<StunGrenadeItem>();

        private static MeshFilter bodyMesh = itemPrefab.transform.GetChild(2).GetComponent<MeshFilter>();
        private static MeshFilter pinMesh = itemPrefab.transform.GetChild(2).GetChild(0).GetComponent<MeshFilter>();
        private static MeshFilter bodyMeshAlt = itemPrefab.transform.GetChild(3).GetComponent<MeshFilter>();
        private static MeshFilter pinMeshAlt = itemPrefab.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            //Mesh
            if (Config.sillyScrap.Value || goldenGrenadeBodyMesh == null || goldenGrenadePinMesh == null)
            {
                bodyMesh.mesh = LoadSillyMesh(itemFolder);
                pinMesh.mesh = null;
                bodyMeshAlt.mesh = null;
                pinMeshAlt.mesh = null;
            }
            else
            {
                bodyMesh.mesh = goldenGrenadeBodyMesh;
                pinMesh.mesh = goldenGrenadePinMesh;
                bodyMeshAlt.mesh = null;
                pinMeshAlt.mesh = null;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemScript.explodeSFX = LoadSillySFX($"{itemFolder}Explode");
                itemScript.pullPinSFX = LoadSillySFX($"{itemFolder}PullPin");
            }
            else
            {
                itemName.grabSFX = sharedSFXflashlightGrab;
                itemName.dropSFX = sharedSFXdropMetalObject1;
                if (Config.replaceSFX.Value || sharedSFXgrenadeExplode == null || sharedSFXgrenadePullPin == null)
                {
                    itemScript.explodeSFX = LoadReplaceSFX("ExplosionSFX");
                    itemScript.pullPinSFX = LoadReplaceSFX("ExplosionAnticipationSFX");
                }
                else
                {
                    itemScript.explodeSFX = sharedSFXgrenadeExplode;
                    itemScript.pullPinSFX = sharedSFXgrenadePullPin;
                }
            }
            //Icon
            if (Config.sillyScrap.Value || sharedItemIconGrenade == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconGrenade;
            }
            //Miscellaneous
            itemScript.stunGrenadeExplosion = flashbangParticle;
            itemScript.grenadeFallCurve = grenadeFallCurve;
            itemScript.grenadeVerticalFallCurve = grenadeVerticalFallCurve;
            itemScript.grenadeVerticalFallCurveNoBounce = grenadeVerticalFallCurveNoBounce;
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }

        public static void RebalanceTool()
        {
            bodyMesh.mesh = null;
            pinMesh.mesh = null;
            bodyMeshAlt.mesh = null;
            pinMeshAlt.mesh = null;
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                itemScript.TimeToExplode = 0.18f;
                itemScript.playerAnimation = "PullGrenadePin2";
                if (Config.sillyScrap.Value)
                {
                    bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                    itemName.itemIcon = sillyItemIcon;
                    itemScript.pullPinSFX = LoadSillySFX($"{itemFolder}PullPin");
                    itemScript.explodeSFX = LoadSillySFX($"{itemFolder}Explode");
                }
                else
                {
                    itemName.grabSFX = sharedSFXgrabBottle;
                    itemName.itemIcon = sharedItemIconScrap;
                    if (goldenGrenadeBodyMeshAlt != null && goldenGrenadePinMeshAlt != null)
                    {
                        bodyMeshAlt.mesh = goldenGrenadeBodyMeshAlt;
                        pinMeshAlt.mesh = goldenGrenadePinMeshAlt;
                    }
                    else
                    {
                        bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    }
                    if (Config.replaceSFX.Value || sharedSFXgrenadePullPinAlt == null || sharedSFXgrenadeExplode == null)
                    {
                        itemScript.pullPinSFX = LoadReplaceSFX("ExplosionAnticipationSFXShort");
                        itemScript.explodeSFX = LoadReplaceSFX("ExplosionSFX");
                    }
                    else
                    {
                        itemScript.pullPinSFX = sharedSFXgrenadePullPinAlt;
                        itemScript.explodeSFX = sharedSFXgrenadeExplode;
                    }
                }
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Grenade...");
            }
            else
            {
                itemName.isConductiveMetal = true;
                itemScript.TimeToExplode = 2.25f;
                itemScript.playerAnimation = "PullGrenadePin";
                if (Config.sillyScrap.Value)
                {
                    bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    itemName.grabSFX = LoadSillySFX("FlashlightGrab");
                    itemName.itemIcon = sillyItemIcon;
                    itemScript.pullPinSFX = LoadSillySFX($"{itemFolder}PullPin");
                    itemScript.explodeSFX = LoadSillySFX($"{itemFolder}Explode");
                }
                else
                {
                    itemName.grabSFX = sharedSFXflashlightGrab;
                    if (goldenGrenadeBodyMesh != null && goldenGrenadePinMesh != null)
                    {
                        bodyMesh.mesh = goldenGrenadeBodyMesh;
                        pinMesh.mesh = goldenGrenadePinMesh;
                    }
                    else
                    {
                        bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    }
                    if (Config.replaceSFX.Value || sharedSFXgrenadePullPin == null || sharedSFXgrenadeExplode == null)
                    {
                        itemScript.pullPinSFX = LoadReplaceSFX("ExplosionAnticipationSFX");
                        itemScript.explodeSFX = LoadReplaceSFX("ExplosionSFX");
                    }
                    else
                    {
                        itemScript.pullPinSFX = sharedSFXgrenadePullPin;
                        itemScript.explodeSFX = sharedSFXgrenadeExplode;
                    }
                    if (sharedItemIconGrenade != null)
                    {
                        itemName.itemIcon = sharedItemIconGrenade;
                    }
                    else
                    {
                        itemName.itemIcon = sillyItemIcon;
                    }
                }
            }
        }
    }


    //GoldBeacon
    public class GoldBeacon
    {
        public static string itemFolder = "GoldBeacon";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static Light itemLight = itemPrefab.transform.GetChild(1).GetComponent<Light>();

        public static void SetUp()
        {
            SetAssets();
            RegisterToLevel();
        }

        public static void SetAssets()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldBeaconMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldBeaconMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectHeavy3");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Config.sillyScrap.Value)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
            //Light
            itemLight.intensity = 50;
            itemLight.shadows = LightShadows.Hard;
            itemLight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public static void RegisterToLevel()
        {
            float itemRarity = RarityManager.CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            RegisterGoldScrapVanilla(itemData, itemRarity);
        }
    }



    //Gold Store
    //GoldNugget
    public class GoldNugget
    {
        public static string itemFolder = "GoldNugget";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value)
            {
                itemMeshFilter.mesh = LoadGoldStoreMesh($"Silly{itemFolder}");
                itemName.restingRotation.Set(0.0f, 0.0f, 0.0f);
                itemName.rotationOffset.Set(10.0f, -90.0f, -90.0f);
                itemName.positionOffset.Set(0f, 0.08f, 0f);
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemName.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemName.dropSFX = sharedSFXdropMetalObject1;
            }
            //ItemIcon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }
    }


    //GoldOre
    public class GoldOre
    {
        public static string itemFolder = "GoldOre";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            //Sounds
            itemName.grabSFX = sharedSFXshovelPickUp;
            itemName.dropSFX = sharedSFXdropMetalObject3;
            //ItemIcon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }
    }


    //CreditsCard
    public class CreditsCard
    {
        public static string itemFolder = "CreditsCard";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;

        public static void SetUp()
        {
            //Sounds
            itemName.grabSFX = sharedSFXshovelPickUp;
            itemName.dropSFX = sharedSFXdropMetalObject1;
            //ItemIcon
            if (Config.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemName.itemIcon = sillyItemIcon;
            }
            else
            {
                itemName.itemIcon = sharedItemIconScrap;
            }
        }
    }


    //GoldenHourglass
    public class GoldenHourglass
    {
        public static string itemFolder = "GoldenHourglass";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenHourglassScript itemScript = itemPrefab.GetComponent<GoldenHourglassScript>();

        public static void SetUp()
        {
            //Sounds
            itemName.grabSFX = sharedSFXshovelPickUp;
            itemName.dropSFX = sharedSFXdropMetalObject1;
            if (Config.replaceSFX.Value || sharedSFXladderFall == null || sharedSFXdropBell == null || sharedSFXmenuCancel == null || sharedSFXmenuConfirm == null || sharedSFXgunShoot == null)
            {
                itemScript.activateClip = LoadReplaceSFX("GoldenHourglassActivateSFX");
                itemScript.deactivateClip = LoadReplaceSFX("GoldenHourglassDeactivateSFX");
                itemScript.failClip = LoadReplaceSFX("GoldenHourglassFailSFX");
                itemScript.breakClip = LoadReplaceSFX("GoldenHourglassBreakSFX");
                itemScript.chargeAudioSource.clip = LoadReplaceSFX("GoldenHourglassChargeSFX");
            }
            else
            {
                itemScript.activateClip = sharedSFXmenuConfirm;
                itemScript.deactivateClip = sharedSFXmenuCancel;
                itemScript.failClip = sharedSFXdropBell;
                itemScript.breakClip = sharedSFXgunShoot;
                itemScript.chargeAudioSource.clip = sharedSFXladderFall;
            }
        }
    }


    //GoldenPickaxe
    public class GoldenPickaxe
    {
        public static string itemFolder = "GoldenPickaxe";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenPickaxeScript itemScript = itemPrefab.GetComponent<GoldenPickaxeScript>();
        public static GoldenPickaxeNode nodeScript = itemScript.nodeScript;

        public static void SetUp()
        {
            //Sounds
            itemName.grabSFX = sharedSFXgrabShovel;
            itemName.pocketSFX = sharedSFXshovelPocket;
            itemName.dropSFX = sharedSFXdropMetalObject2;
            itemScript.reelUp = sharedSFXshovelReel;
            itemScript.swing = sharedSFXshovelSwing;
            AudioClip[] newHitSFX = new AudioClip[4];
            for (int i = 0; i < newHitSFX.Length; i++)
            {
                newHitSFX[i] = LoadReplaceSFX($"PickaxeHit{i}SFX");
            }
            itemScript.hitSFX = newHitSFX;
            itemScript.breakClip = LoadReplaceSFX("PickaxeBreakSFX");
            itemScript.critClip = LoadReplaceSFX("PickaxeCritSFX");
            itemScript.spawnClip = LoadReplaceSFX("PickaxeSpawnSFX");
            AudioClip[] newHitClips = new AudioClip[8];
            for (int i = 0; i < newHitClips.Length; i++)
            {
                newHitClips[i] = LoadReplaceSFX($"PickaxeNodeHit{i}SFX");
            }
            nodeScript.hitClip = newHitClips;
            nodeScript.emptiedClip = LoadReplaceSFX("PickaxeNodeEmptySFX");
            nodeScript.allNodesEmptyJingle = LoadReplaceSFX("PickaxeNodesCompleteSFX");
            AudioClip[] newHotColdClips = new AudioClip[9];
            for (int i = 0; i < newHotColdClips.Length; i++)
            {
                newHotColdClips[i] = LoadReplaceSFX($"PickaxeHotCold{i}");
            }
            GoldenPickaxeScript.hotColdClips = newHotColdClips;
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Pickaxe...");
            }
            else
            {
                itemName.isConductiveMetal = true;
            }
        }
    }


    //GoldToilet
    public class GoldToilet
    {
        public static string itemFolder = "GoldToilet";
        public static GameObject itemPrefab = CustomGoldScrapAssets.LoadAsset<GameObject>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Prefab.prefab");
        public static GoldToiletScript itemScript = itemPrefab.transform.GetChild(2).GetComponent<GoldToiletScript>();

        public static void SetUp()
        {
            MeshFilter itemMeshFilter = itemPrefab.transform.GetChild(0).GetComponent<MeshFilter>();
            //Mesh
            if (Config.sillyScrap.Value || goldToiletMesh == null)
            {
                itemMeshFilter.mesh = LoadGoldStoreMesh($"Silly{itemFolder}");
            }
            else
            {
                itemMeshFilter.mesh = goldToiletMesh;
            }
            //Sounds
            if (Config.sillyScrap.Value)
            {
                itemScript.chargeAudio.clip = LoadSillySFX($"{itemFolder}Rumble");
                itemScript.flushAudio.clip = LoadSillySFX($"{itemFolder}Flush");
            }
            else
            {
                if (Config.replaceSFX.Value || sharedSFXladderFall == null || sharedSFXtoiletFlush == null)
                {
                    itemScript.chargeAudio.clip = LoadReplaceSFX("ToiletChargeSFX");
                    itemScript.flushAudio.clip = LoadReplaceSFX("ToiletFlushSFX");
                }
                else
                {
                    itemScript.chargeAudio.clip = sharedSFXladderFall;
                    itemScript.flushAudio.clip = sharedSFXtoiletFlush;
                }
            }
            //Icon
            itemScript.triggerScript.hoverIcon = handIconPoint;
        }
    }


    //GoldenTicket
    public class GoldenTicket
    {
        public static string itemFolder = "GoldenTicket";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenTicketScript itemScript = itemPrefab.GetComponent<GoldenTicketScript>();

        public static void SetUp()
        {
            //Sounds
            itemName.dropSFX = sharedSFXdropMetalObject1;
            if (Config.replaceSFX.Value || sharedSFXtoiletFlush == null || sharedSFXapplause == null)
            {
                itemScript.teleportClip = LoadReplaceSFX("TicketNormalSFX");
                itemScript.inverseClip = LoadReplaceSFX("TicketInverseSFX");
            }
            else
            {
                itemScript.teleportClip = sharedSFXapplause;
                itemScript.inverseClip = sharedSFXtoiletFlush;
            }
        }
    }


    //GoldCrown
    public class GoldCrown
    {
        public static string itemFolder = "GoldCrown";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static CrownScript itemScript = itemPrefab.GetComponent<CrownScript>();

        public static void SetUp()
        {
            //Sounds
            itemName.grabSFX = sharedSFXshovelPickUp;
            itemName.dropSFX = sharedSFXdropMetalObject3;
            if (Config.replaceSFX.Value || sharedSFXgunShoot == null || sharedSFXladderFall == null || sharedSFXapplause == null || sharedSFXdropBell == null)
            {
                itemScript.launchClip = LoadReplaceSFX("CrownFlySFX");
                itemScript.landClip = LoadReplaceSFX("CrownLandSFX");
                itemScript.wearClip = LoadReplaceSFX("CrownWearSFX");
                itemScript.loopAudio.clip = LoadReplaceSFX("CrownLoopSFX");
            }
            else
            {
                itemScript.launchClip = sharedSFXgunShoot;
                itemScript.landClip = sharedSFXdropBell;
                itemScript.wearClip = sharedSFXapplause;
                itemScript.loopAudio.clip = sharedSFXladderFall;
            }
        }
    }


    //SafeBox
    public class SafeBox
    {
        public static string itemFolder = "SafeBox";
        public static GameObject itemPrefab = CustomGoldScrapAssets.LoadAsset<GameObject>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Prefab.prefab");
        public static SafeBoxScript itemScript = itemPrefab.GetComponentInChildren<SafeBoxScript>();

        public static void SetUp()
        {
            //Sounds
            if (Config.replaceSFX.Value || sharedSFXshovelHit0 == null || sharedSFXflashlightPocket == null || sharedSFXclockTick == null || sharedSFXclockTock == null || sharedSFXdropMetalObject3 == null || sharedSFXapplause == null || sharedSFXgrabShovel == null)
            {
                itemScript.unlockClip = LoadReplaceSFX("SafeOpenNormalSFX");
                itemScript.openingCreakClip = LoadReplaceSFX("SafeCreakSFX");
                itemScript.closeClipNormal = LoadReplaceSFX("SafeCloseNormalSFX");
                itemScript.closeClipTrapPlayer = LoadReplaceSFX("SafeCloseTrapSFX");
                itemScript.closeClipFail = LoadReplaceSFX("SafeCloseFailSFX");
                itemScript.trappedPlayerBonk = LoadReplaceSFX("SafeBonkSFX");
                itemScript.trappedPlayerEscape = LoadReplaceSFX("SafeOpenTrapSFX");
            }
            else
            {
                itemScript.unlockClip = sharedSFXclockTick;
                itemScript.openingCreakClip = sharedSFXflashlightPocket;
                itemScript.closeClipNormal = sharedSFXclockTock;
                itemScript.closeClipTrapPlayer = sharedSFXdropMetalObject3;
                itemScript.closeClipFail = sharedSFXgrabShovel;
                itemScript.trappedPlayerBonk = sharedSFXshovelHit0;
                itemScript.trappedPlayerEscape = sharedSFXapplause;
            }
            //Icon
            itemScript.outsideTrigger.hoverIcon = handIconPoint;
            itemScript.insideTrigger.hoverIcon = handIcon;
            itemScript.placeItemObject.triggerScript.hoverIcon = handIconPoint;
        }
    }



    //GoldfatherClock
    public class GoldfatherClock
    {
        public static string itemFolder = "GoldfatherClock";
        public static GameObject itemPrefab = CustomGoldScrapAssets.LoadAsset<GameObject>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Prefab.prefab");
        public static GoldfatherClockScript itemScript = itemPrefab.GetComponent<GoldfatherClockScript>();

        public static void SetUp()
        {
            //Sounds
            itemScript.chimeRelaxing = LoadReplaceSFX("GoldfatherChime");
            itemScript.chimeStressful = LoadReplaceSFX("GoldfatherOpen");
            itemScript.birdScreech = LoadReplaceSFX("GoldfatherBird");
            itemScript.birdPunch = LoadReplaceSFX("GoldfatherPunch");
            itemScript.pendulumCrash = LoadReplaceSFX("GoldfatherCrash");
            itemScript.slamShut = LoadReplaceSFX("GoldfatherClose");
            AudioClip[] newTickSFX = new AudioClip[4];
            for (int i = 0;  i < newTickSFX.Length; i++)
            {
                newTickSFX[i] = LoadReplaceSFX($"GoldfatherTick{i}");
            }
            itemScript.handTick = newTickSFX;
            AudioClip[] newSwaySFX = new AudioClip[4];
            for (int i = 0; i < newSwaySFX.Length; i++)
            {
                newSwaySFX[i] = LoadReplaceSFX($"GoldfatherSway{i}");
            }
            itemScript.pendulumSway = newSwaySFX;
            AudioClip[] newWeightSFX = new AudioClip[3];
            for (int i = 0; i < newWeightSFX.Length; i++)
            {
                newWeightSFX[i] = LoadReplaceSFX($"GoldfatherWeight{i}");
            }
            itemScript.readjustWeight = newWeightSFX;
            //Icon
            itemScript.clockFaceTrigger.hoverIcon = handIcon;
            foreach (GameObject weight in itemScript.weightObjects)
            {
                weight.GetComponent<InteractTrigger>().hoverIcon = handIconPoint;
            }
        }
    }



    //GoldenGlove
    public class GoldenGlove
    {
        public static string itemFolder = "GoldenGlove";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");
        public static Item itemName = itemData.itemProperties;
        public static GameObject itemPrefab = itemName.spawnPrefab;
        public static GoldenGloveScript itemScript = itemPrefab.GetComponent<GoldenGloveScript>();

        public static void SetUp()
        {
            //Sounds
            itemName.grabSFX = sharedSFXshovelPickUp;
            itemName.dropSFX = sharedSFXdropMetalObject2;
            itemName.pocketSFX = sharedSFXflashlightPocket;
            if (Config.newSFX.Value || sharedSFXladderExtend == null || sharedSFXladderFall == null || sharedSFXgunShoot == null || sharedSFXshovelHit0 == null || sharedSFXshovelHit1 == null || sharedSFXmenuConfirm == null || sharedSFXmenuCancel == null || sharedSFXdropBell == null || sharedSFXflashlightPocket == null || sharedSFXapplause == null || sharedSFXshovelPickUp == null || sharedSFXbunnyHop == null)
            {
                itemScript.extendStartClip = LoadNewSFX("GoldenGloveExtendStart");
                itemScript.extendingLoopClip = LoadNewSFX("GoldenGloveExtendLoop");
                itemScript.extendStopClip = LoadNewSFX("GoldenGloveExtendStop");
                itemScript.retractStopClip = LoadNewSFX("GoldenGloveRetractStop");
                itemScript.retractingLoopClip = LoadNewSFX("GoldenGloveRetractLoop");
                itemScript.switchClip = LoadNewSFX("GoldenGloveSwitch");
                itemScript.struggleClip = LoadNewSFX("GoldenGloveStruggle");
                itemScript.slideLoopClip = LoadNewSFX("GoldenGloveSlideLoop");
                itemScript.rockPunchClip = LoadNewSFX("GoldenGlovePunch");
                itemScript.rockJumpClip = LoadNewSFX("GoldenGloveJump");
                itemScript.paperGrabClip = LoadNewSFX("GoldenGloveGrab");
                itemScript.paperSaveClip = LoadNewSFX("GoldenGloveSave");
                itemScript.scissorsStunClip = LoadNewSFX("GoldenGloveStun");
            }
            else
            {
                itemScript.extendStartClip = sharedSFXgunShoot;
                itemScript.extendingLoopClip = sharedSFXladderExtend;
                itemScript.extendStopClip = sharedSFXshovelHit0;
                itemScript.retractStopClip = sharedSFXmenuCancel;
                itemScript.retractingLoopClip = sharedSFXladderExtend;
                itemScript.switchClip = sharedSFXmenuConfirm;
                itemScript.struggleClip = sharedSFXflashlightPocket;
                itemScript.slideLoopClip = sharedSFXladderFall;
                itemScript.rockPunchClip = sharedSFXshovelHit1;
                itemScript.rockJumpClip = sharedSFXbunnyHop;
                itemScript.paperGrabClip = sharedSFXshovelPickUp;
                itemScript.paperSaveClip = sharedSFXapplause;
                itemScript.scissorsStunClip = sharedSFXdropBell;
            }
        }

        public static void RebalanceTool()
        {
            if (Config.hostToolRebalance)
            {
                itemName.isConductiveMetal = false;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Glove...");
            }
            else
            {
                itemName.isConductiveMetal = true;
            }
        }
    }
}
