using UnityEngine;
using GameNetcodeStuff;
using BepInEx.Logging;
using static AssetsCollection;
using static RegisterGoldScrap;

public class RuntimeChanges
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static void SetAllDefaultNames()
    {
        foreach (ItemData scrapItem in Plugin.allGoldGrabbableObjects)
        {
            if (scrapItem == null || scrapItem.isStoreItem || scrapItem.itemProperties == null)
            {
                continue;
            }
            scrapItem.itemProperties.itemName = scrapItem.displayName;
        }
        foreach (ItemData storeItem in StoreAndTerminal.allGoldStoreItemData)
        {
            if (storeItem == null || !storeItem.isStoreItem)
            {
                continue;
            }
            if (storeItem.itemProperties != null)
            {
                storeItem.itemProperties.itemName = storeItem.displayName;
            }
            if (storeItem.unlockableProperties != null && !string.IsNullOrEmpty(storeItem.unlockableProperties.unlockableName))
            {
                storeItem.unlockableProperties.unlockableName = storeItem.displayName;
                switch (storeItem.unlockableProperties.unlockableName)
                {
                    case "Gold Suit":
                        storeItem.unlockableProperties.suitMaterial = defaultMaterialGold;
                        break;
                    case "Silver Suit":
                        storeItem.unlockableProperties.suitMaterial = defaultMaterialSilver;
                        break;
                    case "Bronze Suit":
                        storeItem.unlockableProperties.suitMaterial = defaultMaterialBronze;
                        break;
                }
            }
        }
    }

    public static void SyncHostConfigs(bool rebalance, float multiplierWeight, float multiplierPrice, int dateCaseHost)
    {
        Logger.LogInfo($"Applying values of host's specialDateCase. Value: {dateCaseHost}");
        Plugin.specialDateCase = dateCaseHost;

        Logger.LogInfo($"Applying values of host's config [Multiplier Weight]. Value: {multiplierWeight}");
        Configs.hostWeightMultiplier = multiplierWeight;
        foreach (ItemData item in Plugin.allGoldGrabbableObjects)
        {
            if (item == null)
            {
                continue;
            }
            item.itemProperties.weight = Configs.CalculateWeightCustom(item.defaultWeight, multiplierWeight);
            if (item.isScrap)
            {
                Configs.SetCustomGoldScrapValues(item);
            }
        }

        Logger.LogInfo($"Applying values of host's config [Multiplier Price]. Value: {multiplierPrice}");
        Configs.SetCustomGoldStorePrices(multiplierPrice);

        Logger.LogInfo($"Applying values of host's config [Other Tools Balance]. Value: {Configs.hostToolRebalance}");
        Configs.hostToolRebalance = rebalance;

        UpdateGoldScrapStatic();
        UpdateGoldScrapInstances();

        for (int l = 0; l < StartOfRound.Instance.levels.Length; l++)
        {
            SelectableLevel level = StartOfRound.Instance.levels[l];
            if (level == null)
            {
                continue;
            }
            bool printDebug = Application.isEditor && Plugin.specialDateCase >= 0 && StartOfRound.Instance != null && StartOfRound.Instance.localPlayerController != null && StartOfRound.Instance.localPlayerController.isHostPlayerObject;
            if (printDebug)
            {
                Plugin.Logger.LogInfo($"Setting up: {level.name}");
            }
            RarityManager.SetRarityForLevel(level.levelID, -1, printDebug);
        }

        Plugin.appliedHostConfigs = true;
    }

    private static void UpdateGoldScrapStatic()
    {
        GoldenBell.RebalanceTool();
        GoldenGlass.RebalanceTool();
        GoldSign.RebalanceTool();
        GoldenGuardian.RebalanceTool();
        JacobsLadder.RebalanceTool();
        GoldRemote.RebalanceTool();
        Goldmine.RebalanceTool();
        GoldenGrenade.RebalanceTool();
        GoldenPickaxe.RebalanceTool();
        GoldenGlove.RebalanceTool();

        if (Plugin.specialDateCase != 1 && Plugin.specialDateCase != 4 && Plugin.specialDateCase != 6)
        {
            return;
        }
        foreach (ItemData scrapItem in Plugin.allGoldGrabbableObjects)
        {
            if (scrapItem == null || scrapItem.isStoreItem || scrapItem.itemProperties == null)
            {
                continue;
            }
            UpdatePrefabForSpecialDate(Plugin.specialDateCase, scrapItem);
        }
        foreach (ItemData storeItem in StoreAndTerminal.allGoldStoreItemData)
        {
            if (storeItem == null || !storeItem.isStoreItem)
            {
                continue;
            }
            UpdatePrefabForSpecialDate(Plugin.specialDateCase, storeItem);
        }
    }

    private static void UpdateGoldScrapInstances()
    {
        GoldScrapObject[] allGoldScrapObjects = Object.FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None);
        Logger.LogDebug($"goldScrapItem length: {allGoldScrapObjects.Length}");
        foreach (GoldScrapObject scrapScript in allGoldScrapObjects)
        {
            if (scrapScript == null || scrapScript.data == null)
            {
                continue;
            }
            if (scrapScript.data.itemProperties != null)
            {
                if (scrapScript.data.itemProperties == GoldenBell.itemData.itemProperties)
                {
                    GoldenBell.RebalanceTool(scrapScript.gameObject);
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                    continue;
                }
                if (scrapScript.data.itemProperties == GoldenGlass.itemData.itemProperties)
                {
                    GoldenGlass.RebalanceTool(scrapScript.gameObject);
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                    continue;
                }
                if (scrapScript.data.itemProperties == GoldSign.itemData.itemProperties)
                {
                    GoldSign.RebalanceTool(scrapScript.gameObject);
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                    continue;
                }
                if (scrapScript.data.itemProperties == GoldRemote.itemData.itemProperties)
                {
                    GoldRemote.RebalanceTool(scrapScript.gameObject);
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                    continue;
                }
                if (scrapScript.data.itemProperties == GoldenGrenade.itemData.itemProperties)
                {
                    GoldenGrenade.RebalanceTool(scrapScript.gameObject);
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                    continue;
                }
                if (scrapScript.data.itemProperties == GoldenGlove.itemData.itemProperties)
                {
                    GoldenGlove.RebalanceTool(scrapScript.gameObject);
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                    continue;
                }
            }
        }
        if (Plugin.specialDateCase == 1 || Plugin.specialDateCase == 4 || Plugin.specialDateCase == 6)
        {
            foreach (UnlockableSuit suit in Object.FindObjectsByType<UnlockableSuit>(FindObjectsSortMode.None))
            {
                if (suit == null)
                {
                    continue;
                }
                if (suit.suitID == StoreAndTerminal.goldSuitID || suit.suitID == StoreAndTerminal.silverSuitID || suit.suitID == StoreAndTerminal.bronzeSuitID)
                {
                    UpdatePrefabForSpecialDate(Plugin.specialDateCase, runtimeSuit: suit);
                }
            }
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null)
                {
                    continue;
                }
                if (player.currentSuitID == StoreAndTerminal.goldSuitID || player.currentSuitID == StoreAndTerminal.silverSuitID || player.currentSuitID == StoreAndTerminal.bronzeSuitID)
                {
                    UnlockableSuit.SwitchSuitForPlayer(player, player.currentSuitID, false);
                }
            }
        }
    }

    public static void UpdatePrefabForSpecialDate(int caseToUse = -1, ItemData staticData = null, GameObject runtimeObject = null, UnlockableSuit runtimeSuit = null)
    {
        if (caseToUse == -1)
        {
            caseToUse = Plugin.specialDateCase;
        }
        Item item = null;
        UnlockableItem unlockable = null;
        int altType = 0;
        if (staticData != null)
        {
            item = staticData.itemProperties;
            unlockable = staticData.unlockableProperties;
            if (staticData.isStoreItem)
            {
                altType = DoesItemHaveAlts(staticData, caseToUse);
            }
        }
        else if (runtimeSuit != null)
        {
            altType = DoesItemHaveAlts(runtimeSuit.GetComponent<InteractTrigger>().hoverTip.Remove(0, 8), true, caseToUse);
        }
        switch (caseToUse)
        {
            case 1:
                bool makeSilver = false;
                if (item != null)
                {
                    if (staticData != null)
                    {
                        makeSilver = staticData.localItemsListIndex % 2 == 0;
                    }
                    if (runtimeObject == null)
                    {
                        item.itemName = ReplaceGoldInName(item.itemName, caseToUse, makeSilver, altType, printDebug: false);
                    }
                }
                if (unlockable != null && !string.IsNullOrEmpty(unlockable.unlockableName))
                {
                    if (staticData != null)
                    {
                        makeSilver = staticData.localUnlockableID % 2 == 0;
                    }
                    if (runtimeObject == null)
                    {
                        unlockable.unlockableName = ReplaceGoldInName(unlockable.unlockableName, caseToUse, makeSilver, altType, printDebug: false);
                        if (unlockable.suitMaterial != null && unlockable.suitMaterial.name.StartsWith("GoldMat"))
                        {
                            Material material = null;
                            switch (altType)
                            {
                                case 0:
                                    material = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                                    break;
                                case 1:
                                    material = makeSilver? defaultMaterialSilverTransparent : defaultMaterialBronzeTransparent;
                                    break;
                            }
                            unlockable.suitMaterial = material;
                        }
                    }
                }
                if (runtimeObject != null)
                {
                    SetObjectMaterialsRuntime(runtimeObject, caseToUse, makeSilver);
                }
                if (runtimeSuit != null && runtimeSuit.suitMaterial.name.StartsWith("GoldMat"))
                {
                    makeSilver = runtimeSuit.suitID % 2 == 0;
                    Material material = null;
                    switch (altType)
                    {
                        case 0:
                            material = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                            break;
                        case 1:
                            material = makeSilver ? defaultMaterialSilverTransparent : defaultMaterialBronzeTransparent;
                            break;
                    }
                    runtimeSuit.suitMaterial = material;
                    runtimeSuit.suitRenderer.material = runtimeSuit.suitMaterial;
                    runtimeSuit.GetComponent<InteractTrigger>().hoverTip = $"Change: {StartOfRound.Instance.unlockablesList.unlockables[runtimeSuit.suitID].unlockableName}";
                }
                break;
            case 4:
                if (item != null && runtimeObject == null)
                {
                    item.itemName = ReplaceGoldInName(item.itemName, caseToUse, altType: altType, printDebug: false);
                }
                if (unlockable != null && !string.IsNullOrEmpty(unlockable.unlockableName) && runtimeObject == null)
                {
                    unlockable.unlockableName = ReplaceGoldInName(unlockable.unlockableName, caseToUse, altType: altType, printDebug: false);
                    if (unlockable.suitMaterial != null && unlockable.suitMaterial.name.StartsWith("BronzeMat"))
                    {
                        unlockable.suitMaterial = altType == 3 ? defaultMaterialGoldEmmissive : defaultMaterialGold;
                    }
                    else if (unlockable.suitMaterial != null && unlockable.suitMaterial.name.StartsWith("SilverMat"))
                    {
                        unlockable.suitMaterial = altType == 2 ? defaultMaterialGoldTransparent : defaultMaterialGold;
                    }
                }
                if (runtimeObject != null)
                {
                    SetObjectMaterialsRuntime(runtimeObject, caseToUse);
                }
                if (runtimeSuit != null)
                {
                    if (runtimeSuit.suitMaterial.name.StartsWith("BronzeMat"))
                    {
                        runtimeSuit.suitMaterial = defaultMaterialGoldEmmissive;
                    }
                    else if (runtimeSuit.suitMaterial.name.StartsWith("SilverMat"))
                    {
                        runtimeSuit.suitMaterial = defaultMaterialGoldTransparent;
                    }
                    runtimeSuit.suitRenderer.material = runtimeSuit.suitMaterial;
                    runtimeSuit.GetComponent<InteractTrigger>().hoverTip = $"Change: {StartOfRound.Instance.unlockablesList.unlockables[runtimeSuit.suitID].unlockableName}";
                }
                break;
            case 6:
                if (sparkleParticle != null)
                {
                    if (runtimeObject != null)
                    {
                        General.InstantiateSparklesOnTransform(runtimeObject.transform);
                    }
                    if (runtimeSuit != null)
                    {
                        General.InstantiateSparklesOnTransform(runtimeSuit.transform);
                    }
                }
                break;
        }
    }

    private static void SetObjectMaterialsRuntime(GameObject runtimeObject, int caseToUse, bool makeSilver = false, ItemData staticData = null)
    {
        if (runtimeObject == null)
        {
            return;
        }
        int altType = 0;
        if (staticData != null)
        {
            altType = DoesItemHaveAlts(staticData, caseToUse);
        }
        switch (caseToUse)
        {
            case 1:
                foreach (MeshRenderer renderer in runtimeObject.GetComponentsInChildren<MeshRenderer>())
                {
                    Material[] newMats = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        if (renderer.materials[i].name.StartsWith("GoldenVision"))
                        {
                            string visionColor = makeSilver ? "Silver" : "Bronze"; 
                            newMats[i] = Plugin.CustomGoldScrapAssets.LoadAsset<Material>($"Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/GoldenVision{visionColor}.mat");
                        }
                        else if (renderer.materials[i].name.StartsWith("GoldenTicket"))
                        {
                            string ticketColor = makeSilver ? "Silver" : "Bronze";
                            newMats[i] = Plugin.CustomGoldScrapAssets.LoadAsset<Material>($"Assets/LCGoldScrapMod/GoldScrapVisuals/Materials/GoldenTicket{ticketColor}.mat");
                        }
                        else if (renderer.materials[i].name.StartsWith("GoldMatTransparent"))
                        {
                            newMats[i] = makeSilver ? defaultMaterialSilverTransparent : defaultMaterialBronzeTransparent;
                        }
                        else if (renderer.materials[i].name.StartsWith("GoldMatEmmissive"))
                        {
                            newMats[i] = makeSilver ? defaultMaterialSilverEmmissive : defaultMaterialBronzeEmmissive;
                        }
                        else if (renderer.materials[i].name.StartsWith("GoldMat"))
                        {
                            newMats[i] = altType == 1 ? (makeSilver ? defaultMaterialSilverTransparent : defaultMaterialBronzeTransparent) : (makeSilver ? defaultMaterialSilver : defaultMaterialBronze);
                        }
                        else
                        {
                            newMats[i] = renderer.materials[i];
                        }
                    }
                    renderer.materials = newMats;
                }
                foreach (ScanNodeProperties node in runtimeObject.GetComponentsInChildren<ScanNodeProperties>(true))
                {
                    node.headerText = ReplaceGoldInName(node.headerText, caseToUse, makeSilver, altType);
                }
                break;
            case 4:
                foreach (MeshRenderer rendererToGold in runtimeObject.GetComponentsInChildren<MeshRenderer>())
                {
                    Material[] newMats = new Material[rendererToGold.materials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        if (rendererToGold.materials[i].name.StartsWith("BronzeMatTransparent") || rendererToGold.materials[i].name.StartsWith("SilverMatTransparent"))
                        {
                            newMats[i] = defaultMaterialGoldTransparent;
                        }
                        else if (rendererToGold.materials[i].name.StartsWith("BronzeMatEmmissive") || rendererToGold.materials[i].name.StartsWith("SilverMatEmmissive"))
                        {
                            newMats[i] = defaultMaterialGoldEmmissive;
                        }
                        else
                        {
                            switch (altType)
                            {
                                case 0:
                                    newMats[i] = defaultMaterialGold;
                                    break;
                                case 2:
                                    newMats[i] = defaultMaterialGoldTransparent;
                                    break;
                                case 3:
                                    newMats[i] = defaultMaterialGoldEmmissive;
                                    break;
                            }
                        }
                    }
                    rendererToGold.materials = newMats;
                }
                foreach (ScanNodeProperties node in runtimeObject.GetComponentsInChildren<ScanNodeProperties>(true))
                {
                    node.headerText = ReplaceGoldInName(node.headerText, caseToUse, altType: altType);
                }
                break;
        }
        MaterialUpdateExceptions(runtimeObject, caseToUse, makeSilver);
    }

    private static void MaterialUpdateExceptions(GameObject runtimeObject, int caseToUse, bool makeSilver = false)
    {
        GoldenGirlScript girlScript = runtimeObject.GetComponent<GoldenGirlScript>();
        CatOGoldScript catScript = runtimeObject.GetComponent<CatOGoldScript>();
        GoldenPickaxeScript pickaxeScript = runtimeObject.GetComponent<GoldenPickaxeScript>();
        switch (caseToUse)
        {
            case 1:
                if (girlScript != null)
                {
                    if (girlScript.appearMat.name.StartsWith("GoldMat"))
                    {
                        girlScript.appearMat = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                    }
                    if (girlScript.invisibleMat.name.StartsWith("GoldMat"))
                    {
                        girlScript.invisibleMat = makeSilver ? defaultMaterialSilverTransparent : defaultMaterialBronzeTransparent;
                    }
                }
                if (catScript != null)
                {
                    for (int i = 0; i < catScript.matsNormal.Length; i++)
                    {
                        if (catScript.matsNormal[i].name.StartsWith("GoldMat"))
                        {
                            catScript.matsNormal[i] = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                        }
                    }
                    for (int j = 0; j < catScript.matsFever.Length; j++)
                    {
                        if (catScript.matsFever[j].name.StartsWith("GoldMatEmmissive"))
                        {
                            catScript.matsFever[j] = makeSilver ? defaultMaterialSilverEmmissive : defaultMaterialBronzeEmmissive;
                        }
                        else if (catScript.matsFever[j].name.StartsWith("GoldMat"))
                        {
                            catScript.matsFever[j] = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                        }
                    }
                }
                if (pickaxeScript != null)
                {
                    for (int k = 0; k < pickaxeScript.normalMats.Length; k++)
                    {
                        if (pickaxeScript.normalMats[k].name.StartsWith("GoldMat"))
                        {
                            pickaxeScript.normalMats[k] = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                        }
                    }
                    for (int l = 0; l < pickaxeScript.feverMats.Length; l++)
                    {
                        if (pickaxeScript.feverMats[l].name.StartsWith("GoldMat"))
                        {
                            pickaxeScript.feverMats[l] = makeSilver ? defaultMaterialSilver : defaultMaterialBronze;
                        }
                    }
                }
                break;
            case 4:
                if (catScript != null)
                {
                    for (int i = 0; i < catScript.matsNormal.Length; i++)
                    {
                        catScript.matsNormal[i] = defaultMaterialGold;
                    }
                    for (int j = 0; j < catScript.matsFever.Length; j++)
                    {
                        catScript.matsFever[j] = defaultMaterialGold;
                    }
                }
                if (pickaxeScript != null)
                {
                    for (int k = 0; k < pickaxeScript.normalMats.Length; k++)
                    {
                        if (pickaxeScript.normalMats[k].name.StartsWith("BronzeMat"))
                        {
                            pickaxeScript.normalMats[k] = defaultMaterialGoldTransparent;
                        }
                        else if (pickaxeScript.normalMats[k].name.StartsWith("SilverMat"))
                        {
                            pickaxeScript.normalMats[k] = defaultMaterialGoldEmmissive;
                        }
                    }
                    for (int l = 0; l < pickaxeScript.feverMats.Length; l++)
                    {
                        if (pickaxeScript.feverMats[l].name.StartsWith("BronzeMat"))
                        {
                            pickaxeScript.feverMats[l] = defaultMaterialGoldTransparent;
                        }
                        else if (pickaxeScript.feverMats[l].name.StartsWith("SilverMat"))
                        {
                            pickaxeScript.feverMats[l] = defaultMaterialGoldEmmissive;
                        }
                    }
                }
                break;
        }
    }

    public static string ReplaceGoldInName(string originalName, int caseToUse, bool makeSilver = false, int altType = 0, bool getReverse = false, bool printDebug = true)
    {
        string newName = originalName;
        switch (caseToUse)
        {
            case 1:
                if (!getReverse)
                {
                    if (newName == originalName) newName = originalName.Replace("Golden", makeSilver ? "Silver" : "Bronze");
                    if (newName == originalName) newName = originalName.Replace("golden", makeSilver ? "silver" : "bronze");
                    if (newName == originalName) newName = originalName.Replace("Gold", makeSilver ? "Silver" : "Bronze");
                    if (newName == originalName) newName = originalName.Replace("gold", makeSilver ? "silver" : "bronze");
                    if (newName == originalName) newName = originalName.Replace("G.O.L.D.", makeSilver ? "S.I.L.V.E.R." : "B.R.O.N.Z.E.");
                    switch (altType)
                    {
                        case 1:
                            newName = makeSilver ? $"Transparent {newName}" : $"Transparent {newName}";
                            break;
                    }
                }
                else
                {
                    if (newName == originalName) newName = originalName.Replace("Bronze", "Gold");
                    if (newName == originalName) newName = originalName.Replace("bronze", "gold");
                    if (newName == originalName) newName = originalName.Replace("Silver", "Gold");
                    if (newName == originalName) newName = originalName.Replace("silver", "gold");
                    switch (altType)
                    {
                        case 2:
                            newName = $"Transparent {newName}";
                            break;
                        case 3:
                            newName = $"Neon {newName}";
                            break;
                    }
                }
                if (printDebug) Logger.LogDebug($"replaced to: {newName}");
                return newName;
            case 4:
                if (newName == originalName) newName = originalName.Replace("Bronze", "Gold");
                if (newName == originalName) newName = originalName.Replace("bronze", "gold");
                if (newName == originalName) newName = originalName.Replace("Silver", "Gold");
                if (newName == originalName) newName = originalName.Replace("silver", "gold");
                switch (altType)
                {
                    case 2:
                        newName = $"Transparent {newName}";
                        break;
                    case 3:
                        newName = $"Neon {newName}";
                        break;
                }
                if (printDebug) Logger.LogDebug($"replaced to: {newName}");
                return newName;
        }
        return originalName;
    }

    public static int DoesItemHaveAlts(ItemData item, int caseToUse)
    {
        if (item == null)
        {
            Logger.LogDebug("DoesItemHaveAlts(): null");
            return 0;
        }
        return DoesItemHaveAlts(item.displayName, item.isStoreItem, caseToUse);
    }

    public static int DoesItemHaveAlts(string itemName, bool forStoreItem, int caseToUse)
    {
        //go through all items, see if there is another item of the same type with the same end of its name (so gold MEDAL matching a silver MEDAL)
        //if this is the case, return one of the below
        //return 0 for items without alts
        //return 1 for gold items to make transparent bronze/silver
        //return 2 for silver items to make transparent gold
        //return 3 for bronze items to make neon gold
        string matchingName = itemName;
        int foundType = 0;
        //get the type: gold (1) / silver (2) / bronze (3)
        if (matchingName.ToLower().Contains("gold"))
        {
            foundType = 1;
        }
        else if (matchingName.ToLower().Contains("silver"))
        {
            foundType = 2;
        }
        else if (matchingName.ToLower().Contains("bronze"))
        {
            foundType = 3;
        }
        //remove prefixes, should hopefully not be lower case
        if (matchingName.Contains("Transparent ") || matchingName.Contains("transparent ") || matchingName.Contains("Transparent") || matchingName.Contains("transparent"))
        {
            matchingName = matchingName.Replace("Transparent ", "");
            matchingName = matchingName.Replace("transparent ", "");
            matchingName = matchingName.Replace("Transparent", "");
            matchingName = matchingName.Replace("transparent", "");
        }
        else if (matchingName.Contains("Neon ") || matchingName.Contains("neon ") || matchingName.Contains("Neon") || matchingName.Contains("neon"))
        {
            matchingName = matchingName.Replace("Neon ", "");
            matchingName = matchingName.Replace("neon ", "");
            matchingName = matchingName.Replace("Neon", "");
            matchingName = matchingName.Replace("neon", "");
        }
        //reset if not being able to find anything
        if (foundType == 0)
        {
            return 0;
        }
        matchingName = ReplaceGoldInName(matchingName, caseToUse, true, printDebug: false);
        int indexOfItem = -1;
        ItemData[] listToCheck = forStoreItem ? StoreAndTerminal.allGoldStoreItemData : Plugin.allGoldGrabbableObjects;
        for (int i = 0; i < listToCheck.Length; i++)
        {
            ItemData data = listToCheck[i];
            if (data == null || data.isStoreItem != forStoreItem)
            {
                continue;
            }
            if (data.displayName == itemName)
            {
                indexOfItem = i;
                break;
            }
        }
        for (int j = 0; j < listToCheck.Length; j++)
        {
            ItemData data = listToCheck[j];
            if (data == null || data.isStoreItem != forStoreItem)
            {
                continue;
            }
            if (data.displayName == matchingName && (indexOfItem == -1 || indexOfItem != j))
            {
                Logger.LogDebug($"DoesItemHaveAlts({itemName}) found other item {data.name} with matching name '{matchingName}' of item at index [{indexOfItem}], returning foundType {foundType}");
                return foundType;
            }
        }
        return 0;
    }
}
