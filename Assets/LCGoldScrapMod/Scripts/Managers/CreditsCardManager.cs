using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using HarmonyLib;

public class CreditsCardManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static CreditsCardManager instance;

    public ItemData creditsCardItem;
    public TerminalNode creditsCardNode;
    public TerminalNode creditsCardNodeBuy;
    public TerminalNode creditsCardNodeUnavailable;

    public static int[] previousCredits = new int[6];
    public static int[] nextCardValue = new int[3];
    public static int tempCardValue;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(WaitForHostToBePicked());
    }

    private IEnumerator WaitForHostToBePicked()
    {
        yield return new WaitUntil(() => GameNetworkManager.Instance.localPlayerController != null);
        yield return new WaitForSeconds(1f);
        SetBuyingRate();
    }

    public void SetBuyingRate() 
    {
        int saveFile = GameNetworkManager.Instance.saveFileNum;
        if (GameNetworkManager.Instance.localPlayerController.isHostPlayerObject && saveFile >= 0 && saveFile < previousCredits.Length)
        {
            SetBuyingRateClientRpc(previousCredits[saveFile]);
        }
    }

    [ClientRpc]
    private void SetBuyingRateClientRpc(int value, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            if (value < -1) value = -1;
            creditsCardItem.itemProperties.creditsWorth = value;
            Logger.LogDebug($"set buying rate to {creditsCardItem.itemProperties.creditsWorth}");
        }
    }

    public static void SetPreviousCredits()
    {
        Terminal terminalScript = FindObjectOfType<Terminal>();
        int currentSave = GameNetworkManager.Instance.saveFileNum;
        int overtimeIndex = currentSave + 3;
        if (terminalScript == null || currentSave < 0 || currentSave >= previousCredits.Length)
        {
            return;
        }
        int valueToSet = terminalScript.groupCredits;
        Logger.LogDebug($"groupCredits: {valueToSet}");
        if (overtimeIndex >= 0 && overtimeIndex < previousCredits.Length)
        {
            Logger.LogDebug($"overtime: {previousCredits[overtimeIndex]}");
            valueToSet += previousCredits[overtimeIndex];
            previousCredits[overtimeIndex] = 0;
        }
        Logger.LogDebug($"setting previousCredits for file {currentSave} to ${valueToSet}");
        previousCredits[currentSave] = valueToSet;
    }

    public void StartSaleReroll(bool guaranteeSale = false)
    {
        StartCoroutine(RerollSalesPercentage(guaranteeSale));
    }

    private IEnumerator RerollSalesPercentage(bool guaranteeSale)
    {
        yield return new WaitUntil(() => !StartOfRound.Instance.shipIsLeaving);
        if (creditsCardItem.itemProperties.creditsWorth >= 0)
        {
            int price = 100;
            if (guaranteeSale || Random.Range(0, 4) == 0)
            {
                price -= Random.Range(3, creditsCardItem.maxFeverSalePercentage / 10 + 1) * 10;
            }
            SetNewSalesClientRpc(price);
        }
    }

    private static void AccumulateOvertime(int overtimeBonus)
    {
        int currentSave = GameNetworkManager.Instance.saveFileNum;
        if (currentSave < 0 || currentSave + 3 >= previousCredits.Length)
        {
            return;
        }
        previousCredits[currentSave + 3] += overtimeBonus;
        Logger.LogDebug($"CreditsCard added ${overtimeBonus} to file [{currentSave}] for total of ${previousCredits[currentSave + 3]}");
    }

    public static int GetCurrentSaveCardValue(int currentSave)
    {
        if (currentSave < 0 || currentSave >= nextCardValue.Length)
        {
            if (tempCardValue > 0)
            {
                Logger.LogDebug($"CreditsCardManager: invalid SAVE FILE NUM, returning TEMP CARD VALUE");
                int toReturn = tempCardValue;
                tempCardValue = 0;
                return toReturn;
            }
            Terminal terminalScript = FindObjectOfType<Terminal>();
            if (terminalScript != null)
            {
                Logger.LogDebug($"CreditsCardManager: invalid TEMP CARD VALUE, returning LOGS & BESTIARY");
                int logsCollected = terminalScript.unlockedStoryLogs.Count;
                int entitiesScanned = terminalScript.scannedEnemyIDs.Count;
                return logsCollected * 10 + entitiesScanned * 5;
            }
            Logger.LogDebug($"CreditsCardManager: invalid TERMINAL, returning RANDOM");
            return Random.Range(0, 100);
        }
        return nextCardValue[currentSave];
    }

    [ClientRpc]
    public void SetNewSalesClientRpc(int newSalesValue, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            Terminal terminalScript = FindObjectOfType<Terminal>();
            if (terminalScript != null)
            {
                terminalScript.itemSalesPercentages[creditsCardNodeBuy.buyItemIndex] = newSalesValue;
                Logger.LogDebug($"terminal sale: {100 - newSalesValue}%");
                for (int i = 0; i < StoreAndTerminal.allGoldStoreItemData.Length; i++)
                {
                    if (StoreAndTerminal.allGoldStoreItemData[i] == creditsCardItem && i >= 0 && i < RarityManager.allItemPricePercentages.Length)
                    {
                        RarityManager.allItemPricePercentages[i] = newSalesValue;
                        Logger.LogDebug($"gold store sale: {100 - newSalesValue}%");
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeCreditsCardOutOfRotationServerRpc()
    {
        int currentSave = GameNetworkManager.Instance.saveFileNum;
        if (currentSave < 0 || currentSave >= nextCardValue.Length)
        {
            for (int i = 0; i < previousCredits.Length; i++)
            {
                if (previousCredits[i] > 0)
                {
                    tempCardValue = previousCredits[i];
                    previousCredits[i] = 0;
                    break;
                }
            }
        }
        else
        {
            nextCardValue[currentSave] = creditsCardItem.itemProperties.creditsWorth;
        }
        TakeCreditsCardOutOfRotationClientRpc();
    }

    [ClientRpc]
    private void TakeCreditsCardOutOfRotationClientRpc()
    {
        creditsCardItem.itemProperties.creditsWorth = -1;
        Terminal terminalScript = FindObjectOfType<Terminal>();
        if (terminalScript != null)
        {
            terminalScript.itemSalesPercentages[creditsCardNodeBuy.buyItemIndex] = 100;
            Logger.LogDebug($"TakeCreditsCardOutOfRotationClientRpc(): value = {creditsCardItem.itemProperties.creditsWorth} | sale = {100 - terminalScript.itemSalesPercentages[creditsCardNodeBuy.buyItemIndex]}%");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SetBuyingRateClientRpc(creditsCardItem.itemProperties.creditsWorth, playerID);
        Terminal terminalScript = FindObjectOfType<Terminal>();
        if (terminalScript != null)
        {
            SetNewSalesClientRpc(terminalScript.itemSalesPercentages[creditsCardNodeBuy.buyItemIndex], playerID);
        }
    }

    [HarmonyPatch(typeof(HUDManager), "DisplayNewDeadline")]
    public class NewHUDManager
    {
        [HarmonyPostfix]
        public static void DisplayNewDeadlinePostfix(int overtimeBonus)
        {
            if (GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
            {
                Logger.LogDebug($"host got bonus: {overtimeBonus}");
                AccumulateOvertime(Mathf.Max(0, overtimeBonus));
            }
        }
    }
}
