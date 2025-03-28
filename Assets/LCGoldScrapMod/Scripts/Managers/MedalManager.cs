using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;

public class MedalManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static MedalManager instance;

    private int[] medalsWornBy;
    public static int goldMedalID;
    public static int silverMedalID;
    public static int bronzeMedalID;

    [Header("On Frame")]
    public GameObject frame;
    public GameObject defaultBadge;
    public GameObject[] medalContainers;

    [Space(3f)]
    [Header("On Player")]
    public GameObject medalChestContainer;
    public GameObject[] medalPrefabs;

    [Space(3f)]
    [Header("Misc")]
    public AutoParentToShip shipPlacement;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        medalsWornBy = new int[StartOfRound.Instance.allPlayerScripts.Length];
        Logger.LogDebug($"MedalManager Start() // length: {medalsWornBy.Length}");
        for (int i = 0; i < medalsWornBy.Length; i++)
        {
            GameObject medalOnPlayer = Instantiate(medalChestContainer, StartOfRound.Instance.allPlayerScripts[i].playerBadgeMesh.transform, false);
            medalOnPlayer.name += i;
        }
        ResetAllMedals();
        foreach (InteractTrigger trigger in GetComponentsInChildren<InteractTrigger>())
        {
            trigger.hoverIcon = AssetsCollection.handIcon;
        }
        defaultBadge.GetComponent<MeshRenderer>().material = StartOfRound.Instance.allPlayerScripts[0].playerBadgeMesh.gameObject.GetComponent<MeshRenderer>().material;
    }

    public void WearMedal(int medalType)
    {
        int localPlayerID = (int)StartOfRound.Instance.localPlayerController.playerClientId;
        WearMedalOnPlayer(localPlayerID, medalType, true);
        WearMedalServerRpc(localPlayerID, medalType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void WearMedalServerRpc(int playerID, int medalType)
    {
        WearMedalClientRpc(playerID, medalType);
    }

    [ClientRpc]
    private void WearMedalClientRpc(int playerID, int medalType)
    {
        if (playerID != (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            WearMedalOnPlayer(playerID, medalType, true);
        }
    }

    private void WearMedalOnPlayer(int playerID, int medalType, bool playChangeSFX = false)
    {
        Logger.LogDebug($"received medal of type: {medalType} on player at index: [{playerID}]");
        if (playerID < 0 || playerID >= medalsWornBy.Length)
        {
            Logger.LogError($"playerID [{playerID}], not executing WearMedalOnPlayer()!!");
            return;
        }
        if (medalType < 0 || medalType >= medalPrefabs.Length)
        {
            Logger.LogError($"medalType [{playerID}], not executing WearMedalOnPlayer()!!");
            return;
        }
        medalsWornBy[playerID] = medalType;
        PlayerControllerB onPlayer = StartOfRound.Instance.allPlayerScripts[playerID];
        Transform medalContainerOnPlayer = onPlayer.playerBadgeMesh.gameObject.transform.Find($"LCGoldScrapModMedalContainer(Clone){playerID}");
        if (medalContainerOnPlayer == null)
        {
            Logger.LogWarning($"did not find medalContainer on player [{playerID}]");
            return;
        }
        if (medalContainerOnPlayer.childCount != 0)
        {
            for (int i = 0; i < medalContainerOnPlayer.childCount; i++)
            {
                Logger.LogDebug($"already child in medal container {medalContainerOnPlayer.gameObject.name}, destroying it");
                Destroy(medalContainerOnPlayer.GetChild(i).gameObject);
            }
        }
        GameObject medalToInstantiate = medalPrefabs[medalType];
        GameObject spawnedMedal = null;
        if (medalToInstantiate != null)
        {
            spawnedMedal = Instantiate(medalToInstantiate, medalContainerOnPlayer, false);
        }
        if (onPlayer == GameNetworkManager.Instance.localPlayerController && spawnedMedal != null)
        {
            spawnedMedal.GetComponent<MeshRenderer>().enabled = false;
        }
        if (playChangeSFX)
        {
            onPlayer.movementAudio.PlayOneShot(StartOfRound.Instance.changeSuitSFX);
        }
    }

    public void ListenToNewUnlockable(int newUnlockableID)
    {
        if (newUnlockableID == goldMedalID)
        {
            SetMedalEnabled(1);
        }
        /*else if (newUnlockableID == silverMedalID)
        {
            SetMedalEnabled(2);
        }
        else if (newUnlockableID == bronzeMedalID)
        {
            SetMedalEnabled(3);
        }*/
    }

    private void SetMedalEnabled(int medalType, bool setTo = true)
    {
        if (medalType < 0 || medalType >= medalContainers.Length)
        {
            Logger.LogWarning($"SetMedalEnabled(): medalType {medalType}");
            return;
        }
        GameObject medal = medalContainers[medalType];
        if (medal == null)
        {
            return;
        }
        Logger.LogDebug($"setting {medal.name} Medal enabled to {setTo}");
        if (setTo)
        {
            Instantiate(medalPrefabs[medalType], medal.transform, false);
        }
        else if (medal.transform.childCount > 0)
        {
            Transform instantiatedMedal = medal.transform.GetChild(0);
            if (instantiatedMedal != null)
            {
                Destroy(instantiatedMedal.gameObject);
            }
        }
        medal.GetComponent<BoxCollider>().enabled = setTo;
        medal.GetComponent<InteractTrigger>().interactable = setTo;
        if (setTo)
        {
            SetFrameDisabled(false);   
        }
    }

    private void SetFrameDisabled(bool setTo = true)
    {
        shipPlacement.disableObject = setTo;
    }

    public void ResetAllMedals()
    {
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            WearMedalOnPlayer(i, 0);
        }
        SetFrameDisabled();
        for (int j = 0; j < medalContainers.Length; j++)
        {
            SetMedalEnabled(j, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int joiningPlayerID)
    {
        SyncUponJoinClientRpc(joiningPlayerID, medalsWornBy);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(int joiningPlayerID, int[] hostMedalsWornBy)
    {
        WearMedalOnPlayer(joiningPlayerID, 0);
        if (joiningPlayerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            medalsWornBy = hostMedalsWornBy;
            for (int i = 0; i < medalsWornBy.Length; i++)
            {
                if (medalsWornBy[i] == 1)
                {
                    WearMedalOnPlayer(i, 1);
                }
            }
        }
    }
}
