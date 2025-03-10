using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using HarmonyLib;

public class LogManager : MonoBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GameObject managerPrefab;
    public static GameObjectList allLogPrefabs;

    public static LogManager instance;

    public TerminalNode[] allLogs;
    public string[] levelOfLog;
    public Vector3[] locationOfLog;
    public Vector3[] rotationOfLog;

    private List<GameObject> currentSpawnedLogs = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        AddStoryLogsToTerminal();
    }

    private void AddStoryLogsToTerminal()
    {
        Terminal terminalScript = FindObjectOfType<Terminal>();
        if (terminalScript == null)
        {
            return;
        }
        for (int i = 0; i < allLogPrefabs.allPrefabs.Length; i++)
        {
            GameObject logPrefab = allLogPrefabs.allPrefabs[i];

            TerminalNode logNode = allLogs[i];
            terminalScript.logEntryFiles.Add(logNode);
            logNode.storyLogFileID = terminalScript.logEntryFiles.Count - 1;

            StoryLog thisLog = logPrefab.GetComponent<StoryLog>();
            thisLog.SetStoryLogID(terminalScript.logEntryFiles.Count - 1);

            InteractTrigger logTrigger = logPrefab.GetComponent<InteractTrigger>();
            logTrigger.hoverIcon = AssetsCollection.handIcon;

            Logger.LogDebug($"Added {logNode.name} to logEntryFiles at storyLogID {thisLog.storyLogID}");
        }
    }

    public void SpawnLogs()
    {
        Terminal terminalScript = FindObjectOfType<Terminal>();
        for (int i = 0; i < levelOfLog.Length; i++)
        {
            if (levelOfLog[i] == StartOfRound.Instance.currentLevel.name && CheckToSpawnLog(terminalScript, allLogs[i].storyLogFileID))
            {
                Logger.LogDebug($"LogManager: {allLogs[i].name} set to spawn on currentLevel {levelOfLog[i]} at location {locationOfLog[i]} with rotation {rotationOfLog[i]}, trying to instantiate across clients");
                GameObject spawnedLog = Instantiate(allLogPrefabs.allPrefabs[i], locationOfLog[i], Quaternion.identity);
                spawnedLog.transform.eulerAngles = rotationOfLog[i];
                spawnedLog.GetComponent<NetworkObject>().Spawn();
                currentSpawnedLogs.Add(spawnedLog);
            }
        }
    }

    public void DespawnCurrentLogs()
    {
        for (int i = currentSpawnedLogs.Count - 1; i >= 0; i--)
        {
            Logger.LogDebug($"LogManager: trying to despawn {currentSpawnedLogs[i].name}");
            currentSpawnedLogs[i].GetComponent<NetworkObject>().Despawn();
        }
        currentSpawnedLogs.Clear();
    }

    private bool CheckToSpawnLog(Terminal foundTerminal, int logID)
    {
        if (foundTerminal == null)
        {
            Logger.LogDebug($"LogManager: did not find terminal, not spawning log");
            return false;
        }
        if (foundTerminal.unlockedStoryLogs.Contains(logID))
        {
            Logger.LogDebug($"LogManager: terminal already has log {logID} unlocked on host, not spawning log");
            return false;
        }
        return true;
    }

    private static void ReplicateRemoveLogCollectible(GameObject logPrefab)
    {
        Logger.LogDebug($"replicating RemoveLogCollectible for {logPrefab.name}");
        MeshRenderer[] componentsInChildren = logPrefab.gameObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            componentsInChildren[i].enabled = false;
        }
        logPrefab.gameObject.GetComponent<InteractTrigger>().interactable = false;
        Collider[] componentsInChildren2 = logPrefab.GetComponentsInChildren<Collider>();
        for (int j = 0; j < componentsInChildren2.Length; j++)
        {
            componentsInChildren2[j].enabled = false;
        }
    }

    [HarmonyPatch(typeof(HUDManager), "GetNewStoryLogClientRpc")]
    public class HUDManagerPatch
    {
        [HarmonyPostfix]
        public static void GetNewStoryLogPostfix(int logID)
        {
            foreach (TerminalNode logNode in instance.allLogs)
            {
                if (logNode.storyLogFileID == logID)
                {
                    Logger.LogDebug($"LogManager: intercepted ClientRpc to replicate RemoveLogCollectible {logID} on all players!");
                    StoryLog[] allLogsInLevel = FindObjectsByType<StoryLog>(FindObjectsSortMode.None);
                    foreach (StoryLog logScript in allLogsInLevel)
                    {
                        if (logScript.storyLogID == logNode.storyLogFileID)
                        {
                            ReplicateRemoveLogCollectible(logScript.gameObject);
                            return;
                        }
                    }
                }
            }
        }
    }
}
