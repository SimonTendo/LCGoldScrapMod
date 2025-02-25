using System.Collections;
using UnityEngine;
using BepInEx.Logging;
using Unity.Netcode;

public class GoldenHourglassScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    private bool buttonHeld;
    private float holdTimer;
    private bool willActivate = false;
    private bool justDeactivated = false;
    private float totalChargeTime = 2.5f;
    private float rotationAmount;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource chargeAudioSource;
    public Transform glassTransform;

    [Space(15f)]
    public AudioSource miscAudioSource;
    public AudioClip activateClip;
    public AudioClip deactivateClip;
    public AudioClip failClip;
    public AudioClip breakClip;

    public override void Update()
    {
        base.Update();
        if (deactivated)
        {
            return;
        }
        if (buttonHeld)
        {
            if (playerHeldBy == null || playerHeldBy.isClimbingLadder || playerHeldBy.inSpecialInteractAnimation || playerHeldBy.isTypingChat || playerHeldBy.inTerminalMenu || playerHeldBy.inVehicleAnimation || (IsOwner && FindObjectOfType<QuickMenuManager>().isMenuOpen))
            {
                buttonHeld = false;
                Logger.LogDebug($"null: {playerHeldBy == null} || ladder: {playerHeldBy.isClimbingLadder} || specialAnimation: {playerHeldBy.inSpecialInteractAnimation} || typingChat: {playerHeldBy.isTypingChat} || inTerminal: {playerHeldBy.inTerminalMenu} || inVehicle: {playerHeldBy.inVehicleAnimation} || menuOpen: {IsOwner && FindObjectOfType<QuickMenuManager>().isMenuOpen}");
                ManualSetHoldServerRpc(false);
                return;
            }
        }
        else if (holdTimer < totalChargeTime)
        {
            holdTimer = 0;
        }
        if (willActivate)
        {
            rotationAmount = Time.deltaTime * 50;
        }
        else if (!justDeactivated)
        {
            if (buttonHeld)
            {
                holdTimer += Time.deltaTime;
                chargeAudioSource.volume = Mathf.Clamp(holdTimer / totalChargeTime, 0.0f, 1.0f);
                if (!chargeAudioSource.isPlaying)
                {
                    chargeAudioSource.Play();
                }
                rotationAmount = Mathf.Clamp(holdTimer / totalChargeTime * Time.deltaTime * 1000, 0.0f, 12.0f);
            }
            else
            {
                chargeAudioSource.volume = Mathf.Lerp(0.0f, chargeAudioSource.volume, 0.99f);
                if (chargeAudioSource.volume < 0.1f)
                {
                    chargeAudioSource.Stop();
                }
                rotationAmount = Mathf.Lerp(0.0f, rotationAmount, 0.98f);
                if (rotationAmount < 0.05f)
                {
                    rotationAmount = 0f;
                }
            }
            if (IsOwner && holdTimer >= totalChargeTime)
            {
                if (HUDManager.Instance.controlTipLines[1].text != "Activate : [Release]")
                {
                    string[] changeTextTo = new string[1] { "Activate : [Release]" };
                    HUDManager.Instance.ChangeControlTipMultiple(changeTextTo, true, itemProperties);
                }
                if (!buttonHeld)
                {
                    holdTimer = 0;
                    if (GoldenHourglassManager.instance.willSetTime || GoldenHourglassManager.instance.setTimeToday)
                    {
                        FailToActivateServerRpc();
                    }
                    else
                    {
                        willActivate = true;
                        ActivateHourGlassLocal(true);
                        ActivateHourglassServerRpc((int)playerHeldBy.playerClientId, true);
                    }
                }
            }
        }
        if (rotationAmount != 0.0f)
        {
            glassTransform.Rotate(0f, rotationAmount, 0f, Space.Self);
        }
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        buttonHeld = buttonDown;
        justDeactivated = false;
        if (playerHeldBy != null)
        {
            playerHeldBy.activatingItem = buttonHeld;
        }
        if (IsOwner && willActivate)
        {
            willActivate = false;
            justDeactivated = true;
            ActivateHourGlassLocal(false);
            ActivateHourglassServerRpc((int)playerHeldBy.playerClientId, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ManualSetHoldServerRpc(bool setTo)
    {
        ManualSetHoldClientRpc(setTo);
    }

    [ClientRpc]
    private void ManualSetHoldClientRpc(bool setTo)
    {
        if (buttonHeld != setTo)
        {
            buttonHeld = setTo;
            if (playerHeldBy != null)
            {
                playerHeldBy.activatingItem = setTo;
            }
            Logger.LogDebug($"interrupted buttonHeld to set to {buttonHeld}");
        }
    }

    public override void EquipItem()
    {
        base.EquipItem();
        SetTooltipActive();
    }

    public override void PocketItem()
    {
        base.PocketItem();
        buttonHeld = false;
        if (playerHeldBy != null)
        {
            playerHeldBy.activatingItem = false;
        }
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        buttonHeld = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateHourglassServerRpc(int playerID, bool setTo)
    {
        ActivateHourglassClientRpc(playerID, setTo);
        GoldenHourglassManager.instance.ToggleTimeOfDayServerRpc(setTo);
    }

    [ClientRpc]
    private void ActivateHourglassClientRpc(int playerID, bool setTo)
    {
        if (playerID != (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            ActivateHourGlassLocal(setTo);
        }
    }

    private void ActivateHourGlassLocal(bool setTo)
    {
        holdTimer = 0;
        willActivate = setTo;
        if (!setTo)
        {
            justDeactivated = true;
        }
        GoldenHourglassManager.instance.hourglassToBreak = setTo ? this : null;
        AudioClip clipToPlay = setTo ? activateClip : deactivateClip;
        chargeAudioSource.Stop();
        miscAudioSource.Stop();
        if (setTo && StartOfRound.Instance.shipHasLanded)
        {
            miscAudioSource.spatialBlend = 0f;
        }
        miscAudioSource.PlayOneShot(clipToPlay);
        WalkieTalkie.TransmitOneShotAudio(miscAudioSource, clipToPlay);
        SetTooltipActive();
    }

    [ServerRpc(RequireOwnership = false)]
    private void FailToActivateServerRpc()
    {
        FailToActivateClientRpc();
    }

    [ClientRpc]
    private void FailToActivateClientRpc()
    {
        willActivate = false;
        holdTimer = 0f;
        rotationAmount = 0f;
        chargeAudioSource.Stop();
        miscAudioSource.Stop();
        miscAudioSource.PlayOneShot(failClip);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 10f, 0.25f);
        WalkieTalkie.TransmitOneShotAudio(miscAudioSource, failClip, 0.5f);
        Logger.LogDebug($"{gameObject.name} #{NetworkObjectId} failed: willActivate {willActivate} | willSetTime {GoldenHourglassManager.instance.willSetTime} | setTimeToday {GoldenHourglassManager.instance.setTimeToday}");
        SetTooltipActive();
    }

    [ClientRpc]
    public void BreakIfActivatedClientRpc()
    {
        if (willActivate)
        {
            DestroyObjectInHand(playerHeldBy);
            if (isPocketed)
            {
                General.BreakPocketedItem(this);
            }
            StartCoroutine(DelaySettingObjectAway());
            if (!StartOfRound.Instance.shipHasLanded)
            {
                miscAudioSource.PlayOneShot(breakClip);
                RoundManager.Instance.PlayAudibleNoise(transform.position, 20, 0.75f);
                WalkieTalkie.TransmitOneShotAudio(miscAudioSource, breakClip, 1f);
            }
            Instantiate(AssetsCollection.poofParticle, transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<ParticleSystem>().Play();
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

    private void SetTooltipActive()
    {
        if (IsOwner && playerHeldBy != null)
        {
            string[] changeTextTo = willActivate ? new string[1] { "Deactivate : [LMB]" } : new string[1] { "Charge : [LMB] (Hold)" };
            HUDManager.Instance.ChangeControlTipMultiple(changeTextTo, true, itemProperties);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(willActivate, buttonHeld, justDeactivated, holdTimer, playerID);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(bool hostWillActivate, bool hostButtonHeld, bool hostDeactivated, float hostHoldTimer, int playerID)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            willActivate = hostWillActivate;
            buttonHeld = hostButtonHeld;
            justDeactivated = hostDeactivated;
            holdTimer = hostHoldTimer;
            Logger.LogDebug($"set values of {gameObject.name} #{NetworkObjectId}: willActivate {willActivate} | buttonHeld {buttonHeld} | justDeactivated {justDeactivated} | holdTimer {holdTimer}");
        }
    }
}
