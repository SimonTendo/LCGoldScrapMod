using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;


public class GoldBirdScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("Alarm")]
    public AudioSource alarmSource;
    public AudioClip alarmClip;
    private float alarmTimer;
    private bool canAlarmFire;

    [Space(3f)]
    [Header("Spotlight")]
    public AudioSource spotlightAudio;
    public AudioClip lightOnClip;
    public AudioClip lightOffClip;
    public Light headlight;
    public Color normalColor;
    public Color alarmColor;

    [Space(3f)]
    [Header("Dormancy")]
    public AudioSource awakeSource;
    public AudioClip awakeClip;
    public AudioClip dieClip;
    public bool dormant = true;
    public ScanNodeProperties scanNode;
    private bool canWakeThisRound = true;

    public override void Start()
    {
        base.Start();
        alarmSource.clip = alarmClip;
        if (isInShipRoom)
        {
            canWakeThisRound = false;
        }
        else
        {
            scanNode.headerText = "Gold Bird (Dormant)";
        }
    }

    public override void Update()
    {
        base.Update();
        if (IsServer && !dormant)
        {
            canAlarmFire = playerHeldBy != null && StartOfRound.Instance.shipDoorsEnabled && !StartOfRound.Instance.inShipPhase;
            if (!canAlarmFire && alarmTimer < 0.1f)
            {
                alarmTimer = 0.1f;
                ToggleAlarmClientRpc(false);
                return;
            }
            alarmTimer += Time.deltaTime;
            if (alarmTimer > 2f)
            {
                alarmTimer = 0;
                DoAlarmInterval();
            }
        }
    }

    private void DoAlarmInterval()
    {
        if (canAlarmFire)
        {
            if (IsNearbyPlayerInSight())
            {
                ToggleAlarmClientRpc(true);
                RoundManager.Instance.PlayAudibleNoise(transform.position, alarmSource.maxDistance + 15, 1f);
            }
            else
            {
                ToggleAlarmClientRpc(false);
            }
        } 
    }

    private bool IsNearbyPlayerInSight()
    {
        if (StartOfRound.Instance.connectedPlayersAmount > 0)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player == playerHeldBy || !player.isPlayerControlled || Vector3.Distance(headlight.transform.position, player.transform.position) > alarmSource.maxDistance - 15)
                {
                    continue;
                }
                return !Physics.Linecast(headlight.transform.position, player.playerEye.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
            }
        }
        return false;
    }

    [ClientRpc]
    private void ToggleAlarmClientRpc(bool alarm)
    {
        if (!headlight.enabled)
        {
            headlight.enabled = true;
        }
        if (alarm && !alarmSource.isPlaying)
        {
            spotlightAudio.PlayOneShot(lightOnClip);
            alarmSource.Play();
            headlight.color = alarmColor;
        }
        else if (!alarm && alarmSource.isPlaying)
        {
            spotlightAudio.PlayOneShot(lightOffClip);
            alarmSource.Stop();
            headlight.color = normalColor;
        }
    }

    [HarmonyPatch(typeof(HUDManager), "RadiationWarningHUD")]
    public class NewHUDManagerRadiationWarning
    {
        [HarmonyPostfix]
        public static void AwakenGoldBirds()
        {
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null && GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
            {
                foreach (GoldBirdScript goldBird in FindObjectsByType<GoldBirdScript>(FindObjectsSortMode.None))
                {
                    goldBird.AwakenClientRpc((int)(goldBird.scrapValue * Random.Range(3f, 4f)));
                }
            }
        }
    }

    [ClientRpc]
    public void AwakenClientRpc(int awakenedValue)
    {
        Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: canWake {canWakeThisRound} // dormant {dormant}");
        if (canWakeThisRound && dormant)
        {
            Logger.LogDebug($"awakening");
            awakeSource.PlayOneShot(awakeClip);
            WalkieTalkie.TransmitOneShotAudio(awakeSource, awakeClip, 0.5f);
            RoundManager.Instance.PlayAudibleNoise(transform.position, awakeSource.maxDistance, 0.1f);
            headlight.enabled = true;
            headlight.color = normalColor;
            dormant = false;
            canWakeThisRound = false;
            if (scanNode.headerText.Contains("(Dormant)"))
            {
                scanNode.headerText = scanNode.headerText.Replace("Dormant", "Awake");
            }
            SetScrapValue(awakenedValue);
        }
    }

    [ClientRpc]
    public void DeactivateAtEndOfDayClientRpc()
    {
        if (canWakeThisRound)
        {
            canWakeThisRound = false;
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId} can no longer wake");
            if (dormant)
            {
                if (scanNode.headerText.Contains("(Dormant)"))
                {
                    scanNode.headerText = scanNode.headerText.Replace("Dormant", "Dead");
                }
                spotlightAudio.PlayOneShot(dieClip);
                Instantiate(AssetsCollection.poofParticle, headlight.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<ParticleSystem>().Play();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(canWakeThisRound, headlight.enabled, scanNode.headerText, playerID);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(bool hostCanWakeValue, bool enableHeadlight, string scanNodeName, int playerID)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            canWakeThisRound = hostCanWakeValue;
            headlight.enabled = enableHeadlight;
            if (enableHeadlight)
            {
                headlight.color = alarmSource.isPlaying ? alarmColor : normalColor;
            }
            scanNode.headerText = scanNodeName;
        }
    }
}
