using UnityEngine;
using HarmonyLib;
using Unity.Netcode;

public class GrabbableObjectPatch
{
    //Initialize DropStop if on object
    [HarmonyPatch(typeof(GrabbableObject), "DiscardItem")]
    public class NewGrabbableObjectDiscard
    {
        [HarmonyPostfix]
        public static void InitializeDropStop(GrabbableObject __instance)
        {
            GoldScrapObject dropStop = __instance.gameObject.GetComponent<GoldScrapObject>();
            if (dropStop != null)
            {
                dropStop.StartDropStop();
            }
        }
    }



    //Reactive the GoldenGrenade
    [HarmonyPatch(typeof(StunGrenadeItem), "ExplodeStunGrenade")]
    public class NewStunGrenadeExplode
    {
        [HarmonyPostfix]
        public static void ReactivateGoldenGrenade(StunGrenadeItem __instance)
        {
            if (__instance.itemProperties != null && __instance.itemProperties == RegisterGoldScrap.GoldenGrenade.itemName)
            {
                __instance.hasExploded = false;
                __instance.itemUsedUp = false;
                __instance.pinPulled = false;
                __instance.explodeTimer = 0;
                PrivateAccesser.SetPrivateValue<Coroutine>(__instance, "pullPinCoroutine").SetValue(__instance, null);
            }
        }
    }



    //Patch to make the Golden Bell simulate the original Bell's random pitch
    [HarmonyPatch(typeof(AnimatedItem), "DiscardItem")]
    public class NewAnimatedItemDiscard
    {
        [HarmonyPrefix]
        public static void NormalBellSoundRandomPitch(AnimatedItem __instance)
        {
            if (__instance.gameObject.name == "GoldenBellPrefab(Clone)" && !Config.sillyScrap.Value && !Config.replaceSFX.Value && AssetsCollection.sharedSFXdropBell != null)
            {
                __instance.itemAudio.pitch = Random.Range(0.66f, 1.33f);
            }
        }
    }



    //Cuddly Gold functionality: audiovisual on grab, prevent drop, and not get angry
    [HarmonyPatch(typeof(HoarderBugAI))]
    public class NewHoarderBugGrabItem
    {
        [HarmonyPostfix, HarmonyPatch("GrabItem")]
        public static void CheckGrabCuddlyGold(HoarderBugAI __instance, NetworkObject item)
        {
            if (__instance.enemyHP == 3 && __instance.heldItem != null && __instance.heldItem.itemGrabbableObject != null && __instance.heldItem.itemGrabbableObject.itemProperties == RegisterGoldScrap.CuddlyGold.itemName)
            {
                Plugin.Logger.LogDebug($"{__instance.name} #{__instance.NetworkObjectId} got CuddlyGold!!");

                __instance.angryTimer = -1;
                __instance.creatureVoice.Stop();
                __instance.creatureVoice.pitch = 1.05f;
                AudioClip clipToPlay = GetGrabCuddlyGoldSFX();
                __instance.creatureVoice.PlayOneShot(clipToPlay);
                WalkieTalkie.TransmitOneShotAudio(__instance.creatureVoice, clipToPlay);

                foreach (HoarderBugAI bug in Object.FindObjectsOfType<HoarderBugAI>())
                {
                    Plugin.Logger.LogDebug($"setting #{bug.NetworkObjectId}'s annoyance DOWN");
                    PrivateAccesser.SetPrivateValue<float>(bug, "annoyanceMeter").SetValue(bug, -9999f);
                }

                if (HoarderBugAI.HoarderBugItems != null)
                {
                    Plugin.Logger.LogDebug("locally clearing HoarderBugItems");
                    HoarderBugAI.HoarderBugItems.Clear();
                }
                if (HoarderBugAI.grabbableObjectsInMap != null)
                {
                    Plugin.Logger.LogDebug("locally clearing grabbableObjectsInMap");
                    HoarderBugAI.grabbableObjectsInMap.Clear();
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch("DropItemAndCallDropRPC")]
        public static bool CuddlyGoldFunctionality(HoarderBugAI __instance)
        {
            if (GetBugHoldingCuddlyGold(__instance) != null)
            {
                if (__instance.angryTimer > 0)
                {
                    HoarderBugAI.RefreshGrabbableObjectsInMapList();
                    return true;
                }
                else if (__instance.enemyHP == 3)
                {
                    if (HoarderBugAI.HoarderBugItems != null && HoarderBugAI.HoarderBugItems.Count > 0)
                    {
                        Plugin.Logger.LogDebug("host clearing HoarderBugItems");
                        HoarderBugAI.HoarderBugItems.Clear();
                    }
                    if (HoarderBugAI.grabbableObjectsInMap != null && HoarderBugAI.grabbableObjectsInMap.Count > 0)
                    {
                        Plugin.Logger.LogDebug("host clearing grabbableObjectsInMap");
                        HoarderBugAI.grabbableObjectsInMap.Clear();
                    }
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch("IsHoarderBugAngry")]
        public static void CannotBeAngryWhenCuddly(HoarderBugAI __instance, ref bool __result)
        {
            if (__instance.enemyHP == 3 && __instance.angryTimer <= 0 && GetBugHoldingCuddlyGold(__instance) != null)
            {
                __result = false;
            }
        }

        [HarmonyPostfix, HarmonyPatch("RefreshGrabbableObjectsInMapList")]
        public static void DontGetNewItemsWithCuddlyGold()
        {
            foreach (HoarderBugAI bug in Object.FindObjectsOfType<HoarderBugAI>())
            {
                if (GetBugHoldingCuddlyGold(bug) != null && bug.angryTimer <= 0 && bug.enemyHP == 3)
                {
                    Plugin.Logger.LogDebug($"{bug.name} #{bug.NetworkObjectId} holding CuddlyGold, clearing list after it's been refreshed");
                    HoarderBugAI.grabbableObjectsInMap.Clear();
                    return;
                }
            }
        }

        private static HoarderBugAI GetBugHoldingCuddlyGold(HoarderBugAI bug)
        {
            if (bug.heldItem != null && bug.heldItem.itemGrabbableObject != null && bug.heldItem.itemGrabbableObject.itemProperties == RegisterGoldScrap.CuddlyGold.itemName)
            {
                return bug;
            }
            return null;
        }

        private static AudioClip GetGrabCuddlyGoldSFX()
        {
            if (Config.sillyScrap.Value)
            {
                return AssetsCollection.LoadSillySFX("CuddlyGoldGrab");
            }
            if (Config.replaceEnemySFX.Value || AssetsCollection.sharedSFXapplause == null)
            {
                return AssetsCollection.LoadReplaceSFX("GrabCuddlyGold");
            }
            return AssetsCollection.sharedSFXapplause;
        }
    }
}