using BepInEx.Logging;
using BepInEx.Configuration;

public class Config
{
    private static ManualLogSource Logger = Plugin.Logger;
    
    public static string selectedMoonsString;
    public static bool hostToolRebalance;

    public static ConfigEntry<string> selectedLevels;
    public static ConfigEntry<float> minValueMultiplier;
    public static ConfigEntry<float> maxValueMultiplier;
    public static ConfigEntry<float> rarityMultiplier;
    public static ConfigEntry<float> weightMultiplier;
    public static ConfigEntry<bool> moddedMoonSpawn;
    public static ConfigEntry<int> moddedMoonRarity;
    public static ConfigEntry<int> moddedMoonCost;
    public static ConfigEntry<bool> toolsRebalance;
    public static ConfigEntry<bool> newSFX;
    public static ConfigEntry<bool> replaceSFX;
    public static ConfigEntry<bool> replaceEnemySFX;
    public static ConfigEntry<bool> fixScan;
    public static ConfigEntry<bool> sillyScrap;

    public Config(ConfigFile cfg)
    {
        selectedLevels = cfg.Bind(
            "Customization",
            "Selected Moons",
            "All",
            "Select every vanilla Lethal Company level you want gold scrap to be able to appear on.\nSelect moons by typing their full name, without the numbers (for example 'Experimentation Offense Dine'), or use 'All' or 'None'. Lobby host's values are used."
            );
        minValueMultiplier = cfg.Bind(
            "Customization",
            "Multiplier Minimum Value",
            2.5f,
            "Set by how much gold scrap multiplies the lowest random value of its original item. Cannot be negative. Lobby host's values are used."
            );
        maxValueMultiplier = cfg.Bind(
            "Customization",
            "Multiplier Maximum Value",
            2f,
            "Set by how much gold scrap multiplies the highest random value of its original item. Cannot be negative. Lobby host's values are used."
            );
        rarityMultiplier = cfg.Bind(
            "Customization",
            "Multiplier Rarity",
            3f,
            "Set the multiplier for the spawn chance of gold scrap on vanilla Lethal Company Moons. Cannot be 0 or negative. Lobby host's values are used."
            );
        weightMultiplier = cfg.Bind(
            "Customization",
            "Multiplier Weight",
            1.5f,
            "Set the multiplier for the weight of gold scrap, most of which is the original item's weight multiplied by this amount. Cannot be negative. Lobby host's values are used."
            );
        moddedMoonSpawn = cfg.Bind(
            "Modded Moons",
            "Modded Spawn Enabled",
            true,
            "Set whether gold scrap can spawn on moons added through mods, or if it will only spawn on Lethal Company's vanilla moons. Lobby host's values are used."
            );
        moddedMoonRarity = cfg.Bind(
            "Modded Moons",
            "Modded Spawn Rarity",
            1,
            "Set how rare all gold scrap is on moons added through mods. Cannot be 0 or negative. Lobby host's values are used."
            );
        moddedMoonCost = cfg.Bind(
            "Modded Moons",
            "Modded Minimum Cost",
            0,
            "Set the travel cost that a moon added through mods needs to pass before gold scrap can spawn there. Lobby host's values are used."
            );
        newSFX = cfg.Bind(
            "Other",
            "Other New Sounds",
            true,
            "Set whether specific items, currently only the Golden Glove, uses new experimental sounds I myself made or reuses Lethal Company sounds."
            );
        replaceSFX = cfg.Bind(
            "Other",
            "Other Sound Effects",
            true,
            "Set whether gold scrap that makes unique noise, such as the Golden Airhorn, uses sounds from other media or vanilla Lethal Company item sounds."
            );
        replaceEnemySFX = cfg.Bind(
            "Other",
            "Other Enemy Sounds",
            false,
            "Set whether gold scrap that is based on enemies or hazards, such as the Gold Spring, uses sounds from other media or vanilla Lethal Company enemy sounds."
            );
        fixScan = cfg.Bind(
            "Other",
            "Other Scan Command",
            false,
            "Choose whether the terminal's 'Scan' command uses a custom scan to search for scrap, or uses vanilla Lethal Company's scan, which has the possibility of breaking due to a bug."
            );
        toolsRebalance = cfg.Bind(
            "Other",
            "Other Tools Balance",
            false,
            "Set whether gold scrap with specific gameplay mechanics, for example through use with the left mouse button, will not be struck by lightning at the cost of a slight penalty.\nLobby host's values are used."
            );
        sillyScrap = cfg.Bind(
            "Other",
            "Other Silly Scrap",
            false,
            "Set whether gold scrap uses placeholder models and sounds, or switches to Lethal Company assets if loaded. Will overwrite [Other Sound Effects] and [Other Enemy Sounds]."
            );
    }

