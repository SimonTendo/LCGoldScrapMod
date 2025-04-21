using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using HarmonyLib;

public class GoldScrapTerminalPatch
{
    //Get the necessary Terminal keyword and Nodes, and add the goldscrap Help and Store upon loading the Terminal
    [HarmonyPatch(typeof(Terminal), "Awake")]
    public class NewTerminalAwake
    {
        [HarmonyPostfix]
        public static void AwakePostfix(Terminal __instance)
        {
            StoreAndTerminal.SetBuyAndInfoTerminal(__instance);
            StoreAndTerminal.AddGoldScrapKeywordsToTerminal(__instance);
            StoreAndTerminal.GetConfirmAndDenyTerminal(StartOfRound.Instance);
            if (!Plugin.alreadyAddedUnlockables)
            {
                StoreAndTerminal.AddGoldUnlockablesToShop(StartOfRound.Instance);
                if (Configs.replaceSFX.Value && Plugin.v50Compatible)
                {
                    StoreAndTerminal.LoadGoldStoreSuitJumpAudio();
                }
                Plugin.alreadyAddedUnlockables = true;
            }
        }
    }



    //Check for GoldScrap keywords so it doesn't keep loading other things
    [HarmonyPatch(typeof(Terminal), "OnSubmit")]
    public class NewTerminalSubmit
    {
        [HarmonyPrefix]
        public static bool OnSubmitPrefix(Terminal __instance)
        {
            string input = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower();
            if (!input.Contains("gold") && !input.Contains("silver") && !input.Contains("bronze"))
            {
                return true;
            }
            switch (Plugin.specialDateCase)
            {
                case 1:
                    if (input.Contains("bronze") || input.Contains("silver"))
                    {
                        if (input == "bronze suit" || input == "silver suit" /*|| input == "bronze medal" || input == "silver medal" || input == "bronze trophy" || input == "silver trophy" || input == "bronze crown" || input == "silver crown"*/)
                        {
                            Plugin.Logger.LogDebug($"!!specialDateCase {Plugin.specialDateCase} caught exception input '{input}', leaving be");
                        }
                        else
                        {
                            input = RuntimeChanges.ReplaceGoldInName(input, Plugin.specialDateCase, getReverse: true).ToLower();
                            if (input.Contains("hourglass") || input.Contains("glove") || input.Contains("pickaxe") || input.Contains("throne") || input.Contains("ticket"))
                            {
                                input = input.Replace("gold", "golden");
                            }
                            if (input.Contains("transparent "))
                            {
                                input = input.Replace("transparent ", "");
                            }
                            else if (input.Contains("neon "))
                            {
                                input = input.Replace("neon ", "");
                            }
                            Plugin.Logger.LogDebug($"specialDateCase {Plugin.specialDateCase} overwriting input to {input}");
                        }
                    }
                    break;
                case 4:
                    if (input.Contains("neon gold"))
                    {
                        input = input.Replace("neon gold", "bronze");
                        Plugin.Logger.LogDebug($"specialDateCase {Plugin.specialDateCase} overwriting input to {input}");
                    }
                    else if (input.Contains("transparent gold"))
                    {
                        input = input.Replace("transparent gold", "silver");
                        Plugin.Logger.LogDebug($"specialDateCase {Plugin.specialDateCase} overwriting input to {input}");
                    }
                    break;
            }
            foreach (TerminalKeyword goldScrapKeyword in StoreAndTerminal.allGoldScrapKeywords.allKeywords)
            {
                if (input.StartsWith(goldScrapKeyword.word) && IsValidNode(goldScrapKeyword))
                {
                    Plugin.Logger.LogDebug($"intercepted input '{input}' with custom keyword {goldScrapKeyword.specialKeywordResult}");
                    string nodeName = goldScrapKeyword.name.Remove(goldScrapKeyword.name.Length - 7, 7).Remove(0, 13);
                    Plugin.Logger.LogDebug($"parsed name '{nodeName}'");
                    string excess = input.Remove(0, goldScrapKeyword.word.Length);
                    Plugin.Logger.LogDebug($"found excess '{excess}'");
                    string amount = Regex.Match(excess, "\\d+").Value;
                    Plugin.Logger.LogDebug($"{amount}; null = {amount.Length == 0}");
                    bool loadInfoNode = false;
                    if (excess.Contains("info"))
                    {
                        loadInfoNode = true;
                    }
                    else if (excess.Contains("buy") || amount.Length != 0)
                    {
                        if (!string.IsNullOrWhiteSpace(amount))
                        {
                            __instance.playerDefinedAmount = Mathf.Clamp(int.Parse(amount), 1, 10);
                        }
                        else
                        {
                            __instance.playerDefinedAmount = 1;
                        }
                    }
                    string nameEnd = loadInfoNode ? "Info.asset" : ".asset";
                    TerminalNode nodeToLoad = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{nodeName}/GoldScrapShop{nodeName}{nameEnd}");
                    if (nodeToLoad != null)
                    {
                        LoadNodeCustom(__instance, nodeToLoad, loadInfoNode);
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsValidNode(TerminalKeyword goldScrapKeyword)
        {
            string[] invalidNames = { "gold store", "credits card" };
            if (goldScrapKeyword.name.StartsWith("GoldScrapShop") && !invalidNames.Contains(goldScrapKeyword.word))
            {
                return true;
            }
            return false;
        }

        private static void LoadNodeCustom(Terminal __instance, TerminalNode nodeToLoad, bool loadInfoNode)
        {
            if (loadInfoNode)
            {
                __instance.LoadNewNode(nodeToLoad);
            }
            else
            {
                __instance.GetType().GetMethod("LoadNewNodeIfAffordable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { nodeToLoad });
            }
            __instance.screenText.ActivateInputField();
            __instance.screenText.Select();
            Plugin.Logger.LogDebug($"loaded {nodeToLoad} with amount {__instance.playerDefinedAmount}");
        }
    }



    //Perform Terminal Scan myself as currently only way to fix scanning-bug, does the original calculation minus the randomness
    [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
    public class NewTerminalTextPostProcessPrefix
    {
        [HarmonyPrefix]
        public static bool TextPostProcessPrefix(ref string modifiedDisplayText, ref string __result)
        {
            //[Custom Terminal Scan] TRUE
            if (Configs.fixScan.Value && modifiedDisplayText.Contains("[scanForItems]"))
            {
                if (!General.DoesMoonHaveGoldScrap()) return true;

                Plugin.Logger.LogInfo("Config [Custom Terminal Scan] is set to TRUE. Calculating value using custom scan.");

                int normalScrapAmount = 0;
                int normalScrapValue = 0;
                int goldScrapAmount = 0;
                int goldScrapValue = 0;

                GrabbableObject[] allItems = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);
                foreach (GrabbableObject item in allItems)
                {
                    if (item.itemProperties.isScrap && !item.isInShipRoom && !item.isInElevator)
                    {
                        normalScrapAmount++;
                        normalScrapValue += item.scrapValue != 0 ? item.scrapValue : Random.Range((int)(item.itemProperties.minValue * RoundManager.Instance.scrapValueMultiplier), (int)(item.itemProperties.maxValue * RoundManager.Instance.scrapValueMultiplier));
                    }

                    if (item.itemProperties.name.Contains("LCGoldScrapMod") && !item.isInShipRoom)
                    {
                        if (item.isInFactory)
                        {
                            goldScrapAmount++;
                        }
                        goldScrapValue += item.scrapValue != 0 ? item.scrapValue : Random.Range((int)(item.itemProperties.minValue * RoundManager.Instance.scrapValueMultiplier), (int)(item.itemProperties.maxValue * RoundManager.Instance.scrapValueMultiplier));
                    }
                }

                normalScrapValue = (int)Mathf.Lerp(0, normalScrapValue, Random.Range(Random.Range(0.6f, 0.8f), Random.Range(1.2f, 1.4f)));

                string newText = $"There are {normalScrapAmount} objects outside the ship, totalling at an approximate value of ${normalScrapValue}.\n";
                if (StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].hasBeenUnlockedByPlayer)
                {
                    newText += $"\nGold Scrap inside facility: {goldScrapAmount}\nValue of Gold Scrap: ${goldScrapValue}\nGold Scrap Score: {DLOGManager.goldScrapScore}\n\n";
                }
                else if (!StartOfRound.Instance.shipDoorsEnabled || !StartOfRound.Instance.currentLevel.planetHasTime)
                {
                    newText += "\nDid you know...?\nThe Gold Store's Directory LOG allows you to find gold scrap much more efficiently!\n\n";
                }
                modifiedDisplayText = modifiedDisplayText.Replace("[scanForItems]", newText);
                __result = modifiedDisplayText;
                return false;
            }
            else
            {
                return true;
            }
        }

        //Tell TerminalStore to update Goldnugget description to link to GoldStore, and config [Custom Terminal Scan] FALSE
        [HarmonyPostfix]
        public static void TextPostProcessPostfix(Terminal __instance, ref string __result)
        {
            //Misc
            if (__result.Contains("Welcome to the Company store."))
            {
                string nuggetColor = "Gold";
                if (Plugin.specialDateCase == 1)
                {
                    foreach (ItemData nuggetData in StoreAndTerminal.allGoldStoreItemData)
                    {
                        if (nuggetData.folderName == "GoldNugget")
                        {
                            nuggetColor = nuggetData.localItemsListIndex % 2 == 0 ? "Silver" : "Bronze";
                            Plugin.Logger.LogDebug($"searching for nuggetColor: {nuggetColor}");
                            break;
                        }
                    }
                }
                if (__result.Contains($"* {nuggetColor} Nugget  //  Price: $") && !__result.Contains("Gold Store"))
                {
                    __result = __result.Replace($"* {nuggetColor} Nugget  //  Price: $", $"\nLCGoldScrapMod\nType GOLD STORE for more upgrades and cosmetics!\n* {nuggetColor} Nugget  //  Price: $");
                }
            }
            if (__result.Contains("[goldScrapHostPlayer]"))
            {
                __result = __result.Replace("[goldScrapHostPlayer]", StartOfRound.Instance.allPlayerScripts[0].playerUsername);
            }
            if (__result.Contains("[goldScrapDateCaseSecretQuestion]"))
            {
                if (Plugin.specialDateCase >= 0)
                {
                    __result = __result.Replace("[goldScrapDateCaseSecretQuestion]", "10) O Spirits of Gold, reveal your secrets to me!\n");
                }
                else
                {
                    __result = __result.Replace("[goldScrapDateCaseSecretQuestion]", "");
                }
            }
            if (__result.Contains("[goldScrapDateCaseSecretCode]"))
            {
                string codeString = General.ConvertSpecialDateCase(Plugin.specialDateCase);
                if (codeString == "0000")
                {
                    __result = __result.Replace("[goldScrapDateCaseSecretCode]", "The spirits have nothing to say to you, mortal.");
                }
                else
                {
                    __result = __result.Replace("[goldScrapDateCaseSecretCode]", $"Today's stars align to form {codeString}");
                }
            }
            if ((Plugin.specialDateCase == 1 || Plugin.specialDateCase == 4) && (__result.Contains("You have requested to order") || __result.Contains("Ordered ")))
            {
                Plugin.Logger.LogDebug($"caught special node");
                ItemData foundItem = null;
                int foundIndex = -1;
                for (int j = 0; j < StoreAndTerminal.allGoldStoreItemData.Length; j++)
                {
                    if (__result.Contains(StoreAndTerminal.allGoldStoreItemData[j].displayName))
                    {
                        foundItem = StoreAndTerminal.allGoldStoreItemData[j];
                        foundIndex = StoreAndTerminal.allGoldStoreItemData[j].localUnlockableID != -1 ? StoreAndTerminal.allGoldStoreItemData[j].localUnlockableID : StoreAndTerminal.allGoldStoreItemData[j].localItemsListIndex;
                        break;
                    }
                }
                if (foundItem != null && foundIndex != -1)
                {
                    Plugin.Logger.LogDebug($"foundItem: {foundItem} | foundIndex: {foundIndex}");
                    int indexOf = __result.IndexOf(foundItem.displayName);
                    string substring = __result.Substring(indexOf);
                    __result = __result.Substring(0, indexOf) + RuntimeChanges.ReplaceGoldInName(substring, Plugin.specialDateCase, foundIndex % 2 == 0, RuntimeChanges.DoesItemHaveAlts(foundItem, Plugin.specialDateCase), printDebug: false);
                }
            }



            //Runtime GoldStore text
            if (__result.Contains("Welcome to the Gold Store!"))
            {
                for (int i = 0; i < __result.Length; i++)
                {
                    string buildingString = null;
                    if (buildingString == null && __result[i].ToString() == "[")
                    {
                        for (int j = i; j < i + 5; j++)
                        {
                            buildingString += __result[j];
                            if (__result[j].ToString() == "]")
                            {
                                break;
                            }
                        }
                        string numberString = buildingString;
                        numberString = numberString.Remove(numberString.Length - 1);
                        numberString = numberString.Remove(0, 1);
                        bool successfulInt = true;
                        if (int.TryParse(numberString, out int parsedIndex))
                        {
                            if (parsedIndex < 0 || parsedIndex >= StoreAndTerminal.allGoldStoreItemData.Length)
                            {
                                successfulInt = false;
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogError($"FAILED PARSE!!!!!!!");
                            successfulInt = false;
                        }
                        string textToWrite = buildingString;
                        if (successfulInt)
                        {
                            ItemData dataToWorkFrom = StoreAndTerminal.allGoldStoreItemData[parsedIndex];
                            if (dataToWorkFrom == null || (dataToWorkFrom.localBuyItemIndex == -1 && dataToWorkFrom.localUnlockableID == -1))
                            {
                                Plugin.Logger.LogError($"invalid dataToWorkFrom, null? {dataToWorkFrom == null}");
                            }
                            else if (dataToWorkFrom.localBuyItemIndex != -1 && dataToWorkFrom.itemProperties != null)
                            {
                                textToWrite = $"* {dataToWorkFrom.itemProperties.itemName} // ";
                                if (!string.IsNullOrEmpty(dataToWorkFrom.alreadyPurchasedText) && dataToWorkFrom.itemProperties.creditsWorth < 0)
                                {
                                    textToWrite += dataToWorkFrom.alreadyPurchasedText;
                                }
                                else
                                {
                                    textToWrite += GetDisplayTextWithSale(textToWrite, parsedIndex, dataToWorkFrom);
                                }
                            }
                            else if (dataToWorkFrom.localUnlockableID != -1 && dataToWorkFrom.unlockableProperties != null && !string.IsNullOrEmpty(dataToWorkFrom.unlockableProperties.unlockableName))
                            {
                                textToWrite = $"* {dataToWorkFrom.unlockableProperties.unlockableName} // ";
                                if (!string.IsNullOrEmpty(dataToWorkFrom.alreadyPurchasedText) && StartOfRound.Instance.unlockablesList.unlockables[dataToWorkFrom.localUnlockableID].hasBeenUnlockedByPlayer)
                                {
                                    textToWrite += dataToWorkFrom.alreadyPurchasedText;
                                }
                                else
                                {
                                    textToWrite += GetDisplayTextWithSale(textToWrite, parsedIndex, dataToWorkFrom);
                                }
                            }
                        }
                        __result = __result.Replace(buildingString, textToWrite);
                        buildingString = null;
                    }
                }
            }



            //CreditsCard text replacement
            ItemData cardItem = CreditsCardManager.instance.creditsCardItem;
            if (__result.Contains("* Credits Card  //  Price: $-1"))
            {
                __result = __result.Replace("Price: $-1", "Not in stock");
            }
            if (__result.Contains("[goldScrapCreditsCardSellingPrice]"))
            {
                string currentPrice = cardItem.itemProperties.creditsWorth >= 0 ? $"${cardItem.itemProperties.creditsWorth}" : cardItem.alreadyPurchasedText;
                __result = __result.Replace("[goldScrapCreditsCardSellingPrice]", currentPrice);
            }
            if (__result.Contains("[goldScrapCreditsCardBuyingPrice]"))
            {
                string currentPrice = cardItem.itemProperties.creditsWorth >= 0 ? $"Price: ${cardItem.itemProperties.creditsWorth / 100f * __instance.itemSalesPercentages[CreditsCardManager.instance.creditsCardNode.buyItemIndex]}" : cardItem.alreadyPurchasedText;
                __result = __result.Replace("[goldScrapCreditsCardBuyingPrice]", currentPrice);
            }
            if (__result.Contains("[goldScrapCreditsCardSalesPercentage]"))
            {
                string textToReplace = null;
                if (cardItem.itemProperties.creditsWorth >= 0)
                {
                    textToReplace = $"({100 - __instance.itemSalesPercentages[CreditsCardManager.instance.creditsCardNodeBuy.buyItemIndex]}% OFF!)";
                }
                __result = __result.Replace("[goldScrapCreditsCardSalesPercentage]", textToReplace);
            }



            //DirectoryLOG text replacement
            if (__result.Contains("[goldScrapDLOGHelp]") && StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].hasBeenUnlockedByPlayer)
            {
                if (DLOGManager.selectedSpace == -1)
                {
                    __result = __result.Replace("[goldScrapDLOGIntro]", DLOGManager.instance.introTextList.allStrings[DLOGManager.selectedIntro]);
                    __result = __result.Replace("[goldScrapDLOGHelp]", DLOGManager.instance.helpTextList.allStrings[DLOGManager.selectedHelp]);
                    __result = __result.Replace("[goldScrapDLOGOutro]", DLOGManager.instance.outroTextList.allStrings[DLOGManager.selectedOutro]);
                }
                else
                {
                    __result = "\n\n\n\n" + DLOGManager.instance.spaceTextList.allStrings[DLOGManager.selectedSpace] + "\n\n";
                }
            }



            //CatOGold & GoldWeather text replacement
            if (__result.Contains("[goldScrapCatOGoldInfo]"))
            {
                int currentInfo = !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID].hasBeenUnlockedByPlayer ? 0 : !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID].inStorage ? 1 : 2;
                __result = __result.Replace("[goldScrapCatOGoldInfo]", StoreAndTerminal.catOGoldInfoStrings.allStrings[currentInfo]);
            }
            if (__result.Contains("Welcome to the exomoons catalogue."))
            {
                if (RarityManager.selectedLevel != -1)
                {
                    int levelIDInList = -1;
                    for (int i = 0; i < __instance.moonsCatalogueList.Length; i++)
                    {
                        SelectableLevel level = __instance.moonsCatalogueList[i];
                        if (level.levelID == RarityManager.selectedLevel)
                        {
                            levelIDInList = i;
                            break;
                        }
                    }
                    Plugin.Logger.LogDebug($"deduced levelIDInList {levelIDInList}");
                    if (levelIDInList != -1)
                    {
                        string[] lines = __result.Split('\n');
                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            string selectedLevelName = __instance.moonsCatalogueList[levelIDInList].name;
                            string normalName = selectedLevelName.Remove(selectedLevelName.Length - 5, 5);
                            string betweenBrackets = RarityManager.isSaleFever ? "Gold Sale" : "Gold Fever";
                            if (line.Contains(normalName))
                            {
                                string textToAdd = line[line.Length - 1] == ' ' ? $"({betweenBrackets})" : $" ({betweenBrackets})";
                                __result = __result.Replace(line, line += textToAdd);
                                break;
                            }
                        }
                    }
                }
            }



            //[Custom Terminal Scan] FALSE
            if (!Configs.fixScan.Value && __result.Contains(" objects outside the ship, totalling at an approximate value of $"))
            {
                if (!General.DoesMoonHaveGoldScrap()) return;

                Plugin.Logger.LogInfo("Config [Custom Terminal Scan] is set to FALSE. Using vanilla scan.");
                int goldScrapAmount = 0;
                int goldScrapValue = 0;

                foreach (GoldScrapObject goldScrap in Object.FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None))
                {
                    if (goldScrap.item != null && goldScrap.item.itemProperties.name.Contains("LCGoldScrapMod") && !goldScrap.item.isInShipRoom)
                    {
                        if (goldScrap.item.isInFactory)
                        {
                            goldScrapAmount++;
                        }
                        goldScrapValue += goldScrap.item.scrapValue != 0 ? goldScrap.item.scrapValue : Random.Range((int)(goldScrap.item.itemProperties.minValue * RoundManager.Instance.scrapValueMultiplier), (int)(goldScrap.item.itemProperties.maxValue * RoundManager.Instance.scrapValueMultiplier));
                    }
                }

                string textToAdd = null;
                if (StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].hasBeenUnlockedByPlayer)
                {
                    textToAdd = $"\nGold Scrap inside facility: {goldScrapAmount}\nValue of Gold Scrap: ${goldScrapValue}\nGold Scrap Score: {DLOGManager.goldScrapScore}\n\n";
                }
                else if (!StartOfRound.Instance.shipDoorsEnabled || !StartOfRound.Instance.currentLevel.planetHasTime)
                {
                    textToAdd = "\nDid you know...?\nThe Gold Store's Directory LOG allows you to find gold scrap much more efficiently!\n\n";
                }
                __result = __result + textToAdd;
            }
        }
    }

    private static string GetDisplayTextWithSale(string textToWrite, int parsedIndex, ItemData dataToWorkFrom)
    {
        int sale = RarityManager.allItemPricePercentages[parsedIndex];
        float salesPercentage = (float)(sale / 100f);
        Plugin.Logger.LogDebug($"parsedIndex: {parsedIndex} | sale of {100 - sale}");
        int displayPrice = 0;
        if (dataToWorkFrom.itemProperties != null)
        {
            displayPrice = (int)((float)dataToWorkFrom.itemProperties.creditsWorth * salesPercentage);
        }   
        else if (dataToWorkFrom.storeTerminalNodes != null && dataToWorkFrom.storeTerminalNodes.Length > 0 && dataToWorkFrom.storeTerminalNodes[0] != null)
        {
            displayPrice = dataToWorkFrom.storeTerminalNodes[0].itemCost;
        }
        textToWrite = $"Price: ${displayPrice}";
        if (sale != 100)
        {
            textToWrite += $"   ({100 - sale}% OFF!)";
        }
        return textToWrite;
    }



    //Set the CreditsCard amount, availability, and sale
    [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
    public class NewTerminalParseSentence
    {
        [HarmonyPostfix]
        public static void ParsePlayerSentencePostfix(Terminal __instance, TerminalNode __result)
        {
            if (__result == CreditsCardManager.instance.creditsCardNode)
            {
                __instance.playerDefinedAmount = 1;
            }
        }
    }
    [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
    public class NewTerminalLoadNode
    {
        [HarmonyPostfix]
        public static void LoadNodePostfix(Terminal __instance, TerminalNode node)
        {
            if (node == CreditsCardManager.instance.creditsCardNode && CreditsCardManager.instance.creditsCardItem.itemProperties.creditsWorth < 0)
            {
                __instance.LoadNewNode(CreditsCardManager.instance.creditsCardNodeUnavailable);
            }
            else if (node == CreditsCardManager.instance.creditsCardNodeBuy)
            {
                CreditsCardManager.instance.TakeCreditsCardOutOfRotationServerRpc();
            }
            else if (node == DLOGManager.instance.dLogScanNode)
            {
                if (!StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].hasBeenUnlockedByPlayer)
                {
                    __instance.LoadNewNode(DLOGManager.instance.dLogUnavailableNode);
                }
                else if (DLOGManager.selectedSpace != -1 && DLOGManager.instance.tutorialUnread)
                {
                    DLOGManager.instance.SetTutorialStateServerRpc();
                }
            }
            else if (node.name == "JeanetteNode" && !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].hasBeenUnlockedByPlayer)
            {
                __instance.LoadNewNode(__instance.terminalNodes.specialNodes[9]);
            }
            else if (node == StoreAndTerminal.goldCrownBuyNode && StartOfRound.Instance.connectedPlayersAmount == 0)
            {
                __instance.LoadNewNode(StoreAndTerminal.goldCrownSingleplayerInfo);
            }
        }
    }
    [HarmonyPatch(typeof(Terminal), "SetItemSales")]
    public class NewTerminalSetSales
    {
        [HarmonyPostfix]
        public static void SetItemSalesPostfix(Terminal __instance)
        {
            if (__instance.IsServer && __instance.buyableItemsList.Contains(CreditsCardManager.instance.creditsCardItem.itemProperties) && StartOfRound.Instance.currentLevel.planetHasTime)
            {
                CreditsCardManager.instance.StartSaleReroll();
            }
        }
    }
}
