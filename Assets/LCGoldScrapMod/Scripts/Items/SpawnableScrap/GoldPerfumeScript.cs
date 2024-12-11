using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class GoldPerfumeScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    public StringList fakeBrandNames;
    public ScanNodeProperties scanNode;

    public override void Start()
    {
        base.Start();
        if (IsServer)
        {
            RerollHeaderText();
        }
    }

    private void RerollHeaderText()
    {
        CheckTextToReplace(Random.Range(0, fakeBrandNames.allStrings.Length));
    }

    private void CheckTextToReplace(int index)
    {
        string textToReplace = fakeBrandNames.allStrings[index];
        if (textToReplace.Contains("[dungeonType]"))
        {
            string dungeonType = GetCurrentDungeonType();
            if (dungeonType == null)
            {
                RerollHeaderText();
                return;
            }
            else
            {
                textToReplace = textToReplace.Replace("[dungeonType]", dungeonType);
            }
        }
        else if (textToReplace.Contains("[currentLevel]"))
        {
            string currentLevel = GetCurrentLevel();
            if (currentLevel == null)
            {
                RerollHeaderText();
                return;
            }
            else
            {
                textToReplace = textToReplace.Replace("[currentLevel]", currentLevel);
            }
        }
        else if (textToReplace.Contains("[randomPlayer]"))
        {
            textToReplace = textToReplace.Replace("[randomPlayer]", General.GetRandomPlayer().playerUsername);
        }
        else if (textToReplace.Contains("[profitQuota]"))
        {
            textToReplace = textToReplace.Replace("[profitQuota]", TimeOfDay.Instance.profitQuota.ToString());
        }
        else if (textToReplace.Contains("[currentWeather]"))
        {
            string currentWeather = GetCurrentWeather();
            if (currentWeather == null)
            {
                RerollHeaderText();
                return;
            }
            else
            {
                textToReplace = textToReplace.Replace("[currentWeather]", currentWeather);
            }
        }
        SyncNameClientRpc(textToReplace);
    }

    private string GetCurrentDungeonType()
    {
        int currentDungeon = RoundManager.Instance.currentDungeonType;
        switch (currentDungeon)
        {
            default:
                return null;
            case 0:
                return "Factory";
            case 1:
                return "Mansion";
            case 2:
                return "Factory";
            case 3:
                return "Factory";
            case 4:
                return "Mineshaft";
        }
    }

    private string GetCurrentLevel()
    {
        if (StartOfRound.Instance.currentLevelID < 0 || StartOfRound.Instance.currentLevelID > Plugin.suspectedLevelListLength)
        {
            return null;
        }
        for (int i = 0; i < StartOfRound.Instance.currentLevel.PlanetName.Length; i++)
        {
            if (StartOfRound.Instance.currentLevel.PlanetName[i] == ' ')
            {
                return StartOfRound.Instance.currentLevel.PlanetName.Substring(i + 1);
            }
        }
        return null;
    }

    private string GetCurrentWeather()
    {
        LevelWeatherType currentWeather = StartOfRound.Instance.currentLevel.currentWeather;
        if (currentWeather != LevelWeatherType.None && currentWeather != LevelWeatherType.Rainy && currentWeather != LevelWeatherType.Stormy && currentWeather != LevelWeatherType.Foggy && currentWeather != LevelWeatherType.Flooded && currentWeather != LevelWeatherType.Eclipsed)
        {
            return null;
        }
        if (currentWeather == LevelWeatherType.None)
        {
            return "Mild";
        }
        return currentWeather.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncNameClientRpc(scanNode.headerText, playerID);
    }

    [ClientRpc]
    private void SyncNameClientRpc(string perfumeName, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            scanNode.headerText = perfumeName;
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: {scanNode.headerText}");
        }
    }
}
