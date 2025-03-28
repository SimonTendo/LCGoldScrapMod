using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GoldScrapNetworkHandler : NetworkBehaviour
{
    public static GoldScrapNetworkHandler instance;

    void Awake()
    {
        instance = this;
    }

    [ClientRpc]
    public void LowerRoundManagerScrapCollectedClientRpc(int valueToLower)
    {
        Plugin.Logger.LogDebug($"Got ClientRpc to lower scrapCollected with {valueToLower}");
        RoundManager.Instance.scrapCollectedInLevel -= valueToLower;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendConfigsToNewPlayerServerRpc(int playerID)
    {
        Plugin.Logger.LogDebug($"Received server call to send CONFIG to new client at index: [{playerID}]");
        SetHostConfigsClientRpc(playerID, Configs.weightMultiplier.Value, Configs.toolsRebalance.Value, Configs.priceMultiplier.Value, Plugin.localDateCase);
    }

    [ClientRpc]
    private void SetHostConfigsClientRpc(int playerID, float multiplierWeight, bool toolsRebalance, float multiplierPrice, int dateCaseHost)
    {
        PlayerControllerB receivedPlayer = StartOfRound.Instance.allPlayerScripts[playerID];
        if (receivedPlayer == GameNetworkManager.Instance.localPlayerController)
        {
            StartCoroutine(SyncConfigsOnDelay(toolsRebalance, multiplierWeight, multiplierPrice, dateCaseHost));
        }
    }

    private IEnumerator SyncConfigsOnDelay(bool toolsRebalance, float multiplierWeight, float multiplierPrice, int dateCaseHost)
    {
        yield return new WaitForSeconds(1f);
        RuntimeChanges.SyncHostConfigs(toolsRebalance, multiplierWeight, multiplierPrice, dateCaseHost);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendUnlockablesToNewPlayerServerRpc(int playerID)
    {
        Plugin.Logger.LogDebug($"Received server call to send UNLOCKABLES to new client at index: [{playerID}]");
        List<int> unlockableIDs = new List<int>();
        List<bool> unlockableStates = new List<bool>();
        foreach (int unlockableID in StoreAndTerminal.allShipUnlockableIDs)
        {
            if (unlockableID < 0 || unlockableID >= StartOfRound.Instance.unlockablesList.unlockables.Count)
            {
                Plugin.Logger.LogWarning($"StoreAndTerminal.allShipUnlockableIDs contained invalid ID: {unlockableID} | Cannot sync this unlockable!");
                continue;
            }
            if (StartOfRound.Instance.unlockablesList.unlockables[unlockableID].unlockableType == -1)
            {
                unlockableIDs.Add(unlockableID);
                unlockableStates.Add(StartOfRound.Instance.unlockablesList.unlockables[unlockableID].hasBeenUnlockedByPlayer);
            }
        }
        SetHostUnlockableClientRpc(playerID, unlockableIDs.ToArray(), unlockableStates.ToArray());
    }

    [ClientRpc]
    private void SetHostUnlockableClientRpc(int playerID, int[] hostUnlockableIDs, bool[] hostUnlockableStates)
    {
        PlayerControllerB receivedPlayer = StartOfRound.Instance.allPlayerScripts[playerID];
        if (receivedPlayer == GameNetworkManager.Instance.localPlayerController)
        {
            for (int i = 0; i < hostUnlockableIDs.Length; i++)
            {
                int unlockableID = hostUnlockableIDs[i];
                UnlockableItem foundUnlockable = StartOfRound.Instance.unlockablesList.unlockables[unlockableID];
                foundUnlockable.hasBeenUnlockedByPlayer = hostUnlockableStates[i];
                Plugin.Logger.LogDebug($"host set unlockable #{unlockableID} '{foundUnlockable.unlockableName}' to {foundUnlockable.hasBeenUnlockedByPlayer}");
            }
            General.SetDiscoBallOnJoin();
            General.SetMedalsOnJoin();
        }
    }
}
