using System.Collections;
using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;

public class GoldenGuardianScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    private bool aboutToExplode;
    private Coroutine explosionCoroutine;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource audioSource;
    public AudioClip buildUpClip;
    public AudioClip explodeClip;
    public GameObject stunGrenadeExplosion;

    public override void Update()
    {
        base.Update();
        if (!IsOwner || playerHeldBy == null)
        {
            return;
        }
        if (playerHeldBy.inAnimationWithEnemy != null && !aboutToExplode)
        {
            LocalPlayerStartExplosion(true);
        }
    }

    public void LocalPlayerStartExplosion(bool delay = false, bool fromKillCommand = false)
    {
        float delayTime = 1.75f;
        if (Configs.hostToolRebalance)
        {
            delayTime = 1.3f;
            delay = true;
        }
        if (fromKillCommand)
        {
            delay = false;
        }
        StartExplosion(delay, delayTime);
        StartExplosionServerRpc(delay, delayTime, (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    private void StartExplosion(bool delay, float delayTime)
    {
        Logger.LogDebug($"{name} #{NetworkObjectId} called StartExplosion with delay {delay} and delayTime {delayTime}");
        aboutToExplode = true;
        if (delay)
        {
            explosionCoroutine = StartCoroutine(ExplodeOnDelay(delayTime));
        }
        else
        {
            if (explosionCoroutine != null)
            {
                Logger.LogDebug($"interrupting coroutine to explode immediately!");
                StopCoroutine(explosionCoroutine);
            }
            Explode();
        }
    }

    private IEnumerator ExplodeOnDelay(float delayTime = 1.75f)
    {
        yield return new WaitForSeconds(0.1f);
        audioSource.PlayOneShot(buildUpClip);
        WalkieTalkie.TransmitOneShotAudio(audioSource, buildUpClip, 0.5f);
        yield return new WaitForSeconds(delayTime);
        Explode();
    }

    public void Explode()
    {
        if (!deactivated)
        {
            audioSource.Stop();
            DestroyObjectInHand(playerHeldBy);
            if (!Configs.hostToolRebalance)
            {
                audioSource.PlayOneShot(explodeClip);
                WalkieTalkie.TransmitOneShotAudio(audioSource, explodeClip, 1f);
                RoundManager.Instance.PlayAudibleNoise(transform.position, 25f, 0.3f);
                StunGrenadeItem.StunExplosion(transform.position, true, 0.2f, 10f);
                Transform parent = isInElevator ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.mapPropsContainer.transform;
                Instantiate(stunGrenadeExplosion, transform.position, Quaternion.identity, parent);
            }
            else
            {
                Landmine.SpawnExplosion(transform.position, true, 4f, 8f, 25, 5f);
            }
            StartCoroutine(DelaySettingObjectAway());
        }
    }

    private IEnumerator DelaySettingObjectAway()
    {
        yield return new WaitForSeconds(5f);
        targetFloorPosition = new Vector3(3000f, -400f, 3000f);
        startFallingPosition = new Vector3(3000f, -400f, 3000f);
    }

    public override void PlayDropSFX()
    {
        if (!deactivated)
        {
            base.PlayDropSFX();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartExplosionServerRpc(bool delay, float delayTime, int heldPlayerID)
    {
        StartExplosionClientRpc(delay, delayTime, heldPlayerID);
    }

    [ClientRpc]
    private void StartExplosionClientRpc(bool delay, float delayTime, int heldPlayerID)
    {
        if (heldPlayerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            Logger.LogDebug($"non-owner client starting GoldenGuardian #{NetworkObjectId} explosion");
            StartExplosion(delay, delayTime);
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
    public class NewPlayerDamage
    {
        [HarmonyPrefix]
        public static bool PreventDamage(PlayerControllerB __instance)
        {
            return PatchToPreventDamage(__instance, false);
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
    public class NewPlayerKill
    {
        [HarmonyPrefix]
        public static bool PreventKill(PlayerControllerB __instance)
        {
            return PatchToPreventDamage(__instance, true);
        }
    }

    public static bool PatchToPreventDamage(PlayerControllerB __instance, bool isKillCommand)
    {
        if (!__instance.IsOwner || __instance.isPlayerDead || !__instance.AllowPlayerDeath() || __instance.currentlyHeldObjectServer == null)
        {
            return true;
        }
        GoldenGuardianScript heldItem = __instance.currentlyHeldObjectServer.GetComponent<GoldenGuardianScript>();
        if (heldItem != null && !heldItem.deactivated && (isKillCommand || !heldItem.aboutToExplode))
        {
            Logger.LogDebug($"GoldenGuardianScript patch: local player likely holding GoldenGuardian on server, trying to execute explosion | isKillCommand = {isKillCommand}");
            bool delay = !isKillCommand && Configs.hostToolRebalance;
            heldItem.LocalPlayerStartExplosion(delay, isKillCommand);
            return false;
        }
        return true;
    }
}
