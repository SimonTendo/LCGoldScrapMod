using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class GoldenClockScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("Original Clock")]
    public Transform minuteHand;
    public Transform secondHand;
    private float timeOfLastSecond;
    public AudioSource tickAudio;
    public AudioClip tickSFX;
    public AudioClip tockSFX;
    private bool tickOrTock;

    [Space(3f)]
    [Header("Custom Timer")]
    public AudioClip intervalClip;
    public AudioClip failClip;
    public int maxRealSeconds;
    public int minRealSeconds;
    public float countdownInterval;

    [Space(3f)]
    [Header("Close Call")]
    public AudioSource closeCallAudio;
    public AudioClip closeCallApproach;
    public AudioClip closeCallSuccess;
    public AudioClip closeCallFail;

    private bool shouldCountDown = false;
    private bool approachedShipFinalPhase = false;
    private int secondsLeft;
    private int thisClocksStartingSeconds;
    private int thisClocksStartingValue;
    private int thisClocksIntervalAmount;
    private int thisClocksSpecialMultiplier;
    private float timeOfLastTickTock;

    public override void Start()
    {
        base.Start();
        if (IsServer && !isInShipRoom)
        {
            StartCoroutine(RollForNewTimer());
        }  
    }

    public override void Update()
    {
        base.Update();
        if (!shouldCountDown)
        {
            return;
        }
        if (Time.realtimeSinceStartup - timeOfLastSecond > countdownInterval)
        {
            timeOfLastSecond = Time.realtimeSinceStartup;
            if (IsServer || !IsServer && secondsLeft % 60 != 1)
            {
                secondsLeft--;
                secondHand.Rotate(6f, 0f, 0f, Space.Self);
            }
            if (IsServer)
            {
                if (secondsLeft <= 0)
                {
                    shouldCountDown = false;
                    SyncFailureClientRpc(scrapValue / thisClocksSpecialMultiplier);
                    return;
                }
                if (secondsLeft % 60 == 0)
                {
                    SyncTimeAndValueClientRpc(secondsLeft, scrapValue - (int)Mathf.Lerp(0, thisClocksStartingValue, Random.Range(40, 70) / thisClocksIntervalAmount / 100f), true);
                }
                if (secondsLeft < 60 && !approachedShipFinalPhase && playerHeldBy != null && Vector3.Distance(playerHeldBy.transform.position, StartOfRound.Instance.shipDoorNode.transform.position) < 10)
                {
                    approachedShipFinalPhase = true;
                    SyncApproachClientRpc();
                }
            }
            if (secondsLeft > 60)
            {
                TickTock();
            }
        }
        if (secondsLeft <= 60 && Time.realtimeSinceStartup - timeOfLastTickTock > countdownInterval / 2)
        {
            TickTock();
        }
    }

    public override void OnBroughtToShip()
    {
        base.OnBroughtToShip();
        if (shouldCountDown)
        {
            SyncSuccessServerRpc();
        }
    }

    private IEnumerator RollForNewTimer()
    {
        yield return new WaitUntil(() => StartOfRound.Instance.shipHasLanded);
        int selectedRealSeconds = Random.Range(minRealSeconds, maxRealSeconds);
        int startingSeconds = (int)(selectedRealSeconds / countdownInterval);
        int intervalAmount = startingSeconds / 60;
        int specialMultiplier = Mathf.Clamp(TimeOfDay.Instance.daysUntilDeadline, 1, 3) + 1;
        int startingValue = scrapValue * specialMultiplier + ((maxRealSeconds - selectedRealSeconds) / 2);
        if (RarityManager.CurrentlyGoldFever())
        {
            startingValue *= 2;
        }
        SendNewTimerClientRpc(startingSeconds, intervalAmount, specialMultiplier, startingValue);
    }

    [ClientRpc]
    private void SendNewTimerClientRpc(int startingSeconds, int intervalAmount, int specialMultiplier, int startingValue)
    {
        thisClocksStartingSeconds = startingSeconds;
        thisClocksIntervalAmount = intervalAmount;
        thisClocksSpecialMultiplier = specialMultiplier;
        thisClocksStartingValue = startingValue;
        shouldCountDown = true;
        minuteHand.Rotate(-20f * (thisClocksIntervalAmount % 18), 0f, 0f, Space.Self);
        SetTimerRotationAndValue(thisClocksStartingSeconds, false, thisClocksStartingValue);
        Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}:");
        Logger.LogDebug($"{thisClocksStartingSeconds}");
        Logger.LogDebug($"{thisClocksIntervalAmount}");
        Logger.LogDebug($"{thisClocksSpecialMultiplier}");
        Logger.LogDebug($"{thisClocksStartingValue}");
    }

    private void SetTimerRotationAndValue(int seconds, bool isMinute = false, int newScrapValue = -1)
    {
        secondsLeft = seconds;
        if (isMinute)
        {
            secondHand.localRotation = new Quaternion(0, 0, 0, 0);
        }
        else
        {
            secondHand.Rotate(-6f * (seconds % 60), 0f, 0f, Space.Self);
        }
        if (newScrapValue != -1)
        {
            SetScrapValue(newScrapValue);
        }
    }

    [ClientRpc]
    private void SyncTimeAndValueClientRpc(int hostSecondsLeft, int newScrapValue, bool isMinute)
    {
        minuteHand.Rotate(20f, 0f, 0f, Space.Self);
        tickAudio.PlayOneShot(intervalClip);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 5);
        WalkieTalkie.TransmitOneShotAudio(tickAudio, intervalClip);
        SetTimerRotationAndValue(hostSecondsLeft, isMinute, newScrapValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncSuccessServerRpc()
    {
        SyncSuccessClientRpc(secondsLeft < 60);
    }

    [ClientRpc]
    private void SyncSuccessClientRpc(bool inFinalPhase)
    {
        shouldCountDown = false;
        tickAudio.PlayOneShot(intervalClip);
        WalkieTalkie.TransmitOneShotAudio(tickAudio, intervalClip);
        if (inFinalPhase)
        {
            closeCallAudio.Stop();
            closeCallAudio.PlayOneShot(closeCallSuccess);
            WalkieTalkie.TransmitOneShotAudio(closeCallAudio, closeCallSuccess);
        }
    }

    [ClientRpc]
    private void SyncFailureClientRpc(int failedScrapValue)
    {
        shouldCountDown = false;
        secondsLeft = 0;
        tickAudio.PlayOneShot(failClip);
        WalkieTalkie.TransmitOneShotAudio(tickAudio, failClip);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 15, 1);
        SetScrapValue(failedScrapValue);
        secondHand.localRotation = new Quaternion(0, 0, 0, 0);
        minuteHand.localRotation = new Quaternion(0, 0, 0, 0);
        if (approachedShipFinalPhase)
        {
            closeCallAudio.Stop();
            closeCallAudio.PlayOneShot(closeCallFail);
            WalkieTalkie.TransmitOneShotAudio(closeCallAudio, closeCallFail);
        }
    }

    [ClientRpc]
    private void SyncApproachClientRpc()
    {
        approachedShipFinalPhase = true;
        closeCallAudio.PlayOneShot(closeCallApproach);
        WalkieTalkie.TransmitOneShotAudio(closeCallAudio, closeCallApproach);
    }

    private void TickTock()
    {
        tickOrTock = !tickOrTock;
        AudioClip clipToPlay = tickOrTock ? tickSFX : tockSFX;
        tickAudio.PlayOneShot(clipToPlay);
        WalkieTalkie.TransmitOneShotAudio(tickAudio, clipToPlay, 0.33f);
        timeOfLastTickTock = Time.realtimeSinceStartup;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(shouldCountDown, secondsLeft, playerID);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(bool hostBroughtToShip, int hostSecondsLeft, int playerID)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            shouldCountDown = hostBroughtToShip;
            SetTimerRotationAndValue(hostSecondsLeft);
        }
    }
}