    public static void LoadAndDisplayConfigs()
    {
        selectedMoonsString = selectedLevels.Value.ToLower();
        if (CheckValidSelectedLevels(selectedMoonsString) && !selectedMoonsString.Contains("all") && selectedMoonsString.Contains("none") && !moddedMoonSpawn.Value)
        {
            Plugin.goldScrapSpawnEnabled = false;
            Logger.LogInfo("Config [Selected Moons] is set to None and config [Modded Spawn Enabled] is set to FALSE. Gold scrap will not spawn :(");
        }

        //Customization:
        //[Selected Moons]
        if (!CheckValidSelectedLevels(selectedMoonsString))
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogWarning("Config [Selected Moons] does not contain any valid level names, likely due to typos. Resetting to default value.");
            selectedLevels.Value = "All";
            selectedMoonsString = selectedLevels.Value.ToLower();
        }
        else
        {
            if (selectedMoonsString.Contains("all"))
            {
                if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo("Config [Selected Moons] is set to All");
            }
            else if (selectedMoonsString.Contains("none") && !selectedMoonsString.Contains("all"))
            {
                if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo("Config [Selected Moons] is set to None");
            }
            else if (!selectedMoonsString.Contains("none") && !selectedMoonsString.Contains("all"))
            {
                if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Selected Moons] received input: {selectedMoonsString}");
            }
        }
        //[Multiplier Minimum Value]
        if (minValueMultiplier.Value < 0)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogWarning($"Config [Multiplier Minimum Value] is set to an invalid value of {minValueMultiplier.Value}. Resetting to default value.");
            minValueMultiplier.Value = 2.5f;
        }
        else
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Multiplier Minimum Value] is set to a value of {minValueMultiplier.Value}");
        }
        //[Multiplier Maximum Value]
        if (maxValueMultiplier.Value < 0)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogWarning($"Config [Multiplier Maximum Value] is set to an invalid value of {maxValueMultiplier.Value}. Resetting to default value.");
            maxValueMultiplier.Value = 2f;
        }
        else
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Multiplier Maximum Value] is set to a value of {maxValueMultiplier.Value}");
        }
        //[Multiplier Rarity]
        if ((selectedMoonsString.Contains("all") || !selectedMoonsString.Contains("all") && !selectedMoonsString.Contains("none")) && rarityMultiplier.Value <= 0)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogWarning($"Config [Multiplier Rarity] is set to an invalid value of {rarityMultiplier.Value}. Resetting to default value.");
            rarityMultiplier.Value = 3f;
        }
        else if ((selectedMoonsString.Contains("all") || !selectedMoonsString.Contains("all") && !selectedMoonsString.Contains("none")) && rarityMultiplier.Value > 0)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Multiplier Rarity] is set to a value of {rarityMultiplier.Value}");
        }
        //[Multiplier Weight]
        if (weightMultiplier.Value < 0)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogWarning($"Config [Multiplier Weight] is set to an invalid value of {weightMultiplier.Value}. Resetting to default value.");
            weightMultiplier.Value = 1.5f;
        }
        else
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Multiplier Weight] is set to a value of {weightMultiplier.Value}");
        }

        //Modded Moons:
        //[Modded Spawn Enabled]
        if (moddedMoonSpawn.Value)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo("Config [Modded Spawn Enabled] is set to TRUE. Gold scrap can spawn on moons added through mods.");
        }
        else
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo("Config [Modded Spawn Enabled] is set to FALSE. Gold scrap will only spawn on vanilla Lethal Company moons.");
        }
        //[Modded Spawn Rarity]
        if (moddedMoonSpawn.Value)
        {
            if (moddedMoonRarity.Value < 1)
            {
                if (Plugin.goldScrapSpawnEnabled) Logger.LogWarning($"Config [Modded Spawn Rarity] is set to an invalid value of {moddedMoonRarity.Value}. Resetting to default value.");
                moddedMoonRarity.Value = 1;
            }
            else
            {
                if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Modded Spawn Rarity] is set to a value of {moddedMoonRarity.Value}");
            }
        }
        //[Modded Minimum Cost]
        if (moddedMoonSpawn.Value)
        {
            if (Plugin.goldScrapSpawnEnabled) Logger.LogInfo($"Config [Modded Minimum Cost] is set to a value of {moddedMoonCost.Value}");
        }

        //Other:
        //[Other New Sounds]
        if (newSFX.Value && !sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other New Sounds] is set to TRUE. Loading new Golden Glove sounds...");
        }
        else if (!newSFX.Value && !sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other New Sounds] is set to FALSE. Vanilla Lethal Company sounds will be loaded.");
        }
        //[Other Sound Effects]
        if (replaceSFX.Value && !sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other Sound Effects] is set to TRUE. Loading unique item sounds...");
        }
        else if (!replaceSFX.Value && !sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other Sound Effects] is set to FALSE. Vanilla Lethal Company sounds will be loaded.");
        }
        //[Other Enemy Sounds]
        if (replaceEnemySFX.Value && !sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other Enemy Sounds] is set to TRUE. Loading unique enemy-item sounds...");
        }
        else if (!replaceEnemySFX.Value && !sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other Enemy Sounds] is set to FALSE. Vanilla Lethal Company enemy sounds will be loaded.");
        }
        //[Other Scan Command]
        if (fixScan.Value)
        {
            Logger.LogInfo("Config [Other Scan Command] is set to TRUE. The Terminal's scan command will use this mod's custom scan.");
        }
        else
        {
            Logger.LogInfo("Config [Other Scan Command] is set to FALSE. The Terminal will use the vanilla Lethal Company scan.");
        }
        //[Other Tools Balance]
        if (toolsRebalance.Value)
        {
            Logger.LogInfo("Config [Other Tools Balance] is set to TRUE. Gold scrap with gameplay uses will avoid lightning strikes, but get an extra penalty.");
        }
        else
        {
            Logger.LogInfo("Config [Other Tools Balance] is set to FALSE. Gold scrap with gameplay uses will function as intended, by getting struck by lightning with no other penalty.");
        }
        //[Silly Scrap]
        if (sillyScrap.Value)
        {
            Logger.LogInfo("Config [Other Silly Scrap] is set to TRUE. Loading silliness...");
        }
    }

    public static bool CheckValidSelectedLevels(string selection)
    {
        if (Plugin.suspectedLevelListLength == 12)
        {
            if (!selection.Contains("all") &&
            !selection.Contains("none") &&
            !selection.Contains("experimentation") &&
            !selection.Contains("assurance") &&
            !selection.Contains("vow") &&
            !selection.Contains("offense") &&
            !selection.Contains("march") &&
            !selection.Contains("rend") &&
            !selection.Contains("dine") &&
            !selection.Contains("titan") &&
            !selection.Contains("adamance") &&
            !selection.Contains("embrion") &&
            !selection.Contains("artifice"))
            {
                return false;
            }
        }
        else if (Plugin.suspectedLevelListLength == 8)
        {
            if (!selection.Contains("all") &&
            !selection.Contains("none") &&
            !selection.Contains("experimentation") &&
            !selection.Contains("assurance") &&
            !selection.Contains("vow") &&
            !selection.Contains("offense") &&
            !selection.Contains("march") &&
            !selection.Contains("rend") &&
            !selection.Contains("dine") &&
            !selection.Contains("titan"))
            {
                return false;
            }
        }
        return true;
    }

    public static void SetCustomGoldScrapValues()
    {
        foreach (ItemData item in Plugin.allGoldScrap.allItemData)
        {
            if (item.isScrap)
            {
                item.localMinValue = (int)(item.defaultMinValue * minValueMultiplier.Value);
                item.itemProperties.minValue = item.localMinValue;
                item.localMaxValue = (int)(item.defaultMaxValue * maxValueMultiplier.Value);
                item.itemProperties.maxValue = item.localMaxValue;
                if (item.itemProperties.minValue > item.itemProperties.maxValue)
                {
                    item.itemProperties.maxValue = item.itemProperties.minValue;
                }
            }
        }
    }

    public static bool IsLevelSelected(string selectedLevelName)
    {
        if (selectedMoonsString.Contains("all"))
        {
            return true;
        }
        if (selectedMoonsString.Contains("none"))
        {
            return false;
        }

        string NameToCheck = selectedLevelName.ToLower();
        if (selectedMoonsString.Contains(NameToCheck))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static float CalculateWeightCustom(float weightDefault, float weightMultiplier)
    {
        return ((weightDefault - 1) * weightMultiplier) + 1;
    }
}
