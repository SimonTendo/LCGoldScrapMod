using UnityEngine;
using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;

public class StartOfRoundPatch
{
    //Add and register Gold Scrap right before it is loaded into the ship
    [HarmonyPatch(typeof(StartOfRound), "Awake")]
    public class NewStartOfRoundStart
    {
        [HarmonyPostfix]
        public static void AwakePostfix(StartOfRound __instance)
        {
            AssetsCollection.GetGameObjectAssets();
            AssetsCollection.GetItemAssets(__instance);
            AssetsCollection.GetUnlockableAssets(__instance);
            string levelName = Plugin.v50Compatible ? "ArtificeLevel" : "TitanLevel";
            foreach (SelectableLevel level in __instance.levels)
            {
                if (level.name != null && level.name == levelName)
                {
                    AssetsCollection.GetEnemyAssets(level.Enemies.ToArray(), level.OutsideEnemies.ToArray());
                    AssetsCollection.GetHazardAssets(level.spawnableMapObjects);
                    break;
                }
            }
            General.AddJacobsLadderToHelmetLights();
            General.AddAllItemsToAllItemsListAwake();
            RegisterGoldScrap.SetAllItemsAwake();

            Config.hostToolRebalance = false;
            Plugin.appliedHostConfigs = false;
        }
    }



    //Method for linking LocalPlayerController-related methods on StartOfRound Start
    [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
    public class NewPlayerConnectClient
    {
        [HarmonyPostfix]
        public static void ConnectClientToPlayerObjectPostfix(PlayerControllerB __instance)
        {
            StoreAndTerminal.AddGoldItemsToShop();

            if (StoreAndTerminal.defaultHelmetMaterial == null)
            {
                StoreAndTerminal.defaultHelmetMaterial = GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>().materials[0];
            }

            if (Config.moddedMoonSpawn.Value && !RegisterGoldScrap.alreadyAddedGoldScrapOnModded)
            {
                RegisterGoldScrap.RegisterGoldScrapModded();
            }

            GoldScrapNetworkHandler.instance.SendConfigsToNewPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);

            if (__instance.isHostPlayerObject)
            {
                TagPlayer.PutTagOnPlayers();
                DLOGManager.instance.SetTextServerRpc(true, false);
                General.SetDiscoBallOnJoin();
                General.SetMedalsOnJoin();
                if (StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID].hasBeenUnlockedByPlayer && !RarityManager.hadFreeFirstFever)
                {
                    Plugin.Logger.LogDebug("cat unlocked upon start, likely from previous save file");
                    RarityManager.hadFreeFirstFever = true;
                    RarityManager.instance.RollForGoldFever();
                }
                UnlockableItem safeBox = StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.safeBoxID];
                if (safeBox.hasBeenUnlockedByPlayer && safeBox.inStorage)
                {
                    Plugin.Logger.LogDebug("safe in storage upon start, setting to instantiate");
                    safeBox.spawnPrefab = true;
                }
            }
            else
            {
                General.SyncUponJoin((int)StartOfRound.Instance.localPlayerController.playerClientId);
            }

            //LightSwitchRevealer
            GameObject lightSwitch = GameObject.Find("LightSwitch");
            if (lightSwitch != null)
            {
                Object.Instantiate(LightSwitchRevealer.lightRevealerPrefab, lightSwitch.transform, false);
            }

