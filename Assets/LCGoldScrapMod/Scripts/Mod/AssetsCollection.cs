using UnityEngine;
using BepInEx.Logging;

public class AssetsCollection
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static bool alreadyCheckedItems = false;
    public static bool alreadyCheckedEnemies = false;
    public static bool alreadyCheckedGameObjects = false;
    public static bool alreadyCheckedUnlockables = false;
    public static bool alreadyCheckedHazards = false;
    public static bool alreadyCheckedMenu = false;

    //Meshes (items)
    public static Mesh goldBoltMesh;
    public static Mesh goldenAirhornMesh;
    public static Mesh goldenEggbeaterMesh;
    public static Mesh talkativeGoldBarMesh;
    public static Mesh goldRegisterMainMesh;
    public static Mesh goldRegisterCrankMesh;
    public static Mesh goldRegisterDrawerMesh;
    public static Mesh goldenBootsMesh;
    public static Mesh goldenHornMesh;
    public static Mesh purifiedMaskMesh;
    public static Mesh goldAxleMesh;
    public static Mesh goldJugMesh;
    public static Mesh goldenBellMesh;
    public static Mesh goldenGlassMesh;
    public static Mesh goldMugMesh;
    public static Mesh goldenFlaskMesh;
    public static Mesh duckOfGoldMesh;
    public static Mesh goldSignMesh;
    public static Mesh goldSignMeshAlt;
    public static Mesh goldPuzzleMesh;
    public static Mesh comedyGoldMesh;
    public static Mesh cookieGoldPanMesh;
    public static Mesh golderBarMesh;
    public static Mesh marigoldMesh;
    public static Mesh goldenGruntMesh;
    public static Mesh cuddlyGoldMesh;
    public static Mesh goldkeeperMesh;
    public static Mesh goldSpringMesh;
    public static Mesh goldSpringHeadMesh;
    public static Mesh goldenGuardianMesh;
    public static Mesh goldTypeEngineMesh;
    public static Mesh tiltControlsMesh;
    public static Mesh jacobsLadderMesh;
    public static Mesh goldToyRobotMainMesh;
    public static Mesh goldToyRobotRightMesh;
    public static Mesh goldToyRobotLeftMesh;
    public static Mesh tatteredGoldSheetMesh;
    public static Mesh tatteredGoldSheetAltMesh;
    public static Mesh goldenGirlMesh;
    public static Mesh goldPanMesh;
    public static Mesh artOfGoldMesh;
    public static Mesh goldPerfumeMesh;
    public static Mesh earlGoldMesh;
    public static Mesh extremelyGoldenCupMesh;
    public static Mesh goldFishPropMesh;
    public static Mesh goldfishMesh;
    public static Mesh goldRemoteMesh;
    public static Mesh goldPicklesJarMesh;
    public static Mesh goldPicklesPickleMesh;
    public static Mesh goldenVisionMesh;
    public static Mesh goldRingMesh;
    public static Mesh goldenRetrieverBodyMesh;
    public static Mesh goldenRetrieverTopMesh;
    public static Mesh goldenRetrieverBottomMesh;
    public static Mesh jackInTheGoldBodyMesh;
    public static Mesh jackInTheGoldLidMesh;
    public static Mesh jackInTheGoldCrankMesh;
    public static Mesh jackInTheGoldUpperJawMesh;
    public static Mesh jackInTheGoldLowerJawMesh;
    public static Mesh goldBirdMesh;
    public static Mesh goldenClockBodyMesh;
    public static Mesh goldenClockMinuteMesh;
    public static Mesh goldmineMesh;
    public static Mesh goldenGrenadeBodyMesh;
    public static Mesh goldenGrenadePinMesh;
    public static Mesh goldenGrenadeBodyMeshAlt;
    public static Mesh goldenGrenadePinMeshAlt;
    public static Mesh goldBeaconMesh;
    public static Mesh goldToiletMesh;

    //Sound Effects (items)
    public static AudioClip sharedSFXshovelPickUp;
    public static AudioClip sharedSFXgrabBottle;
    public static AudioClip sharedSFXgrabDuck;
    public static AudioClip sharedSFXgrabShovel;
    public static AudioClip sharedSFXdropMetalObject1;
    public static AudioClip sharedSFXdropMetalObject2;
    public static AudioClip sharedSFXdropMetalObject3;
    public static AudioClip sharedSFXdropGlass1;
    public static AudioClip sharedSFXdropBell;
    public static AudioClip sharedSFXdropThinMetal;
    public static AudioClip sharedSFXdropDuck;
    public static AudioClip sharedSFXcashRegisterNoise;
    public static AudioClip sharedSFXairhornNoise;
    public static AudioClip sharedSFXairhornFarNoise;
    public static AudioClip sharedSFXclownhornNoise;
    public static AudioClip sharedSFXclownhornFarNoise;
    public static AudioClip sharedSFXoldPhoneNoise;
    public static AudioClip sharedSFXwhoopieCushionNoise0;
    public static AudioClip sharedSFXwhoopieCushionNoise1;
    public static AudioClip sharedSFXwhoopieCushionNoise2;
    public static AudioClip sharedSFXwhoopieCushionNoise3;
    public static AudioClip sharedSFXshovelHit0;
    public static AudioClip sharedSFXshovelHit1;
    public static AudioClip sharedSFXshovelReel;
    public static AudioClip sharedSFXshovelSwing;
    public static AudioClip sharedSFXshovelPocket;
    public static AudioClip sharedSFXflashlightClip;
    public static AudioClip sharedSFXflashlightFlicker;
    public static AudioClip sharedSFXflashlightOut;
    public static AudioClip sharedSFXflashlightGrab;
    public static AudioClip sharedSFXflashlightPocket;
    public static AudioClip sharedSFXrobotToyCheer;
    public static AudioClip sharedSFXremoteClick;
    public static AudioClip sharedSFXgunShoot;
    public static AudioClip sharedSFXclockTick;
    public static AudioClip sharedSFXclockTock;
    public static AudioClip sharedSFXapplause;
    public static AudioClip sharedSFXgrenadePullPin;
    public static AudioClip sharedSFXgrenadePullPinAlt;
    public static AudioClip sharedSFXgrenadeExplode;
    public static AudioClip sharedSFXladderExtend;
    public static AudioClip sharedSFXladderFall;

    //Sound effects (enemies)
    public static AudioClip sharedSFXspringHardClip;
    public static AudioClip sharedSFXspringSoftClip; 
    public static AudioClip sharedSFXnutcrackerAim;
    public static AudioClip sharedSFXgirlLaugh;
    public static AudioClip sharedSFXjesterTheme;
    public static AudioClip sharedSFXjesterPop;
    public static AudioClip sharedSFXoldBirdAlarm;
    public static AudioClip sharedSFXoldBirdWake;
    public static AudioClip sharedSFXoldBirdOn;
    public static AudioClip sharedSFXoldBirdOff;
    public static AudioClip sharedSFXsnareBuildUp;

    //Sound effects (misc)
    public static AudioClip sharedSFXmenuConfirm;
    public static AudioClip sharedSFXmenuCancel;
    public static AudioClip sharedSFXlandmineTrigger;
    public static AudioClip sharedSFXlandmineBeep;
    public static AudioClip sharedSFXlandmineOn;
    public static AudioClip sharedSFXlandmineOff;
    public static AudioClip sharedSFXdiscoBallMusic;
    public static AudioClip sharedSFXtoiletFlush;
    public static AudioClip sharedSFXbunnyHop;

    //Icons (items)
    public static Sprite sharedItemIconScrap;
    public static Sprite sharedItemIconFlashlight;
    public static Sprite sharedItemIconGrenade;
    public static Sprite sillyItemIcon;

    //Miscellaneous
    public static Material defaultMaterialGold;
    public static Material defaultMaterialGoldTransparent;
    public static Material defaultMaterialGoldEmmissive;
    public static Material defaultMaterialSilver;
    public static Material defaultMaterialBronze;
    public static GameObject jacobsLadderPrefab;
    public static int jacobsLadderFlashlightID;
    public static ArtOfGoldMaterials artOfGoldMaterials;
    public static GameObject discoBallStaticPrefab;
    public static GameObject poofParticle;
    public static GameObject flashbangParticle;
    public static AnimationCurve grenadeFallCurve;
    public static AnimationCurve grenadeVerticalFallCurve;
    public static AnimationCurve grenadeVerticalFallCurveNoBounce;
    public static Sprite handIcon;
    public static Sprite handIconPoint;

    public static void LoadEditorAssets()
    {
        defaultMaterialGold = Plugin.CustomGoldScrapAssets.LoadAsset<Material>("Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/GoldMat.mat");
        defaultMaterialGoldTransparent = Plugin.CustomGoldScrapAssets.LoadAsset<Material>("Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/GoldMatTransparent.mat");
        defaultMaterialGoldEmmissive = Plugin.CustomGoldScrapAssets.LoadAsset<Material>("Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/GoldMatEmmissive.mat");
        defaultMaterialSilver = Plugin.CustomGoldScrapAssets.LoadAsset<Material>("Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/SilverMat.mat");
        defaultMaterialBronze = Plugin.CustomGoldScrapAssets.LoadAsset<Material>("Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/BronzeMat.mat");
        sillyItemIcon = Plugin.CustomGoldScrapAssets.LoadAsset<Sprite>("Assets/LCGoldScrapMod/GoldScrapVisuals/Sprites/GoldScrapItemIconPlaceholder.png");
        artOfGoldMaterials = Plugin.CustomGoldScrapAssets.LoadAsset<ArtOfGoldMaterials>("Assets/LCGoldScrapMod/GoldScrapAssets/ArtOfGold/ArtOfGoldMaterials.asset");
        jacobsLadderPrefab = Plugin.CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapAssets/JacobsLadder/JacobsLadderHelmetLight.prefab");
    }

    public static void GetItemAssets(StartOfRound __instance)
    {
        if (alreadyCheckedItems || __instance == null || __instance.allItemsList == null || __instance.allItemsList.itemsList == null)
        {
            return;
        }

        for (int i = 0; i < __instance.allItemsList.itemsList.Count; i++)
        {
            Item itemData = __instance.allItemsList.itemsList[i];
            if (itemData == null || itemData.spawnPrefab == null || itemData.name == null) continue;

            //SharedSFXApplause
            if (itemData.name == "Jetpack")
            {
                JetpackItem jetpackScript = itemData.spawnPrefab.GetComponent<JetpackItem>();
                if (jetpackScript != null && jetpackScript.applause != null && jetpackScript.applause.samples == 197632)
                {
                    sharedSFXapplause = jetpackScript.applause;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Applause");
                }
                continue;
            }



            //JacobsLadder
            if (itemData.name == "ProFlashlight")
            {
                Logger.LogDebug("Trying to build JACOB'S LADDER");
                Mesh originalMesh = itemData.spawnPrefab.transform.GetChild(2).GetComponent<MeshFilter>().sharedMesh;
                FlashlightItem flashlightScript = itemData.spawnPrefab.GetComponent<FlashlightItem>();
                //Mesh
                if (originalMesh.vertexCount == 3150)
                {
                    jacobsLadderMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Pro-Flashlight");
                }
                //SFX (FlashlightOutOfBatteries)
                if (flashlightScript != null && flashlightScript.outOfBatteriesClip != null && flashlightScript.outOfBatteriesClip.samples == 22745)
                {
                    sharedSFXflashlightOut = flashlightScript.outOfBatteriesClip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: FlashlightOutOfBatteries");
                }
                //SFX (FlashlightFlicker)
                if (flashlightScript != null && flashlightScript.flashlightFlicker != null && flashlightScript.flashlightFlicker.samples == 38174)
                {
                    sharedSFXflashlightFlicker = flashlightScript.flashlightFlicker;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: FlashlightFlicker");
                }
                //SFX (FlashlightClick)
                if (flashlightScript != null && flashlightScript.flashlightClips != null && flashlightScript.flashlightClips[0] != null && flashlightScript.flashlightClips[0].samples == 13375)
                {
                    sharedSFXflashlightClip = flashlightScript.flashlightClips[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: FlashlightClick");
                }
                //SFX (GrabFlashlight)
                if (itemData.grabSFX != null && itemData.grabSFX.samples == 27864)
                {
                    sharedSFXflashlightGrab = itemData.grabSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: GrabFlashlight");
                }
                //SFX (PocketFlashlight)
                if (itemData.pocketSFX != null && itemData.pocketSFX.samples == 13853)
                {
                    sharedSFXflashlightPocket = itemData.pocketSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: PocketFlashlight");
                }
                //ItemIcon (Flashlight)
                if (itemData.itemIcon != null && itemData.itemIcon.name == "FlashlightIcon")
                {
                    sharedItemIconFlashlight = itemData.itemIcon;
                }
                else
                {
                    Logger.LogWarning("Icon was not recognized. Only icons from vanilla Lethal Company will be applied. Icon: FlashlightIcon");
                }
                continue;
            }



            //GoldenPickaxe GrabSFX & PocketSFX
            if (itemData.name == "Shovel")
            {
                //SFX (Grab)
                if (itemData.grabSFX != null && itemData.grabSFX.samples == 37199)
                {
                    sharedSFXgrabShovel = itemData.grabSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: GrabShovel");
                }
                //SFX (Pocket)
                if (itemData.pocketSFX != null && itemData.pocketSFX.samples == 22273)
                {
                    sharedSFXshovelPocket = itemData.pocketSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: PocketShovel");
                }
                continue;
            } 



            //GoldenGrenade
            if (itemData.name == "StunGrenade")
            {
                Logger.LogDebug("Trying to build GOLDEN GRENADE");
                Mesh originalBodyMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                Mesh originalPinMesh = itemData.spawnPrefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                StunGrenadeItem grenadeScript = itemData.spawnPrefab.GetComponent<StunGrenadeItem>();
                //Mesh (Body)
                if (originalBodyMesh.vertexCount == 890)
                {
                    goldenGrenadeBodyMesh = originalBodyMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Stun Grenade (Body)");
                }
                //Mesh (Pin)
                if (originalPinMesh.vertexCount == 64)
                {
                    goldenGrenadePinMesh = originalPinMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Stun Grenade (Pin)");
                }
                //SFX (Explode)
                if (grenadeScript != null && grenadeScript.explodeSFX != null && grenadeScript.explodeSFX.samples == 32879)
                {
                    sharedSFXgrenadeExplode = grenadeScript.explodeSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: FlashbangExplode");
                }
                //SFX (Pull)
                if (grenadeScript != null && grenadeScript.pullPinSFX != null && grenadeScript.pullPinSFX.samples == 125667)
                {
                    sharedSFXgrenadePullPin = grenadeScript.pullPinSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: FlashbangPullPin");
                }
                //ItemIcon (Grenade)
                if (itemData.itemIcon != null && itemData.itemIcon.name == "StunGrenadeIcon")
                {
                    sharedItemIconGrenade = itemData.itemIcon;
                }
                else
                {
                    Logger.LogWarning("Icon was not recognized. Only icons from vanilla Lethal Company will be applied. Icon: StunGrenadeIcon");
                }
                //Miscellaneous
                flashbangParticle = grenadeScript.stunGrenadeExplosion;
                grenadeFallCurve = grenadeScript.grenadeFallCurve;
                grenadeVerticalFallCurve = grenadeScript.grenadeVerticalFallCurve;
                grenadeVerticalFallCurveNoBounce = grenadeScript.grenadeVerticalFallCurveNoBounce;
                continue;
            }



            //LadderSFX
            if (itemData.name == "ExtensionLadder")
            {
                ExtensionLadderItem ladderScript = itemData.spawnPrefab.GetComponent<ExtensionLadderItem>();
                //SFX (Extend)
                if (ladderScript != null && ladderScript.ladderExtendSFX != null && ladderScript.ladderExtendSFX.samples == 43653)
                {
                    sharedSFXladderExtend = ladderScript.ladderExtendSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ExtensionLadderExtend");
                }
                //SFX (Fall)
                if (ladderScript != null && ladderScript.ladderFallSFX != null && ladderScript.ladderFallSFX.samples == 171363)
                {
                    sharedSFXladderFall = ladderScript.ladderFallSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: FallingAir");
                }
                continue;
            }



            //GoldenAirhorn
            if (itemData.name == "Airhorn")
            {
                Logger.LogDebug("Trying to build GOLDEN AIRHORN");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                NoisemakerProp airhornScript = itemData.spawnPrefab.GetComponent<NoisemakerProp>();
                //Mesh
                if (originalMesh.vertexCount == 496)
                {
                    goldenAirhornMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Airhorn");
                }
                //SFX (AirHorn1)
                if (airhornScript != null && airhornScript.noiseSFX != null && airhornScript.noiseSFX[0] != null && airhornScript.noiseSFX[0].samples == 70913)
                {
                    sharedSFXairhornNoise = airhornScript.noiseSFX[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: AirHorn1");
                }
                //SFX (AirHornFar)
                if (airhornScript != null && airhornScript.noiseSFXFar != null && airhornScript.noiseSFXFar[0] != null && airhornScript.noiseSFXFar[0].samples == 70913)
                {
                    sharedSFXairhornFarNoise = airhornScript.noiseSFXFar[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: AirHornFar");
                }
                //ItemIcon (Scrap)
                if (itemData.itemIcon != null && itemData.itemIcon.name == "ScrapItemIcon2")
                {
                    sharedItemIconScrap = itemData.itemIcon;
                }
                else
                {
                    Logger.LogWarning("Icon was not recognized. Only icons from vanilla Lethal Company will be applied. Icon: ScrapItemIcon2");
                }
                continue;
            }



            //GoldenBell
            if (itemData.name == "Bell")
            {
                Logger.LogDebug("Trying to build GOLDEN BELL");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                //Mesh
                if (originalMesh.vertexCount == 469)
                {
                    goldenBellMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Brass Bell");
                }
                //SFX (DropBell)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 122127)
                {
                    sharedSFXdropBell = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropBell");
                }
                //SFX (ShovelPickUp)
                if (itemData.grabSFX != null && itemData.grabSFX.samples == 19923)
                {
                    sharedSFXshovelPickUp = itemData.grabSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ShovelPickUp");
                }
                continue;
            }



            //GoldBolt
            if (itemData.name == "BigBolt")
            {
                Logger.LogDebug("Trying to build GOLD BOLT");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                //Mesh
                if (originalMesh.vertexCount == 458)
                {
                    goldBoltMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Big Bolt");
                }
                //SFX (DropMetalObject1)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 37878)
                {
                    sharedSFXdropMetalObject1 = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropMetalObject1");
                }
                continue;
            }



            //GoldRegister
            if (itemData.name == "CashRegister")
            {
                Logger.LogDebug("Trying to build GOLD REGISTER");
                GameObject GoldRegisterMain = itemData.spawnPrefab.transform.GetChild(0).gameObject;
                GameObject GoldRegisterCrank = itemData.spawnPrefab.transform.GetChild(1).GetChild(0).gameObject;
                GameObject GoldRegisterDrawer = itemData.spawnPrefab.transform.GetChild(1).GetChild(1).gameObject;
                NoisemakerProp noisemakerScript = itemData.spawnPrefab.transform.GetComponent<NoisemakerProp>();
                Mesh originalMesh1 = GoldRegisterMain.GetComponent<MeshFilter>().sharedMesh;
                Mesh originalMesh2 = GoldRegisterCrank.GetComponent<MeshFilter>().sharedMesh;
                Mesh originalMesh3 = GoldRegisterDrawer.GetComponent<MeshFilter>().sharedMesh;
                //Mesh
                if (originalMesh1.vertexCount == 997)
                {
                    goldRegisterMainMesh = originalMesh1;
                    goldRegisterCrankMesh = originalMesh2;
                    goldRegisterDrawerMesh = originalMesh3;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Cash Register");
                }
                //SFX (CashRegisterDing)
                if (noisemakerScript != null && noisemakerScript.noiseSFX != null && noisemakerScript.noiseSFX[0] != null && noisemakerScript.noiseSFX[0].samples == 135976)
                {
                    sharedSFXcashRegisterNoise = noisemakerScript.noiseSFX[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: CashRegisterDing");
                }
                //SFX (DropMetalObject3)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 60666)
                {
                    sharedSFXdropMetalObject3 = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropMetalObject3");
                }
                continue;
            }



            //GoldJug
            if (itemData.name == "ChemicalJug")
            {
                Logger.LogDebug("Trying to build GOLD JUG");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 320)
                {
                    goldJugMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Chemical Jug");
                }
                continue;
            }



            //GoldenHorn
            if (itemData.name == "ClownHorn")
            {
                Logger.LogDebug("Trying to build GOLDEN HORN");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                NoisemakerProp clownhornScript = itemData.spawnPrefab.GetComponent<NoisemakerProp>();
                //Mesh
                if (originalMesh.vertexCount == 269)
                {
                    goldenHornMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Clown Horn");
                }
                //SFX (ClownHorn1)
                if (clownhornScript != null && clownhornScript.noiseSFX != null && clownhornScript.noiseSFX[0] != null && clownhornScript.noiseSFX[0].samples == 22081)
                {
                    sharedSFXclownhornNoise = clownhornScript.noiseSFX[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ClownHorn1");
                }
                //SFX (ClownHornFar)
                if (clownhornScript != null && clownhornScript.noiseSFXFar != null && clownhornScript.noiseSFXFar[0] != null && clownhornScript.noiseSFXFar[0].samples == 63795)
                {
                    sharedSFXclownhornFarNoise = clownhornScript.noiseSFXFar[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ClownHornFar");
                }
                continue;
            }



            //GoldAxle
            if (itemData.name == "Cog1")
            {
                Logger.LogDebug("Trying to build GOLD AXLE");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 1400)
                {
                    goldAxleMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Large Axle");
                }
                continue;
            }



            //GoldPan
            if (itemData.name == "DustPan")
            {
                Logger.LogDebug("Trying to build GOLD PAN");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 105)
                {
                    goldPanMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Dust Pan");
                }
                continue;
            }



            //GoldenEggbeater
            if (itemData.name == "EggBeater")
            {
                Logger.LogDebug("Trying to build GOLDEN EGGBEATER");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 269)
                {
                    goldenEggbeaterMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Eggbeater");
                }
                continue;
            }



            //GoldTypeEngine
            if (itemData.name == "EnginePart1")
            {
                Logger.LogDebug("Trying to build GOLD-TYPE ENGINE");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 4972)
                {
                    goldTypeEngineMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: V-Type Engine");
                }
                continue;
            }



            //ExtremelyGoldenCup
            if (itemData.name == "FancyCup")
            {
                Logger.LogDebug("Trying to build EXTREMELY GOLDEN CUP");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 481)
                {
                    extremelyGoldenCupMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Golden Cup");
                }
                continue;
            }



            //GoldBeacon
            if (itemData.name == "FancyLamp")
            {
                Logger.LogDebug("Trying to build GOLD BEACON");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 1160)
                {
                    goldBeaconMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Fancy Lamp");
                }
                continue;
            }



            //ArtOfGold
            if (itemData.name == "FancyPainting")
            {
                Logger.LogDebug("Trying to build ART OF GOLD");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 92)
                {
                    artOfGoldMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Painting");
                }
                artOfGoldMaterials.allArtwork.AddRange(itemData.materialVariants);
                continue;
            }



            //GoldFishProp
            if (itemData.name == "FishTestProp")
            {
                Logger.LogDebug("Trying to build GOLD FISH");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 347)
                {
                    goldFishPropMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Plastic Fish");
                }
                continue;
            }



            //GolderBar
            if (itemData.name == "GoldBar")
            {
                Logger.LogDebug("Trying to build GOLDER BAR");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 74)
                {
                    golderBarMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Gold Bar");
                }
                continue;
            }



            //GoldenGlass
            if (itemData.name == "MagnifyingGlass")
            {
                Logger.LogDebug("Trying to build GOLDEN GLASS");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 551)
                {
                    goldenGlassMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Magnifying Glass");
                }
                continue;
            }



            //TatteredGoldSheet
            if (itemData.name == "MetalSheet")
            {
                Logger.LogDebug("Trying to build TATTERED GOLD SHEET");
                Mesh originalMesh1 = itemData.meshVariants[0];
                Mesh originalMesh2 = itemData.meshVariants[1];
                //Mesh (Variant 1)
                if (originalMesh1.vertexCount == 272)
                {
                    tatteredGoldSheetMesh = originalMesh1;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Tattered Metal Sheet");
                }
                //Mesh (Variant 2)
                if (originalMesh2.vertexCount == 232)
                {
                    tatteredGoldSheetAltMesh = originalMesh2;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Tattered Metal Sheet (variant)");
                }
                //SFX (DropThinMetal)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 62415)
                {
                    sharedSFXdropThinMetal = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropThinMetal");
                }
                continue;
            }



            //CookieGoldPan
            if (itemData.name == "MoldPan")
            {
                Logger.LogDebug("Trying to build COOKIE GOLD PAN");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 1108)
                {
                    cookieGoldPanMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Cookie Mold Pan");
                }
                continue;
            }



            //GoldMug
            if (itemData.name == "Mug")
            {
                Logger.LogDebug("Trying to build GOLD MUG");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 146)
                {
                    goldMugMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Mug");
                }
                continue;
            }



            //GoldPerfume
            if (itemData.name == "PerfumeBottle")
            {
                Logger.LogDebug("Trying to build GOLD PERFUME");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 370)
                {
                    goldPerfumeMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Perfume Bottle");
                }
                continue;
            }



            //TalkativeGoldBar
            if (itemData.name == "Phone")
            {
                Logger.LogDebug("Trying to build TALKATIVE GOLD BAR");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                AnimatedItem oldPhoneScript = itemData.spawnPrefab.GetComponent<AnimatedItem>();
                //Mesh
                if (originalMesh.vertexCount == 629)
                {
                    talkativeGoldBarMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Old Phone");
                }
                //SFX
                if (oldPhoneScript != null && oldPhoneScript.grabAudio != null && oldPhoneScript.grabAudio.samples == 310290)
                {
                    sharedSFXoldPhoneNoise = oldPhoneScript.grabAudio;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: PhoneScream");
                }
                continue;
            }



            //JarOfGoldPickles
            if (itemData.name == "PickleJar")
            {
                Logger.LogDebug("Trying to build JAR OF GOLD PICKLES");
                Mesh originalJarMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                Mesh originalPicklesMesh = itemData.spawnPrefab.transform.GetChild(0).gameObject.GetComponent<MeshFilter>().sharedMesh;
                //Mesh (Jar)
                if (originalJarMesh.vertexCount == 154)
                {
                    goldPicklesJarMesh = originalJarMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Pickle Jar");
                }
                //Mesh (Pickles)
                if (originalPicklesMesh.vertexCount == 262)
                {
                    goldPicklesPickleMesh = originalPicklesMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Pickles");
                }
                continue;
            }



            //GoldRemote
            if (itemData.name == "Remote")
            {
                Logger.LogDebug("Trying to build GOLD REMOTE");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                RemoteProp itemScript = itemData.spawnPrefab.GetComponent<RemoteProp>();
                //Mesh
                if (originalMesh.vertexCount == 467)
                {
                    goldRemoteMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Remote");
                }
                //SFX
                if (itemScript != null && itemScript.remoteAudio != null && itemScript.remoteAudio.clip != null && itemScript.remoteAudio.clip.samples == 8986)
                {
                    sharedSFXremoteClick = itemScript.remoteAudio.clip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: RemoteClick");
                }
                continue;
            }



            //GoldRing
            if (itemData.name == "Ring")
            {
                Logger.LogDebug("Trying to build GOLD RING");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 223)
                {
                    goldRingMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Wedding Ring");
                }
                continue;
            }



            //GoldToyRobot
            if (itemData.name == "RobotToy")
            {
                Logger.LogDebug("Trying to build G.O.L.D.");
                Mesh originalMesh1 = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                Mesh originalMesh2 = itemData.spawnPrefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                Mesh originalMesh3 = itemData.spawnPrefab.transform.GetChild(2).GetComponent<MeshFilter>().sharedMesh;
                AnimatedItem originalToyRobot = itemData.spawnPrefab.GetComponent<AnimatedItem>();
                //Mesh
                if (originalMesh1.vertexCount == 436 || originalMesh1.vertexCount == 440)
                {
                    goldToyRobotMainMesh = originalMesh1;
                    goldToyRobotRightMesh = originalMesh2;
                    goldToyRobotLeftMesh = originalMesh3;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Robot Toy");
                }
                //SFX
                if (originalToyRobot != null && originalToyRobot.grabAudio != null && originalToyRobot.grabAudio.samples == 375606)
                {
                    sharedSFXrobotToyCheer = originalToyRobot.grabAudio;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: RobotToyCheer");
                }
                continue;
            }



            //DuckOfGold
            if (itemData.name == "RubberDuck")
            {
                Logger.LogDebug("Trying to build DUCK OF GOLD");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                //Mesh
                if (originalMesh.vertexCount == 257)
                {
                    duckOfGoldMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Rubber Ducky");
                }
                //SFX (DuckQuack)
                if (itemData.grabSFX != null && itemData.grabSFX.samples == 8562)
                {
                    sharedSFXgrabDuck = itemData.grabSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DuckQuack");
                }
                //SFX (DropRubberDuck)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 24925)
                {
                    sharedSFXdropDuck = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropRubberDuck");
                }
                continue;
            }



            //TiltControls
            if (itemData.name == "SteeringWheel")
            {
                Logger.LogDebug("Trying to build TILT CONTROLS");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 462)
                {
                    tiltControlsMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Steering Wheel");
                }
                continue;
            }



            //GoldSign
            if (itemData.name == "StopSign")
            {
                Logger.LogDebug("Trying to build GOLD SIGN");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                Shovel shovelScript = itemData.spawnPrefab.GetComponent<Shovel>();
                //Mesh
                if (originalMesh.vertexCount == 246)
                {
                    goldSignMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Stop Sign");
                }
                //SFX (ShovelHitDefault)
                if (shovelScript != null && shovelScript.hitSFX != null && shovelScript.hitSFX[0] != null && shovelScript.hitSFX[0].samples == 60486)
                {
                    sharedSFXshovelHit0 = shovelScript.hitSFX[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ShovelHitDefault");
                }
                //SFX (ShovelHitDefault2)
                if (shovelScript != null && shovelScript.hitSFX != null && shovelScript.hitSFX[1] != null && shovelScript.hitSFX[1].samples == 78795)
                {
                    sharedSFXshovelHit1 = shovelScript.hitSFX[1];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ShovelHitDefault2");
                }
                //SFX (ShovelReelUp)
                if (shovelScript != null && shovelScript.reelUp != null && shovelScript.reelUp.samples == 26610)
                {
                    sharedSFXshovelReel = shovelScript.reelUp;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ShovelReelUp");
                }
                //SFX (ShovelSwing)
                if (shovelScript != null && shovelScript.swing != null && shovelScript.swing.samples == 19784)
                {
                    sharedSFXshovelSwing = shovelScript.swing;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ShovelSwing");
                }
                //SFX (DropMetalObject2)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 28710)
                {
                    sharedSFXdropMetalObject2 = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropMetalObject2");
                }
                continue;
            }



            //EarlGold
            if (itemData.name == "TeaKettle")
            {
                Logger.LogDebug("Trying to build EARL GOLD");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 495)
                {
                    earlGoldMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Tea Kettle");
                }
                continue;
            }



            //GoldPuzzle
            if (itemData.name == "ToyCube")
            {
                Logger.LogDebug("Trying to build GOLD PUZZLE");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 112)
                {
                    goldPuzzleMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Toy Cube");
                }
                continue;
            }



            //GoldSign (Config Tools Rebalanced)
            if (itemData.name == "YieldSign")
            {
                Logger.LogDebug("Trying to build GOLD SIGN (ALT)");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 240)
                {
                    goldSignMeshAlt = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Yield Sign");
                }
                continue;
            }



            //GoldenGrenade (Config Tools Rebalance)
            if (itemData.name == "DiyFlashbang")
            {
                Logger.LogDebug("Trying to build GOLDEN GRENADE (ALT)");
                Mesh originalBodyMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                Mesh originalPinMesh = itemData.spawnPrefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                StunGrenadeItem grenadeScript = itemData.spawnPrefab.GetComponent<StunGrenadeItem>();
                //Mesh (Body)
                if (originalBodyMesh.vertexCount == 433)
                {
                    goldenGrenadeBodyMeshAlt = originalBodyMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: DIY-Flashbang (Body)");
                }
                //Mesh (Pin)
                if (originalPinMesh.vertexCount == 263)
                {
                    goldenGrenadePinMeshAlt = originalPinMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: DIY-Flashbang (Pin)");
                }
                //SFX (Pull)
                if (grenadeScript != null && grenadeScript.pullPinSFX != null && grenadeScript.pullPinSFX.samples == 92420)
                {
                    sharedSFXgrenadePullPinAlt = grenadeScript.pullPinSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: HomemadeFlashbangPullCork");
                }
                continue;
            }



            //PoofParticle
            if (itemData.name == "GiftBox")
            {
                poofParticle = itemData.spawnPrefab.transform.Find("PoofParticle").gameObject;
                continue;
            }



            //GoldenFlask
            if (itemData.name == "Flask")
            {
                Logger.LogDebug("Trying to build GOLDEN FLASK");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                //Mesh
                if (originalMesh.vertexCount == 176 || originalMesh.vertexCount == 128)
                {
                    goldenFlaskMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Flask");
                }
                //SFX (GrabBottle)
                if (itemData.grabSFX != null && itemData.grabSFX.samples == 25564)
                {
                    sharedSFXgrabBottle = itemData.grabSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: GrabBottle");
                }
                //SFX (DropGlass1)
                if (itemData.dropSFX != null && itemData.dropSFX.samples == 13562)
                {
                    sharedSFXdropGlass1 = itemData.dropSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: DropGlass1");
                }
                continue;
            }



            //PurifiedMask
            if (itemData.name == "ComedyMask")
            {
                Logger.LogDebug("Trying to build PURIFIED MASK");
                Mesh originalMesh = itemData.spawnPrefab.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 624)
                {
                    purifiedMaskMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Comedy");
                }
                continue;
            }



            //ComedyGold
            if (itemData.name == "WhoopieCushion")
            {
                Logger.LogDebug("Trying to build COMEDY GOLD");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                WhoopieCushionItem whoopieCushionScript = itemData.spawnPrefab.GetComponent<WhoopieCushionItem>();
                //Mesh
                if (originalMesh.vertexCount == 97)
                {
                    comedyGoldMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Whoopie Cushion");
                }
                //SFX (Fart1)
                if (whoopieCushionScript != null && whoopieCushionScript.fartAudios != null && whoopieCushionScript.fartAudios[0] != null && whoopieCushionScript.fartAudios[0].samples == 13357)
                {
                    sharedSFXwhoopieCushionNoise0 = whoopieCushionScript.fartAudios[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Fart1");
                }
                //SFX (Fart2)
                if (whoopieCushionScript != null && whoopieCushionScript.fartAudios != null && whoopieCushionScript.fartAudios[1] != null && whoopieCushionScript.fartAudios[1].samples == 10101)
                {
                    sharedSFXwhoopieCushionNoise1 = whoopieCushionScript.fartAudios[1];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Fart2");
                }
                //SFX (Fart3)
                if (whoopieCushionScript != null && whoopieCushionScript.fartAudios != null && whoopieCushionScript.fartAudios[2] != null && whoopieCushionScript.fartAudios[2].samples == 33781)
                {
                    sharedSFXwhoopieCushionNoise2 = whoopieCushionScript.fartAudios[2];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Fart3");
                }
                //SFX (Fart5)
                if (whoopieCushionScript != null && whoopieCushionScript.fartAudios != null && whoopieCushionScript.fartAudios[3] != null && whoopieCushionScript.fartAudios[3].samples == 14280)
                {
                    sharedSFXwhoopieCushionNoise3 = whoopieCushionScript.fartAudios[3];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Fart5");
                }
                continue;
            }



            //GoldenVision
            if (itemData.name == "ToiletPaperRolls")
            {
                Logger.LogDebug("Trying to build GOLDEN VISION");
                Mesh originalMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 1170)
                {
                    goldenVisionMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Toilet Paper");
                }
                continue;
            }



            //GoldenClock
            if (itemData.name == "Clock")
            {
                Logger.LogDebug("Trying to build GOLDEN CLOCK");
                Mesh bodyMesh = itemData.spawnPrefab.GetComponent<MeshFilter>().sharedMesh;
                Mesh minuteMesh = itemData.spawnPrefab.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
                ClockProp clockScript = itemData.spawnPrefab.GetComponent<ClockProp>();
                //Mesh (Body)
                if (bodyMesh.vertexCount == 690)
                {
                    goldenClockBodyMesh = bodyMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Clock (Body)");
                }
                //Mesh (Minute)
                if (minuteMesh.vertexCount == 4)
                {
                    goldenClockMinuteMesh = minuteMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Clock (Minute Hand)");
                }
                //SFX (Tick)
                if (clockScript != null && clockScript.tickSFX != null && clockScript.tickSFX.samples == 6162)
                {
                    sharedSFXclockTick = clockScript.tickSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ClockTick");
                }
                //SFX (Tock)
                if (clockScript != null && clockScript.tockSFX != null && clockScript.tockSFX.samples == 5067)
                {
                    sharedSFXclockTock = clockScript.tockSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ClockTock");
                }
                continue;
            }
        }
        alreadyCheckedItems = true;
    }

    public static void GetEnemyAssets(SpawnableEnemyWithRarity[] allEnemies, SpawnableEnemyWithRarity[] allOutsideEnemies)
    {
        if (alreadyCheckedEnemies)
        {
            return;
        }

        for (int i = 0; i < allEnemies.Length; i++)
        {
            SpawnableEnemyWithRarity spawnableEnemy = allEnemies[i];
            if (spawnableEnemy == null || spawnableEnemy.enemyType == null) continue;
            EnemyType enemy = spawnableEnemy.enemyType;
            if (enemy == null || enemy.enemyName == null) continue;

            //GoldenGirl
            if (enemy.enemyName == "Girl")
            {
                Logger.LogDebug("Trying to build GOLDEN GIRL");
                Mesh originalMesh = enemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                DressGirlAI girlScript = enemy.enemyPrefab.GetComponent<DressGirlAI>();
                if (originalMesh.vertexCount == 2022)
                {
                    goldenGirlMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Ghost Girl");
                }
                if (girlScript != null && girlScript.appearStaringSFX != null)
                {
                    foreach (AudioClip clip in girlScript.appearStaringSFX)
                    {
                        if (clip != null && clip.name == "Laugh2" && clip.samples == 36780)
                        {
                            sharedSFXgirlLaugh = clip;
                            break;
                        }
                    }
                    if (sharedSFXgirlLaugh == null)
                    {
                        Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Laugh2");
                    }
                }
                continue;
            }



            //Marigold
            if (enemy.enemyName == "Flowerman")
            {
                Logger.LogDebug("Trying to build MARIGOLD");
                Mesh originalMesh = enemy.enemyPrefab.transform.GetChild(1).GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                if (originalMesh.vertexCount == 4866)
                {
                    marigoldMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Bracken");
                }
                continue;
            }



            //GoldSpring
            if (enemy.enemyName == "Spring")
            {
                Logger.LogDebug("Trying to build GOLD SPRING");
                Mesh originalMesh1 = enemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                Mesh originalMesh2 = enemy.enemyPrefab.gameObject.transform.GetChild(0).GetChild(1).GetComponent<MeshFilter>().sharedMesh;
                SpringManAI coilheadScript = enemy.enemyPrefab.GetComponent<SpringManAI>();
                //Meshes
                if (originalMesh1.vertexCount == 2411)
                {
                    goldSpringMesh = originalMesh1;
                    goldSpringHeadMesh = originalMesh2;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Coil-head");
                }
                //SFX (Hard)
                if (coilheadScript != null && coilheadScript.springNoises != null && coilheadScript.springNoises[0] != null && coilheadScript.springNoises[0].samples == 183392)
                {
                    sharedSFXspringHardClip = coilheadScript.springNoises[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Spring1");
                }
                //SFX (Soft)
                if (coilheadScript != null && coilheadScript.springNoises != null && coilheadScript.springNoises[4] != null && coilheadScript.springNoises[4].samples == 77901)
                {
                    sharedSFXspringSoftClip = coilheadScript.springNoises[4];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: SpringWobble2");
                }
                continue;
            }



            //CuddlyGold
            if (enemy.enemyName == "Hoarding bug")
            {
                Logger.LogDebug("Trying to build CUDDLY GOLD");
                Mesh originalMesh = enemy.enemyPrefab.transform.GetChild(2).GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                if (originalMesh.vertexCount == 1101)
                {
                    cuddlyGoldMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Hoarding Bug");
                }
                continue;
            }



            //JackInTheGold
            if (enemy.enemyName == "Jester")
            {
                Logger.LogDebug("Trying to build JACK IN THE GOLD");
                Mesh originalBodyMesh = enemy.enemyPrefab.transform.GetChild(0).GetChild(2).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                Mesh originalLidMesh = enemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                Mesh originalCrankMesh = enemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<MeshFilter>().sharedMesh;
                Mesh originalUpperJawMesh = enemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                Mesh originalLowerJawMesh = enemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                JesterAI jesterScript = enemy.enemyPrefab.GetComponent<JesterAI>();
                //Mesh (Body)
                if (originalBodyMesh.vertexCount == 891)
                {
                    jackInTheGoldBodyMesh = originalBodyMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Jester (Body)");
                }
                //Mesh (Lid)
                if (originalLidMesh.vertexCount == 279)
                {
                    jackInTheGoldLidMesh = originalLidMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Jester (Lid)");
                }
                //Mesh (Crank)
                if (originalCrankMesh.vertexCount == 121)
                {
                    jackInTheGoldCrankMesh = originalCrankMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Jester (Crank)");
                }
                //Mesh (Jaw Upper)
                if (originalUpperJawMesh.vertexCount == 798)
                {
                    jackInTheGoldUpperJawMesh = originalUpperJawMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Jester (Jaw - Upper)");
                }
                //Mesh (Jaw Lower)
                if (originalLowerJawMesh.vertexCount == 334)
                {
                    jackInTheGoldLowerJawMesh = originalLowerJawMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Jester (Jaw - Lower)");
                }
                //SFX (Music)
                if (jesterScript != null && jesterScript.popGoesTheWeaselTheme != null && jesterScript.popGoesTheWeaselTheme.samples == 1890304)
                {
                    sharedSFXjesterTheme = jesterScript.popGoesTheWeaselTheme;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: JackInTheBoxTheme");
                }
                //SFX (Pop)
                if (jesterScript != null && jesterScript.popUpSFX != null && jesterScript.popUpSFX.samples == 115069)
                {
                    sharedSFXjesterPop = jesterScript.popUpSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Pop1");
                }
                continue;
            }



            //GoldenGuardian
            if (enemy.enemyName == "Nutcracker")
            {
                Logger.LogDebug("Trying to build GOLDEN GUARDIAN");
                Mesh originalMesh = enemy.enemyPrefab.transform.GetChild(1).GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                NutcrackerEnemyAI nutcrackerEnemyAI = enemy.enemyPrefab.GetComponent<NutcrackerEnemyAI>();
                //Mesh
                if (originalMesh.vertexCount == 1111)
                {
                    goldenGuardianMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Nutcracker");
                }
                //AimSFX
                if (nutcrackerEnemyAI != null && nutcrackerEnemyAI.aimSFX != null && nutcrackerEnemyAI.aimSFX.samples == 50712)
                {
                    sharedSFXnutcrackerAim = nutcrackerEnemyAI.aimSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: JackInTheBoxTheme");
                }
                continue;
            }



            //Barber
            if (enemy.enemyName == "Clay Surgeon")
            {
                ClaySurgeonAI barberScript = enemy.enemyPrefab.GetComponent<ClaySurgeonAI>();
                if (barberScript != null && barberScript.snareDrum != null && barberScript.snareDrum.samples == 62799)
                {
                    sharedSFXsnareBuildUp = barberScript.snareDrum;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: SnareBuildUp");
                }
                continue;
            }
        }

        for (int j = 0; j < allOutsideEnemies.Length; j++)
        {
            SpawnableEnemyWithRarity spawnableOutsideEnemy = allOutsideEnemies[j];
            if (spawnableOutsideEnemy == null || spawnableOutsideEnemy.enemyType == null) continue;
            EnemyType outsideEnemy = spawnableOutsideEnemy.enemyType;
            if (outsideEnemy == null) continue;

            //Goldkeeper
            if (outsideEnemy.enemyName == "ForestGiant")
            {
                Logger.LogDebug("Trying to build GOLDKEEPER");
                Mesh originalMesh = outsideEnemy.enemyPrefab.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                if (originalMesh.vertexCount == 2416)
                {
                    goldkeeperMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Forest Keeper");
                }
            }



            //GoldenRetriever
            if (outsideEnemy.enemyName == "MouthDog")
            {
                Logger.LogDebug("Trying to build GOLDEN RETRIEVER");
                Mesh originalBodyMesh = outsideEnemy.enemyPrefab.transform.GetChild(1).GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                Mesh originalTopMesh = outsideEnemy.enemyPrefab.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(1).GetComponent<MeshFilter>().sharedMesh;
                Mesh originalBottomMesh = outsideEnemy.enemyPrefab.transform.GetChild(1).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetComponent<MeshFilter>().sharedMesh;
                //Mesh (Body)
                if (originalBodyMesh.vertexCount == 1080)
                {
                    goldenRetrieverBodyMesh = originalBodyMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Eyeless Dog (Body)");
                }
                //Mesh (Upper Teeth)
                if (originalTopMesh.vertexCount == 315)
                {
                    goldenRetrieverTopMesh = originalTopMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Eyeless Dog (Teeth - Upper)");
                }
                //Mesh (Lower Teeth)
                if (originalBottomMesh.vertexCount == 134)
                {
                    goldenRetrieverBottomMesh = originalBottomMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Eyeless Dog (Teeth - Lower)");
                }
                continue;
            }



            //GoldBird
            if (outsideEnemy.enemyName == "RadMech")
            {
                Logger.LogDebug("Trying to build GOLD BIRD");
                Mesh originalMesh = outsideEnemy.enemyPrefab.transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>().sharedMesh;
                RadMechAI oldBirdScript = outsideEnemy.enemyPrefab.GetComponent<RadMechAI>();
                //Mesh
                if (originalMesh.vertexCount == 3865)
                {
                    goldBirdMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Old Bird");
                }
                //SFX (Alarm)
                if (oldBirdScript != null && oldBirdScript.LocalLRADAudio != null && oldBirdScript.LocalLRADAudio.clip != null && oldBirdScript.LocalLRADAudio.clip.samples == 99175)
                {
                    sharedSFXoldBirdAlarm = oldBirdScript.LocalLRADAudio.clip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: LRADAlarm3");
                }
                //SFX (Awake)
                if (oldBirdScript != null && oldBirdScript.creatureSFX != null && oldBirdScript.creatureSFX.clip != null && oldBirdScript.creatureSFX.clip.samples == 101166)
                {
                    sharedSFXoldBirdWake = oldBirdScript.creatureSFX.clip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: RadMechWake");
                }
                //SFX (On)
                if (oldBirdScript != null && oldBirdScript.spotlightOnAudio != null && oldBirdScript.spotlightOnAudio.clip != null && oldBirdScript.spotlightOnAudio.clip.samples == 49344)
                {
                    sharedSFXoldBirdOn = oldBirdScript.spotlightOnAudio.clip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: SpotlightOn");
                }
                //SFX (Off)
                if (oldBirdScript != null && oldBirdScript.spotlightOff != null && oldBirdScript.spotlightOff.samples == 27904)
                {
                    sharedSFXoldBirdOff = oldBirdScript.spotlightOff;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: AirConditionerSwitch");
                }
                //SFX (Shoot)
                if (oldBirdScript != null && oldBirdScript.shootGunSFX != null && oldBirdScript.shootGunSFX[0] != null && oldBirdScript.shootGunSFX[0].samples == 46587)
                {
                    sharedSFXgunShoot = oldBirdScript.shootGunSFX[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ShotgunBlast");
                }
                continue;
            }
        }
        alreadyCheckedEnemies = true;
    }

    public static void GetGameObjectAssets()
    {
        if (alreadyCheckedGameObjects)
        {
            return;
        }

        //GoldenGrunt
        Logger.LogDebug("Trying to build GOLDEN GRUNT");
        Mesh hostMesh = StartOfRound.Instance.allPlayerObjects[0].transform.GetChild(0).GetChild(2).GetComponent<SkinnedMeshRenderer>().sharedMesh;
        if (hostMesh.vertexCount == 2302)
        {
            goldenGruntMesh = hostMesh;
        }
        else
        {
            Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Player");
        }

        //GoldenBoots
        GameObject[] allGameObjectsOnStart = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject gObject in allGameObjectsOnStart)
        {
            if (gObject.name != null && gObject.name == "Circle.004")
            {
                Logger.LogDebug("Trying to build GOLDEN BOOTS");
                Mesh originalMesh = gObject.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 875)
                {
                    goldenBootsMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Boots");
                }
                break;
            }
        }

        alreadyCheckedGameObjects = true;
    }

    public static void GetUnlockableAssets(StartOfRound __instance)
    {
        if (alreadyCheckedUnlockables || __instance == null || __instance.unlockablesList == null || __instance.unlockablesList.unlockables == null)
        {
            return;
        }

        for (int i = 0; i < __instance.unlockablesList.unlockables.Count; i++)
        {
            UnlockableItem unlockable = __instance.unlockablesList.unlockables[i];
            if (unlockable == null || unlockable.unlockableName == null) continue;

            //GoldToilet
            if (unlockable.unlockableName == "Toilet")
            {
                Logger.LogDebug("Trying to build GOLD TOILET");
                Mesh originalMesh = unlockable.prefabObject.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                InteractTrigger interactTrigger = unlockable.prefabObject.transform.GetChild(2).GetComponent<InteractTrigger>();
                AnimatedObjectTrigger animatedObjectTrigger = unlockable.prefabObject.transform.GetChild(2).GetComponent<AnimatedObjectTrigger>();
                //Mesh
                if (originalMesh.vertexCount == 558)
                {
                    goldToiletMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Toilet");
                }
                //SFX
                if (animatedObjectTrigger != null && animatedObjectTrigger.boolFalseAudios != null && animatedObjectTrigger.boolFalseAudios[0] != null && animatedObjectTrigger.boolFalseAudios[0].samples == 81363)
                {
                    sharedSFXtoiletFlush = animatedObjectTrigger.boolFalseAudios[0];
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: ToiletFlush");
                }
                //HandIconPoint
                if (interactTrigger.hoverIcon.name == "HandIconPoint")
                {
                    handIconPoint = interactTrigger.hoverIcon;
                }
                else
                {
                    Logger.LogWarning("Icon was not recognized. Only icons from vanilla Lethal Company will be applied. Icon: HandIconPoint");
                }
                continue;
            }



            //HandIcon
            if (unlockable.unlockableName == "Loud horn")
            {
                InteractTrigger interactTrigger = unlockable.prefabObject.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<InteractTrigger>();
                if (interactTrigger.hoverIcon.name == "HandIcon")
                {
                    handIcon = interactTrigger.hoverIcon;
                }
                else
                {
                    Logger.LogWarning("Icon was not recognized. Only icons from vanilla Lethal Company will be applied. Icon: HandIcon");
                }
                continue;
            }



            //Goldfish
            if (unlockable.unlockableName == "Goldfish")
            {
                Logger.LogDebug("Trying to build GOLDFISH");
                Mesh originalMesh = unlockable.prefabObject.transform.GetChild(1).GetChild(0).GetChild(0).gameObject.GetComponent<MeshFilter>().sharedMesh;
                if (originalMesh.vertexCount == 74)
                {
                    goldfishMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Goldfish");
                }
                continue;
            }



            //BunnyHopSFX
            if (unlockable.unlockableName == "Bunny Suit")
            {
                AudioClip bunnyHopSFX = unlockable.jumpAudio;
                if (bunnyHopSFX != null && bunnyHopSFX.samples == 34667)
                {
                    sharedSFXbunnyHop = bunnyHopSFX;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: BunnyHop");
                }
                continue;
            }



            //DiscoBallMusic
            if (unlockable.unlockableName == "Disco Ball")
            {
                Logger.LogDebug("Trying to build DISCO BALL PLAYLIST");
                discoBallStaticPrefab = unlockable.prefabObject;
                AudioSource discoBallMusic = unlockable.prefabObject.transform.Find("AnimContainer")?.Find("Audio")?.GetComponent<AudioSource>();
                if (discoBallMusic != null && discoBallMusic.clip != null && discoBallMusic.clip.samples == 2501673)
                {
                    sharedSFXdiscoBallMusic = discoBallMusic.clip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: Boombox6QuestionMark");
                }
                continue;
            } 
        }
        alreadyCheckedUnlockables = true;
    }

    public static void GetHazardAssets(SpawnableMapObject[] allTitanHazards)
    {
        if (alreadyCheckedHazards)
        {
            return;
        }

        for (int i = 0; i < allTitanHazards.Length; i++)
        {
            SpawnableMapObject hazard = allTitanHazards[i];
            if (hazard == null || hazard.prefabToSpawn == null || hazard.prefabToSpawn.name == null) continue;

            //Goldmine
            if (hazard.prefabToSpawn.name == "Landmine")
            {
                Logger.LogDebug("Trying to build GOLDMINE");
                Mesh originalMesh = hazard.prefabToSpawn.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
                Landmine landmineScript = hazard.prefabToSpawn.transform.GetChild(0).GetComponent<Landmine>();
                PlayAudioAnimationEvent landmineAudio = hazard.prefabToSpawn.transform.GetChild(0).GetComponent<PlayAudioAnimationEvent>();
                //Mesh
                if (originalMesh.vertexCount == 935)
                {
                    goldmineMesh = originalMesh;
                }
                else
                {
                    Logger.LogWarning("Model was not recognized. Only models from vanilla Lethal Company will be applied. Model: Landmine");
                }
                //SFX (Trigger)
                if (landmineScript != null && landmineScript.mineTrigger != null && landmineScript.mineTrigger.samples == 41324)
                {
                    sharedSFXlandmineTrigger = landmineScript.mineTrigger;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: MineTrigger");
                }
                //SFX (Beep)
                if (landmineAudio != null && landmineAudio.audioClip != null && landmineAudio.audioClip.samples == 14628)
                {
                    sharedSFXlandmineBeep = landmineAudio.audioClip;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: MineBeep");
                }
                //SFX (On)
                if (landmineScript != null && landmineScript.minePress != null && landmineScript.minePress.samples == 16608)
                {
                    sharedSFXlandmineOn = landmineScript.minePress;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: PressLandmine");
                }
                //SFX (Off)
                if (landmineScript != null && landmineScript.mineDeactivate != null && landmineScript.mineDeactivate.samples == 5573)
                {
                    sharedSFXlandmineOff = landmineScript.mineDeactivate;
                }
                else
                {
                    Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: SmallClick");
                }
                break;
            }
        }
        alreadyCheckedHazards = true;
    }

    public static void GetMenuAssets(GameNetworkManager __instance)
    {
        if (alreadyCheckedMenu || __instance == null)
        {
            return;
        }

        //Confirm SFX
        if (__instance.buttonPressSFX != null &&  __instance.buttonPressSFX.samples == 10296)
        {
            sharedSFXmenuConfirm = __instance.buttonPressSFX;
        }
        else
        {
            Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: MenuConfirm");
        }

        //Cancel SFX
        if (__instance.buttonCancelSFX != null && __instance.buttonCancelSFX.samples == 14195)
        {
            sharedSFXmenuCancel = __instance.buttonCancelSFX;
        }
        else
        {
            Logger.LogWarning("Sound was not recognized. Only sounds from vanilla Lethal Company will be applied. Sound: MenuCancel");
        }

        alreadyCheckedMenu = true;
    }

    public static Mesh LoadSillyMesh(string fileName)
    {
        Mesh meshToReturn = Plugin.CustomGoldScrapAssets.LoadAsset<Mesh>($"Assets/LCGoldScrapMod/GoldScrapVisuals/Meshes/SillyScrapModels/{fileName}.blend");
        if (meshToReturn == null)
        {
            Logger.LogError($"LoadSillyMesh() called with null '{fileName}'");
        }
        return meshToReturn;
    }

    public static Mesh LoadGoldStoreMesh(string fileName)
    {
        string folderName = fileName;
        if (fileName.Contains("Silly"))
        {
            folderName = fileName.Remove(0, 5);
        }
        Mesh meshToReturn = Plugin.CustomGoldScrapAssets.LoadAsset<Mesh>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{folderName}/{fileName}.blend");
        if (meshToReturn == null)
        {
            Logger.LogError($"LoadGoldStoreMesh() called with null '{folderName}' & '{fileName}'");
        }
        return meshToReturn;
    }

    public static AudioClip LoadSillySFX(string fileName)
    {
        AudioClip clipToReturn = Plugin.CustomGoldScrapAssets.LoadAsset<AudioClip>($"Assets/LCGoldScrapMod/GoldScrapAudio/SillyScrapSFX/{fileName}.ogg");
        if (clipToReturn == null)
        {
            Logger.LogError($"LoadSillySFX() called with null '{fileName}'");
        }
        return clipToReturn;
    }

    public static AudioClip LoadReplaceSFX(string fileName)
    {
        AudioClip clipToReturn = Plugin.CustomGoldScrapAssets.LoadAsset<AudioClip>($"Assets/LCGoldScrapMod/GoldScrapAudio/ReplaceSFX/{fileName}.ogg");
        if (clipToReturn == null)
        {
            Logger.LogError($"LoadReplaceSFX() called with null '{fileName}'");
        }
        return clipToReturn;
    }

    public static AudioClip LoadNewSFX(string fileName)
    {
        AudioClip clipToReturn = Plugin.CustomGoldScrapAssets.LoadAsset<AudioClip>($"Assets/LCGoldScrapMod/GoldScrapAudio/NewSFX/{fileName}.ogg");
        if (clipToReturn == null)
        {
            Logger.LogError($"LoadNewSFX() called with null '{fileName}'");
        }
        return clipToReturn;
    }

    public static AudioClip LoadDiscoBallMusic(string fileName)
    {
        AudioClip clipToReturn = Plugin.CustomGoldScrapAssets.LoadAsset<AudioClip>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/DiscoBallMusic/GoldScrapShop{fileName}Music.ogg");
        if (clipToReturn == null)
        {
            Logger.LogError($"LoadDiscoBallMusic() called with null '{fileName}'");
        }
        return clipToReturn;
    }
}
