using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class GoldToiletScript : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Header("Audio")]
    public AudioSource chargeAudio;
    public AudioSource flushAudio;

    [Space(3f)]
    [Header("Misc")]
    public InteractTrigger triggerScript;
    private int playersUsing;
    private float timeHeldUninterrupted;
    private UnlockableItem unlockable;

    private void Start()
    {
        chargeAudio.volume = 0f;
        unlockable = StartOfRound.Instance.unlockablesList.unlockables[transform.parent.GetComponentInChildren<PlaceableShipObject>().unlockableID];
    }

    private void Update()
    {
        if (unlockable.inStorage)
        {
            timeHeldUninterrupted = 0f;
            if (chargeAudio.isPlaying)
            {
                chargeAudio.Stop();
                chargeAudio.volume = 0f;
            }
            if (flushAudio.isPlaying)
            {
                flushAudio.Stop();
            }
            return;
        }
        if (playersUsing == 0)
        {
            timeHeldUninterrupted = 0;
            if (chargeAudio.isPlaying)
            {
                chargeAudio.volume = Mathf.Lerp(0, chargeAudio.volume, 0.97f);
                if (chargeAudio.volume < 0.05f)
                {
                    chargeAudio.Stop();
                }
            }
        }
        else
        {
            timeHeldUninterrupted += Time.deltaTime;
            if (!chargeAudio.isPlaying)
            {
                chargeAudio.Play();
            }
            chargeAudio.volume = timeHeldUninterrupted / triggerScript.timeToHold;
        }
    }

    public void SetLocalPlayerUsing(bool isUsing)
    {
        int difference = isUsing ? 1 : -1;
        SetTotalPlayersUsingServerRpc(difference);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTotalPlayersUsingServerRpc(int difference)
    {
        SetTotalPlayersUsingClientRpc(Mathf.Clamp(playersUsing + difference, 0, StartOfRound.Instance.connectedPlayersAmount + 1));
    }

    [ClientRpc]
    private void SetTotalPlayersUsingClientRpc(int hostPlayersUsing)
    {
        playersUsing = hostPlayersUsing;
    }

    public void Flush()
    {
        triggerScript.interactable = false;
        FlushServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void FlushServerRpc()
    {
        FlushClientRpc();
    }

    [ClientRpc]
    private void FlushClientRpc()
    {
        if (!flushAudio.isPlaying)
        {
            triggerScript.interactable = false;
            flushAudio.Play();
            WalkieTalkie.TransmitOneShotAudio(flushAudio, flushAudio.clip, 0.5f);
            RoundManager.Instance.PlayAudibleNoise(flushAudio.transform.position + Vector3.up, flushAudio.maxDistance, 1, 0, StartOfRound.Instance.hangarDoorsClosed);
            StartCoroutine(DisableInteractFor(flushAudio.clip.length * 1.1f));
        }
    }

    private IEnumerator DisableInteractFor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        triggerScript.interactable = true;
    }
}
