using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using HarmonyLib;

public class RarityManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static RarityManager instance;

    [Header("Gold Fever Weather")]
    [Header("Initial chance")]
    public int defaultDaysBetweenFevers;
    public int[] chanceOfXDaysBetweenFevers;
    public int daysUntilNextFever = -1;

    [Space(3f)]
    [Header("Weather multipliers")]
    public int multiplierNormal;
    public int multiplierFoggy;
    public int multiplierRainy;
    public int multiplierStormy;
    public int multiplierFlooded;
    public int multiplierEclipsed;

    [Space(3f)]
    [Header("Sale")]
    public int minSalesPercentage;
    public int maxSalesPercentage;
    public int[] tempSalesPercentages = null;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource audio2D;
    public AudioClip bell;
    public AudioClip[] meows;

    public static int selectedLevel = -1;
    public static int[] allItemPricePercentages;
    public static bool isSaleFever;

    private void Awake()
    {
        instance = this;
        allItemPricePercentages = new int[StoreAndTerminal.allGoldStoreItemData.Length];
        SetGoldFeverSales(true);
        CheckToResetFever();
    }

    private void CheckToResetFever()
    {
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
            CheckToResetFever();
            Logger.LogDebug("Cat: general error in RollForGoldFever()");
            return;
        }
        if (TimeOfDay.Instance == null || (TimeOfDay.Instance.daysUntilDeadline == 0 && TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled > 0f))
        {
            CheckToResetFever();
            Logger.LogDebug("Cat: firing, not continuing");
            return;
        }
        UnlockableItem cat = StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID];
        if (!cat.hasBeenUnlockedByPlayer)
        {
            CheckToResetFever();
            Logger.LogDebug("Cat: not unlocked");
            return;
        }

        if (daysUntilNextFever == -1)
        {
            daysUntilNextFever = RollForDaysToNextFever();
        }
        else if (daysUntilNextFever > 0)
        {
            if (cat.inStorage && Random.Range(0, 2) == 0)
            {
                Logger.LogDebug($"skipping day-countdown: storage = {cat.inStorage}");
            }
            else
            {
                daysUntilNextFever--;
                Logger.LogDebug($"days left: {daysUntilNextFever}");
            }
        }
        SyncDaysToFeverClientRpc(daysUntilNextFever); 
        if (daysUntilNextFever <= 0)
        {
            SucceededGoldFeverCheck();
        }
    }

    private void SucceededGoldFeverCheck()
    {
        int levelID = General.GetRandomLevelID();
        if (daysUntilNextFever <= 1 && TimeOfDay.Instance.daysUntilDeadline == 1 && daysUntilNextFever != -1)
        {
            for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
            {
                if (StartOfRound.Instance.levels[i].name == "CompanyBuildingLevel")
                {
                    levelID = StartOfRound.Instance.levels[i].levelID;
                    break;
                }
            }
            SetGoldFeverForLevel(levelID, -1, true);
        }
        else if (!General.DoesMoonHaveGoldScrap(levelID, false))
        {
            Logger.LogDebug($"SALES: rolled randomLevelID: {levelID}");
            SetGoldFeverForLevel(levelID, -1, true);
        }
        else
        {
            Logger.LogDebug($"SCRAP: rolled randomLevelID: {levelID}");
            SetGoldFeverForLevel(levelID, GetMultiplierByWeather(levelID));
        }
        daysUntilNextFever = RollForDaysToNextFever();
    }

    public int GetMultiplierByWeather(int currentLevelID = -1)
    {
        if (currentLevelID == -1)
        {
            currentLevelID = StartOfRound.Instance.currentLevelID;
        }
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

    public static bool CurrentlyGoldFever(bool saleFever = false)
    {
        return selectedLevel == StartOfRound.Instance.currentLevelID && saleFever == isSaleFever;
    }

    private int RollForDaysToNextFever()
    {
        int rolledDays = defaultDaysBetweenFevers;
        for (int i = chanceOfXDaysBetweenFevers.Length - 1; i >= 0; i--)
        {
            int randomNr = Random.Range(1, 101);
            if (randomNr <= chanceOfXDaysBetweenFevers[i])
            {
                rolledDays = i;
                break;
            }
        }
        Logger.LogDebug($"rolledDays: {rolledDays}");
        return rolledDays;
    }

    public void SetGoldFeverSales(bool resetSales = false)
    {
        isSaleFever = !resetSales;
        Terminal terminalScript = FindAnyObjectByType<Terminal>();
        if (resetSales)
        {
            tempSalesPercentages = null;
            for (int i = 0; i < allItemPricePercentages.Length; i++)
            {
                allItemPricePercentages[i] = 100;
            } 
        }
        else
        {
            tempSalesPercentages = new int[allItemPricePercentages.Length];
            for (int i = 0; i < tempSalesPercentages.Length; i++)
            {
                ItemData itemData = StoreAndTerminal.allGoldStoreItemData[i];
                if (itemData.maxFeverSalePercentage == -1)
                {
                    tempSalesPercentages[i] = 100 - GetMultiplierByWeather(selectedLevel) * 10;
                }
                else
                {
                    int randomSalePercentage = Random.Range((int)((float)Mathf.Min(minSalesPercentage, itemData.maxFeverSalePercentage) / 10f), (int)((float)Mathf.Min(maxSalesPercentage, itemData.maxFeverSalePercentage) / 10f) + 1);
                    tempSalesPercentages[i] = 100 - randomSalePercentage * 10;
                }
                Logger.LogDebug($"[{i}] ({itemData.folderName}): sale = {tempSalesPercentages[i]}");
            }
        }
    }

    public void SetGoldFeverForLevel(int feverLevelID = -1, int multiplier = -1, bool salesFever = false)
    {
        if (!IsServer)
        {
            return;
        }
        if (feverLevelID != -1)
        {
            selectedLevel = feverLevelID;
            Logger.LogDebug($"GoldFever! feverLevelID: {feverLevelID} | multiplier: {multiplier}");
            if (salesFever)
            {
                SetGoldFeverSales();
            }
            else
            {
                SetRarityForLevel(feverLevelID, multiplier);
            }
        }
        else
        {
            SetGoldFeverSales(true);
            SyncFeverSalesClientRpc();
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
        SyncSelectedLevelClientRpc(selectedLevel, salesFever);
    }

    [ClientRpc]
    public void SyncSelectedLevelClientRpc(int hostSelectedLevelID, bool isSale, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            selectedLevel = hostSelectedLevelID;
            isSaleFever = isSale;
            Logger.LogDebug($"goldFeverID: {selectedLevel} | isSaleFever: {isSaleFever}");
        }
    }

    [ClientRpc]
    public void SyncFeverSalesClientRpc(int[] hostSalesPercentages = null, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            StartCoroutine(SyncFeverSalesOnDelay(hostSalesPercentages));   
        }
    }

    private IEnumerator SyncFeverSalesOnDelay(int[] hostSalesPercentages)
    {
        yield return new WaitUntil(() => Plugin.appliedHostConfigs);
        SyncFeverSales(hostSalesPercentages);
    }

    private void SyncFeverSales(int[] hostSalesPercentages)
    {
        Terminal terminalScript = FindAnyObjectByType<Terminal>();
        bool isSale = hostSalesPercentages != null;
        isSaleFever = isSale;
        if (isSale)
        {
            allItemPricePercentages = hostSalesPercentages;
            CreditsCardManager.instance.StartSaleReroll(true);
        }
        tempSalesPercentages = null;
        for (int i = 0; i < allItemPricePercentages.Length; i++)
        {
            ItemData itemData = StoreAndTerminal.allGoldStoreItemData[i];
            if (itemData.storeDefaultPrice == -1)
            {
                continue;
            }
            if (!isSale)
            {
                allItemPricePercentages[i] = 100;
            }
            Logger.LogDebug($"SALE: ('{itemData.folderName}' [{i}]) = {allItemPricePercentages[i]}%");
            if (itemData.itemProperties != null && terminalScript != null)
            {
                terminalScript.itemSalesPercentages[itemData.localBuyItemIndex] = allItemPricePercentages[i];
            }
            if (itemData.storeTerminalNodes != null && itemData.storeTerminalNodes.Length > 0)
            {
                for (int j = 0; j < itemData.storeTerminalNodes.Length; j++)
                {
                    if (itemData.storeTerminalNodes[j] == null)
                    {
                        continue;
                    }
                    float multiplier = (float)(allItemPricePercentages[i] / 100f);
                    itemData.storeTerminalNodes[j].itemCost = (int)(itemData.localStorePrice * multiplier);
                }
            }
        }
    }

    [ClientRpc]
    private void SyncDaysToFeverClientRpc(int hostDaysUntilFever, int playerID = -1)
    {
        if (CatOGoldScript.Instance != null && (playerID == -1 || playerID == (int)GameNetworkManager.Instance.localPlayerController.playerClientId))
        {
            CatOGoldScript.Instance.OnDayChange(hostDaysUntilFever);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncSelectedLevelClientRpc(selectedLevel, isSaleFever, playerID);
        int daysToSend = selectedLevel != -1 ? 0 : daysUntilNextFever;
        SyncDaysToFeverClientRpc(daysToSend, playerID);
    }

    public void PlayGoldFeverSFX()
    {
        int randomNr = Random.Range(0, meows.Length);
        float randomDelay = Random.Range(0.1f, 0.75f);
        DoGoldFeverSFXClientRpc(randomNr, randomDelay);
        if (tempSalesPercentages != null && CurrentlyGoldFever(true))
        {
            SyncFeverSalesClientRpc(tempSalesPercentages);
        }
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
    public static void SetRarityForLevel(int levelID, int multiplier = -1, bool printDebug = true)
    {
        if (levelID < 0 || levelID >= StartOfRound.Instance.levels.Length)
        {
            return;
        }
        SelectableLevel level = StartOfRound.Instance.levels[levelID];
        if (level == null || level.spawnableScrap == null || level.spawnableScrap.Count < 1)
        {
            return;
        }
        for (int i = 0; i < level.spawnableScrap.Count; i++)
        {
            SpawnableItemWithRarity scrapData = level.spawnableScrap[i];
            if (scrapData == null || scrapData.spawnableItem == null)
            {
                continue;
            }
            for (int j = 0; j < Plugin.allGoldGrabbableObjects.Length; j++)
            {
                ItemData itemData = Plugin.allGoldGrabbableObjects[j];
                if (itemData == null || itemData.itemProperties == null)
                {
                    continue;
                }
                if (scrapData.spawnableItem == itemData.itemProperties)
                {
                    if (multiplier != -1)
                    {
                        scrapData.rarity *= multiplier;
                    }
                    else
                    {
                        scrapData.rarity = CalculateRarity(itemData, level);
                    }
                    if (printDebug) Logger.LogDebug($"set ItemWithRarity for item {scrapData.spawnableItem.itemName} on level '{level.name}' with ID [{levelID}] to rarity {scrapData.rarity}");
                }
            }
        }
    }

    public static int CalculateRarity(ItemData itemData, SelectableLevel forLevel)
    {
        if (itemData == null || forLevel == null)
        {
            return 1;
        }
        float rarity = 1f;
        float difference = 0f;
        if (forLevel.levelID <= Plugin.suspectedLevelListLength)
        {
            rarity = CalculateDefaultRarityWithConfig(itemData.defaultRarity);
            difference = CalculateRarityDifferenceModifier(GetThisLevelsDifferenceModifier(itemData, forLevel.levelID), itemData);
        }
        else
        {
            rarity = Configs.moddedMoonRarity.Value;
        }
        float dateCaseMultiplier = CalculateDateCaseMultiplier(itemData);
        int toReturn = (int)((rarity + difference) * dateCaseMultiplier);
        return Mathf.Max(1, toReturn);
    }

    public static float CalculateDefaultRarityWithConfig(int rarityToMultiply)
    {
        return rarityToMultiply / 3f * Configs.rarityMultiplier.Value;
    }

    public static float CalculateRarityDifferenceModifier(int minusPlusOrCustom, ItemData itemData = null)
    {
        switch (minusPlusOrCustom)
        {
            default:
                return 0f;
            case 0:
                return -0.34f / Configs.rarityMultiplier.Value;
            case 1:
                return 0.67f * Configs.rarityMultiplier.Value;
            case 2:
                if (itemData == null)
                {
                    return 0f;
                }
                return itemData.customChange > 0 ? itemData.customChange / 3f * Configs.rarityMultiplier.Value : itemData.customChange * 3f / Configs.rarityMultiplier.Value;
        }
    }

    private static int GetThisLevelsDifferenceModifier(ItemData item, int levelID)
    {
        if (item == null || levelID < 0 || levelID > Plugin.suspectedLevelListLength)
        {
            return -1;
        }
        string thisLevel = StartOfRound.Instance.levels[levelID].name;
        string levelName = thisLevel.Remove(thisLevel.Length - 5, 5);
        foreach (GoldScrapLevels levelToAddMinus in item.levelsToAddMinus)
        {
            if (levelToAddMinus.ToString().Equals(levelName))
            {
                return 0;
            }
        }
        foreach (GoldScrapLevels levelToAddPlus in item.levelsToAddPlus)
        {
            if (levelToAddPlus.ToString().Equals(levelName))
            {
                return 1;
            }
        }
        foreach (GoldScrapLevels levelToAddCustom in item.levelsToAddCustom)
        {
            if (levelToAddCustom.ToString().Equals(levelName))
            {
                return 2;
            }
        }
        return -1;
    }

    private static float CalculateDateCaseMultiplier(ItemData itemData)
    {
        switch (Plugin.specialDateCase)
        {
            default:
                return 1.0f;
            case 0:
                return General.ItemHasMatchingDateCase(itemData) ? 7.7f : 0.9f;
            case 1:
                return 1.2f;
            case 2:
                return General.ItemHasMatchingDateCase(itemData) ? 2.0f : 1.0f;
            case 3:
                return General.ItemHasMatchingDateCase(itemData) ? 0.1f : 1.0f;
            case 4:
                return 1.5f;
            case 5:
                return General.ItemHasMatchingDateCase(itemData) ? 3.3f : 1.0f;
        }
    }



    //HarmonyPatch to spawn one free gold scrap upon a Gold Fever
    [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
    public class NewRoundManagerSpawnScrap
    {
        [HarmonyPostfix]
        public static void SpawnScrapInLevelPostfix()
        {
            if (instance == null || !instance.IsServer)
            {
                return;
            }
            if (CurrentlyGoldFever() && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
            {
                Logger.LogDebug("RoundManager patch detected Gold Fever, spawning gold scrap!");
                instance.StartSpawnFreeGold();
            }
        }
    }

    public void StartSpawnFreeGold()
    {
        StartCoroutine(SpawnFreeGold());
    }

    private IEnumerator SpawnFreeGold()
    {
        yield return new WaitForEndOfFrame();
        GameObject[] allAINodes = GameObject.FindGameObjectsWithTag("AINode");
        if (allAINodes == null || allAINodes.Length == 0)
        {
            Logger.LogWarning("SpawnFreeGold did not find any AINodes, returning!");
            yield break;
        }
        float[] allDistances = new float[allAINodes.Length];
        EntranceTeleport[] allEntrances = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.None);
        int indexToUse = -1;
        float highestDistanceSoFar = 0.0f;
        for (int n = 0; n < allAINodes.Length; n++)
        {
            Vector3 thisNode = allAINodes[n].transform.position;
            float distanceFromEntrances = 0.0f;
            for (int e = 0; e < allEntrances.Length; e++)
            {
                EntranceTeleport entrance = allEntrances[e];
                if (entrance.isEntranceToBuilding)
                {
                    continue;
                }
               distanceFromEntrances += Vector3.Distance(thisNode, entrance.transform.position);
            }
            allDistances[n] = distanceFromEntrances;
            if (distanceFromEntrances > highestDistanceSoFar)
            {
                highestDistanceSoFar = distanceFromEntrances;
                indexToUse = n;
            }
            yield return null;
        }
        ItemData itemData = null;
        Item itemToSpawn = null;
        while (itemData == null || !itemData.isScrap || itemData.itemProperties == null)
        {
            itemData = Plugin.allGoldGrabbableObjects[Random.Range(0, Plugin.allGoldGrabbableObjects.Length)];
            Logger.LogDebug($"randomly rolled {itemData.name} (scrap: {itemData.isScrap} | store: {itemData.isStoreItem})");
            yield return null;
        }
        itemToSpawn = itemData.itemProperties;
        Vector3 spawnAt = allAINodes[0].transform.position;
        if (indexToUse >= 0 && indexToUse < allAINodes.Length)
        {
            spawnAt = allAINodes[indexToUse].transform.position + Vector3.up;
            Logger.LogDebug($"successfully picked spawnAt {spawnAt} at node [{indexToUse}] with distance {allDistances[indexToUse]}");
        }
        int itemValue = (int)(Mathf.Max(itemToSpawn.minValue, itemToSpawn.maxValue) * RoundManager.Instance.scrapValueMultiplier * GetMultiplierByWeather());
        Logger.LogDebug($"picked itemValue {itemValue}");
        GameObject spawnedItem = Instantiate(itemToSpawn.spawnPrefab, spawnAt + Vector3.up, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
        NetworkObject netObj = spawnedItem.GetComponent<NetworkObject>();
        netObj.Spawn();
        yield return new WaitForSeconds(15f);
        SpawnFreeGoldScrapClientRpc(netObj, itemValue);
    }

    [ClientRpc]
    public void SpawnFreeGoldScrapClientRpc(NetworkObjectReference itemNOR, int itemValue)
    {
        if (itemNOR.TryGet(out var netObj))
        {
            GrabbableObject item = netObj.GetComponent<GrabbableObject>();
            if (item == null)
            {
                Logger.LogWarning("failed to get ITEM SCRIPT in SpawnFreeGoldScrapClientRpc!!");
                return;
            }
            Logger.LogDebug($"locally received item {item} with value {itemValue}");
            item.SetScrapValue(itemValue);
            RoundManager.Instance.totalScrapValueInLevel += itemValue;
            if (IsServer)
            {
                DLOGManager.instance.SetTextServerRpc();
            }
        }
        else
        {
            Logger.LogError("failed to get NET OBJ in SpawnFreeGoldScrapClientRpc!!");
        }
    }
}
