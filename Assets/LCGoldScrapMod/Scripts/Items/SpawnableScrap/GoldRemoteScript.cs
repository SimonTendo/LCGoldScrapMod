using UnityEngine;
using BepInEx.Logging;

public class GoldRemoteScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    public AudioSource remoteAudio;

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        ReplicateRemoteClick();
        if (!StartOfRound.Instance.inShipPhase)
        {
            ToggleFacilityLights();
        }
    }

    private void ReplicateRemoteClick()
    {
        remoteAudio.PlayOneShot(remoteAudio.clip);
        WalkieTalkie.TransmitOneShotAudio(remoteAudio, remoteAudio.clip, 0.7f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 12f, 0.6f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
    }

    private void ToggleFacilityLights()
    {
        if (!Config.hostToolRebalance)
        {
            if (!IsServer)
            {
                return;
            }
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId} call NORMAL");
            RoundManager roundManager = FindObjectOfType<RoundManager>();
            BreakerBox breakerBox = FindObjectOfType<BreakerBox>();
            if (roundManager != null && breakerBox != null)
            {
                roundManager.SwitchPower(!breakerBox.isPowerOn);
            }
        }
        else
        {
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId} call FLICKER");
            RoundManager roundManager = FindObjectOfType<RoundManager>();
            if (roundManager != null)
            {
                roundManager.FlickerLights();
            }
        }
    }
}