            Transform logParent = __instance.isHostPlayerObject ? Plugin.SearchForObject("LCGoldScrapMod", StartOfRound.Instance.gameObject.transform.parent).transform : null;
            Object.Instantiate(LogManager.managerPrefab, logParent);
        }
    }



    //Patch existing items that might not have been patched yet after starting the round
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
    public class NewStartOfRoundOpenDoors
    {
        [HarmonyPostfix]
        public static void openingDoorsSequencePostfix(StartOfRound __instance)
        {
            foreach (CrownScript crown in Object.FindObjectsOfType<CrownScript>())
            {
                crown.RegisterPlayerWearingDayStart();
            }

            if (RarityManager.CurrentlyGoldFever())
            {
                Plugin.Logger.LogInfo("Starting Gold Fever - Have fun!!");
            }

            if (__instance.IsServer)
            {
                DLOGManager.instance.SetTextServerRpc(true, true);
                GoldenHourglassManager.instance.CheckToSetTime(false);
                GoldNodeManager.instance.StartSpawnNodes();
                LogManager.instance.SpawnLogs();
                if (RarityManager.CurrentlyGoldFever())
                {
                    RarityManager.instance.PlayGoldFeverSFX();
                }
            }
        }
    }



    //Make sure GoldNuggets are not counted at the collected scrap value at the end of every day
    [HarmonyPatch(typeof(StartOfRound), "GetValueOfAllScrap")]
    public class NewStartOfRoundGetValueAllScrap
    {
        [HarmonyPostfix]
        public static void GetValueOfAllScrapPostfix(ref int __result)
        {
            __result -= SubtractGoldNuggetsFromCollectedValue(__result);
        }

        private static int SubtractGoldNuggetsFromCollectedValue(int __result)
        {
            int goldStoreItemValue = 0;
            GoldStoreItem[] allStoreItems = Object.FindObjectsOfType<GoldStoreItem>();
            foreach (GoldStoreItem gold in allStoreItems)
            {
                GrabbableObject itemProperties = gold.GetComponent<GrabbableObject>();
                if (itemProperties != null && itemProperties.isInShipRoom && !itemProperties.scrapPersistedThroughRounds)
                {
                    goldStoreItemValue += itemProperties.scrapValue;
                }
            }
            if (goldStoreItemValue != 0 && GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
            {
                GoldScrapNetworkHandler.instance.LowerRoundManagerScrapCollectedClientRpc(goldStoreItemValue);
            }
            return goldStoreItemValue;
        }
    }



    //End-of-day method
    [HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
    public class NewStartOfRoundEndGame
    {
        [HarmonyPostfix]
        public static void UponShipHasLeft(StartOfRound __instance)
        {
            if (__instance.IsServer)
            {
                foreach (GoldBirdScript goldBird in Object.FindObjectsOfType<GoldBirdScript>())
                {
                    goldBird.DeactivateAtEndOfDayClientRpc();
                }

                foreach (GoldNuggetScript goldOre in Object.FindObjectsOfType<GoldNuggetScript>())
                {
                    goldOre.IncreaseGoldOre();
                }

                foreach (GoldenPickaxeNode goldNode in Object.FindObjectsOfType<GoldenPickaxeNode>())
                {
                    goldNode.GetComponent<NetworkObject>().Despawn();
                }

                foreach (CrownScript crown in Object.FindObjectsOfType<CrownScript>())
                {
                    crown.CalculateStreakValueIncrease();
                }

                if (RarityManager.selectedLevel != -1)
                {
                    RarityManager.instance.SetGoldFeverForLevel();
                }

                DLOGManager.instance.SetTextServerRpc(true, false);
                GoldenHourglassManager.instance.CheckToSetTime(true);
                LogManager.instance.DespawnCurrentLogs();
            }
        }
    }



    [HarmonyPatch(typeof(StartOfRound), "SetPlanetsWeather")]
    public class NewStartOfRoundSetWeather
    {
        [HarmonyPostfix]
        public static void RerollForGoldWeather(StartOfRound __instance)
        {
            if (__instance.IsServer)
            {
                RarityManager.instance.RollForGoldFever();

                foreach (GoldfatherClockScript goldfatherClock in Object.FindObjectsOfType<GoldfatherClockScript>())
                {
                    if (goldfatherClock.timeCanTick)
                    {
                        goldfatherClock.SetSolarClockRotationClientRpc();
                    }
                }
            }
        }
    }



    //Save the CreditsCardManager scrapvalue to save for next time
    [HarmonyPatch(typeof(StartOfRound), "playersFiredGameOver")]
    public class NewStartOfRoundPlayersFired
    {
        [HarmonyPrefix]
        public static void PlayersFiredPrefix(StartOfRound __instance)
        {
            if (__instance.IsServer)
            {
                CreditsCardManager.SetPreviousCredits();
            }
            General.ToggleDiscoBall(false);
            MedalManager.instance.ResetAllMedals();
        }
    }
    [HarmonyPatch(typeof(StartOfRound), "PlayFirstDayShipAnimation")]
    public class NewStartOfRoundFirstDay
    {
        [HarmonyPostfix]
        public static void PlayFirstDayShipAnimationPostfix()
        {
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
            {
                CreditsCardManager.instance.SetBuyingRate();
            }
        }
    }
}
