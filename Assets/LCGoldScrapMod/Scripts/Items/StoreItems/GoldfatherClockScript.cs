using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class GoldfatherClockScript : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;
    private static bool debugCheckEveryMinute = false;

    [Header("AUDIO")]
    [Header("Relaxing")]
    public AudioSource audioRelaxingFar;
    public AudioSource audioRelaxingClose;
    public AudioClip chimeRelaxing;
    public AudioClip[] handTick;
    public AudioClip[] pendulumSway;
    public AudioClip[] readjustWeight;
    [Header("Stressful")]
    public AudioSource audioStressfulBird;
    public AudioSource audioStressfulPendulum;
    public AudioClip chimeStressful;
    public AudioClip birdScreech;
    public AudioClip birdPunch;
    public AudioClip pendulumCrash;
    public AudioClip slamShut;

    [Space(3f)]
    [Header("VISUAL")]
    [Header("Animations")]
    public Animator birdAnimator;
    public Animator pendulumAnimator;
    [Header("Time")]
    public Transform minuteHand;
    public Transform hourHand;
    public Transform solarClock;
    [Header("Weights")]
    public GameObject[] weightObjects;
    public float[] weightStartingYs;
    public float maxDropDistance;
    public bool timeCanTick;


    [Space(3f)]
    [Header("MISC")]
    public InteractTrigger clockFaceTrigger;
    public int[] badHours;
    private static bool alreadyHadBadHour;
    private static bool alreadyPerformedStressful;
    private float minuteTimer;
    private List<int> collectedHours = new List<int>();
    private Coroutine relaxingBellsCoroutine;



    //Local
    void Start()
    {
        SetTimeLocal(null, null, false);
    }

    void Update()
    {
        if (IsServer)
        {
            minuteTimer += Time.deltaTime;
            if (minuteTimer >= 60 && timeCanTick)
            {
                int[] validWeights = CheckWeightsAboveMaxDrop();
                if (validWeights.Length > 0)
                {
                    List<float> dropByAmount = new List<float>();
                    for (int i = 0; i < validWeights.Length; i++)
                    {
                        dropByAmount.Add(GetRandomDropAmount(validWeights[i]));
                    }
                    SetTimeClientRpc(validWeights, dropByAmount.ToArray());
                }
                else
                {
                    SetWeightsInteractibleClientRpc();
                }
                minuteTimer = 0;
            }
        }
    }

    private void SetTimeLocal(int[] weightsToDrop = null, float[] dropByAmount = null, bool performBells = true)
    {
        int currentMinute = DateTime.Now.Minute;
        int currentHour = DateTime.Now.Hour;
        minuteHand.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        minuteHand.Rotate(0f, 6f * currentMinute, 0f, Space.Self);
        hourHand.localRotation = new Quaternion(0f, 0f, 0f, 0f);
        hourHand.Rotate(0f, 30f * currentHour + 30 * ((float)currentMinute / 60), 0f, Space.Self);
        audioRelaxingFar.PlayOneShot(handTick[UnityEngine.Random.Range(0, handTick.Length)]);
        if (weightsToDrop != null && dropByAmount != null)
        {
            for (int i = 0; i < weightsToDrop.Length; i++)
            {
                GameObject weight = weightObjects[weightsToDrop[i]];
                StartCoroutine(LowerWeight(weight, weight.transform.localPosition.y - dropByAmount[i]));
            }
        }
        if ((currentMinute == 0 && performBells) || debugCheckEveryMinute)
        {
            relaxingBellsCoroutine = StartCoroutine(DoRelaxingBells(currentHour));
            if (IsServer && !RarityManager.CurrentlyGoldFever() && !alreadyPerformedStressful)
            {
                StartCoroutine(CheckForStressfulBells());
            }
        }
    }

    private void StartStressfulBells()
    {
        alreadyPerformedStressful = true;
        if (relaxingBellsCoroutine != null)
        {
            StopCoroutine(relaxingBellsCoroutine);
            relaxingBellsCoroutine = null;
        }
        timeCanTick = false;
        birdAnimator.SetTrigger("Open");
    }

    public void StartStressfulLoop()
    {
        birdAnimator.SetTrigger("Stressful");
        pendulumAnimator.SetTrigger("Crash");
        clockFaceTrigger.interactable = true;
    }

    private int[] CheckWeightsAboveMaxDrop()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < weightObjects.Length; i++)
        {
            GameObject weight = weightObjects[i];
            Logger.LogDebug($"{weight.name}: local Y = {weight.transform.localPosition.y} & maxDrop Y = {weightStartingYs[i] - maxDropDistance}");
            if (weight.transform.localPosition.y > weightStartingYs[i] - maxDropDistance)
            {
                Logger.LogDebug($"above bottom, adding to count");
                list.Add(i);
            }
        }
        return list.ToArray();
    }

    private float GetRandomDropAmount(int weightNr)
    {
        Logger.LogDebug($"checking for weight: {weightObjects[weightNr].name}");
        Transform thisWeight = weightObjects[weightNr].transform;
        float maxDropY = weightStartingYs[weightNr] - maxDropDistance;
        float thisWeightDistanceToMax = thisWeight.localPosition.y - maxDropY;
        float randomDropAmount = UnityEngine.Random.Range(0.003f, 0.02f);
        if (randomDropAmount > thisWeightDistanceToMax)
        {
            randomDropAmount = thisWeightDistanceToMax;
        }
        return randomDropAmount;
    }

    private void StopClock()
    {
        foreach (GameObject weight in weightObjects)
        {
            weight.GetComponent<InteractTrigger>().interactable = true;
        }
        pendulumAnimator.SetTrigger("Stop");
    }



    //Rpc's
    [ClientRpc]
    private void SetTimeClientRpc(int[] weightsToDrop, float[] dropByAmount)
    {
        SetTimeLocal(weightsToDrop, dropByAmount);
    }

    [ServerRpc(RequireOwnership = true)]
    private void CollectHoursServerRpc()
    {
        collectedHours.Clear();
        CollectHoursClientRpc();
    }

    [ClientRpc]
    private void CollectHoursClientRpc()
    {
        Logger.LogDebug($"clientRPC to send hour {DateTime.Now.Hour}");
        SendHourServerRpc(DateTime.Now.Hour);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendHourServerRpc(int receivedHour)
    {
        Logger.LogDebug($"received: {receivedHour}");
        collectedHours.Add(receivedHour);
    }

    [ClientRpc]
    private void InitiateStressfulBellsClientRpc()
    {
        StartStressfulBells();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopStressfulServerRpc()
    {
        StopStressfulClientRpc();
    }

    [ClientRpc]
    private void StopStressfulClientRpc()
    {
        clockFaceTrigger.interactable = false;
        birdAnimator.SetTrigger("Close");
        pendulumAnimator.SetTrigger("Sway");
        timeCanTick = true;
    }

    [ClientRpc]
    private void SetWeightsInteractibleClientRpc()
    {
        timeCanTick = false;
        StopClock();
    }

    [ServerRpc(RequireOwnership = false)]
    public void LiftWeightServerRpc(int weightNr)
    {
        LiftWeightClientRpc(weightNr);
    }

    [ClientRpc]
    private void LiftWeightClientRpc(int weightNr)
    {
        StartCoroutine(LiftWeight(weightNr));
    }

    [ClientRpc]
    private void StartPendulumSwayClientRpc()
    {
        timeCanTick = true;
        pendulumAnimator.SetTrigger("Sway");
    }

    [ClientRpc]
    public void SetSolarClockRotationClientRpc()
    {
        StartCoroutine(RotateSolarClock());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(playerID, weightObjects[0].transform.localPosition.y, weightObjects[1].transform.localPosition.y, weightObjects[2].transform.localPosition.y, solarClock.localRotation.eulerAngles.y);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(int playerID, float hostLeftY, float hostMidY, float hostRightY, float hostSolarRotationZ)
    {
        if (playerID == (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            weightObjects[0].transform.localPosition = new Vector3(weightObjects[0].transform.localPosition.x, hostLeftY, weightObjects[0].transform.localPosition.z);
            weightObjects[1].transform.localPosition = new Vector3(weightObjects[1].transform.localPosition.x, hostMidY, weightObjects[1].transform.localPosition.z);
            weightObjects[2].transform.localPosition = new Vector3(weightObjects[2].transform.localPosition.x, hostRightY, weightObjects[2].transform.localPosition.z);
            if (weightObjects[0].transform.localPosition.y == weightStartingYs[0] && weightObjects[1].transform.localPosition.y == weightStartingYs[1] && weightObjects[2].transform.localPosition.y == weightStartingYs[2])
            {
                StopClock();
            }
            solarClock.Rotate(0f, 0f, hostSolarRotationZ * -1, Space.Self);
        }
    }



    //Coroutines
    private IEnumerator CheckForStressfulBells()
    {
        CollectHoursServerRpc();
        float waitedFor = 0;
        while (collectedHours.Count < StartOfRound.Instance.connectedPlayersAmount + 1)
        {
            if (waitedFor >= 5)
            {
                Logger.LogDebug("waited for too long, breaking collection loop and continuing with received values");
                break;
            }
            float timeToWait = 0.5f;
            yield return new WaitForSeconds(timeToWait);
            waitedFor += timeToWait;
        }
        Logger.LogDebug($"going to evaluation with collectedHours count: {collectedHours.Count}");
        foreach (int hour in collectedHours)
        {
            Logger.LogDebug($"hour: {hour}");
            if (!badHours.Contains(hour))
            {
                Logger.LogDebug("somebody playing with good hour");
                yield break;
            }
        }
        if (alreadyHadBadHour)
        {
            InitiateStressfulBellsClientRpc();
            yield break;
        }
        Logger.LogDebug("did not perform stressful bells but setting up for next one");
        alreadyHadBadHour = true;
    }

    private IEnumerator DoRelaxingBells(int currentHour)
    {
        int totalAmount = currentHour % 12;
        if (totalAmount == 0)
        {
            totalAmount = 12;
        }
        int performedAmount = 0;
        while (performedAmount < totalAmount)
        {
            audioRelaxingFar.PlayOneShot(chimeRelaxing);
            WalkieTalkie.TransmitOneShotAudio(audioRelaxingFar, chimeRelaxing, 0.1f);
            performedAmount++;
            yield return new WaitForSeconds(3f);
        }
        relaxingBellsCoroutine = null;
    }

    private IEnumerator LiftWeight(int weightNr)
    {
        GameObject weight = weightObjects[weightNr];
        weight.GetComponent<InteractTrigger>().interactable = false;
        AudioClip randomClip = readjustWeight[UnityEngine.Random.Range(0, readjustWeight.Length)];
        audioRelaxingFar.PlayOneShot(randomClip);
        WalkieTalkie.TransmitOneShotAudio(audioRelaxingFar, randomClip, 0.25f);
        while (weight.transform.localPosition.y < weightStartingYs[weightNr])
        {
            weight.transform.localPosition = new Vector3(weight.transform.localPosition.x, Mathf.Lerp(weight.transform.localPosition.y, weightStartingYs[weightNr], 0.02f), weight.transform.localPosition.z);
            yield return null;
            if (weightStartingYs[weightNr] - weight.transform.localPosition.y < 0.01)
            {
                weight.transform.localPosition = new Vector3(weight.transform.localPosition.x, weightStartingYs[weightNr], weight.transform.localPosition.z);
                break;
            }
        }
        if (IsServer)
        {
            int count = 0;
            for (int i = 0; i < weightObjects.Length; i++)
            {
                if (weightObjects[i].transform.localPosition.y == weightStartingYs[i])
                {
                    Logger.LogDebug($"{weightObjects[i].name} reached top");
                    count++;
                }
            }
            Logger.LogDebug($"finished coroutine of {weight.name}: amount at Ys = {count}");
            if (count == 3)
            {
                StartPendulumSwayClientRpc();
            }
        }
    }

    private IEnumerator LowerWeight(GameObject weight, float targetY)
    {
        while (weight.transform.localPosition.y > targetY)
        {
            yield return null;
            float nextY = weight.transform.localPosition.y - 0.0001f;
            weight.transform.localPosition = new Vector3(weight.transform.localPosition.x, nextY, weight.transform.localPosition.z);
        }
        Logger.LogDebug($"set local Y of {weight.name} to {weight.transform.localPosition.y}");
    }

    private IEnumerator RotateSolarClock()
    {
        int degreesRotated = 0;
        while (degreesRotated < 30)
        {
            yield return new WaitForSeconds(0.1f);
            solarClock.Rotate(0f, 0f, -1f, Space.Self);
            degreesRotated++;
        }
    }



    //AnimationEvents
    public void AnimEventPendulumSway()
    {
        audioRelaxingClose.PlayOneShot(pendulumSway[UnityEngine.Random.Range(0, pendulumSway.Length)]);
    }

    public void AnimEventPendulumCrash()
    {
        audioStressfulPendulum.PlayOneShot(pendulumCrash);
        StressfulAudibleNoise(pendulumCrash);
        audioStressfulPendulum.PlayOneShot(chimeRelaxing);
        StressfulAudibleNoise(chimeRelaxing);
    }

    public void AnimEventBirdhouseOpen()
    {
        audioStressfulBird.PlayOneShot(chimeStressful);
        StressfulAudibleNoise(chimeStressful);
    }

    public void AnimEventBirdScreech()
    {
        audioStressfulBird.PlayOneShot(birdScreech);
        StressfulAudibleNoise(birdScreech);
    }

    public void AnimEventPunchBird()
    {
        audioRelaxingFar.PlayOneShot(birdPunch);
    }

    public void AnimEventSlamShut()
    {
        audioStressfulBird.PlayOneShot(slamShut);
    }

    private void StressfulAudibleNoise(AudioClip clipToPlay)
    {
        WalkieTalkie.TransmitOneShotAudio(audioStressfulPendulum, clipToPlay);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 9999f, 1f);
    }
}
