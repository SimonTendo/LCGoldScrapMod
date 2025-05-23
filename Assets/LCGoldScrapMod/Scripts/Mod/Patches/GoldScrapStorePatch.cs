using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;

public class GoldScrapStorePatch
{
    private static ManualLogSource Logger = Plugin.Logger;

    //Make sure GoldNuggets are not counted at the end-of-game value of collected scrap
    [HarmonyPatch(typeof(DepositItemsDesk), "SellAndDisplayItemProfits")]
    public class NewDepositItemsSell
    {
        [HarmonyPrefix]
        public static void SellAndDisplayItemProfitsPrefix(DepositItemsDesk __instance)
        {
            SubtractGoldNuggetsFromSoldValue(__instance);
        }

        private static void SubtractGoldNuggetsFromSoldValue(DepositItemsDesk __instance)
        {
            int goldNuggetValueSold = 0;
            for (int i = 0; i < __instance.itemsOnCounter.Count; i++)
            {
                if (__instance.itemsOnCounter[i] != null)
                {
                    GoldStoreItem goldNugget = __instance.itemsOnCounter[i].GetComponent<GoldStoreItem>();
                    if (goldNugget != null)
                    {
                        goldNuggetValueSold += (int)(__instance.itemsOnCounter[i].scrapValue * StartOfRound.Instance.companyBuyingRate);
                    }
                }
            }
            Plugin.Logger.LogDebug($"sold goldstore items at total value of: {goldNuggetValueSold}");
            StartOfRound.Instance.gameStats.scrapValueCollected -= goldNuggetValueSold;
        }
    }



    //Switch HelmetMaterial to Bronze, Silver, or Gold if the switched-to suit is one of those
    [HarmonyPatch(typeof(UnlockableSuit))]
    public class NewUnlockableSuitSwitch
    {
        [HarmonyPostfix, HarmonyPatch("SwitchSuitForPlayer")]
        public static void SwitchSuitForPlayerPostfix(PlayerControllerB player)
        {
            if (player == null || StoreAndTerminal.defaultHelmetMaterial == null) return;
            bool putOnSuit = true;
            int playerID = player.currentSuitID;
            int goldID = StoreAndTerminal.goldSuitID;
            int silverID = StoreAndTerminal.silverSuitID;
            int bronzeID = StoreAndTerminal.bronzeSuitID;
            if (playerID != bronzeID && playerID != silverID && playerID != goldID)
            {
                if (player == GameNetworkManager.Instance.localPlayerController)
                {
                    player.localVisor.GetChild(0).GetComponent<MeshRenderer>().material = StoreAndTerminal.defaultHelmetMaterial;
                }
                putOnSuit = false;
            }
            else if (player == GameNetworkManager.Instance.localPlayerController)
            {
                player.localVisor.GetChild(0).GetComponent<MeshRenderer>().material = StartOfRound.Instance.unlockablesList.unlockables[playerID].suitMaterial;
            }
            if (putOnSuit)
            {
                General.InstantiateSparklesOnTransform(player.lowerSpine);
            }
            else
            {
                General.DestroySparklesOnTransform(player.lowerSpine);
            }
        }

        [HarmonyPostfix, HarmonyPatch("SwitchSuitClientRpc")]
        public static void SwitchSuitClientRpcPostfix(UnlockableSuit __instance)
        {
            if (__instance != null && (__instance.suitID == StoreAndTerminal.goldSuitID || __instance.suitID == StoreAndTerminal.silverSuitID || __instance.suitID == StoreAndTerminal.bronzeSuitID))
            {
                General.InstantiateSparklesOnTransform(__instance.transform);
            }
        }
    }



