using UnityEngine;

[CreateAssetMenu(menuName = "LCGoldScrapMod/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("General")]
    [Tooltip("The folder name used to access this item/furniture with for setting up all the mod's data.\nMust match exactly, and cannot contain spaces.")]
    public string folderName = "FolderName";
    [Tooltip("The display name to show on the Item's itemName and UnlockableItem's unlockableName.\nPrimarily used to reset to as a default value.")]
    public string displayName = "Display Name";
    [Tooltip("Determines what values for hostDateCase this item's prefab will be influences by.\n\n0: More common on New Year's. 2: More common on World Art Day. 3: Does not appear on Freedom Day. 5: More common on Halloween.")]
    public int[] specialDateCases;

    [Header("Item")]
    [Tooltip("The LCGoldScrapMod itemProperties the game will read from on the host.")]
    public Item itemProperties;
    [Tooltip("If true, it will be registered to levels and its itemProperties' weight, max-, and minValues will be put through the configMultipliers.\nIf false, only weight will be multiplied by its configMultiplier.")]
    public bool isScrap = true;

    [Space(3f)]
    [Header("Defaults")]
    [Tooltip("The intended default weight not yet taking the default 1.5x weightMultiplier into account.\nUsually the weight of the original item, unless that has no weight.")]
    public float defaultWeight = 1;
    [Tooltip("The intended default maxValue not yet taking the default 2x maxValueMultiplier into account.\nUsually the maxValue of the original item.")]
    public int defaultMaxValue = 0;
    [Tooltip("The intended default minValue not yet taking the default 2.5x minValueMultiplier into account.\nUsually the minValue of the original item.")]
    public int defaultMinValue = 0;
    [Tooltip("The intended default rarity not yet taking the custom plusGain and minusLoss into account.\nRefer to 'GoldScrap - Items' document.")]
    public int defaultRarity = 3;

    [Space(3f)]
    [Header("Levels")]
    [Tooltip("The vanilla levels where this gold scrap can appear.\nTakes a calculation of 'Default Rarity - 1' into account.")]
    public GoldScrapLevels[] levelsToAddMinus;
    [Tooltip("The vanilla levels where this gold scrap can appear.\nOnly takes Default Rarity into account.")]
    public GoldScrapLevels[] levelsToAddDefault;
    [Tooltip("The vanilla levels where this gold scrap can appear.\nTakes a calculation of 'Default Rarity + 2' into account.")]
    public GoldScrapLevels[] levelsToAddPlus;
    [Tooltip("A custom positive gain or negative loss that will influence the Default Rarity of gold scrap on levels selected in 'Levels To Add Custom'.\nWill have no effect if Default Rarity is 2 or smaller and Custom Change is -2 or lower.")]
    public int customChange = 0;
    [Tooltip("The vanilla levels where this gold scrap can appear.\nTakes a calculation of 'Default Rarity + Custom Change' into account.")]
    public GoldScrapLevels[] levelsToAddCustom;

    [Space(3f)]
    [Header("Store")]
    [Tooltip("If true, its value will not be counted when collected in levels and when sold on the Company desk.")]
    public bool isStoreItem = false;
    [Tooltip("The intended default price not yet taking modifiers such as sales or configs into account.\nSet to -1 to not have this value be read during set-up.")]
    public int storeDefaultPrice = 100;
    [Tooltip("The cap that determines for how much this item or furniture can go be marked off during a Gold Fever sale.\nSet to -1 to have the Weather Multipliers take over.")]
    public int maxFeverSalePercentage = 90;
    [Tooltip("The text to display in case this item/furniture can no longer be bought, either thanks to already being unlocked in StartOfRound or having a negative price.")]
    public string alreadyPurchasedText = null;
    [Tooltip("The TerminalNodes for furniture containing the itemCost to modify in case of sales or configs.")]
    public TerminalNode[] storeTerminalNodes = new TerminalNode[2];
    [Tooltip("The UnlockableItem to be added to the list of StartOfRound unlockables.")]
    public UnlockableItem unlockableProperties;




    [Space(3f)]
    [Header("Local host values")]
    [HideInInspector]
    public int localItemsListIndex = -1;
    [HideInInspector]
    public int localMaxValue = 0;
    [HideInInspector]
    public int localMinValue = 0;
    [HideInInspector]
    public int localStorePrice = 0;
    [HideInInspector]
    public int localBuyItemIndex = -1;
    [HideInInspector]
    public int localUnlockableID = -1;
}