using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;

public class StoreAndTerminal
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static TerminalNodesList allGoldScrapKeywords;
    public static ItemData[] allGoldStoreItemData;
    public static Material defaultHelmetMaterial;

    public static List<int> allShipUnlockableIDs = new List<int>();
    public static int bronzeSuitID = -1;
    public static int silverSuitID = -1;
    public static int goldSuitID = -1;
    public static int directoryLogID = -1;
    public static int groovyGoldID = -1;
    public static int catOGoldID = -1;
    public static int safeBoxID = -1;

    public static AudioClipList runtimeDiscoBallSongs;
    public static StringList catOGoldInfoStrings;

    public static TerminalNode goldCrownBuyNode;
    public static TerminalNode goldCrownSingleplayerInfo;

    public static AudioClip sharedSFXPlaceShipObject;
    public static TerminalKeyword keywordBuy;
    public static TerminalKeyword keywordConfirm;
    public static TerminalKeyword keywordRoute;
    public static CompatibleNoun compatibleNounDeny;

    public static void LoadEditorAssets()
    {
        allGoldScrapKeywords = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNodesList>("Assets/LCGoldScrapMod/GoldScrapTerminal/allGoldScrapKeywords.asset");
        allGoldStoreItemData = Plugin.CustomGoldScrapAssets.LoadAsset<ItemDataList>("Assets/LCGoldScrapMod/GoldScrapShop/AllGoldStoreItemData.asset").allItemData;
        runtimeDiscoBallSongs = Plugin.CustomGoldScrapAssets.LoadAsset<AudioClipList>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/DiscoBallMusic/RuntimeDiscoBallSongs.asset");
        catOGoldInfoStrings = Plugin.CustomGoldScrapAssets.LoadAsset<StringList>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/CatOGold/CatOGoldInfo.asset");
        goldCrownBuyNode = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/GoldCrown/GoldScrapShopGoldCrown.asset");
        goldCrownSingleplayerInfo = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/GoldCrown/GoldScrapShopGoldCrownWarning.asset");
    }

    public static void GetConfirmAndDenyTerminal(StartOfRound __instance)
    {
        if (keywordConfirm == null)
        {
            keywordConfirm = __instance.unlockablesList.unlockables[1].shopSelectionNode.terminalOptions[0].noun;
        }

        if (compatibleNounDeny == null)
        {
            compatibleNounDeny = __instance.unlockablesList.unlockables[1].shopSelectionNode.terminalOptions[1];
        }
    }

    public static void SetBuyAndInfoTerminal(Terminal __instance)
    {
        TerminalKeyword keywordInfo = null;
        foreach (TerminalKeyword keyword in __instance.terminalNodes.allKeywords)
        {
            if (keywordBuy != null && keywordInfo != null && keywordRoute != null)
            {
                break;
            }
            if (keyword.word == "buy")
            {
                keywordBuy = keyword;
            }
            else if (keyword.word == "info")
            {
                keywordInfo = keyword;
            }
            else if (keyword.word == "route")
            {
                keywordRoute = keyword;
            }
        }

        TerminalNode otherMenu = null;
        foreach (TerminalKeyword keyword in __instance.terminalNodes.allKeywords)
        {
            if (keyword.word == "other")
            {
                otherMenu = keyword.specialKeywordResult;
                break;
            }
        }

        if (!otherMenu.displayText.Contains("LCGoldScrapMod"))
        {
            List<CompatibleNoun> originalBuyCompatibleNouns = keywordBuy.compatibleNouns.ToList();
            List<CompatibleNoun> originalInfoCompatibleNouns = keywordInfo.compatibleNouns.ToList();

            foreach (ItemData itemData in allGoldStoreItemData)
            {
                if (itemData == null || string.IsNullOrEmpty(itemData.folderName))
                {
                    continue;
                }
                string itemName = itemData.folderName;
                originalBuyCompatibleNouns.Add(GoldScrapShopCompatibleNoun(itemName));
                originalInfoCompatibleNouns.Add(GoldScrapInfoCompatibleNoun(itemName));
            }
            originalBuyCompatibleNouns.Add(GoldScrapSpecialCompatibleNoun("GoldCrown", "Warning"));

            keywordBuy.compatibleNouns = originalBuyCompatibleNouns.ToArray();
            keywordInfo.compatibleNouns = originalInfoCompatibleNouns.ToArray();
        }
    }

    public static void SetSpecialStoreNodes()
    {
        goldCrownSingleplayerInfo.terminalOptions[0].noun = keywordConfirm;
        goldCrownSingleplayerInfo.terminalOptions[1] = compatibleNounDeny;
    }

    public static CompatibleNoun GoldScrapShopCompatibleNoun(string itemName)
    {
        CompatibleNoun newCompatibleNoun = new CompatibleNoun();
        newCompatibleNoun.noun = GoldScrapShopKeyword(itemName);
        newCompatibleNoun.result = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemName}/GoldScrapShop{itemName}.asset");
        return newCompatibleNoun;
    }

    public static CompatibleNoun GoldScrapSpecialCompatibleNoun(string itemName, string nodeAddition)
    {
        CompatibleNoun newCompatibleNoun = new CompatibleNoun();
        newCompatibleNoun.noun = GoldScrapShopKeyword(itemName);
        newCompatibleNoun.result = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemName}/GoldScrapShop{itemName}{nodeAddition}.asset");
        return newCompatibleNoun;
    }

    public static CompatibleNoun GoldScrapInfoCompatibleNoun(string itemName)
    {
        CompatibleNoun newCompatibleNoun = new CompatibleNoun();
        newCompatibleNoun.noun = GoldScrapShopKeyword(itemName);
        newCompatibleNoun.result = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{itemName}/GoldScrapShop{itemName}Info.asset");
        return newCompatibleNoun;
    }

    public static TerminalKeyword GoldScrapShopKeyword(string itemName)
    {
        TerminalKeyword newKeyword = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalKeyword>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/" + itemName + "/GoldScrapShop" + itemName + "Keyword.asset");
        newKeyword.defaultVerb = keywordBuy;
        return newKeyword;
    }

    public static void AddGoldUnlockablesToShop(StartOfRound __instance)
    {
        if (sharedSFXPlaceShipObject == null)
        {
            foreach (UnlockableItem unlockable in __instance.unlockablesList.unlockables)
            {
                if (unlockable.prefabObject == null) continue;

                PlaceableShipObject placeableObjectScript = unlockable.prefabObject.GetComponentInChildren<PlaceableShipObject>();
                if (placeableObjectScript == null) continue;

                AudioClip placeObjectClip = placeableObjectScript.placeObjectSFX;
                if (placeObjectClip == null) continue;

                if (placeObjectClip.name == "MoveShipObjectBig")
                {
                    sharedSFXPlaceShipObject = placeObjectClip;
                    break;
                }
            }
        }

        allShipUnlockableIDs.Clear();
        foreach (ItemData itemData in allGoldStoreItemData)
        {
            if (itemData == null || itemData.unlockableProperties == null || string.IsNullOrEmpty(itemData.unlockableProperties.unlockableName))
            {
                continue;
            }
            UnlockableItem item = itemData.unlockableProperties;
            string itemName = itemData.folderName;
            int itemID = __instance.unlockablesList.unlockables.Count;
            item.shopSelectionNode.terminalOptions[0].noun = keywordConfirm;
            item.shopSelectionNode.terminalOptions[1] = compatibleNounDeny;
            __instance.unlockablesList.unlockables.Add(item);
            item.shopSelectionNode.shipUnlockableID = itemID;
            item.shopSelectionNode.terminalOptions[0].result.shipUnlockableID = itemID;

            if (item.prefabObject != null)
            {
                item.prefabObject.AddComponent<GoldScrapObject>().data = itemData;
                item.prefabObject.AddComponent<GoldStoreItem>();
                if (item.IsPlaceable)
                {
                    PlaceableShipObject placeableObject = item.prefabObject.GetComponentInChildren<PlaceableShipObject>();
                    if (placeableObject != null)
                    {
                        placeableObject.unlockableID = itemID;
                        placeableObject.placeObjectSFX = sharedSFXPlaceShipObject;
                    }
                    AutoParentToShip parentToShip = item.prefabObject.GetComponentInChildren<AutoParentToShip>();
                    if (parentToShip != null)
                    {
                        parentToShip.unlockableID = itemID;
                    }
                }
            }

            switch (itemName)
            {
                case "BronzeSuit":
                    bronzeSuitID = itemID;
                    break;
                case "SilverSuit":
                    silverSuitID = itemID;
                    break;
                case "GoldSuit":
                    goldSuitID = itemID;
                    break;
                case "DirectoryLOG":
                    directoryLogID = itemID;
                    break;
                case "GroovyGold":
                    groovyGoldID = itemID;
                    break;
                case "CatOGold":
                    catOGoldID = itemID;
                    break;
                case "SafeBox":
                    safeBoxID = itemID;
                    break;
                case "GoldMedal":
                    MedalManager.goldMedalID = itemID;
                    break;
            }

            itemData.localUnlockableID = itemID;
            allShipUnlockableIDs.Add(itemID);
            Logger.LogDebug($"Added {itemName} to unlockablesList with shipUnlockableID {itemID}");
        }

        SetSpecialStoreNodes();
    }

    public static void AddGoldItemsToShop()
    {
        Terminal __instance = Object.FindAnyObjectByType<Terminal>();
        if (__instance == null)
        {
            Logger.LogWarning("Failed to find Terminal! Not adding LCGoldScrapMod items to store.");
            return;
        }
        List<Item> originalBuyableItems = __instance.buyableItemsList.ToList();
        List<int> originalItemSalesPercentageList = __instance.itemSalesPercentages.ToList();
        foreach (ItemData itemData in allGoldStoreItemData)
        {
            if (itemData == null || itemData.itemProperties == null)
            {
                continue;
            }
            Item item = itemData.itemProperties;
            string itemName = itemData.folderName;
            TerminalNode goldShopItemNode = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/" + itemName + "/GoldScrapShop" + itemName + ".asset");
            goldShopItemNode.terminalOptions[0].noun = keywordConfirm;
            goldShopItemNode.terminalOptions[1] = compatibleNounDeny;
            originalBuyableItems.Add(item);
            originalItemSalesPercentageList.Add(100);
            int itemIndex = originalBuyableItems.Count - 1;
            goldShopItemNode.buyItemIndex = itemIndex;
            goldShopItemNode.terminalOptions[0].result.buyItemIndex = itemIndex;
            itemData.localBuyItemIndex = itemIndex;
            Logger.LogDebug($"Added {itemName} to buyableItemsList at buyItemIndex {goldShopItemNode.buyItemIndex}");
        }
        __instance.buyableItemsList = originalBuyableItems.ToArray();
        __instance.itemSalesPercentages = originalItemSalesPercentageList.ToArray();
    }

    public static void LoadGoldStoreSuitJumpAudio()
    {
        foreach (ItemData item in allGoldStoreItemData)
        {
            if (item == null || item.unlockableProperties == null)
            {
                continue;
            }
            UnlockableItem unlockable = item.unlockableProperties;
            if (unlockable.suitMaterial != null)
            {
                string itemName = unlockable.shopSelectionNode.name.Remove(0, 13);
                string[] allJumpAudioItems = { "BronzeSuit", "SilverSuit", "GoldSuit" };
                if (allJumpAudioItems.Contains(itemName))
                {
                    unlockable.jumpAudio = AssetsCollection.LoadReplaceSFX(itemName);
                }
            }
        }
    }

    public static void AddGoldScrapKeywordsToTerminal(Terminal __instance)
    {
        TerminalNode otherMenu = null;
        foreach (TerminalKeyword keyword in __instance.terminalNodes.allKeywords)
        {
            if (keyword.word == "other")
            {
                otherMenu = keyword.specialKeywordResult;
                break;
            }
        }

        //Add GoldScrap Store and Help to terminal
        if (!otherMenu.displayText.Contains("LCGoldScrapMod"))
        {
            //Set text in Others menu to include GoldScrap
            otherMenu.displayText = otherMenu.displayText + ">GOLD STORE\nTo browse expensive gold items and cosmetics.\n\n>GOLD SCRAP\nTo answer possible questions you might have about the mod 'LCGoldScrapMod'.\n\n\n";

            //Add the TerminalKeywords loaded from the asset bundle and make it the new TerminalKeywords array
            List<TerminalKeyword> originalAllKeywords = __instance.terminalNodes.allKeywords.ToList();
            foreach (TerminalKeyword goldScrapKeyword in allGoldScrapKeywords.allKeywords)
            {
                if (goldScrapKeyword.name.Contains("GoldScrapShop") && goldScrapKeyword.word != "gold store")
                {
                    goldScrapKeyword.defaultVerb = keywordBuy;
                }
                originalAllKeywords.Add(goldScrapKeyword);
            }
            __instance.terminalNodes.allKeywords = originalAllKeywords.ToArray();
        }
    }

    public static int GetMoonTravelCost(int levelID)
    {
        foreach (CompatibleNoun cNoun in keywordRoute.compatibleNouns)
        {
            if (cNoun == null || cNoun.result == null) continue;
            foreach (CompatibleNoun tOptions in cNoun.result.terminalOptions)
            {
                if (tOptions != null && tOptions.noun != null && tOptions.noun.name == "Confirm" && tOptions.result != null && tOptions.result.buyRerouteToMoon == levelID)
                {
                    return tOptions.result.itemCost;
                }
            }
        }
        Logger.LogDebug("error when trying to get itemCost of moon, returning zero");
        return 0;
    }
}
