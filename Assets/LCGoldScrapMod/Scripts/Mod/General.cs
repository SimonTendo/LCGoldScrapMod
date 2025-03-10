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
            if (foundHelmetLight == null || jacobsLadderPrefab == null)
            {
                Logger.LogError($"failed to AddJacobsLadderToHelmetLights for player {player} (foundHelmetLight? {foundHelmetLight != null})");
                return;
            }
            GameObject JacobsLadderHelmetLights = Object.Instantiate(jacobsLadderPrefab, foundHelmetLight.transform, false);
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
            foreach (GoldScrapObject goldScrap in Object.FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None))
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
        if (StartOfRound.Instance.currentLevel != null && StartOfRound.Instance.currentLevel.currentWeather == LevelWeatherType.Stormy)
        {
            StormyWeather stormyWeatherInstance = Object.FindObjectOfType<StormyWeather>(false);
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
        return activePlayers[Random.Range(0, activePlayers.Count)];
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

        foreach (GoldenGirlScript goldenGirlScript in Object.FindObjectsByType<GoldenGirlScript>(FindObjectsSortMode.None))
        {
            goldenGirlScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldPerfumeScript goldPerfumeScript in Object.FindObjectsByType<GoldPerfumeScript>(FindObjectsSortMode.None))
        {
            goldPerfumeScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (JackInTheGoldScript jackInTheGoldScript in Object.FindObjectsByType<JackInTheGoldScript>(FindObjectsSortMode.None))
        {
            jackInTheGoldScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldBirdScript goldBirdScript in Object.FindObjectsByType<GoldBirdScript>(FindObjectsSortMode.None))
        {
            goldBirdScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenClockScript goldenClockScript in Object.FindObjectsByType<GoldenClockScript>(FindObjectsSortMode.None))
        {
            goldenClockScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldmineScript goldmineScript in Object.FindObjectsByType<GoldmineScript>(FindObjectsSortMode.None))
        {
            goldmineScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenHourglassScript goldenHourglassScript in Object.FindObjectsByType<GoldenHourglassScript>(FindObjectsSortMode.None))
        {
            goldenHourglassScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenPickaxeScript goldenPickaxeScript in Object.FindObjectsByType<GoldenPickaxeScript>(FindObjectsSortMode.None))
        {
            goldenPickaxeScript.SyncDurabilityServerRpc(false, -1, playerID);
        }

        foreach (GoldkeeperScript goldkeeperScript in Object.FindObjectsByType<GoldkeeperScript>(FindObjectsSortMode.None))
        {
            goldkeeperScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (CrownScript crownScript in Object.FindObjectsByType<CrownScript>(FindObjectsSortMode.None))
        {
            crownScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (SafeBoxScript safeBoxScript in Object.FindObjectsByType<SafeBoxScript>(FindObjectsSortMode.None))
        {
            safeBoxScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldfatherClockScript goldfatherClockScript in Object.FindObjectsByType<GoldfatherClockScript>(FindObjectsSortMode.None))
        {
            goldfatherClockScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenGloveScript goldenGloveScript in Object.FindObjectsByType<GoldenGloveScript>(FindObjectsSortMode.None))
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
}
