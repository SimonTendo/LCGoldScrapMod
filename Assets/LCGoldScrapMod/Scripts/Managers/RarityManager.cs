using System;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using System.Collections;

public class RarityManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static RarityManager instance;

    [Header("Gold Fever Weather")]
    [Header("Initial chance")]
    public int defaultDaysBetweenFevers;
    public int[] chanceOfXDaysBetweenFevers;
    private int daysUntilNextFever = -1;
    private bool hadFeverYesterday;
    public static bool hadFreeFirstFever = false;

    [Space(3f)]
    [Header("Weather multipliers")]
    public int multiplierNormal;
    public int multiplierFoggy;
    public int multiplierRainy;
    public int multiplierStormy;
    public int multiplierFlooded;
    public int multiplierEclipsed;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource audio2D;
    public AudioClip bell;
    public AudioClip[] meows;

    public static int selectedLevel = -1;


    private void Awake()
    {
        instance = this;
        if (selectedLevel != -1)
        {
            SetGoldFeverForLevel();
        }
    }

    //Cat O Gold functionality
    public void RollForGoldFever()   
    {
        if (StartOfRound.Instance == null || StartOfRound.Instance.unlockablesList == null || StartOfRound.Instance.unlockablesList.unlockables == null || StoreAndTerminal.catOGoldID == -1 || StoreAndTerminal.catOGoldID >= StartOfRound.Instance.unlockablesList.unlockables.Count)
        {
            Logger.LogDebug("Cat: general error in RollForGoldFever()");
            return;
        }
        if (TimeOfDay.Instance == null || (daysUntilNextFever <= 1 && TimeOfDay.Instance.daysUntilDeadline == 1 && daysUntilNextFever != -1) || (TimeOfDay.Instance.daysUntilDeadline == 0 && TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled > 0f))
        {
            Logger.LogDebug("Cat: deadline/firing, not continuing");
            return;
        }
        UnlockableItem cat = StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID];
        if (!cat.hasBeenUnlockedByPlayer)
        {
            Logger.LogDebug("Cat: not unlocked");
            return;
        }

        if (daysUntilNextFever == -1)
        {
            daysUntilNextFever = RollForDaysToNextFever();
        }
        else if (daysUntilNextFever > 0)
        {
            if (hadFeverYesterday || (cat.inStorage && UnityEngine.Random.Range(0, 2) == 0))
            {
                Logger.LogDebug($"skipping day-countdown: storage = {cat.inStorage} | yesterday = {hadFeverYesterday}");
                hadFeverYesterday = false;
            }
            else
            {
                daysUntilNextFever--;
                Logger.LogDebug($"days left: {daysUntilNextFever}");
            }
        }

        if (!hadFreeFirstFever)
        {
            hadFreeFirstFever = true;
            SucceededGoldFeverCheck();
        }
        else if (daysUntilNextFever <= 0)
        {
            daysUntilNextFever = RollForDaysToNextFever();
            SucceededGoldFeverCheck();
        }
        else
        {
            hadFeverYesterday = false;
        }
    }

    private void SucceededGoldFeverCheck()
    {
        hadFeverYesterday = true;
        int randomLevelID = -1;
        int attempts = 0;
        while (attempts < 10 && (randomLevelID == -1 || !General.DoesMoonHaveGoldScrap(randomLevelID, false)))
        {
            randomLevelID = GetRandomLevelID();
            attempts++;
        }
        Logger.LogDebug($"rolled randomLevelID: {randomLevelID}");
        int multiplier = GetMultiplierByWeather(randomLevelID);
        
        SetGoldFeverForLevel(randomLevelID, multiplier);
    }

    private int GetRandomLevelID()
    {
        return UnityEngine.Random.Range(0, Plugin.suspectedLevelListLength + 1);
    }

    public int GetMultiplierByWeather(int currentLevelID)
    {
        switch (StartOfRound.Instance.levels[currentLevelID].currentWeather)
        {
            case LevelWeatherType.Foggy:
                return multiplierFoggy;
            case LevelWeatherType.Rainy:
                return multiplierRainy;
            case LevelWeatherType.Stormy:
                return multiplierStormy;
            case LevelWeatherType.Flooded:
                return multiplierFlooded;
            case LevelWeatherType.Eclipsed:
                return multiplierEclipsed;
            default:
                return multiplierNormal;
        }
    }

    public static bool CurrentlyGoldFever()
    {
        return selectedLevel == StartOfRound.Instance.currentLevelID;
    }

    private int RollForDaysToNextFever()
    {
        int rolledDays = defaultDaysBetweenFevers;
        for (int i = chanceOfXDaysBetweenFevers.Length - 1; i >= 0; i--)
        {
            int randomNr = UnityEngine.Random.Range(1, 101);
            if (randomNr <= chanceOfXDaysBetweenFevers[i])
            {
                rolledDays = i;
                break;
            }
        }
        Logger.LogDebug($"rolledDays: {rolledDays}");
        return rolledDays;
    }

    public void SetGoldFeverForLevel(int feverLevelID = -1, int multiplier = -1)
    {
        if (feverLevelID != -1)
        {
            Logger.LogDebug($"GoldFever! feverLevelID: {feverLevelID} | multiplier: {multiplier}");
            SetRarityForLevel(feverLevelID, multiplier);
            selectedLevel = feverLevelID;
        }
        else
        {
            if (selectedLevel != -1)
            {
                Logger.LogDebug($"Reset selectedLevel {selectedLevel}!");
                SetRarityForLevel(selectedLevel);
            }
            else
            {
                Logger.LogWarning($"WARNING!! RarityManager called to SetGoldFeverForLevel with feverLevelID {feverLevelID} && selectedLevel {selectedLevel}; resetting ALL LEVEL'S rarities!!");
                for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
                {
                    SetRarityForLevel(i);
                }
            }
            selectedLevel = -1;
        }
        SyncSelectedLevelClientRpc(selectedLevel);
    }

    [ClientRpc]
    public void SyncSelectedLevelClientRpc(int hostSelectedLevelID, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            selectedLevel = hostSelectedLevelID;
            Logger.LogDebug($"goldFeverID: {selectedLevel}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncSelectedLevelClientRpc(selectedLevel, playerID);
    }

    public void PlayGoldFeverSFX()
    {
        int randomNr = UnityEngine.Random.Range(0, meows.Length);
        float randomDelay = UnityEngine.Random.Range(0.1f, 0.75f);
        DoGoldFeverSFXClientRpc(randomNr, randomDelay);
    }

    [ClientRpc]
    private void DoGoldFeverSFXClientRpc(int randomNr, float randomDelay)
    {
        StartCoroutine(PlayBellAndMeow(randomNr, randomDelay));
    }

    private IEnumerator PlayBellAndMeow(int randomNr, float randomDelay)
    {
        audio2D.PlayOneShot(bell);
        yield return new WaitForSeconds(randomDelay);
        if (!StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID].inStorage)
        {
            audio2D.PlayOneShot(meows[randomNr]);
        }
    }



    //Reusable runtime rarity changes
    public static void SetRarityForLevel(int levelID, int multiplier = -1)
    {
        if (levelID < 0 || levelID >= StartOfRound.Instance.levels.Length)
        {
            return;
        }
        SelectableLevel level = StartOfRound.Instance.levels[levelID];
        for (int i = 0; i < level.spawnableScrap.Count; i++)
        {
            SpawnableItemWithRarity scrapData = level.spawnableScrap[i];
            for (int j = 0; j < Plugin.allGoldScrap.allItemData.Length; j++)
            {
                ItemData itemData = Plugin.allGoldScrap.allItemData[j];
                if (scrapData.spawnableItem == itemData.itemProperties)
                {
                    if (multiplier != -1)
                    {
                        scrapData.rarity *= multiplier;
                    }
                    else
                    {
                        scrapData.rarity = (int)CalculateDefaultRarityWithConfig(GetThisLevelsDefaultRarity(itemData, levelID));
                    }
                    Logger.LogDebug($"set ItemWithRarity for item {scrapData.spawnableItem.itemName} on level '{level.name}' with ID [{levelID}] to rarity {scrapData.rarity}");
                }
            }
        }
    }

    public static float CalculateDefaultRarityWithConfig(int rarityToMultiply)
    {
        return rarityToMultiply / 3f * Config.rarityMultiplier.Value;
    }

    private static int GetThisLevelsDefaultRarity(ItemData thisItem, int levelID)
    {
        if (levelID <= Plugin.suspectedLevelListLength)
        {
            string thisLevel = StartOfRound.Instance.levels[levelID].name;
            string levelName = thisLevel.Remove(thisLevel.Length - 5, 5);
            foreach (GoldScrapLevels levelToAddMinus in thisItem.levelsToAddMinus)
            {
                if (levelToAddMinus.ToString().Equals(levelName))
                {
                    return thisItem.defaultRarity - 1;
                }
            }
            foreach (GoldScrapLevels levelToAddDefault in thisItem.levelsToAddDefault)
            {
                if (levelToAddDefault.ToString().Equals(levelName))
                {
                    return thisItem.defaultRarity;
                }
            }
            foreach (GoldScrapLevels levelToAddPlus in thisItem.levelsToAddPlus)
            {
                if (levelToAddPlus.ToString().Equals(levelName))
                {
                    return thisItem.defaultRarity + 2;
                }
            }
            foreach (GoldScrapLevels levelToAddCustom in thisItem.levelsToAddCustom)
            {
                if (levelToAddCustom.ToString().Equals(levelName))
                {
                    return thisItem.defaultRarity + thisItem.customChange;
                }
            }
        }
        Logger.LogError($"RarityManager did not find level with ID {levelID} for item {thisItem.name}");
        return 1;
    }
}
