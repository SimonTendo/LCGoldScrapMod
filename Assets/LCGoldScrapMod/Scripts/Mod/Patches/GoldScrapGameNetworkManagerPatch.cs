using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using HarmonyLib;


public class GoldScrapGameNetworkManagerPatch
{
    //Spawn the NetworkObject used for Netcode patching, taken from the Lethal Company Modding Wiki
    [HarmonyPatch]
    public class NetworkObjectManager
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init(GameNetworkManager __instance)
        {
            AssetsCollection.GetMenuAssets(__instance);

            if (networkPrefab != null)
                return;

            networkPrefab = Plugin.CustomGoldScrapAssets.LoadAsset<GameObject>("Assets/LCGoldScrapMod/GoldScrapNetcode/NetworkPrefabs/GoldScrapNetworkHandler.prefab");

            //Register NetworkPrefabs of all miscellaneous gameObjects
            foreach (GameObject prefab in Plugin.allMiscNetworkPrefabs)
            {
                if (prefab != null && prefab.GetComponent<NetworkObject>())
                {
                    NetworkManager.Singleton.AddNetworkPrefab(prefab);
                }
            }

            //Register NetworkPrefabs of all Gold Scrap
            foreach (ItemData item in Plugin.allGoldGrabbableObjects)
            {
                if (item != null && item.itemProperties != null && item.itemProperties.spawnPrefab != null && item.itemProperties.spawnPrefab.GetComponent<NetworkObject>())
                {
                    NetworkManager.Singleton.AddNetworkPrefab(item.itemProperties.spawnPrefab);
                }
            }

            //Register NetworkPrefabs of all placeable GoldStore Unlockables
            foreach (ItemData unlockable in StoreAndTerminal.allGoldStoreItemData)
            {
                if (unlockable != null && unlockable.unlockableProperties != null && unlockable.unlockableProperties.prefabObject != null && unlockable.unlockableProperties.prefabObject.GetComponent<NetworkObject>())
                {
                    NetworkManager.Singleton.AddNetworkPrefab(unlockable.unlockableProperties.prefabObject);
                }
            }

            //Register NetworkPrefabs of all StoryLogs
            foreach (GameObject item in LogManager.allLogPrefabs.allPrefabs)
            {
                if (item != null && item.GetComponent<NetworkObject>())
                {
                    NetworkManager.Singleton.AddNetworkPrefab(item);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Transform goldScrapManager = new GameObject("LCGoldScrapMod").transform;
                goldScrapManager.transform.SetParent(Plugin.SearchForObject("SimonTendoManagers", StartOfRound.Instance.gameObject.transform.parent).transform);

                GameObject networkHandler = Object.Instantiate(networkPrefab);
                networkHandler.transform.SetParent(goldScrapManager, false);
                networkHandler.GetComponent<NetworkObject>().Spawn();

                foreach (GameObject prefab in Plugin.allMiscNetworkPrefabs)
                {
                    if (prefab.name.Contains("Manager"))
                    {
                        GameObject manager = Object.Instantiate(prefab);
                        manager.transform.SetParent(goldScrapManager, false);
                        manager.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
        }

        static GameObject networkPrefab;
    }



    [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
    public class NewGameNetworkManagerDisconnect
    {
        [HarmonyPrefix]
        public static void DisconnectPostfix(GameNetworkManager __instance)
        {
            if (__instance == null || __instance.localPlayerController == null || !__instance.localPlayerController.isHostPlayerObject)
            {
                Plugin.Logger.LogDebug("null or client, returning");
                return;
            }
            GoldScrapSaveData.SaveData saveData = new GoldScrapSaveData.SaveData();
            saveData.Save();
        }
    }
}
