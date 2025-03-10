using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class DLOGManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static DLOGManager instance;

    public TerminalNode dLogScanNode;
    public TerminalNode dLogUnavailableNode;
    public StringList introTextList;
    public StringList helpTextList;
    public StringList outroTextList;
    public StringList spaceTextList;
    public bool tutorialUnread;

    private static int goldScrapAmount;
    private static int goldScrapValue;
    private static int goldScrapTwoHanded;
    private static int goldScrapTool;
    private static int goldScrapNearEntrance;
    private static float goldScrapWeight;
    public static string goldScrapScore;

    public static int selectedIntro = 0;
    public static int selectedHelp = 0;
    public static int selectedOutro = 0;
    public static int selectedSpace = -1;

    private void Awake()
    {
        instance = this;
    }

    public void SetTutorialText()
    {
        if (IsServer && !tutorialUnread)
        {
            instance.tutorialUnread = true;
            instance.SetTutorialStateClientRpc(tutorialUnread);
            instance.SetTextServerRpc(true, !StartOfRound.Instance.inShipPhase);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTutorialStateServerRpc()
    {
        if (tutorialUnread)
        {
            tutorialUnread = false;
        }
        SetTutorialStateClientRpc(tutorialUnread);
    }

    [ClientRpc]
    private void SetTutorialStateClientRpc(bool hostTutorialUnread)
    {
        tutorialUnread = hostTutorialUnread;
        Logger.LogDebug($"set tutorialState: {tutorialUnread}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTextServerRpc(bool reroll = true, bool rollDayText = true)
    {
        if (reroll)
        {
            if (rollDayText)
            {
                SetTextClientRpc(RollNewIntro(), RollNewHelp(), RollNewOutro(), -1, CalculateScore());
            }
            else
            {
                SetTextClientRpc(-1, -1, -1, RollNewSpace(), "F");
            }
        }
        else
        {
            SetTextClientRpc(selectedIntro, selectedHelp, selectedOutro, selectedSpace, goldScrapScore);
        }
    }

    [ClientRpc]
    private void SetTextClientRpc(int introIndex, int helpIndex, int outroIndex, int spaceIndex, string hostScore)
    {
        selectedIntro = introIndex;
        selectedHelp = helpIndex;
        selectedOutro = outroIndex;
        selectedSpace = spaceIndex;
        goldScrapScore = hostScore;
        Logger.LogDebug($"{selectedIntro} | {selectedHelp} | {selectedOutro} | {selectedSpace} | {goldScrapScore}");
    }

    private int RollNewIntro()
    {
        return Random.Range(0, introTextList.allStrings.Length);
    }

    private int RollNewHelp()
    {
        goldScrapAmount = 0;
        goldScrapValue = 0;
        goldScrapTwoHanded = 0;
        goldScrapTool = 0;
        goldScrapNearEntrance = 0;
        goldScrapWeight = 0;

        if (!StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
        {
            return Random.Range(49, helpTextList.allStrings.Length);
        }

        GoldScrapObject[] allGoldScrap = FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None);
        EntranceTeleport[] allEntrances = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.None);
        string specialFlag = null;

        foreach (GoldScrapObject goldScrap in allGoldScrap)
        {
            GrabbableObject item = goldScrap.item;
            if (item == null || !item.itemProperties.name.Contains("LCGoldScrapMod")) continue;
            if (!item.isInShipRoom && item.isInFactory)
            {
                goldScrapAmount++;
                goldScrapValue += goldScrap.item.scrapValue;
                goldScrapWeight += (goldScrap.item.itemProperties.weight - 1) * 100;
                if (item.itemProperties.twoHanded)
                {
                    goldScrapTwoHanded++;
                }
                foreach (EntranceTeleport entrance in allEntrances)
                {
                    if (Vector3.Distance(entrance.transform.position, item.transform.position) < 40)
                    {
                        goldScrapNearEntrance++;
                        break;
                    }
                }
                string itemName = item.itemProperties.name.Remove(item.itemProperties.name.Length - 14, 14);
                switch (itemName)
                {
                    case "GoldBolt":
                        specialFlag = itemName;
                        break;
                    case "GoldenAirhorn":
                        specialFlag = itemName;
                        break;
                    case "GoldRegister":
                        specialFlag = itemName;
                        break;
                    case "PurifiedMask":
                        specialFlag = itemName;
                        break;
                    case "GoldenBell":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "GoldenGlass":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "GoldSign":
                        specialFlag = itemName;
                        goldScrapTool++; 
                        break;
                    case "GolderBar":
                        specialFlag = itemName;
                        break;
                    case "CuddlyGold":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "Goldkeeper":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "GoldSpring":
                        specialFlag = itemName;
                        break;
                    case "GoldenGuardian":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "JacobsLadder":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "TatteredGoldSheet":
                        specialFlag = itemName;
                        break;
                    case "GoldenGirl":
                        specialFlag = itemName;
                        break;
                    case "Goldfish":
                        specialFlag = itemName;
                        break;
                    case "GoldRemote":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "JackInTheGold":
                        specialFlag = itemName;
                        break;
                    case "GoldBird":
                        specialFlag = itemName;
                        break;
                    case "GoldenClock":
                        specialFlag = itemName;
                        break;
                    case "Goldmine":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    case "GoldenGrenade":
                        specialFlag = itemName;
                        goldScrapTool++;
                        break;
                    default:
                        if (Random.Range(0, 100) >= 50) specialFlag = null;
                        break;
                }
            }
        }
        if (goldScrapAmount != 0)
        {
            switch (specialFlag)
            {
                case "GoldBolt":
                    return 0;
                case "GoldenAirhorn":
                    return 1;
                case "GoldRegister":
                    return 2;
                case "PurifiedMask":
                    return 3;
                case "GoldenBell":
                    return 4;
                case "GoldenGlass":
                    return 5;
                case "GoldSign":
                    return 6;
                case "GolderBar":
                    return 7;
                case "CuddlyGold":
                    return 8;
                case "Goldkeeper":
                    return 9;
                case "GoldSpring":
                    return 10;
                case "GoldenGuardian":
                    return 11;
                case "JacobsLadder":
                    return 12;
                case "TatteredGoldSheet":
                    return 13;
                case "GoldenGirl":
                    return 14;
                case "Goldfish":
                    return 15;
                case "GoldRemote":
                    return 16;
                case "JackInTheGold":
                    return 17;
                case "GoldBird":
                    return 18;
                case "GoldenClock":
                    return 19;
                case "Goldmine":
                    return 20;
                case "GoldenGrenade":
                    return 21;
                default:
                    int randomNr = Random.Range(0, 3);
                    int indexToReturn = 0;
                    switch (randomNr)
                    {
                        default:
                            indexToReturn = ReturnHelpValue();
                            break;
                        case 1:
                            indexToReturn = ReturnHelpHanded();
                            break;
                        case 2:
                            indexToReturn = ReturnHelpDistance();
                            break;
                    }
                    return indexToReturn;
                    
            }
        }
        return Random.Range(22, 25);
    }

    private int ReturnHelpValue()
    {
        if (goldScrapValue > 0 && goldScrapValue < 100)
        {
            return Random.Range(25, 27);
        }
        else if (goldScrapValue >= 100 && goldScrapValue < 200)
        {
            return Random.Range(27, 29);
        }
        else if (goldScrapValue >= 200 && goldScrapValue < 300)
        {
            return Random.Range(29, 31);
        }
        else if (goldScrapValue >= 300 && goldScrapValue < 400)
        {
            return Random.Range(31, 33);
        }
        else if (goldScrapValue >= 400 && goldScrapValue < 500)
        {
            return Random.Range(33, 35);
        }
        else if (goldScrapValue >= 500)
        {
            return Random.Range(35, 37);
        }
        return Random.Range(22, 25);
    }

    private int ReturnHelpHanded()
    {
        if (goldScrapTwoHanded == 0)
        {
            return Random.Range(37, 40);
        }
        return Random.Range(40, 43);
    }

    private int ReturnHelpDistance()
    {
        if (goldScrapNearEntrance == 0)
        {
            return Random.Range(46, 49);
        }
        return Random.Range(43, 46);
    }

    private string CalculateScore()
    {
        if (goldScrapAmount == 0)
        {
            return "F";
        }

        int scoreWeight = 1;

        //Add for value
        if (goldScrapValue >= 100)
        {
            scoreWeight++;
        }
        if (goldScrapValue >= 200)
        {
            scoreWeight++;
        }
        scoreWeight += goldScrapValue / 333;
        Logger.LogDebug($"value: {goldScrapValue}");

        //Subtract for two-handed
        if (goldScrapTwoHanded >= 1)
        {
            scoreWeight--;
        }
        if (goldScrapTwoHanded >= 3)
        {
            scoreWeight--;
        }
        if (goldScrapTwoHanded >= 5)
        {
            scoreWeight--;
        }
        if (goldScrapTwoHanded >= 7)
        {
            scoreWeight--;
        }
        if (goldScrapTwoHanded >= 10)
        {
            scoreWeight--;
        }
        Logger.LogDebug($"handed: {goldScrapTwoHanded}");

        //Add for tools
        if (goldScrapTool > 0)
        {
            scoreWeight++;
        }
        if (goldScrapTool >= 3)
        {
            scoreWeight++;
        }
        Logger.LogDebug($"tools: {goldScrapTool}");

        //Subtract for weight
        scoreWeight -= (int)goldScrapWeight / 64;
        Logger.LogDebug($"weight: {goldScrapWeight}");

        //Add for being close to entrance
        scoreWeight += goldScrapNearEntrance;
        Logger.LogDebug($"nearby: {goldScrapNearEntrance}");

        //Finally, clamp between max values and return
        scoreWeight = Mathf.Clamp(scoreWeight, 1, 7);
        Logger.LogDebug($"final score: {scoreWeight}");
        switch (scoreWeight)
        {
            default:
                return "F";
            case 1:
                return "D";
            case 2:
                return "C";
            case 3:
                return "B";
            case 4:
                return "A";
            case 5:
                return "S";
            case 6:
                return "S+";
            case 7:
                return "S++";
        }
    }

    private int RollNewOutro()
    {
        return Random.Range(0, outroTextList.allStrings.Length);
    }

    private int RollNewSpace()
    {
        if (tutorialUnread)
        {
            return spaceTextList.allStrings.Length - 1;
        }
        return Random.Range(0, spaceTextList.allStrings.Length - 1);
    }
}
