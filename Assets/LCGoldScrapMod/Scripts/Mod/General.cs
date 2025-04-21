using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GameNetcodeStuff;
using BepInEx.Logging;
using STSharedAudioLib;
using static AssetsCollection;

public class General
{
    private static ManualLogSource Logger = Plugin.Logger;

    private static bool alreadyAddedItems = false;

    public static void AddAllItemsToAllItemsListAwake()
    {
        if (!alreadyAddedItems)
        {
            foreach (ItemData item in Plugin.allGoldGrabbableObjects)
            {
                item.itemProperties.spawnPrefab.AddComponent<GoldScrapObject>().data = item;
                if (item.isStoreItem)
                {
                    item.itemProperties.spawnPrefab.AddComponent<GoldStoreItem>();
                }
                item.localItemsListIndex = StartOfRound.Instance.allItemsList.itemsList.Count;
                StartOfRound.Instance.allItemsList.itemsList.Add(item.itemProperties);
                Logger.LogDebug($"Added {item.folderName} to allItemsList with localItemsListIndex {item.localItemsListIndex}");
            }
            alreadyAddedItems = true;
        }
    }

    public static void AddJacobsLadderToHelmetLights()
    {
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (player == null)
            {
                continue;
            }
            GameObject foundHelmetLight = FindHelmetLights(player.gameObject);
            GameObject jacobsLadderPrefab = Plugin.CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapAssets/JacobsLadder/JacobsLadderHelmetLight.prefab");
            if (foundHelmetLight == null || jacobsLadderPrefab == null)
            {
                Logger.LogError($"failed to AddJacobsLadderToHelmetLights for player {player} (foundHelmetLight? {foundHelmetLight != null})");
                return;
            }
            GameObject JacobsLadderHelmetLights = UnityEngine.Object.Instantiate(jacobsLadderPrefab, foundHelmetLight.transform, false);
            List<Light> HelmetLights = player.allHelmetLights.ToList();
            jacobsLadderFlashlightID = HelmetLights.Count; 
            HelmetLights.Add(JacobsLadderHelmetLights.GetComponent<Light>());
            HelmetLights[jacobsLadderFlashlightID].lightShadowCasterMode = LightShadowCasterMode.Everything;
            HelmetLights[jacobsLadderFlashlightID].shadows = LightShadows.Hard;
            HelmetLights[jacobsLadderFlashlightID].intensity = 400f;
            HelmetLights[jacobsLadderFlashlightID].enabled = false;
            player.allHelmetLights = HelmetLights.ToArray();
        }
    }

    public static GameObject FindHelmetLights(GameObject playerObject)
    {
        GameObject helmetLightsObject = null;
        try
        {
            GameObject scavengerModelObject = playerObject.transform.Find("ScavengerModel").gameObject;
            GameObject metarigObject = scavengerModelObject.transform.Find("metarig").gameObject;
            GameObject cameraContainerObject = metarigObject.transform.Find("CameraContainer").gameObject;
            GameObject mainCameraObject = cameraContainerObject.transform.Find("MainCamera").gameObject;
            helmetLightsObject = mainCameraObject.transform.Find("HelmetLights").gameObject;
        }
        catch (System.Exception e)
        {
            Logger.LogError($"error caught when trying to FindHelmetLights: {e}");
        }

        if (helmetLightsObject == null)
        {
            Logger.LogWarning($"{playerObject.name}'s HelmetLights GameObject could not be found. JacobsLadder likely will not work on them.");
        }

        return helmetLightsObject;
    }

    public static bool DoesMoonHaveGoldScrap(int moonLevelID = -1, bool checkScene = true)
    {
        if (moonLevelID < 0 || moonLevelID >= StartOfRound.Instance.levels.Length)
        {
            moonLevelID = StartOfRound.Instance.currentLevelID;
        }
        SelectableLevel level = StartOfRound.Instance.levels[moonLevelID];

        //Check all items in level data as first call, shouldn't fail unless people have wildly different configs
        if (!level.spawnEnemiesAndScrap || level.spawnableScrap == null || level.spawnableScrap.Count == 0)
        {
            Logger.LogDebug($"no scrap on level #{moonLevelID}");
            return false;
        }
        foreach (SpawnableItemWithRarity item in level.spawnableScrap)
        {
            if (item != null && item.spawnableItem != null && item.spawnableItem.name.Contains("LCGoldScrapMod"))
            {
                Logger.LogDebug($"level #{moonLevelID} item list returned true");
                return true;
            }
        }

        //As a failsafe, check to see if the current planet has any gold scrap that is not in the ship
        if (checkScene)
        {
            foreach (GoldScrapObject goldScrap in UnityEngine.Object.FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None))
            {
                if (goldScrap.item != null && !goldScrap.item.isInShipRoom)
                {
                    Logger.LogDebug("current scene not in ship returned true");
                    return true;
                }
            }
        }

        //If both fail, return false
        Logger.LogDebug($"DoesMoonHaveGoldScrap({moonLevelID}) returned false");
        return false;
    }

    public static bool IsMoonAccessible(int moonLevelID)
    {
        //Check if the Terminal has a route keyword with this levelID, if not, players cannot play it and thus this moon should not be counted
        TerminalKeyword routeKeyword = StoreAndTerminal.keywordRoute;
        if (routeKeyword == null || routeKeyword.compatibleNouns == null || routeKeyword.compatibleNouns.Length == 0)
        {
            Logger.LogWarning($"failed to find keywordRoute ({routeKeyword}) or compatibleNouns in DoesMoonHaveGoldScrap()");
            return false;
        }
        else
        {
            foreach (CompatibleNoun cNoun in routeKeyword.compatibleNouns)
            {
                if (cNoun == null || cNoun.result == null || cNoun.result.terminalOptions == null || cNoun.result.terminalOptions.Length == 0)
                {
                    continue;
                }
                for (int c = 0; c < cNoun.result.terminalOptions.Length; c++)
                {
                    CompatibleNoun tOption = cNoun.result.terminalOptions[c];
                    if (tOption == null || tOption.noun == null || string.IsNullOrEmpty(tOption.noun.word) || tOption.result == null)
                    {
                        continue;
                    }
                    if (tOption.noun.word == "confirm" && tOption.result.buyRerouteToMoon == moonLevelID)
                    {
                        Logger.LogDebug($"IsMoonAccessible() found buyRerouteToMoon #{tOption.result.buyRerouteToMoon} on {tOption.result}");
                        return true;
                    }
                }
            }
        }
        Logger.LogDebug($"IsMoonAccessible could not find CompatibleNoun with moonLevelID {moonLevelID}");
        return false;
    }

    public static void AddMetalObjectsRuntime(GrabbableObject item)
    {
        if (StartOfRound.Instance != null && StartOfRound.Instance.currentLevel != null && StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Stormy)
        {
            StormyWeather stormyWeatherInstance = UnityEngine.Object.FindAnyObjectByType<StormyWeather>();
            if (stormyWeatherInstance != null)
            {
                PrivateAccesser.GetPrivateField<List<GrabbableObject>>(stormyWeatherInstance, "metalObjects").Add(item);
                Logger.LogDebug($"added {item.gameObject.name} to currentLevel's currentWeather {StartOfRound.Instance.currentLevel.currentWeather.ToString()}.metalObjects");
            }
        }
    }

    public static PlayerControllerB GetRandomPlayer()
    {
        List<PlayerControllerB> activePlayers = new List<PlayerControllerB>();
        foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
        {
            if (player.isPlayerControlled)
            {
                activePlayers.Add(player);
            }
        }
        return activePlayers[UnityEngine.Random.Range(0, activePlayers.Count)];
    }

    public static int GetRandomLevelID(bool checkAccessible = true)
    {
        int randomID = UnityEngine.Random.Range(0, StartOfRound.Instance.levels.Length);
        if (checkAccessible)
        {
            int attempts = 0;
            while (attempts < 10 && !IsMoonAccessible(randomID))
            {
                randomID = UnityEngine.Random.Range(0, StartOfRound.Instance.levels.Length);
                attempts++;
            }
        }
        return randomID;
    }

    public static float GetClosestCollision(Vector3 position, Vector3 direction, float maxDistance)
    {
        if (Physics.Raycast(position, direction, out var hitInfo, maxDistance, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
            return hitInfo.distance - 1;
        }
        return maxDistance - 1;
    }

    public static void SyncUponJoin(int playerID)
    {
        Logger.LogDebug($"started SyncUponJoin() with playerID at index [{playerID}]");

        foreach (GoldenGirlScript goldenGirlScript in UnityEngine.Object.FindObjectsByType<GoldenGirlScript>(FindObjectsSortMode.None))
        {
            goldenGirlScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldPerfumeScript goldPerfumeScript in UnityEngine.Object.FindObjectsByType<GoldPerfumeScript>(FindObjectsSortMode.None))
        {
            goldPerfumeScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (JackInTheGoldScript jackInTheGoldScript in UnityEngine.Object.FindObjectsByType<JackInTheGoldScript>(FindObjectsSortMode.None))
        {
            jackInTheGoldScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldBirdScript goldBirdScript in UnityEngine.Object.FindObjectsByType<GoldBirdScript>(FindObjectsSortMode.None))
        {
            goldBirdScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenClockScript goldenClockScript in UnityEngine.Object.FindObjectsByType<GoldenClockScript>(FindObjectsSortMode.None))
        {
            goldenClockScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldmineScript goldmineScript in UnityEngine.Object.FindObjectsByType<GoldmineScript>(FindObjectsSortMode.None))
        {
            goldmineScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenHourglassScript goldenHourglassScript in UnityEngine.Object.FindObjectsByType<GoldenHourglassScript>(FindObjectsSortMode.None))
        {
            goldenHourglassScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenPickaxeScript goldenPickaxeScript in UnityEngine.Object.FindObjectsByType<GoldenPickaxeScript>(FindObjectsSortMode.None))
        {
            goldenPickaxeScript.SyncDurabilityServerRpc(false, -1, playerID);
        }

        foreach (GoldkeeperScript goldkeeperScript in UnityEngine.Object.FindObjectsByType<GoldkeeperScript>(FindObjectsSortMode.None))
        {
            goldkeeperScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (CrownScript crownScript in UnityEngine.Object.FindObjectsByType<CrownScript>(FindObjectsSortMode.None))
        {
            crownScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (SafeBoxScript safeBoxScript in UnityEngine.Object.FindObjectsByType<SafeBoxScript>(FindObjectsSortMode.None))
        {
            safeBoxScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldfatherClockScript goldfatherClockScript in UnityEngine.Object.FindObjectsByType<GoldfatherClockScript>(FindObjectsSortMode.None))
        {
            goldfatherClockScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenGloveScript goldenGloveScript in UnityEngine.Object.FindObjectsByType<GoldenGloveScript>(FindObjectsSortMode.None))
        {
            goldenGloveScript.SyncUponJoinServerRpc(playerID);
        }

        RarityManager.instance.SyncUponJoinServerRpc(playerID);
        CreditsCardManager.instance.SyncUponJoinServerRpc(playerID);
        DLOGManager.instance.SetTextServerRpc(false);
        GoldenHourglassManager.instance.SyncUponJoinServerRpc(playerID);
        GoldScrapNetworkHandler.instance.SendUnlockablesToNewPlayerServerRpc(playerID);
    }

    public static void SetMedalsOnJoin()
    {
        if (StartOfRound.Instance.unlockablesList.unlockables[MedalManager.goldMedalID].hasBeenUnlockedByPlayer)
        {
            MedalManager.instance.ListenToNewUnlockable(MedalManager.goldMedalID);
        }
        if (!MedalManager.instance.IsServer)
        {
            Plugin.Logger.LogDebug("Client calling SetMedalsOnJoin()");
            MedalManager.instance.SyncUponJoinServerRpc((int)StartOfRound.Instance.localPlayerController.playerClientId);
        }
    }

    public static void SetDiscoBallOnJoin()
    {
        ToggleDiscoBall(false);
        foreach (int unlockableIndex in StoreAndTerminal.allShipUnlockableIDs)
        {
            UnlockableItem unlockable = StartOfRound.Instance.unlockablesList.unlockables[unlockableIndex];
            if (unlockable.hasBeenUnlockedByPlayer)
            {
                string songName = unlockable.shopSelectionNode.name.Remove(0, 13);
                if (songName == "GroovyGold")
                {
                    ToggleDiscoBall(true, songName);
                }
            }
        }
    }

    public static void ToggleDiscoBall(bool unlock, string fileName = null)
    {
        if (Plugin.playlistsModCompatible)
        {
            SetDiscoBallSharedAudioLib(unlock, fileName);
        }
        else
        {
            if (!unlock)
            {
                StoreAndTerminal.runtimeDiscoBallSongs.allClips.Clear();
                if (sharedSFXdiscoBallMusic != null)
                {
                    StoreAndTerminal.runtimeDiscoBallSongs.allClips.Add(sharedSFXdiscoBallMusic);
                }
                Logger.LogDebug("Reset Disco Ball songs!");
            }
            else
            {
                AudioClip clipToAdd = LoadDiscoBallMusic(fileName);
                if (StoreAndTerminal.runtimeDiscoBallSongs.allClips.Contains(clipToAdd))
                {
                    Logger.LogDebug($"!!runtimeDiscoBallSongs already contained {clipToAdd.name}!!");
                }
                else
                {
                    StoreAndTerminal.runtimeDiscoBallSongs.allClips.Add(clipToAdd);
                    Logger.LogDebug($"Added Disco Ball song '{clipToAdd.name}' at index {StoreAndTerminal.runtimeDiscoBallSongs.allClips.Count - 1}");
                }
            }
        }
    }

    public static void SetDiscoBallSharedAudioLib(bool unlock, string fileName = null)
    {
        AudioList discoBallPlaylist = SharedAudioMethods.AudioListGetByName("LCSimonTendoPlaylistsMod", discoBallStaticPrefab);
        if (!unlock)
        {
            AudioClipList allDiscoBallSongs = Plugin.CustomGoldScrapAssets.LoadAsset<AudioClipList>("Assets/LCGoldScrapMod/GoldScrapShop/GoldScrapShopData/DiscoBallMusic/AllDiscoBallSongs.asset");
            foreach (AudioClip clip in allDiscoBallSongs.allClips)
            {
                SharedAudioMethods.RemoveAudioClip(clip, discoBallPlaylist, true);
            }
        }
        else
        {
            AudioClip clip = LoadDiscoBallMusic(fileName);
            if (!SharedAudioMethods.AudioListContainsAudioClip(discoBallPlaylist, clip, true))
            {
                SharedAudioMethods.AudioClipAddNew(clip, discoBallPlaylist, true);
            }
        }
    }

    public static void BreakPocketedItem(GrabbableObject item, int itemSlot = -1)
    {
        if (StartOfRound.Instance.shipBounds.bounds.Contains(item.transform.position))
        {
            item.transform.SetParent(StartOfRound.Instance.elevatorTransform);
        }
        else
        {
            item.transform.SetParent(StartOfRound.Instance.propsContainer);
        }
        item.startFallingPosition = item.transform.parent.InverseTransformPoint(item.transform.position);
        item.targetFloorPosition = item.transform.parent.InverseTransformPoint(item.transform.position);
        item.fallTime = 1f;
        item.reachedFloorTarget = true;
        item.hasHitGround = true;
        item.parentObject = null;
        item.heldByPlayerOnServer = false;
        item.isHeld = false;
        item.isPocketed = false;
        item.playerHeldBy.carryWeight = Mathf.Clamp(item.playerHeldBy.carryWeight - (item.itemProperties.weight - 1), 1f, 10f);
        if (itemSlot == -1)
        {
            for (int i = 0; i < item.playerHeldBy.ItemSlots.Length; i++)
            {
                GrabbableObject itemInSlot = item.playerHeldBy.ItemSlots[i];
                if (itemInSlot != null && itemInSlot == item)
                {
                    itemSlot = i;
                    break;
                }
            }
        }
        if (itemSlot == -1)
        {
            Logger.LogWarning("General failed to find itemSlot for BreakPocketedItem()!");
            return;
        }
        item.playerHeldBy.ItemSlots[itemSlot] = null;
        if (item.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
        }
    }

    public static void InstantiateSparklesOnTransform(Transform instantiateOn)
    {
        if (Plugin.specialDateCase != 6 || instantiateOn == null || sparkleParticle == null)
        {
            return;
        }
        Transform sparkles = instantiateOn.Find("SparkleGold(Clone)");
        if (sparkles == null)
        {
            Plugin.Logger.LogDebug($"specialDateCase {Plugin.specialDateCase} instantiating sparkles on {instantiateOn.name}");
            UnityEngine.Object.Instantiate(sparkleParticle, instantiateOn, false);
        }
    }

    public static void DestroySparklesOnTransform(Transform destroyFrom)
    {
        if (destroyFrom == null)
        {
            return;
        }
        Transform sparkles = destroyFrom.Find("SparkleGold(Clone)");
        if (sparkles != null)
        {
            Plugin.Logger.LogDebug($"specialDateCase {Plugin.specialDateCase} destroying {sparkles.name} on {destroyFrom.name}");
            UnityEngine.Object.Destroy(sparkles.gameObject);
        }
    }

    public static void GetSpecialDateCase()
    {
        int month = DateTime.Now.Month;
        int day = DateTime.Now.Day;
        int yearDay = DateTime.Now.DayOfYear;
        int weekDay = (int)DateTime.Now.DayOfWeek;
        Logger.LogDebug($"starting GetSpecialDateCase() with: month {month} | day {day} | yearDay {yearDay} | weekDay {weekDay}");
        int setTo = ConvertSpecialDateCase(Configs.dateCaseCode.Value);

        if (setTo == -1)
        {
            //0: New Year's (fireworks)
            if (yearDay == 1 || (month == 12 && day == 31))
            {
                setTo = 0;
            }
            //1: April Fools and Friday The 13th (materials anything but gold)
            else if ((month == 4 && day == 1) || (weekDay == 5 && day == 13))
            {
                setTo = 1;
            }
            //2: World Art Day (more expensive, heavy, and late-game items <- custom set in ItemData's)
            else if ((month == 4 && day == 15) || (month == 7 && day == 8))
            {
                setTo = 2;
            }
            //3: Freedom Day (no monster scrap but rest higher value)
            else if ((month == 5 && day == 5) || (month == 7 && day == 4))
            {
                setTo = 3;
            }
            //4: Gold Days (materials nothing but gold, more gold, gold more valuable, lower prices)
            else if ((month == 3 && day == 28) || (month == 9 && day == 2) || (month == 12 && day == 13) || yearDay == 79)
            {
                setTo = 4;
            }
            //5: Halloween (more monster scrap with higher value)
            else if (month == 10 && day >= 23 && day <= 31)
            {
                setTo = 5;
            }
            //6: Christmas & Valentine's (sparkles)
            else if ((month == 12 && day >= 24 && day <= 26) || (month == 2 && day == 14))
            {
                setTo = 6;
            }
        }

        Plugin.localDateCase = setTo;
        Logger.LogDebug($"GetSpecialDateCase() set local to: {Plugin.localDateCase}");
    }

    public static int ConvertSpecialDateCase(string code)
    {
        switch (code)
        {
            default:
                return -1;
            //Disable all specialDateCases
            case "-1":
                return -99;
            //New Year's
            case "2024":
                return 0;
            //April Fools
            case "8561":
                return 1;
            //World Art Day
            case "1798":
                return 2;
            //Freedom Day
            case "4045":
                return 3;
            //Gold Day
            case "0328":
                return 4;
            //Halloween
            case "6613":
                return 5;
            //Christmas
            case "9753":
                return 6;
        }  
    }

    public static string ConvertSpecialDateCase(int result)
    {
        switch (result)
        {
            default:
                return "0000";
            case 0:
                return "2024";
            case 1:
                return "8561";
            case 2:
                return "1798";
            case 3:
                return "4045";
            case 4:
                return "0328";
            case 5:
                return "6613";
            case 6:
                return "9753";
        }
    }

    public static bool ItemHasMatchingDateCase(ItemData item, int matchCase = -1)
    {
        if (matchCase == -1)
        {
            matchCase = Plugin.specialDateCase;
        }
        if (item.specialDateCases != null && item.specialDateCases.Length > 0)
        {
            for (int h = 0; h < item.specialDateCases.Length; h++)
            {
                if (item.specialDateCases[h] == matchCase)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
