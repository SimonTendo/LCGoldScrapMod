using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using BepInEx.Logging;
using GameNetcodeStuff;

public class GoldkeeperScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("Blockers")]
    public Collider losBlocker;
    public NavMeshObstacle navBlocker;
    private Coroutine checkDistanceCoroutine;

    public override void Start()
    {
        base.Start();
        navBlocker.carveOnlyStationary = true;
    }

    public override void EquipItem()
    {
        base.EquipItem();
        if (checkDistanceCoroutine != null)
        {
            Logger.LogDebug("stopped CheckDistance Coroutine");
            StopCoroutine(checkDistanceCoroutine);
            checkDistanceCoroutine = null;
        }
        losBlocker.enabled = false;
        navBlocker.carving = false;
        navBlocker.enabled = false;
        LogEnabled();
    }

    public override void PlayDropSFX()
    {
        base.PlayDropSFX();
        if (checkDistanceCoroutine != null)
        {
            Logger.LogDebug("stopped CheckDistance Coroutine");
            StopCoroutine(checkDistanceCoroutine);
            checkDistanceCoroutine = null;
        }
        checkDistanceCoroutine = StartCoroutine(CheckDistance());
    }

    private IEnumerator CheckDistance()
    {
        bool playerInRange = true;
        bool setTo = true;
        PlayerControllerB player = StartOfRound.Instance.localPlayerController;
        while (playerInRange)
        {
            yield return new WaitForSeconds(0.1f);
            if (GetComponentInParent<VehicleController>() != null || GetComponentInParent<MineshaftElevatorController>() != null)
            {
                setTo = false;
                Logger.LogDebug($"WARNING!!! {gameObject.name} #{NetworkObjectId} dropped inside vehicle or elevator! breaking and setting collision enabled to {setTo}");
                break;
            }
            if (player == null || !player.isPlayerControlled)
            {
                playerInRange = false;
            }
            float distance = Vector3.Distance(transform.position - Vector3.back * 0.15f, player.transform.position);
            if (distance >= 0.65f)
            {
                playerInRange = false;
            }
        }
        checkDistanceCoroutine = null;
        losBlocker.enabled = setTo;
        navBlocker.carving = setTo;
        navBlocker.enabled = setTo;
        LogEnabled();
    }

    private void LogEnabled()
    {
        Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: {losBlocker.gameObject.name} enabled = {losBlocker.enabled}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(playerHeldBy == null, playerID);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(bool hostNullHeld, int playerID)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            losBlocker.enabled = hostNullHeld;
            navBlocker.carving = hostNullHeld;
            navBlocker.enabled = hostNullHeld;
            if (!hostNullHeld)
            {
                Logger.LogDebug($"custom injecting hasHitGround on {gameObject.name} #{NetworkObjectId} since likely held on host");
                hasHitGround = false;
            }
            LogEnabled();
        }
    }
}
