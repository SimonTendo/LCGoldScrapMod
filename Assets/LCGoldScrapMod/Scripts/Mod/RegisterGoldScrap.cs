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

    private static void RegisterGoldScrapVanilla(ItemData itemData)
    {
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddMinus)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, level, selectedLevel);
            }
            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddDefault)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, level, selectedLevel);
            }
            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddPlus)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, level, selectedLevel);
            }
            foreach (GoldScrapLevels selectedLevel in itemData.levelsToAddCustom)
            {
                AddSpawnableScrapToLevel(itemData.itemProperties, level, selectedLevel);
            }
        }
    }

    private static void AddSpawnableScrapToLevel(Item item, SelectableLevel level, GoldScrapLevels selectedLevel)
    {
        string selectedLevelName = selectedLevel.ToString();
        if (!Configs.IsLevelSelected(selectedLevelName)) return;
        string selectedLevelFull = selectedLevelName + "Level";
        if (level.name == selectedLevelFull)
        {
            SpawnableItemWithRarity GoldScrapWithRarity = new SpawnableItemWithRarity();
            GoldScrapWithRarity.spawnableItem = item;
            GoldScrapWithRarity.rarity = 1;
            level.spawnableScrap.Add(GoldScrapWithRarity);
        }
    }

    public static void RegisterGoldScrapModded()
    {
        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            foreach (ItemData itemData in allGoldGrabbableObjects)
            {
                if (itemData == null || itemData.itemProperties == null || itemData.isStoreItem)
                {
                    continue;
                }
                if (level.levelID > suspectedLevelListLength && StoreAndTerminal.GetMoonTravelCost(level.levelID) >= Configs.moddedMoonCost.Value)
                {
                    SpawnableItemWithRarity GoldScrapWithRarity = new SpawnableItemWithRarity();
                    GoldScrapWithRarity.spawnableItem = itemData.itemProperties;
                    GoldScrapWithRarity.rarity = Configs.moddedMoonRarity.Value;
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldBoltMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldBoltMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldenAirhorn
    public class GoldenAirhorn
    {
        public static string itemFolder = "GoldenAirhorn";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            NoisemakerProp itemScript = itemPrefab.GetComponent<NoisemakerProp>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenAirhornMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenAirhornMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight1");
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
                itemData.itemProperties.grabSFX = sharedSFXflashlightGrab;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXairhornNoise == null || sharedSFXairhornFarNoise == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldenEggbeater
    public class GoldenEggbeater
    {
        public static string itemFolder = "GoldenEggbeater";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenEggbeaterMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenEggbeaterMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid3");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //TalkativeGoldBar
    public class TalkativeGoldBar
    {
        public static string itemFolder = "TalkativeGoldBar";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();//Mesh
            if (Configs.sillyScrap.Value || talkativeGoldBarMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(120.0f, -4.2f, -462.81f);
                itemData.itemProperties.positionOffset.Set(0.15f, 0.16f, -0.08f);
            }
            else
            {
                itemMeshFilter.mesh = talkativeGoldBarMesh;
                itemData.itemProperties.rotationOffset.Set(145.0f, -4.2f, -462.81f);
                itemData.itemProperties.positionOffset.Set(0.11f, 0.15f, -0.04f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight2");
                itemScript.grabAudio = LoadSillySFX("OldPhone");
                itemScript.noiseLoudness = 0.5f;
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXoldPhoneNoise == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldRegister
    public class GoldRegister
    {
        public static string itemFolder = "GoldRegister";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GameObject goldRegisterMain = itemPrefab.transform.GetChild(1).gameObject;
            GameObject goldRegisterCrank = itemPrefab.transform.GetChild(2).GetChild(0).gameObject;
            GameObject goldRegisterDrawer = itemPrefab.transform.GetChild(2).GetChild(1).gameObject;
            MeshFilter mainMeshFilter = goldRegisterMain.GetComponent<MeshFilter>();
            MeshFilter crankMeshFilter = goldRegisterCrank.GetComponent<MeshFilter>();
            MeshFilter drawerMeshFilter = goldRegisterDrawer.GetComponent<MeshFilter>();
            NoisemakerProp itemScript = itemPrefab.GetComponent<NoisemakerProp>();
            //Mesh
            if (Configs.sillyScrap.Value || goldRegisterMainMesh == null || goldRegisterCrankMesh == null || goldRegisterDrawerMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectHeavy2");
                itemScript.noiseSFX[0] = LoadSillySFX("CashRegister");
                itemScript.useCooldown = 1.5f;
                itemScript.maxLoudness = 1f;
                itemScript.minLoudness = 0.9f;
                itemScript.maxPitch = 1f;
                itemScript.minPitch = 0.95f;
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
                if (Configs.replaceSFX.Value || sharedSFXcashRegisterNoise == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldenBoots
    public class GoldenBoots
    {
        public static string itemFolder = "GoldenBoots";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenBootsMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.restingRotation.Set(-90.0f, 0.0f, 90.0f);
                itemData.itemProperties.positionOffset.Set(-0.2f, -0.2f, 0.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldenBootsMesh;
                itemData.itemProperties.restingRotation.Set(-90f, 0.0f, 0.0f);
                itemData.itemProperties.positionOffset.Set(0.1f, -0.1f, 0.15f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldenHorn
    public class GoldenHorn
    {
        public static string itemFolder = "GoldenHorn";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            NoisemakerProp itemScript = itemPrefab.GetComponent<NoisemakerProp>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenHornMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenHornMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid3");
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
                itemData.itemProperties.grabSFX = sharedSFXflashlightGrab;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXclownhornNoise == null || sharedSFXclownhornFarNoise == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //PurifiedMask
    public class PurifiedMask
    {
        public static string itemFolder = "PurifiedMask";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            HauntedMaskItem itemScript = itemPrefab.GetComponent<HauntedMaskItem>(); 
            itemScript.maskIsHaunted = false;
            //Mesh
            if (Configs.sillyScrap.Value || purifiedMaskMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = purifiedMaskMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldAxle
    public class GoldAxle
    {
        public static string itemFolder = "GoldAxle";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldAxleMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldAxleMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectHeavy1");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldJug
    public class GoldJug
    {
        public static string itemFolder = "GoldJug";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldJugMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(190f, 20f, 0.0f);
                itemData.itemProperties.positionOffset.Set(0.2f, 0.08f, 0.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldJugMesh;
                itemData.itemProperties.rotationOffset.Set(180f, 17.52f, 0.0f);
                itemData.itemProperties.positionOffset.Set(-0.1f, 0.08f, 0.21f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldenBell
    public class GoldenBell
    {
        public static string itemFolder = "GoldenBell";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenBellMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(0.0f, 100.0f, -29.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldenBellMesh;
                itemData.itemProperties.rotationOffset.Set(29.5f, 98f, -29.2f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropBell");
                itemScript.dropAudio = LoadSillySFX("DropBell");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                if (Configs.replaceSFX.Value || sharedSFXdropBell == null)
                {
                    itemData.itemProperties.dropSFX = LoadReplaceSFX("BellSFX");
                    itemScript.dropAudio = LoadReplaceSFX("BellSFX");
                }
                else
                {
                    itemData.itemProperties.dropSFX = sharedSFXdropBell;
                    itemScript.dropAudio = sharedSFXdropBell;
                }
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool(GameObject itemPrefab = null)
        {
            if (itemPrefab == null)
            {
                itemPrefab = itemData.itemProperties.spawnPrefab;
            }
            AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>(); 
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                itemScript.noiseRange = 128;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Bell...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
                itemScript.noiseRange = 64;
            }
        }
    }
        

    //GoldenGlass
    public class GoldenGlass
    {
        public static string itemFolder = "GoldenGlass";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            GoldenGlassScript itemScript = itemPrefab.GetComponent<GoldenGlassScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenGlassMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGlassMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemScript.beginRevealClip = LoadSillySFX($"{itemFolder}BeginReveal");
                itemScript.endRevealClip = LoadSillySFX($"{itemFolder}EndReveal");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXmenuConfirm == null ||  sharedSFXmenuCancel == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool(GameObject itemPrefab = null)
        {
            if (itemPrefab == null)
            {
                itemPrefab = itemData.itemProperties.spawnPrefab;
            }
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                Material[] newMats = { defaultMaterialGold, defaultMaterialGold };
                itemPrefab.GetComponent<MeshRenderer>().materials = newMats;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Glass...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldMugMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldMugMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldenFlask
    public class GoldenFlask
    {
        public static string itemFolder = "GoldenFlask";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenFlaskMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenFlaskMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("GrabBottle");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXgrabBottle;
                itemData.itemProperties.dropSFX = sharedSFXdropGlass1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //DuckOfGold
    public class DuckOfGold
    {
        public static string itemFolder = "DuckOfGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();
            //Mesh
            if (Configs.sillyScrap.Value || duckOfGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(0.0f, 90.0f, 17.16f);
                itemData.itemProperties.positionOffset.Set(0.06f, 0.1f, -0.15f);
            }
            else
            {
                itemMeshFilter.mesh = duckOfGoldMesh;
                itemData.itemProperties.rotationOffset.Set(0.0f, 90.0f, 17.16f);
                itemData.itemProperties.positionOffset.Set(0.06f, 0.19f, -0.03f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("GrabDuck");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropDuck");
                itemScript.dropAudio = LoadSillySFX("DropDuck");
            }
            else if (Configs.replaceSFX.Value || sharedSFXgrabDuck == null || sharedSFXdropDuck == null)
            {
                itemData.itemProperties.grabSFX = LoadReplaceSFX("DuckSFX");
                itemData.itemProperties.dropSFX = LoadReplaceSFX("DropSFX");
                itemScript.dropAudio = LoadReplaceSFX("DropSFX");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXgrabDuck;
                itemData.itemProperties.dropSFX = sharedSFXdropDuck;
                itemScript.dropAudio = sharedSFXgrabDuck;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldSign
    public class GoldSign
    {
        public static string itemFolder = "GoldSign";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            Shovel itemScript = itemPrefab.GetComponent<Shovel>();
            //Mesh
            if (Configs.sillyScrap.Value || goldSignMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.restingRotation.Set(0.0f, 0.0f, 168.0f);
                itemData.itemProperties.positionOffset.Set(-0.07f, -0.05f, 0.2f);
            }
            else
            {
                itemMeshFilter.mesh = goldSignMesh;
                itemData.itemProperties.restingRotation.Set(0.0f, 90.0f, -90.0f);
                itemData.itemProperties.positionOffset.Set(-0.04f, -0.1f, 0.22f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid1");
                itemScript.hitSFX[0] = LoadSillySFX("ShovelHit1");
                itemScript.hitSFX[1] = LoadSillySFX("ShovelHit2");
                itemScript.reelUp = LoadSillySFX("ShovelReel");
                itemScript.swing = LoadSillySFX("ShovelSwing");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceSFX.Value || sharedSFXshovelHit0 == null || sharedSFXshovelHit1 == null || sharedSFXshovelReel == null || sharedSFXshovelSwing == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool(GameObject itemPrefab = null)
        {
            if (itemPrefab == null)
            {
                itemPrefab = itemData.itemProperties.spawnPrefab;
            }
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                itemData.itemProperties.weight = Configs.CalculateWeightCustom(1.4f, Configs.hostWeightMultiplier);
                if (!Configs.sillyScrap.Value && goldSignMeshAlt != null)
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
                itemData.itemProperties.isConductiveMetal = true;
                itemData.itemProperties.weight = Configs.CalculateWeightCustom(itemData.defaultWeight, Configs.hostWeightMultiplier);
                if (!Configs.sillyScrap.Value && goldSignMesh != null)
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldPuzzleMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.positionOffset.Set(0.0f, 0.1f, -0.33f);
            }
            else
            {
                itemMeshFilter.mesh = goldPuzzleMesh;
                itemData.itemProperties.positionOffset.Set(0.0f, 0.0f, 0.0f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
                itemData.itemProperties.pocketSFX = LoadSillySFX($"Pocket{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                itemData.itemProperties.pocketSFX = null;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //ComedyGold
    public class ComedyGold
    {
        public static string itemFolder = "ComedyGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            WhoopieCushionItem itemScript = itemPrefab.GetComponent<WhoopieCushionItem>();
            //Mesh
            if (Configs.sillyScrap.Value || comedyGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = comedyGoldMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight1");
                itemScript.fartAudios[0] = LoadSillySFX("Fart1");
                itemScript.fartAudios[1] = LoadSillySFX("Fart2");
                itemScript.fartAudios[2] = LoadSillySFX("Fart3");
                itemScript.fartAudios[3] = LoadSillySFX("Fart4");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXwhoopieCushionNoise0 == null || sharedSFXwhoopieCushionNoise1 == null || sharedSFXwhoopieCushionNoise2 == null || sharedSFXwhoopieCushionNoise3 == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
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
            RegisterGoldScrapVanilla(itemData);
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || cookieGoldPanMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = cookieGoldPanMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid3");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropThinMetal;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GolderBar
    public class GolderBar
    {
        public static string itemFolder = "GolderBar";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || golderBarMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(0.0f, 0.0f, 280.0f);
                itemData.itemProperties.positionOffset.Set(0.15f, 0.09f, 0.025f);
            }
            else
            {
                itemMeshFilter.mesh = golderBarMesh;
                itemData.itemProperties.rotationOffset.Set(-2.43f, 128.71f, -330.4f);
                itemData.itemProperties.positionOffset.Set(0.04f, 0.15f, -0.09f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //CuddlyGold
    public class CuddlyGold
    {
        public static string itemFolder = "CuddlyGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || cuddlyGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = cuddlyGoldMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
                itemData.itemProperties.pocketSFX = LoadSillySFX($"{itemFolder}Pocket");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                itemData.itemProperties.pocketSFX = null;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldenGrunt
    public class GoldenGrunt
    {
        public static string itemFolder = "GoldenGrunt";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenGruntMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGruntMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //Goldkeeper
    public class Goldkeeper
    {
        public static string itemFolder = "Goldkeeper";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldkeeperMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldkeeperMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldSpring
    public class GoldSpring
    {
        public static string itemFolder = "GoldSpring";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter HeadMeshFilter = itemPrefab.transform.GetChild(1).GetComponent<MeshFilter>();
            GoldSpringScript itemScript = itemPrefab.GetComponent<GoldSpringScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldSpringMesh == null || goldSpringHeadMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldSpringMesh;
                HeadMeshFilter.mesh = goldSpringHeadMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
                itemScript.stopClip = LoadSillySFX($"{itemFolder}Stop");
                itemScript.goClip = LoadSillySFX($"{itemFolder}Go");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceEnemySFX.Value || sharedSFXspringHardClip == null || sharedSFXspringSoftClip == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //Marigold
    public class Marigold
    {
        public static string itemFolder = "Marigold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || marigoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(-40.0f, 160.0f, 285.0f);
                itemData.itemProperties.positionOffset.Set(-0.5f, -0.1f, 0.25f);
            }
            else
            {
                itemMeshFilter.mesh = marigoldMesh;
                itemData.itemProperties.rotationOffset.Set(-30.0f, 210.0f, 250.0f);
                itemData.itemProperties.positionOffset.Set(0.2f, -0.15f, 0.525f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //GoldenGuardian
    public class GoldenGuardian
    {
        public static string itemFolder = "GoldenGuardian";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            GoldenGuardianScript itemScript = itemPrefab.GetComponent<GoldenGuardianScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenGuardianMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGuardianMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
                itemScript.buildUpClip = LoadSillySFX($"{itemFolder}BuildUp");
                itemScript.explodeClip = LoadSillySFX($"{itemFolder}Explode");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceEnemySFX.Value || sharedSFXnutcrackerAim == null || sharedSFXgunShoot == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
            //Misc
            itemScript.stunGrenadeExplosion = flashbangParticle;
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool()
        {
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Guardian...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
            }
        }
    }
        

    //GoldTypeEngine
    public class GoldTypeEngine
    {
        public static string itemFolder = "GoldTypeEngine";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldTypeEngineMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldTypeEngineMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //TiltControls
    public class TiltControls
    {
        public static string itemFolder = "TiltControls";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || tiltControlsMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = tiltControlsMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"Grab{itemFolder}");
                itemData.itemProperties.dropSFX = LoadSillySFX($"Drop{itemFolder}");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }
        

    //JacobsLadder
    public class JacobsLadder
    {
        public static string itemFolder = "JacobsLadder";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.transform.GetChild(1).GetComponent<MeshFilter>();
            GameObject JacobsLadderLight = itemPrefab.transform.GetChild(2).gameObject;
            GameObject JacobsLadderGlow = itemPrefab.transform.GetChild(3).gameObject;
            FlashlightItem itemScript = itemPrefab.GetComponent<FlashlightItem>();
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
            if (Configs.sillyScrap.Value || jacobsLadderMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = jacobsLadderMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemData.itemProperties.pocketSFX = LoadSillySFX("FlashlightPocket");
                itemScript.outOfBatteriesClip = LoadSillySFX("FlashlightOut");
                itemScript.flashlightFlicker = LoadSillySFX("FlashlightFlicker");
                itemScript.flashlightClips[0] = LoadSillySFX("FlashlightClick1");
                itemScript.flashlightClips[1] = LoadSillySFX("FlashlightClick2");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXflashlightGrab;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                itemData.itemProperties.pocketSFX = sharedSFXflashlightPocket;
                if (Configs.replaceSFX.Value || sharedSFXflashlightOut == null || sharedSFXflashlightFlicker == null || sharedSFXflashlightClip == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconFlashlight == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconFlashlight;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool()
        {
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                itemData.itemProperties.batteryUsage = 150;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Jacob's Ladder...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
                itemData.itemProperties.batteryUsage = 210;
            }
        }
    }
        

    //GoldToyRobot
    public class GoldToyRobot
    {
        public static string itemFolder = "GoldToyRobot";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GameObject rightArm = itemPrefab.transform.GetChild(1).gameObject;
            GameObject leftArm = itemPrefab.transform.GetChild(2).gameObject; 
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter rightArmMeshFilter = rightArm.GetComponent<MeshFilter>();
            MeshFilter leftArmMeshFilter = leftArm.GetComponent<MeshFilter>();
            AnimatedItem itemScript = itemPrefab.GetComponent<AnimatedItem>();
            //Mesh
            if (Configs.sillyScrap.Value || goldToyRobotMainMesh == null || goldToyRobotMainMesh == null || goldToyRobotLeftMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.rotationOffset.Set(-10.0f, 89.46f, 110.0f);
                itemData.itemProperties.positionOffset.Set(-0.15f, 0.12f, -0.4f);
            }
            else
            {
                itemMeshFilter.mesh = goldToyRobotMainMesh;
                rightArmMeshFilter.mesh = goldToyRobotRightMesh;
                leftArmMeshFilter.mesh = goldToyRobotLeftMesh;
                itemData.itemProperties.rotationOffset.Set(8.0f, 89.46f, 110.0f);
                itemData.itemProperties.positionOffset.Set(0.0f, 0.09f, -0.15f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid2");
                itemScript.grabAudio = LoadSillySFX("RobotToyCheer");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceSFX.Value || sharedSFXrobotToyCheer == null)
                {
                    itemScript.grabAudio = LoadReplaceSFX("ToyRobotSFX");
                }
                else
                {
                    itemScript.grabAudio = sharedSFXrobotToyCheer;
                }
            }
            //Icon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        private static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //TatteredGoldSheet
    public class TatteredGoldSheet
    {
        public static string itemFolder = "TatteredGoldSheet";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || tatteredGoldSheetMesh == null || tatteredGoldSheetAltMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
                itemData.itemProperties.meshVariants[0] = LoadSillyMesh(itemFolder);
                itemData.itemProperties.meshVariants[1] = LoadSillyMesh($"{itemFolder}Alt");
            }
            else
            {
                itemMeshFilter.mesh = tatteredGoldSheetMesh;
                itemData.itemProperties.meshVariants[0] = tatteredGoldSheetMesh;
                itemData.itemProperties.meshVariants[1] = tatteredGoldSheetAltMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid3");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropThinMetal;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldenGirl
    public class GoldenGirl
    {
        public static string itemFolder = "GoldenGirl";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            GoldenGirlScript itemScript = itemPrefab.GetComponent<GoldenGirlScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenGirlMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenGirlMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight2");
                itemScript.reappearClip = LoadSillySFX($"{itemFolder}Appear");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceEnemySFX.Value || sharedSFXgirlLaugh == null)
                {
                    itemScript.reappearClip = LoadReplaceSFX($"{itemFolder}SFX");
                }
                else
                {
                    itemScript.reappearClip = sharedSFXgirlLaugh;
                }
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldPan
    public class GoldPan
    {
        public static string itemFolder = "GoldPan";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldPanMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldPanMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid1");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //ArtOfGold
    public class ArtOfGold
    {
        public static string itemFolder = "ArtOfGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter frameMeshFilter = itemPrefab.transform.GetChild(1).gameObject.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || artOfGoldMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectHeavy1");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
            //Artwork
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.materialVariants = artOfGoldMaterials.allSillyArtwork.ToArray();
            }
            else
            {
                itemData.itemProperties.materialVariants = artOfGoldMaterials.allArtwork.ToArray();
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldPerfume
    public class GoldPerfume
    {
        public static string itemFolder = "GoldPerfume";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldPerfumeMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldPerfumeMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //EarlGold
    public class EarlGold
    {
        public static string itemFolder = "EarlGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || earlGoldMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = earlGoldMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropThinMetal;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //ExtremelyGoldenCup
    public class ExtremelyGoldenCup
    {
        public static string itemFolder = "ExtremelyGoldenCup";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || extremelyGoldenCupMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = extremelyGoldenCupMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldFishProp
    public class GoldFishProp
    {
        public static string itemFolder = "GoldFishProp";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldFishPropMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldFishPropMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight2");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //Goldfish
    public class Goldfish
    {
        public static string itemFolder = "Goldfish";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldfishMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldfishMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldRemote
    public class GoldRemote
    {
        public static string itemFolder = "GoldRemote";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            GoldRemoteScript itemScript = itemPrefab.GetComponent<GoldRemoteScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldRemoteMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldRemoteMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemScript.remoteAudio.clip = LoadSillySFX($"{itemFolder}Click");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXremoteClick == null)
                {
                    itemScript.remoteAudio.clip = LoadReplaceSFX($"{itemFolder}SFX");
                }
                else
                {
                    itemScript.remoteAudio.clip = sharedSFXremoteClick;
                }
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool(GameObject itemPrefab = null)
        {
            if (itemPrefab == null)
            {
                itemPrefab = itemData.itemProperties.spawnPrefab;
            }
            GoldRemoteScript itemScript = itemPrefab.GetComponent<GoldRemoteScript>(); 
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                itemScript.useCooldown = 1.5f;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Gold Remote...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
                itemScript.useCooldown = 0f;
            }
        }
    }


    //JarOfGoldPickles
    public class JarOfGoldPickles
    {
        public static string itemFolder = "JarOfGoldPickles";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter jarMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter pickleMeshFilter = itemPrefab.transform.GetChild(0).GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldPicklesJarMesh == null || goldPicklesPickleMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("GrabBottle");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropGlass1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldenVision
    public class GoldenVision
    {
        public static string itemFolder = "GoldenVision";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenVisionMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldenVisionMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldRing
    public class GoldRing
    {
        public static string itemFolder = "GoldRing";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldRingMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldRingMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldenRetriever
    public class GoldenRetriever
    {
        public static string itemFolder = "GoldenRetriever";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter bodyMeshFilter = itemPrefab.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter topMeshFilter = itemPrefab.transform.GetChild(1).GetChild(1).GetComponent<MeshFilter>();
            MeshFilter bottomMeshFilter = itemPrefab.transform.GetChild(1).GetChild(2).GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenRetrieverBodyMesh == null || goldenRetrieverTopMesh == null || goldenRetrieverBottomMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX($"{itemFolder}Grab");
                itemData.itemProperties.dropSFX = LoadSillySFX($"{itemFolder}Drop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //JackInTheGold
    public class JackInTheGold
    {
        public static string itemFolder = "JackInTheGold";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter bodyMeshFilter = itemPrefab.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter lidMeshFilter = itemPrefab.transform.GetChild(1).GetChild(1).GetComponent<MeshFilter>();
            MeshFilter crankMeshFilter = itemPrefab.transform.GetChild(1).GetChild(2).GetComponent<MeshFilter>();
            MeshFilter upperJawMeshFilter = itemPrefab.transform.GetChild(1).GetChild(3).GetComponent<MeshFilter>();
            MeshFilter lowerJawMeshFilter = itemPrefab.transform.GetChild(1).GetChild(4).GetComponent<MeshFilter>();
            JackInTheGoldScript itemScript = itemPrefab.GetComponent<JackInTheGoldScript>();
            //Mesh
            if (Configs.sillyScrap.Value || jackInTheGoldBodyMesh == null || jackInTheGoldLidMesh == null || jackInTheGoldCrankMesh == null || jackInTheGoldUpperJawMesh == null || jackInTheGoldLowerJawMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp3");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid2");
                itemScript.pocketedThemeClip = LoadSillySFX($"{itemFolder}Song");
                itemScript.explodeClip = LoadSillySFX($"{itemFolder}Pop");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceEnemySFX.Value || sharedSFXjesterTheme == null || sharedSFXjesterPop == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //GoldBird
    public class GoldBird
    {
        public static string itemFolder = "GoldBird";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.transform.GetChild(1).GetComponent<MeshFilter>();
            GoldBirdScript itemScript = itemPrefab.GetComponent<GoldBirdScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldBirdMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldBirdMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectHeavy3");
                itemScript.alarmClip = LoadSillySFX($"{itemFolder}Alarm");
                itemScript.awakeClip = LoadSillySFX($"{itemFolder}Awake");
                itemScript.lightOnClip = LoadSillySFX($"{itemFolder}On");
                itemScript.lightOffClip = LoadSillySFX($"{itemFolder}Off");
                itemScript.dieClip = LoadSillySFX($"{itemFolder}Die");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
                if (Configs.replaceEnemySFX.Value || sharedSFXoldBirdAlarm == null || sharedSFXoldBirdWake == null || sharedSFXoldBirdOn == null || sharedSFXoldBirdOff == null || sharedSFXgunShoot == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
            //Lights
            itemScript.headlight.enabled = false;
            itemScript.headlight.shadows = LightShadows.Hard;
            itemScript.headlight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }

    
    //GoldenClock
    public class GoldenClock
    {
        public static string itemFolder = "GoldenClock";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter bodyMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            MeshFilter secondMeshFilter = itemPrefab.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter minuteMeshFilter = itemPrefab.transform.GetChild(3).GetChild(1).GetComponent<MeshFilter>();
            GoldenClockScript itemScript = itemPrefab.GetComponent<GoldenClockScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenClockBodyMesh == null || goldenClockMinuteMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp2");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid1");
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
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceSFX.Value || sharedSFXclockTick == null || sharedSFXclockTock == null || sharedSFXapplause == null || sharedSFXoldBirdOff == null || sharedSFXremoteClick == null || sharedSFXsnareBuildUp == null || sharedSFXmenuCancel == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }


    //Goldmine
    public class Goldmine
    {
        public static string itemFolder = "Goldmine";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            GoldmineScript itemScript = itemPrefab.GetComponent<GoldmineScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldmineMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldmineMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("ShovelPickUp1");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectMid1");
                itemScript.triggerClip = LoadSillySFX($"{itemFolder}Trigger");
                itemScript.beepClip = LoadSillySFX($"{itemFolder}Beep");
                itemScript.onClip = LoadSillySFX($"{itemFolder}On");
                itemScript.offClip = LoadSillySFX($"{itemFolder}Off");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
                if (Configs.replaceEnemySFX.Value || sharedSFXlandmineTrigger == null || sharedSFXlandmineBeep == null || sharedSFXlandmineOn == null || sharedSFXlandmineOff == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
            //Miscellaneous
            itemScript.triggerLight.shadows = LightShadows.Hard;
            itemScript.triggerLight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool()
        {
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                itemData.itemProperties.toolTips[2] = "";
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Goldmine...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
                itemData.itemProperties.toolTips[2] = "Toggle : [E]";
            }
        }
    }


    //GoldenGrenade
    public class GoldenGrenade
    {
        public static string itemFolder = "GoldenGrenade";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapAssets/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter bodyMesh = itemPrefab.transform.GetChild(2).GetComponent<MeshFilter>();
            MeshFilter pinMesh = itemPrefab.transform.GetChild(2).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter bodyMeshAlt = itemPrefab.transform.GetChild(3).GetComponent<MeshFilter>();
            MeshFilter pinMeshAlt = itemPrefab.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();
            StunGrenadeItem itemScript = itemPrefab.GetComponent<StunGrenadeItem>();
            //Mesh
            if (Configs.sillyScrap.Value || goldenGrenadeBodyMesh == null || goldenGrenadePinMesh == null)
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
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight3");
                itemScript.explodeSFX = LoadSillySFX($"{itemFolder}Explode");
                itemScript.pullPinSFX = LoadSillySFX($"{itemFolder}PullPin");
            }
            else
            {
                itemData.itemProperties.grabSFX = sharedSFXflashlightGrab;
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
                if (Configs.replaceSFX.Value || sharedSFXgrenadeExplode == null || sharedSFXgrenadePullPin == null)
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
            if (Configs.sillyScrap.Value || sharedItemIconGrenade == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconGrenade;
            }
            //Miscellaneous
            itemScript.stunGrenadeExplosion = flashbangParticle;
            itemScript.grenadeFallCurve = grenadeFallCurve;
            itemScript.grenadeVerticalFallCurve = grenadeVerticalFallCurve;
            itemScript.grenadeVerticalFallCurveNoBounce = grenadeVerticalFallCurveNoBounce;
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }

        public static void RebalanceTool(GameObject itemPrefab = null)
        {
            if (itemPrefab == null)
            {
                itemPrefab = itemData.itemProperties.spawnPrefab;
            }
            MeshFilter bodyMesh = itemPrefab.transform.GetChild(2).GetComponent<MeshFilter>();
            MeshFilter pinMesh = itemPrefab.transform.GetChild(2).GetChild(0).GetComponent<MeshFilter>();
            MeshFilter bodyMeshAlt = itemPrefab.transform.GetChild(3).GetComponent<MeshFilter>();
            MeshFilter pinMeshAlt = itemPrefab.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();
            StunGrenadeItem itemScript = itemPrefab.GetComponent<StunGrenadeItem>(); 
            bodyMesh.mesh = null;
            pinMesh.mesh = null;
            bodyMeshAlt.mesh = null;
            pinMeshAlt.mesh = null;
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                itemScript.TimeToExplode = 0.18f;
                itemScript.playerAnimation = "PullGrenadePin2";
                if (Configs.sillyScrap.Value)
                {
                    bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                    itemData.itemProperties.itemIcon = sillyItemIcon;
                    itemScript.pullPinSFX = LoadSillySFX($"{itemFolder}PullPin");
                    itemScript.explodeSFX = LoadSillySFX($"{itemFolder}Explode");
                }
                else
                {
                    itemData.itemProperties.grabSFX = sharedSFXgrabBottle;
                    itemData.itemProperties.itemIcon = sharedItemIconScrap;
                    if (goldenGrenadeBodyMeshAlt != null && goldenGrenadePinMeshAlt != null)
                    {
                        bodyMeshAlt.mesh = goldenGrenadeBodyMeshAlt;
                        pinMeshAlt.mesh = goldenGrenadePinMeshAlt;
                    }
                    else
                    {
                        bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    }
                    if (Configs.replaceSFX.Value || sharedSFXgrenadePullPinAlt == null || sharedSFXgrenadeExplode == null)
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
                itemData.itemProperties.isConductiveMetal = true;
                itemScript.TimeToExplode = 2.25f;
                itemScript.playerAnimation = "PullGrenadePin";
                if (Configs.sillyScrap.Value)
                {
                    bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    itemData.itemProperties.grabSFX = LoadSillySFX("FlashlightGrab");
                    itemData.itemProperties.itemIcon = sillyItemIcon;
                    itemScript.pullPinSFX = LoadSillySFX($"{itemFolder}PullPin");
                    itemScript.explodeSFX = LoadSillySFX($"{itemFolder}Explode");
                }
                else
                {
                    itemData.itemProperties.grabSFX = sharedSFXflashlightGrab;
                    if (goldenGrenadeBodyMesh != null && goldenGrenadePinMesh != null)
                    {
                        bodyMesh.mesh = goldenGrenadeBodyMesh;
                        pinMesh.mesh = goldenGrenadePinMesh;
                    }
                    else
                    {
                        bodyMesh.mesh = LoadSillyMesh(itemFolder);
                    }
                    if (Configs.replaceSFX.Value || sharedSFXgrenadePullPin == null || sharedSFXgrenadeExplode == null)
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
                        itemData.itemProperties.itemIcon = sharedItemIconGrenade;
                    }
                    else
                    {
                        itemData.itemProperties.itemIcon = sillyItemIcon;
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
            RegisterToLevel();
        }

        public static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            Light itemLight = itemPrefab.transform.GetChild(1).GetComponent<Light>();
            //Mesh
            if (Configs.sillyScrap.Value || goldBeaconMesh == null)
            {
                itemMeshFilter.mesh = LoadSillyMesh(itemFolder);
            }
            else
            {
                itemMeshFilter.mesh = goldBeaconMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectHeavy3");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
            }
            //Icon
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
            //Light
            itemLight.intensity = 50;
            itemLight.shadows = LightShadows.Hard;
            itemLight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        }

        public static void RegisterToLevel()
        {
            RegisterGoldScrapVanilla(itemData);
        }
    }



    //Gold Store
    //GoldNugget
    public class GoldNugget
    {
        public static string itemFolder = "GoldNugget";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.GetComponent<MeshFilter>();
            //Mesh
            if (Configs.sillyScrap.Value)
            {
                itemMeshFilter.mesh = LoadGoldStoreMesh($"Silly{itemFolder}");
                itemData.itemProperties.restingRotation.Set(0.0f, 0.0f, 0.0f);
                itemData.itemProperties.rotationOffset.Set(10.0f, -90.0f, -90.0f);
                itemData.itemProperties.positionOffset.Set(0f, 0.08f, 0f);
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemData.itemProperties.dropSFX = LoadSillySFX("DropMetalObjectLight1");
            }
            else
            {
                itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            }
            //ItemIcon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }
    }


    //GoldOre
    public class GoldOre
    {
        public static string itemFolder = "GoldOre";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets();
        }

        private static void SetAssets()
        {
            //Sounds
            itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            //ItemIcon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }
    }


    //CreditsCard
    public class CreditsCard
    {
        public static string itemFolder = "CreditsCard";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets();
        }

        private static void SetAssets()
        {
            //Sounds
            itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            //ItemIcon
            if (Configs.sillyScrap.Value || sharedItemIconScrap == null)
            {
                itemData.itemProperties.itemIcon = sillyItemIcon;
            }
            else
            {
                itemData.itemProperties.itemIcon = sharedItemIconScrap;
            }
        }
    }


    //GoldenHourglass
    public class GoldenHourglass
    {
        public static string itemFolder = "GoldenHourglass";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GoldenHourglassScript itemScript = itemPrefab.GetComponent<GoldenHourglassScript>();
            //Sounds
            itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            if (Configs.replaceSFX.Value || sharedSFXladderFall == null || sharedSFXdropBell == null || sharedSFXmenuCancel == null || sharedSFXmenuConfirm == null || sharedSFXgunShoot == null)
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GoldenPickaxeScript itemScript = itemPrefab.GetComponent<GoldenPickaxeScript>();
            GoldenPickaxeNode nodeScript = itemScript.nodeScript;
            //Sounds
            itemData.itemProperties.grabSFX = sharedSFXgrabShovel;
            itemData.itemProperties.pocketSFX = sharedSFXshovelPocket;
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
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
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Pickaxe...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
            }
        }
    }


    //GoldToilet
    public class GoldToilet
    {
        public static string itemFolder = "GoldToilet";
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.unlockableProperties.prefabObject);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            MeshFilter itemMeshFilter = itemPrefab.transform.GetChild(0).GetComponent<MeshFilter>();
            GoldToiletScript itemScript = itemPrefab.transform.GetChild(2).GetComponent<GoldToiletScript>();
            //Mesh
            if (Configs.sillyScrap.Value || goldToiletMesh == null)
            {
                itemMeshFilter.mesh = LoadGoldStoreMesh($"Silly{itemFolder}");
            }
            else
            {
                itemMeshFilter.mesh = goldToiletMesh;
            }
            //Sounds
            if (Configs.sillyScrap.Value)
            {
                itemScript.chargeAudio.clip = LoadSillySFX($"{itemFolder}Rumble");
                itemScript.flushAudio.clip = LoadSillySFX($"{itemFolder}Flush");
            }
            else
            {
                if (Configs.replaceSFX.Value || sharedSFXladderFall == null || sharedSFXtoiletFlush == null)
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GoldenTicketScript itemScript = itemPrefab.GetComponent<GoldenTicketScript>();
            //Sounds
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject1;
            if (Configs.replaceSFX.Value || sharedSFXtoiletFlush == null || sharedSFXapplause == null)
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            CrownScript itemScript = itemPrefab.GetComponent<CrownScript>();
            //Sounds
            itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject3;
            if (Configs.replaceSFX.Value || sharedSFXgunShoot == null || sharedSFXladderFall == null || sharedSFXapplause == null || sharedSFXdropBell == null)
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
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.unlockableProperties.prefabObject);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            SafeBoxScript itemScript = itemPrefab.GetComponentInChildren<SafeBoxScript>();
            //Sounds
            if (Configs.replaceSFX.Value || sharedSFXshovelHit0 == null || sharedSFXflashlightPocket == null || sharedSFXclockTick == null || sharedSFXclockTock == null || sharedSFXdropMetalObject3 == null || sharedSFXapplause == null || sharedSFXgrabShovel == null)
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
        public static ItemData itemData = CustomGoldScrapAssets.LoadAsset<ItemData>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemFolder}/{itemFolder}Data.asset");

        public static void SetUp()
        {
            SetAssets(itemData.unlockableProperties.prefabObject);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GoldfatherClockScript itemScript = itemPrefab.GetComponent<GoldfatherClockScript>();
            //Sounds
            itemScript.chimeRelaxing = LoadReplaceSFX("GoldfatherChime");
            itemScript.chimeStressful = LoadReplaceSFX("GoldfatherOpen");
            itemScript.birdScreech = LoadReplaceSFX("GoldfatherBird");
            itemScript.birdPunch = LoadReplaceSFX("GoldfatherPunch");
            itemScript.pendulumCrash = LoadReplaceSFX("GoldfatherCrash");
            itemScript.slamShut = LoadReplaceSFX("GoldfatherClose");
            AudioClip[] newTickSFX = new AudioClip[4];
            for (int i = 0; i < newTickSFX.Length; i++)
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

        public static void SetUp()
        {
            SetAssets(itemData.itemProperties.spawnPrefab);
        }

        private static void SetAssets(GameObject itemPrefab)
        {
            GoldenGloveScript itemScript = itemPrefab.GetComponent<GoldenGloveScript>();
            //Sounds
            itemData.itemProperties.grabSFX = sharedSFXshovelPickUp;
            itemData.itemProperties.dropSFX = sharedSFXdropMetalObject2;
            itemData.itemProperties.pocketSFX = sharedSFXflashlightPocket;
            if (Configs.newSFX.Value || sharedSFXladderExtend == null || sharedSFXladderFall == null || sharedSFXgunShoot == null || sharedSFXshovelHit0 == null || sharedSFXshovelHit1 == null || sharedSFXmenuConfirm == null || sharedSFXmenuCancel == null || sharedSFXdropBell == null || sharedSFXflashlightPocket == null || sharedSFXapplause == null || sharedSFXshovelPickUp == null || sharedSFXbunnyHop == null)
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

        public static void RebalanceTool(GameObject itemPrefab = null)
        {
            if (itemPrefab == null)
            {
                itemPrefab = itemData.itemProperties.spawnPrefab;
            }
            if (Configs.hostToolRebalance)
            {
                itemData.itemProperties.isConductiveMetal = false;
                Plugin.Logger.LogInfo("Config [Other Tools Balance] is set to TRUE on the host. Rebalancing Golden Glove...");
            }
            else
            {
                itemData.itemProperties.isConductiveMetal = true;
            }
            itemPrefab.GetComponent<GoldenGloveScript>().RebalanceTool();
        }
    }
}
