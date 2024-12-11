using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using HarmonyLib;


public class GameNetworkManagerPatch
{
    //Spawn the NetworkObject used for Netcode patching, taken from the Lethal Company Modding Wiki
    [HarmonyPatch]
    public class NetworkObjectManager
    {
        private static List<GameObject> _networkPrefabs = new List<GameObject>();

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
                NetworkManager.Singleton.AddNetworkPrefab(prefab);
            }

            //Register NetworkPrefabs of all Gold Scrap
            foreach (ItemData item in Plugin.allGoldScrap.allItemData)
            {
                NetworkManager.Singleton.AddNetworkPrefab(item.itemProperties.spawnPrefab);
            }

            //Register NetworkPrefabs of all placeable GoldStore Unlockables
            foreach (UnlockableItem item in Plugin.allGoldUnlockables)
            {
                if (item.prefabObject != null)
                {
                    NetworkManager.Singleton.AddNetworkPrefab(item.prefabObject);
                }
            }

            //Register NetworkPrefabs of all StoryLogs
            foreach (GameObject item in LogManager.allLogPrefabs.allPrefabs)
            {
                NetworkManager.Singleton.AddNetworkPrefab(item);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                GameObject networkHandlerHost = Object.Instantiate(networkPrefab, Plugin.SearchForObject("SimonTendoNetworkHandlers").transform, false);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();

                Transform goldScrapManager = new GameObject("LCGoldScrapMod").transform;
                goldScrapManager.transform.SetParent(Plugin.SearchForObject("SimonTendoManagers", StartOfRound.Instance.gameObject.transform.parent).transform);

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
            if (__instance.localPlayerController != null && __instance.localPlayerController.isHostPlayerObject && CreditsCardManager.previousCredits[__instance.saveFileNum] != 0)
            {
                GoldScrapSaveData.SaveData saveData = new GoldScrapSaveData.SaveData();
                saveData.Save();
            }
        }
    }
}
