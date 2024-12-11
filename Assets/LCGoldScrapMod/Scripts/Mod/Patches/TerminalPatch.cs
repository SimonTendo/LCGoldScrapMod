using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using HarmonyLib;

public class TerminalPatch
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
                if (Config.replaceSFX.Value && Plugin.v50Compatible)
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
            string input = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
            if (!input.ToLower().Contains("gold"))
            {
                return true;
            }
            foreach (TerminalKeyword goldScrapKeyword in StoreAndTerminal.allGoldScrapKeywords.allKeywords)
            {
                if (input.StartsWith(goldScrapKeyword.word) && IsValidNode(goldScrapKeyword))
                {
                    Plugin.Logger.LogDebug($"intercepted input '{input}' with custom keyword {goldScrapKeyword.specialKeywordResult}");
                    string nodeName = goldScrapKeyword.name.Remove(goldScrapKeyword.name.Length - 7, 7).Remove(0, 13);
                    Plugin.Logger.LogDebug($"parsed name '{nodeName}'");
                    string excess = input.Remove(0, goldScrapKeyword.word.Length).ToLower();
                    Plugin.Logger.LogDebug($"found excess '{excess}'");
                    string amount = Regex.Match(excess, "\\d+").Value;
                    Plugin.Logger.LogDebug($"{amount}; null = {amount.Length == 0}");
                    if (excess.Contains("info"))
                    {
                        TerminalNode nodeToLoad = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{nodeName}/GoldScrapShop{nodeName}Info.asset");
                        LoadNodeCustom(__instance, nodeToLoad, true);
                        return false;
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
                        TerminalNode nodeToLoad = Plugin.CustomGoldScrapAssets.LoadAsset<TerminalNode>($"Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/{nodeName}/GoldScrapShop{nodeName}.asset");
                        LoadNodeCustom(__instance, nodeToLoad, false);
                        return false;
                    }
                    __instance.playerDefinedAmount = 1;
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
            if (Config.fixScan.Value && modifiedDisplayText.Contains("[scanForItems]"))
            {
                if (!General.DoesMoonHaveGoldScrap()) return true;

                Plugin.Logger.LogInfo("Config [Custom Terminal Scan] is set to TRUE. Calculating value using custom scan.");

                int normalScrapAmount = 0;
                int normalScrapValue = 0;
                int goldScrapAmount = 0;
                int goldScrapValue = 0;

                GrabbableObject[] allItems = Object.FindObjectsOfType<GrabbableObject>();
                foreach (GrabbableObject item in allItems)
                {
                    if (item.itemProperties.isScrap && !item.isInShipRoom && !item.isInElevator)
                    {
                        normalScrapAmount++;
                        normalScrapValue += item.scrapValue;
                    }

                    if (item.itemProperties.name.Contains("LCGoldScrapMod") && !item.isInShipRoom)
                    {
                        if (item.isInFactory)
                        {
                            goldScrapAmount++;
                        }
                        goldScrapValue += item.scrapValue;
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
            if (__result.Contains("* Gold Nugget  //  Price: $100") && !__result.Contains("Gold Store"))
            {
                __result = __result.Replace("* Gold Nugget  //  Price: $100", "\nLCGoldScrapMod:\n* Gold Nugget  //  Price: $100");
            }
            if (__result.Contains("* Gold Crown  //  Price: $5000") && !__result.Contains("Gold Store"))
            {
                __result = __result.Replace("* Gold Crown  //  Price: $5000", "* Gold Crown  //  Price: $5000\nType GOLD STORE for more upgrades and cosmetics!");
            }
            if (__result.Contains("[goldScrapHostPlayer]"))
            {
                __result = __result.Replace("[goldScrapHostPlayer]", StartOfRound.Instance.allPlayerScripts[0].playerUsername);
            }



            //CreditsCard text replacement
            if (__result.Contains("* Credits Card  //  Price: $0"))
            {
                __result = __result.Replace("Price: $0", "Not in stock");
            }
            if (__result.Contains("[goldScrapCreditsCardSellingPrice]"))
            {
                string currentPrice = CreditsCardManager.instance.creditsCardItem.creditsWorth != 0 ? $"${CreditsCardManager.instance.creditsCardItem.creditsWorth}" : "Not in stock";
                __result = __result.Replace("[goldScrapCreditsCardSellingPrice]", currentPrice);
            }
            if (__result.Contains("[goldScrapCreditsCardBuyingPrice]"))
            {
                string currentPrice = CreditsCardManager.instance.creditsCardItem.creditsWorth != 0 ? $"Price: ${CreditsCardManager.instance.creditsCardItem.creditsWorth / 100f * __instance.itemSalesPercentages[CreditsCardManager.instance.creditsCardNode.buyItemIndex]}" : "Not in stock";
                __result = __result.Replace("[goldScrapCreditsCardBuyingPrice]", currentPrice);
            }
            if (__result.Contains("[goldScrapCreditsCardSalesPercentage]"))
            {
                string textToReplace = null;
                if (CreditsCardManager.instance.creditsCardItem.creditsWorth != 0)
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
            if (__result.Contains("[goldScrapDLOGBuyingPrice]"))
            {
                string currentPrice = !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].hasBeenUnlockedByPlayer ? $"Price: ${StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.directoryLogID].shopSelectionNode.itemCost}" : "Accessed";
                __result = __result.Replace("[goldScrapDLOGBuyingPrice]", currentPrice);
            }



            //CatOGold & GoldWeather text replacement
            if (__result.Contains("[goldScrapCatOGoldBuyingPrice]"))
            {
                string currentPrice = !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID].hasBeenUnlockedByPlayer ? $"Price: ${StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.catOGoldID].shopSelectionNode.itemCost}" : "Domesticated";
                __result = __result.Replace("[goldScrapCatOGoldBuyingPrice]", currentPrice);
            }
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
                            if (line.Contains(normalName))
                            {
                                string textToAdd = line[line.Length - 1] == ' ' ? "(Gold Fever)" : " (Gold Fever)";
                                __result = __result.Replace(line, line += textToAdd);
                                break;
                            }
                        }
                    }
                }
            }



            //Purchased SafeBox
            if (__result.Contains("[goldScrapSafeBoxBuyingPrice]"))
            {
                string currentPrice = !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.safeBoxID].hasBeenUnlockedByPlayer ? $"Price: ${StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.safeBoxID].shopSelectionNode.itemCost}" : "Secured";
                __result = __result.Replace("[goldScrapSafeBoxBuyingPrice]", currentPrice);
            }



            //Purchased DiscoBallMusic
            if (__result.Contains("[groovyGoldPurchased]"))
            {
                string textToset = !StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.groovyGoldID].hasBeenUnlockedByPlayer ? $"Price: ${StartOfRound.Instance.unlockablesList.unlockables[StoreAndTerminal.groovyGoldID].shopSelectionNode.itemCost}" : "Unlocked";
                __result = __result.Replace("[groovyGoldPurchased]", textToset);
            }



            //[Custom Terminal Scan] FALSE
            if (!Config.fixScan.Value && __result.Contains(" objects outside the ship, totalling at an approximate value of $"))
            {
                if (!General.DoesMoonHaveGoldScrap()) return;

                Plugin.Logger.LogInfo("Config [Custom Terminal Scan] is set to FALSE. Using vanilla scan.");
                int goldScrapAmount = 0;
                int goldScrapValue = 0;

                foreach (GoldScrapObject goldScrap in Object.FindObjectsOfType<GoldScrapObject>())
                {
                    if (goldScrap.item != null && goldScrap.item.itemProperties.name.Contains("LCGoldScrapMod") && !goldScrap.item.isInShipRoom)
                    {
                        if (goldScrap.item.isInFactory)
                        {
                            goldScrapAmount++;
                        }
                        goldScrapValue += goldScrap.item.scrapValue;
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
            if (node == CreditsCardManager.instance.creditsCardNode && CreditsCardManager.instance.creditsCardItem.creditsWorth == 0)
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
        }
    }
    [HarmonyPatch(typeof(Terminal), "SetItemSales")]
    public class NewTerminalSetSales
    {
        [HarmonyPostfix]
        public static void SetItemSalesPostfix(Terminal __instance)
        {
            if (__instance.IsServer && __instance.buyableItemsList.Contains(CreditsCardManager.instance.creditsCardItem) && StartOfRound.Instance.currentLevel.planetHasTime)
            {
                CreditsCardManager.instance.StartSaleReroll();
            }
        }
    }
}
