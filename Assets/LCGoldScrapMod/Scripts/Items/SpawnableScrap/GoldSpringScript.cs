using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using GameNetcodeStuff;

public class GoldSpringScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("Stop/Go Audio")]
    public AudioSource audioSource;
    public AudioClip stopClip;
    public AudioClip goClip;

    private float intervalTimer;
    private bool stateNew;
    private bool stateLast;
    private float speedBeforeStopping = -1;

    public override void Update()
    {
        base.Update();
        if (IsOwner && playerHeldBy != null)
        {
            intervalTimer += Time.deltaTime;
            if (intervalTimer > 0.33f)
            {
                intervalTimer = 0;
                DoStopGoInterval();
            }
        }
    }

    private void DoStopGoInterval()
    {
        stateLast = stateNew;
        stateNew = ShouldStopMovement();
        if (stateNew != stateLast)
        {
            if (stateNew)
            {
                SetPlayerStop();
            }
            else if (!stateNew)
            {
                SetPlayerGo();
            }
        }
    }

    private void SetPlayerStop()
    {
        Logger.LogDebug("calling Stop()");
        if (playerHeldBy.movementSpeed > 0)
        {
            Logger.LogDebug("STOP!");
            speedBeforeStopping = playerHeldBy.movementSpeed;
            playerHeldBy.movementSpeed = 0;
            MakeSpringSoundServerRpc(true);
        }
    }

    private void SetPlayerGo()
    {
        Logger.LogDebug("calling Go()");
        if (speedBeforeStopping > 0)
        {
            Logger.LogDebug("GO!");
            playerHeldBy.movementSpeed = speedBeforeStopping;
            MakeSpringSoundServerRpc(false);
        }
    }

    public override void DiscardItem()
    {
        if (stateNew)
        {
            stateNew = false;
            intervalTimer = 0;
            SetPlayerGo();
        }
        base.DiscardItem();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MakeSpringSoundServerRpc(bool isStopClip)
    {
        MakeSpringSoundClientRpc(isStopClip);
    }

    [ClientRpc]
    private void MakeSpringSoundClientRpc(bool isStopClip)
    {
        AudioClip clip = isStopClip ? stopClip : goClip;
        audioSource.PlayOneShot(clip);
        WalkieTalkie.TransmitOneShotAudio(audioSource, clip, 0.5f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 20, 0.25f);
    }

    private bool ShouldStopMovement()
    {
        if (!StartOfRound.Instance.shipDoorsEnabled || StartOfRound.Instance.inShipPhase)
        {
            return false;
        }
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (player != null && player != playerHeldBy && player.isPlayerControlled && !player.isPlayerDead && player.inAnimationWithEnemy == null && player.sinkingValue < 0.73f && player.HasLineOfSightToPosition(playerHeldBy.playerEye.position, 60, 20))
            {
                return true;
            }
        }
        return false;
    }
}
