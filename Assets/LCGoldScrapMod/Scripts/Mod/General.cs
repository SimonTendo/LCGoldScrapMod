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
            foreach (ItemData item in Plugin.allGoldScrap.allItemData)
            {
                item.itemProperties.spawnPrefab.AddComponent<GoldScrapObject>();
                if (item.isStoreItem)
                {
                    item.itemProperties.spawnPrefab.AddComponent<GoldStoreItem>();
                }
                StartOfRound.Instance.allItemsList.itemsList.Add(item.itemProperties);
                Logger.LogDebug($"Added {item.name.Remove(item.name.Length - 4, 4)} to allItemsList");
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
            GameObject JacobsLadderHelmetLights = Object.Instantiate(jacobsLadderPrefab, FindHelmetLights(player.gameObject).transform, false);
            List<Light> HelmetLights = player.allHelmetLights.ToList();
            HelmetLights.Add(JacobsLadderHelmetLights.GetComponent<Light>());
            HelmetLights[HelmetLights.Count - 1].lightShadowCasterMode = LightShadowCasterMode.Everything;
            HelmetLights[HelmetLights.Count - 1].shadows = LightShadows.Hard;
            HelmetLights[HelmetLights.Count - 1].intensity = 400f;
            HelmetLights[HelmetLights.Count - 1].enabled = false;
            jacobsLadderFlashlightID = HelmetLights.Count - 1;
            player.allHelmetLights = HelmetLights.ToArray();
        }
    }

    public static GameObject FindHelmetLights(GameObject playerObject)
    {
        GameObject scavengerModelObject = playerObject.transform.Find("ScavengerModel").gameObject;
        GameObject metarigObject = scavengerModelObject.transform.Find("metarig").gameObject;
        GameObject cameraContainerObject = metarigObject.transform.Find("CameraContainer").gameObject;
        GameObject mainCameraObject = cameraContainerObject.transform.Find("MainCamera").gameObject;
        GameObject helmetLightsObject = mainCameraObject.transform.Find("HelmetLights").gameObject;

        if (helmetLightsObject == null)
        {
            Logger.LogError($"{playerObject.name}'s HelmetLights GameObject could not be found. JacobsLadder likely will not work on them.");
        }

        return helmetLightsObject;
    }

    public static bool DoesMoonHaveGoldScrap(int moonLevelID = -1, bool checkScene = true)
    {
        if (moonLevelID == -1 || moonLevelID < 0 || moonLevelID >= StartOfRound.Instance.levels.Length)
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
            if (item.spawnableItem.name.Contains("LCGoldScrapMod"))
            {
                Logger.LogDebug($"level #{moonLevelID} item list returned true");
                return true;
            }
        }

        //As a failsafe, check to see if the current planet has any gold scrap that is not in the ship
        if (checkScene)
        {
            foreach (GoldScrapObject goldScrap in Object.FindObjectsOfType<GoldScrapObject>())
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

        foreach (GoldenGirlScript goldenGirlScript in Object.FindObjectsOfType<GoldenGirlScript>())
        {
            goldenGirlScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldPerfumeScript goldPerfumeScript in Object.FindObjectsOfType<GoldPerfumeScript>())
        {
            goldPerfumeScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (JackInTheGoldScript jackInTheGoldScript in Object.FindObjectsOfType<JackInTheGoldScript>())
        {
            jackInTheGoldScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldBirdScript goldBirdScript in Object.FindObjectsOfType<GoldBirdScript>())
        {
            goldBirdScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenClockScript goldenClockScript in Object.FindObjectsOfType<GoldenClockScript>())
        {
            goldenClockScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldmineScript goldmineScript in Object.FindObjectsOfType<GoldmineScript>())
        {
            goldmineScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenHourglassScript goldenHourglassScript in Object.FindObjectsOfType<GoldenHourglassScript>())
        {
            goldenHourglassScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenPickaxeScript goldenPickaxeScript in Object.FindObjectsOfType<GoldenPickaxeScript>())
        {
            goldenPickaxeScript.SyncDurabilityServerRpc(false, -1, playerID);
        }

        foreach (GoldkeeperScript goldkeeperScript in Object.FindObjectsOfType<GoldkeeperScript>())
        {
            goldkeeperScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (CrownScript crownScript in Object.FindObjectsOfType<CrownScript>())
        {
            crownScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (SafeBoxScript safeBoxScript in Object.FindObjectsOfType<SafeBoxScript>())
        {
            safeBoxScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldfatherClockScript goldfatherClockScript in Object.FindObjectsOfType<GoldfatherClockScript>())
        {
            goldfatherClockScript.SyncUponJoinServerRpc(playerID);
        }

        foreach (GoldenGloveScript goldenGloveScript in Object.FindObjectsOfType<GoldenGloveScript>())
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