    //DiscoBallMusic functionality
    [HarmonyPatch(typeof(CozyLights), "SetAudio")]
    public class NewCozyLightsSetAudio
    {
        [HarmonyPrefix]
        public static void DiscoBallPrefix(CozyLights __instance, bool ___cozyLightsOn)
        {
            if (!Plugin.playlistsModCompatible && __instance.gameObject.transform.parent?.name == "DiscoBallContainer(Clone)" && ___cozyLightsOn && !__instance.turnOnAudio.isPlaying)
            {
                if (!HasUnlockedDiscoBallSong())
                {
                    Logger.LogDebug("no LCGoldScrapMod songs have been unlocked, not touching DiscoBall");
                    return;
                }
                if (TimeOfDay.Instance == null || (TimeOfDay.Instance.daysUntilDeadline == 0 && TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled > 0f))
                {
                    Logger.LogDebug("DiscoBall: likely in the process of getting fired");
                    return;
                }
                for (int i = 0; i < StoreAndTerminal.runtimeDiscoBallSongs.allClips.Count; i++)
                {
                    if (__instance.turnOnAudio.clip == StoreAndTerminal.runtimeDiscoBallSongs.allClips[i])
                    {
                        Logger.LogDebug($"found currentSong: {StoreAndTerminal.runtimeDiscoBallSongs.allClips[i].name}");
                        int nextSong = i + 1;
                        if (nextSong == StoreAndTerminal.runtimeDiscoBallSongs.allClips.Count)
                        {
                            nextSong = 0;
                        }
                        __instance.turnOnAudio.clip = StoreAndTerminal.runtimeDiscoBallSongs.allClips[nextSong];
                        Logger.LogDebug($"set to next song: {__instance.turnOnAudio.clip.name}");
                        return;
                    }
                }
                Logger.LogDebug("WARNING!! reached end of DiscoBallPrefix() without having set song, likely because discoBall contained another song, trying to add new song!");
                StoreAndTerminal.runtimeDiscoBallSongs.allClips.Add(__instance.turnOnAudio.clip);
            }
        }
        private static bool HasUnlockedDiscoBallSong()
        {
            return StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.groovyGoldID].hasBeenUnlockedByPlayer;
        }
    }
    [HarmonyPatch(typeof(StartOfRound), "BuyShipUnlockableClientRpc")]
    public class NewStartOfRoundBuyUnlockable
    {
        [HarmonyPostfix]
        public static void BuyUnlockablePostfix(int unlockableID)
        {
            MedalManager.instance.ListenToNewUnlockable(unlockableID);
            if (unlockableID == StoreAndTerminal.groovyGoldID)
            {
                General.ToggleDiscoBall(true, "GroovyGold");
            }
            else if (unlockableID == StoreAndTerminal.directoryLogID)
            {
                DLOGManager.instance.SetTutorialText();
            }
            else if (unlockableID == StoreAndTerminal.catOGoldID)
            {
                RarityManager.instance.daysUntilNextFever = 1;
            }
        }
    }



    //Safe Box patches setting spawnPrefab to move it rather than despawn it
    [HarmonyPatch(typeof(StartOfRound), "SpawnUnlockable")]
    public class NewStartOfRoundSpawnUnlockable
    {
        [HarmonyPrefix]
        public static void SpawnUnlockablePrefix(StartOfRound __instance, int unlockableIndex)
        {
            if (unlockableIndex == StoreAndTerminal.safeBoxID)
            {
                UnlockableItem safeData = __instance.unlockablesList.unlockables[unlockableIndex];
                SafeBoxScript safeScript = Object.FindObjectOfType<SafeBoxScript>();
                safeData.spawnPrefab = safeScript == null;
                Logger.LogDebug($"spawned SafeBox with instantiate {safeData.spawnPrefab}");
            }
        }
    }
    [HarmonyPatch(typeof(ShipBuildModeManager))]
    public class SafeBoxStorage
    {
        [HarmonyPrefix, HarmonyPatch("StoreObjectLocalClient")]
        public static void Local(ShipBuildModeManager __instance)
        {
            PlaceableShipObject placingObject = PrivateAccesser.GetPrivateField<PlaceableShipObject>(__instance, "placingObject");
            if (placingObject == null)
            {
                Logger.LogDebug("null, returning");
                return;
            }
            if (placingObject.unlockableID == StoreAndTerminal.safeBoxID && Object.FindAnyObjectByType<SafeBoxScript>() != null)
            {
                Logger.LogDebug("storing SafeBox: Local");
                UnlockableItem safeData = StartOfRound.Instance.unlockablesList.unlockables[placingObject.unlockableID];
                safeData.spawnPrefab = false;
            }
        }
        [HarmonyPrefix, HarmonyPatch("StoreObjectServerRpc")]
        public static void Server(NetworkObjectReference objectRef)
        {
            if (!objectRef.TryGet(out var networkObject))
            {
                Logger.LogDebug("false, returning");
                return;
            }
            if (networkObject == null)
            {
                Logger.LogDebug("still null, returning");
                return;
            }
            PlaceableShipObject placingObject = networkObject.gameObject.GetComponentInChildren<PlaceableShipObject>();
            if (placingObject == null)
            {
                Logger.LogDebug("null, returning");
                return;
            }
            if (placingObject.unlockableID == StoreAndTerminal.safeBoxID && Object.FindAnyObjectByType<SafeBoxScript>() != null)
            {
                Logger.LogDebug("storing SafeBox: Server");
                UnlockableItem safeData = StartOfRound.Instance.unlockablesList.unlockables[placingObject.unlockableID];
                safeData.spawnPrefab = false;
            }
        }
        [HarmonyPrefix, HarmonyPatch("StoreShipObjectClientRpc")]
        public static void Client(NetworkObjectReference objectRef, int playerWhoStored)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || playerWhoStored == (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
            {
                Logger.LogDebug("null or local, returning");
                return;
            }
            if (!objectRef.TryGet(out var networkObject))
            {
                Logger.LogDebug("false, returning");
                return;
            }
            if (networkObject == null)
            {
                Logger.LogDebug("still null, returning");
                return;
            }
            PlaceableShipObject placingObject = networkObject.gameObject.GetComponentInChildren<PlaceableShipObject>();
            if (placingObject == null)
            {
                Logger.LogDebug("null, returning");
                return;
            }
            if (placingObject.unlockableID == StoreAndTerminal.safeBoxID && Object.FindAnyObjectByType<SafeBoxScript>() != null)
            {
                Logger.LogDebug("storing SafeBox: Client");
                UnlockableItem safeData = StartOfRound.Instance.unlockablesList.unlockables[placingObject.unlockableID];
                safeData.spawnPrefab = false;
            }
        }
    }



    //Set values if corpse/mimic uses gold/silver/bronze suit
    /*[HarmonyPatch(typeof(RagdollGrabbableObject), "Start")]
    public class NewRagdollGrabbableObject
    {
        [HarmonyPostfix]
        public static void StartPostfix(RagdollGrabbableObject __instance)
        {
            if (__instance.bodyID == null || __instance.bodyID.Value == -1 || __instance.bodyID.Value >= StartOfRound.Instance.allPlayerScripts.Length)
            {
                return;
            }
            int corpseSuitID = StartOfRound.Instance.allPlayerScripts[__instance.bodyID.Value].currentSuitID;
            int newCorpseValue = -1;
            if (corpseSuitID == StoreAndTerminal.goldSuitID)
            {
                newCorpseValue = 100;
            }
            else if (corpseSuitID == StoreAndTerminal.silverSuitID)
            {
                newCorpseValue = 50;
            }
            else if (corpseSuitID == StoreAndTerminal.bronzeSuitID)
            {
                newCorpseValue = 25;
            }
            if (newCorpseValue != -1)
            {
                __instance.scrapValue = newCorpseValue;
                Logger.LogDebug($"set value of corpse {__instance.name} of player at index [{__instance.bodyID.Value}] to {__instance.scrapValue}");
            }
        }
    }
    [HarmonyPatch(typeof(MaskedPlayerEnemy), "SetMaskType")]
    public class NewMaskedPlayerEnemy
    {
        [HarmonyPostfix]
        public static void SetMaskTypePostfix(MaskedPlayerEnemy __instance)
        {
            if (__instance.mimickingPlayer != null && __instance.maskTypeIndex <= 0 && __instance.maskTypeIndex >= __instance.maskTypes.Length)
            {
                int mimicSuitID = __instance.mimickingPlayer.currentSuitID;
                if (mimicSuitID == StoreAndTerminal.goldSuitID || mimicSuitID == StoreAndTerminal.silverSuitID || mimicSuitID == StoreAndTerminal.bronzeSuitID)
                {
                    GameObject mask = __instance.maskTypes[__instance.maskTypeIndex];
                    foreach (MeshRenderer renderer in mask.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (renderer.enabled)
                        {
                            renderer.sharedMaterial = StartOfRound.Instance.unlockablesList.unlockables[mimicSuitID].suitMaterial;
                        }
                    }
                }
            }
        }
    }*/
}
