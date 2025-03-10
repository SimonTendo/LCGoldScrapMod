using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;

public class SafeBoxScript : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Header("Scripts")]
    public InteractTrigger outsideTrigger;
    public InteractTrigger insideTrigger;
    public PlaceableShipObject buildModeObject;
    public PlaceableObjectsSurface placeItemObject;
    public Animator doorAnimator;

    [Space(3f)]
    [Header("Audiovisual")]
    [Header("Outside")]
    public AudioSource outsideAudio;
    public AudioClip unlockClip;
    public AudioClip openingCreakClip;
    public AudioClip closeClipNormal;
    public AudioClip closeClipFail;
    [Header("Inside")]
    public AudioSource insideAudio;
    public AudioClip closeClipTrapPlayer;
    public AudioClip trappedPlayerBonk;
    public AudioClip trappedPlayerEscape;
    public Light insideLight;

    [Space(3f)]
    [Header("Safe")]
    public bool isOpen;
    public Collider insideBounds;
    private List<PlayerControllerB> trappedPlayers = new List<PlayerControllerB>();
    private bool localPlayerTrapped;
    public int minBonksToEscape;
    public int maxItemsToSave;
    private int overCapacity;
    private int currentBonks;
    public float checkTrappedInterval;
    private float checkTrappedTimer;

    private void Start()
    {
        transform.parent.SetParent(StartOfRound.Instance.elevatorTransform, true);
        insideLight.intensity = 5f;
        insideLight.lightShadowCasterMode = LightShadowCasterMode.Everything;
        insideLight.shadows = LightShadows.Hard;
        GeneralFunctions(false);
    }

    private void Update()
    {
        if (!IsServer || trappedPlayers.Count == 0)
        {
            return;
        }
        checkTrappedTimer += Time.deltaTime;
        if (checkTrappedTimer > checkTrappedInterval)
        {
            for (int i = trappedPlayers.Count - 1; i >= 0; i--)
            {
                if (!insideBounds.bounds.Contains(trappedPlayers[i].transform.position))
                {
                    FreePlayerClientRpc((int)trappedPlayers[i].playerClientId);
                }
            }
            checkTrappedTimer = 0;
        }
    }

    private void GeneralFunctions(bool setOpenTo)
    {
        outsideTrigger.hoverTip = setOpenTo ? "Close : [E]" : "Open : [E]";
        isOpen = setOpenTo;
        insideLight.enabled = setOpenTo;
        if (isOpen)
        {
            OnOpen();
        }
    }

    private void OnOpen()
    {
        currentBonks = 0;
        buildModeObject.inUse = false;
        insideTrigger.interactable = false;
        for (int i = trappedPlayers.Count - 1; i >= 0; i--)
        {
            FreePlayer(trappedPlayers[i]);
        }
    }

    private void OnClose()
    {
        buildModeObject.inUse = trappedPlayers.Count != 0;
        insideTrigger.interactable = true;
        GrabbableObject[] allItemsInBounds = GetItemsInSafe();
        foreach (GrabbableObject item in allItemsInBounds)
        {
            if (item.transform.parent != placeItemObject.parentTo.transform && !item.isHeld && !item.isPocketed)
            {
                item.targetFloorPosition = placeItemObject.parentTo.transform.InverseTransformPoint(item.transform.position);
                item.transform.SetParent(placeItemObject.parentTo.transform, true);
            }
        }
        GeneralFunctions(false);
    }



    //AnimationEvents
    public void PlayCreakSFX()
    {
        outsideAudio.pitch = Random.Range(0.8f, 1.2f);
        outsideAudio.PlayOneShot(openingCreakClip);
        WalkieTalkie.TransmitOneShotAudio(outsideAudio, openingCreakClip, 0.5f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 5f, 0.3f);
    }

    public void PlayCloseSFX()
    {
        outsideAudio.pitch = Random.Range(0.95f, 1.05f);
        outsideAudio.PlayOneShot(closeClipNormal);
        WalkieTalkie.TransmitOneShotAudio(outsideAudio, closeClipNormal, 0.25f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 15f, 0.25f);
        CheckOnClose();
    }

    public void PlaySlamSFX()
    {
        insideAudio.pitch = 1f;
        insideAudio.PlayOneShot(closeClipTrapPlayer);
        WalkieTalkie.TransmitOneShotAudio(insideAudio, closeClipTrapPlayer, 1f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 30f, 1f);
        CheckOnClose();
    }

    public void PlayFailSFX()
    {
        outsideAudio.pitch = Random.Range(0.9f, 1.1f);
        outsideAudio.PlayOneShot(closeClipFail);
        WalkieTalkie.TransmitOneShotAudio(outsideAudio, closeClipFail, 0.5f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 10f, 0.5f);
        outsideTrigger.hoverTip = $"Over capacity ({overCapacity}) : [E]";
    }



    //Closing
    private void CheckOnClose()
    {
        if (!IsServer)
        {
            return;
        }
        int totalWeight = GetWeightOfItems(GetItemsInSafe());
        int overCapacity = totalWeight - maxItemsToSave;
        if (overCapacity > 0)
        {
            CloseSafeFailClientRpc(overCapacity);
            return;
        }
        int[] playersIDs = GetPlayersInSafe();
        OnCloseClientRpc(playersIDs);
    }

    private int[] GetPlayersInSafe()
    {
        List<int> trappedPlayerIDs = new List<int>();
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (player != null && player.isPlayerControlled && insideBounds.bounds.Contains(player.transform.position))
            {
                trappedPlayerIDs.Add((int)player.playerClientId);
            }
        }
        return trappedPlayerIDs.ToArray();
    }

    [ClientRpc]
    private void OnCloseClientRpc(int[] trappedPlayerIDs, int joiningPlayerID = -1)
    {
        if (joiningPlayerID == -1 || joiningPlayerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            foreach (int ID in trappedPlayerIDs)
            {
                trappedPlayers.Add(StartOfRound.Instance.allPlayerScripts[ID]);
            }
            localPlayerTrapped = trappedPlayers.Contains(StartOfRound.Instance.localPlayerController);
            if (localPlayerTrapped)
            {
                StartOfRound.Instance.allowLocalPlayerDeath = false;
            }
            else
            {
                foreach (PlayerControllerB player in trappedPlayers)
                {
                    ReplicateMuffle(player);
                }
            }
            OnClose();
        }
    }

    [ClientRpc]
    private void FreePlayerClientRpc(int playerID)
    {
        Logger.LogDebug($"got call to remove player at index [{playerID}] from trappedPlayers");
        FreePlayer(StartOfRound.Instance.allPlayerScripts[playerID]);
    }

    private void FreePlayer(PlayerControllerB player)
    {
        trappedPlayers.Remove(player);
        if (player == StartOfRound.Instance.localPlayerController)
        {
            StartOfRound.Instance.allowLocalPlayerDeath = true;
            localPlayerTrapped = false;
        }
        ReplicateMuffle(player, false);
    }
    

    private void ReplicateMuffle(PlayerControllerB playerScript, bool muffle = true)
    {
        if (playerScript.currentVoiceChatAudioSource == null)
        {
            StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
        }
        if (playerScript.currentVoiceChatAudioSource != null)
        {
            float resonance = muffle ? 5f : 1f;
            float lowPass = muffle ? 500f : 20000f;
            playerScript.currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().lowpassResonanceQ = resonance;
            OccludeAudio component = playerScript.currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
            component.overridingLowPass = muffle;
            component.lowPassOverride = lowPass;
            playerScript.voiceMuffledByEnemy = muffle;
        }
    }



    //OutsideTrigger
    [ServerRpc(RequireOwnership = false)]
    public void SetSafeOpenedClosedServerRpc()
    {
        if (isOpen)
        {
            if (GetPlayersInSafe().Length > 0)
            {
                CloseSafeTrapClientRpc();
            }
            else
            {
                CloseSafeNormalClientRpc();
            }
        }
        else
        {
            OpenSafeClientRpc();
        }
    }

    [ClientRpc]
    private void OpenSafeClientRpc()
    {
        OpenSafeLocal();
    }

    private void OpenSafeLocal()
    {
        doorAnimator.SetTrigger("OpenNormal");
        outsideAudio.PlayOneShot(unlockClip);
        GeneralFunctions(true);
    }

    [ClientRpc]
    public void CloseSafeNormalClientRpc()
    {
        buildModeObject.inUse = true;
        doorAnimator.SetTrigger("CloseNormal");
    }

    [ClientRpc]
    private void CloseSafeTrapClientRpc()
    {
        buildModeObject.inUse = true;
        doorAnimator.SetTrigger("CloseTrapped");
    }

    [ClientRpc]
    private void CloseSafeFailClientRpc(int hostOvercapacity)
    {
        buildModeObject.inUse = false;
        overCapacity = hostOvercapacity;
        doorAnimator.SetTrigger("CloseFail");
    }

    public void SetInteractableFromAnimation(string setBoolValue)
    {
        outsideTrigger.interactable = setBoolValue.ToLower() == "true";
        Logger.LogDebug($"SetInteractableFromAnimation({setBoolValue}) on {name} #{NetworkObjectId} set outsideTrigger.interactable to {outsideTrigger.interactable}");
    }



    //InsideTrigger
    public void TrappedPlayerInteract()
    {
        int randomNr = Random.Range(0, 101);
        int chanceToEscape = Mathf.Clamp((40 - StartOfRound.Instance.connectedPlayersAmount * 5) / Mathf.Max(1, trappedPlayers.Count), 1, 101);
        if (currentBonks > minBonksToEscape * trappedPlayers.Count && randomNr < chanceToEscape)
        {
            insideTrigger.enabled = false;
            TrappedPlayersEscapeServerRpc();
        }
        else
        {
            TrappedPlayersBonkOnDoorServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TrappedPlayersEscapeServerRpc()
    {
        TrappedPlayersEscapeClientRpc();
    }

    [ClientRpc]
    private void TrappedPlayersEscapeClientRpc()
    {
        insideAudio.pitch = 1f;
        insideAudio.PlayOneShot(trappedPlayerEscape);
        WalkieTalkie.TransmitOneShotAudio(insideAudio, trappedPlayerEscape, 0.5f);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 20, 0.8f);
        doorAnimator.SetTrigger("OpenTrapped");
        GeneralFunctions(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TrappedPlayersBonkOnDoorServerRpc()
    {
        TrappedPlayersBonkOnDoorClientRpc();
    }

    [ClientRpc]
    private void TrappedPlayersBonkOnDoorClientRpc()
    {
        currentBonks++;
        insideAudio.volume = Random.Range(0.9f, 1.0f);
        insideAudio.pitch = Random.Range(0.8f, 1.2f);
        insideAudio.PlayOneShot(trappedPlayerBonk);
        WalkieTalkie.TransmitOneShotAudio(insideAudio, trappedPlayerBonk, 0.2f);
        RoundManager.Instance.PlayAudibleNoise(insideAudio.transform.position, 10, 0.4f);
    }



    //SyncUponJoin
    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        List<int> trappedPlayerIDs = new List<int>();
        foreach (PlayerControllerB player in trappedPlayers)
        {
            trappedPlayerIDs.Add((int)player.playerClientId);
        }
        OnCloseClientRpc(trappedPlayerIDs.ToArray(), playerID);
        SyncUponJoinClientRpc(playerID, isOpen);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(int playerID, bool hostIsOpen)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId && hostIsOpen)
        {
            OpenSafeLocal();
        }
    }



    //Stored Items
    private GrabbableObject[] GetItemsInSafe()
    {
        List<GrabbableObject> itemsInBounds = new List<GrabbableObject>();
        GrabbableObject[] allItems = FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);
        foreach (GrabbableObject item in allItems)
        {
            if (item.deactivated || !item.isInShipRoom)
            {
                continue;
            }
            if (insideBounds.bounds.Contains(item.transform.position))
            {
                itemsInBounds.Add(item);
            }
        }
        return itemsInBounds.ToArray();
    }

    private int GetWeightOfItems(GrabbableObject[] itemArray)
    {
        int weight = 0;
        foreach (GrabbableObject item in itemArray)
        {
            weight += item.itemProperties.twoHanded ? 2 : 1;
        }
        Logger.LogDebug($"total weight: {weight}");
        return weight;
    }

    [HarmonyPatch(typeof(NetworkObject), "Despawn")]
    public class NewNetworkObjectDespawn
    {
        [HarmonyPrefix]
        public static bool CheckIfInSafe(NetworkObject __instance)
        {
            if (StartOfRound.Instance.allPlayersDead)
            {
                SafeBoxScript safeBox = FindObjectOfType<SafeBoxScript>();
                if (safeBox != null && !safeBox.isOpen && __instance.GetComponent<GrabbableObject>() != null && __instance.GetComponent<RagdollGrabbableObject>() == null && __instance.transform.parent == safeBox.placeItemObject.parentTo.transform)
                {
                    Logger.LogDebug($"!!!{__instance.gameObject.name} #{__instance.NetworkObjectId} NOT DESPAWNING IN {__instance.transform.parent.name}!!!");
                    return false;
                }
            }
            return true;
        }
    }
}
