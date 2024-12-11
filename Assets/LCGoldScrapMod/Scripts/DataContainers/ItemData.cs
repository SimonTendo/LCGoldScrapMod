using UnityEngine;

[CreateAssetMenu(menuName = "LCGoldScrapMod/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("Item")]
    [Tooltip("The LCGoldScrapMod itemProperties the game will read from on the host.")]
    public Item itemProperties;
    [Tooltip("If true, it will be registered to levels and its itemProperties' weight, max-, and minValues will be put through the configMultipliers.\nIf false, only weight will be multiplied by its configMultiplier.")]
    public bool isScrap = true;
    [Tooltip("If true, its value will not be counted when collected in levels and when sold on the Company desk.")]
    public bool isStoreItem = false;

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
    [Header("Host values")]
    [HideInInspector]
    public int localMaxValue = 0;
    [HideInInspector]
    public int localMinValue = 0;
}