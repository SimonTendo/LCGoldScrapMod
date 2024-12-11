using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class JackInTheGoldScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    private float timeAtLastExplosion;
    private float audibleNoiseTimer;

    [Space(3f)]
    [Header("Music")]
    public AudioSource pocketedThemeSource;
    public AudioSource explodeSource;
    public AudioClip pocketedThemeClip;
    public AudioClip explodeClip;

    public override void Start()
    {
        base.Start();
        pocketedThemeSource.clip = pocketedThemeClip;
    }

    public override void Update()
    {
        base.Update();
        if (IsServer && pocketedThemeSource.isPlaying)
        {
            if (playerHeldBy == null || !StartOfRound.Instance.shipDoorsEnabled || StartOfRound.Instance.inShipPhase)
            {
                SyncPauseAndTimeClientRpc(false);
                return;
            }
            audibleNoiseTimer += Time.deltaTime;
            if (audibleNoiseTimer > 2.5f)
            {
                audibleNoiseTimer = 0;
                Vector3 noisePosition = playerHeldBy != null ? playerHeldBy.transform.position : transform.position;
                RoundManager.Instance.PlayAudibleNoise(noisePosition, 13, 0.7f);
            }
            if (pocketedThemeSource.time > 40f)
            {
                ExplodeUponEndingClientRpc(scrapValue + Random.Range(22, 44));
                timeAtLastExplosion = Time.realtimeSinceStartup;
            }
        }
    }

    public override void PocketItem()
    {
        base.PocketItem();
        if (IsServer)
        {
            SyncPauseAndTimeClientRpc(true);
        }
    }

    public override void EquipItem()
    {
        base.EquipItem();
        if (IsServer)
        {
            SyncPauseAndTimeClientRpc(false);
        }
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        if (IsServer && Time.realtimeSinceStartup - timeAtLastExplosion > 1f)
        {
            SyncPauseAndTimeClientRpc(false, pocketedThemeSource.time);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncPauseAndTimeClientRpc(pocketedThemeSource.isPlaying, pocketedThemeSource.time, playerID);
    }

    [ClientRpc]
    private void SyncPauseAndTimeClientRpc(bool playOrPause, float hostTime = -1, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            if (hostTime != -1)
            {
                pocketedThemeSource.time = hostTime;
            }
            if (playOrPause && !pocketedThemeSource.isPlaying)
            {
                pocketedThemeSource.Play();
            }
            else if (!playOrPause && pocketedThemeSource.isPlaying)
            {
                pocketedThemeSource.Pause();
            }
        }
    }

    [ClientRpc]
    private void ExplodeUponEndingClientRpc(int newScrapValue)
    {
        SetScrapValue(newScrapValue);
        pocketedThemeSource.time = 0f;
        pocketedThemeSource.Stop();
        explodeSource.PlayOneShot(explodeClip);
        Vector3 explosionPosition = playerHeldBy != null ? playerHeldBy.transform.position : transform.position;
        Landmine.SpawnExplosion(explosionPosition, true, 3.5f, 6, 70, 15);
        RoundManager.Instance.PlayAudibleNoise(explosionPosition, 40, 1);
        WalkieTalkie.TransmitOneShotAudio(explodeSource, explodeClip);
    }
}
