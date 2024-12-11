using UnityEngine;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;

public class StorePatch
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
    [HarmonyPatch(typeof(UnlockableSuit), "SwitchSuitForPlayer")]
    public class NewUnlockableSuitSwitch
    {
        [HarmonyPostfix]
        public static void SwitchSuitForPlayerPostfix(ref PlayerControllerB player)
        {
            if (StoreAndTerminal.defaultHelmetMaterial == null || player != GameNetworkManager.Instance.localPlayerController) return;

            if (player.currentSuitID != StoreAndTerminal.bronzeSuitID && player.currentSuitID != StoreAndTerminal.silverSuitID && player.currentSuitID != StoreAndTerminal.goldSuitID)
            {
                GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>().material = StoreAndTerminal.defaultHelmetMaterial;
            }
            else if (player.currentSuitID == StoreAndTerminal.bronzeSuitID)
            {
                GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>().material = AssetsCollection.defaultMaterialBronze;
            }
            else if (player.currentSuitID == StoreAndTerminal.silverSuitID)
            {
                GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>().material = AssetsCollection.defaultMaterialSilver;
            }
            else if (player.currentSuitID == StoreAndTerminal.goldSuitID)
            {
                GameNetworkManager.Instance.localPlayerController.localVisor.GetChild(0).GetComponent<MeshRenderer>().material = AssetsCollection.defaultMaterialGold;
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
