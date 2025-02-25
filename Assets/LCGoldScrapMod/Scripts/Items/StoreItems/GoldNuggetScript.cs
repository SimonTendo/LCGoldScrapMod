using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class GoldNuggetScript : GrabbableObject
{
    public enum ItemTypes
    {
        GoldNugget,
        GoldOre,
        CreditsCard
    }

    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("General")]
    public ItemTypes itemType;

    [Space(3f)]
    [Header("Gold Nugget")]
    public AnimationCurve nuggetMaxValueOverTime;
    public AnimationCurve nuggetMinValueOverTime;
    public AnimationCurve nuggetChanceOverTime;
    public AnimationCurve nuggetMultiplierOverTime;
    private static bool nuggetFreeBonusReceived;

    [Space(3f)]
    [Header("Gold Ore")]
    public int oreStartingValue;
    public int oreMinIncrease;
    public int oreMaxIncrease;
    public int oreBonusPerQuotaMaxIncrease;
    public int oreQuotasWithoutBonus;
    private int orePreviousIncrease;

    //General
    public override void Start()
    {
        base.Start();
        if (IsServer)
        {
            switch (itemType)
            {
                case ItemTypes.GoldNugget:
                    StartGoldNugget();
                    return;
                case ItemTypes.GoldOre:
                    StartGoldOre();
                    return;
                case ItemTypes.CreditsCard:
                    StartCreditsCard();
                    return;
            }
        }
    }

    private void LogNewValue()
    {
        Logger.LogDebug($"set value of {gameObject.name} #{NetworkObjectId} on local client, value: {scrapValue}");
    }



    //Gold Nugget
    private void StartGoldNugget()
    {
        if (!isInShipRoom)
        {
            RollForNuggetMultiplier();
        }
    }

    private void RollForNuggetMultiplier()
    {
        int daysLeft = TimeOfDay.Instance.daysUntilDeadline;
        
        int rolledValue = Random.Range((int)nuggetMinValueOverTime.Evaluate(daysLeft), (int)nuggetMaxValueOverTime.Evaluate(daysLeft));
        Logger.LogDebug($"goldnugget #{NetworkObjectId} spawned, initial value: {rolledValue}");

        if (daysLeft > 3 || daysLeft < 0)
        {
            Logger.LogWarning("non-vanilla days until deadline detected, gold nugget does not get multiplier");
        }
        else
        {
            int randomNr = Random.Range(1, 101);
            int chanceToMultiply = (int)nuggetChanceOverTime.Evaluate(daysLeft);
            if (RarityManager.CurrentlyGoldFever())
            {
                chanceToMultiply *= RarityManager.instance.GetMultiplierByWeather(StartOfRound.Instance.currentLevelID);
            }

            if (!nuggetFreeBonusReceived || randomNr <= chanceToMultiply)
            {
                rolledValue *= (int)nuggetMultiplierOverTime.Evaluate(daysLeft);
                Logger.LogDebug($"GOLDNUGGET DID YES GET MULTIPLIER, VALUE: {rolledValue}");
                nuggetFreeBonusReceived = true;
            }
            else
            {
                Logger.LogDebug($"goldnugget #{NetworkObjectId} did not get multiplier, value: {rolledValue}");
            }
        }
        SetGoldNuggetValueClientRpc(rolledValue);
    }

    [ClientRpc]
    public void SetGoldNuggetValueClientRpc(int itemValue)
    {
        SetScrapValue(itemValue);
        LogNewValue();
    }



    //Gold Ore
    private void StartGoldOre()
    {
        orePreviousIncrease = oreMaxIncrease;
        if (!isInShipRoom)
        {
            if (RarityManager.CurrentlyGoldFever())
            {
                orePreviousIncrease = oreMinIncrease;
                oreStartingValue = 100 * RarityManager.instance.GetMultiplierByWeather(StartOfRound.Instance.currentLevelID);
            }
            SetGoldOreStartClientRpc(oreStartingValue);
        }
    }

    [ClientRpc]
    private void SetGoldOreStartClientRpc(int startingValue)
    {
        SetScrapValue(startingValue);
        LogNewValue();
    }

    public void IncreaseGoldOre()
    {
        if (itemType == ItemTypes.GoldOre && StartOfRound.Instance.currentLevel.planetHasTime)
        {
            int newOreIncrease = -1;
            int attempts = 0;
            while (attempts < 10 && (newOreIncrease == -1 || NewIncreaseInSameQuadrant(newOreIncrease)))
            {
                newOreIncrease = GetNewOreIncrease();
                attempts++;
            }
            int bonusQuotas = TimeOfDay.Instance.timesFulfilledQuota + 1 - oreQuotasWithoutBonus;
            if (bonusQuotas > 0)
            {
                int oreBonus = Random.Range(bonusQuotas, oreBonusPerQuotaMaxIncrease * bonusQuotas);
                Logger.LogDebug($"#{NetworkObjectId}: Bonus for quotas ({bonusQuotas}) rolled {oreBonus}");
                newOreIncrease += oreBonus;
            }
            SetGoldOreIncreaseClientRpc(scrapValue + newOreIncrease);
            orePreviousIncrease = newOreIncrease;
        }
    }

    private int GetNewOreIncrease()
    {
        return Random.Range(oreMinIncrease, oreMaxIncrease);
    }

    private bool NewIncreaseInSameQuadrant(int newIncrease)
    {
        int topQuadrantPrevious = oreMaxIncrease;
        int bottomQuadrantPrevious = oreMaxIncrease - 25;
        for (int i = 0; i <= oreMaxIncrease; i += 25)
        {
            if (i >= orePreviousIncrease)
            {
                topQuadrantPrevious = i;
                bottomQuadrantPrevious = i - 25;
                break;
            }
        }
        bool sameQuadrant = newIncrease > bottomQuadrantPrevious && newIncrease <= topQuadrantPrevious;
        Logger.LogDebug($"#{NetworkObjectId}: NewIncreaseInSameQuadrant({newIncrease}) = {sameQuadrant}");
        return sameQuadrant;
    }

    [ClientRpc]
    private void SetGoldOreIncreaseClientRpc(int newValue)
    {
        SetScrapValue(newValue);
        LogNewValue();
    }



    //Credits Card
    private void StartCreditsCard()
    {
        if (!isInShipRoom)
        {
            int currentSave = GameNetworkManager.Instance.saveFileNum;
            SetCreditsCardStartClientRpc(CreditsCardManager.GetCurrentSaveCardValue(currentSave));
            if (currentSave < 0 || currentSave >= CreditsCardManager.nextCardValue.Length)
            {
                Logger.LogDebug($"could not load nextCardValue[{currentSave}], resetting all values");
                for (int i = 0; i < CreditsCardManager.previousCredits.Length; i++)
                {
                    CreditsCardManager.previousCredits[i] = 0;
                }
                CreditsCardManager.tempCardValue = 0;
                for (int j = 0; j < CreditsCardManager.nextCardValue.Length; j++)
                {
                    CreditsCardManager.nextCardValue[j] = 0;
                }
            }
            else
            {
                CreditsCardManager.previousCredits[currentSave] = 0;
                CreditsCardManager.nextCardValue[currentSave] = 0;
                Logger.LogDebug($"reset [{currentSave}] to: previous = {CreditsCardManager.previousCredits[currentSave]} && next = {CreditsCardManager.nextCardValue[currentSave]}");
            }
        }
    }

    [ClientRpc]
    private void SetCreditsCardStartClientRpc(int hostCost)
    {
        SetScrapValue(hostCost);
        LogNewValue();
    }
}
