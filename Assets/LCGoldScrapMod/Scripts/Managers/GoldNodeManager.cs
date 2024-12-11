using System.Collections;
using UnityEngine;
using Unity.Netcode;
using DunGen;
using BepInEx.Logging;


public class GoldNodeManager : NetworkBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GoldNodeManager instance;

    public GameObject goldNodePrefab;

    private int nrOfOffsetsMadeSoFar = 3;

    private void Awake()
    {
        instance = this;
    }

    public void StartSpawnNodes()
    {
        if (RoundManager.Instance.currentDungeonType != 4 || !StartOfRound.Instance.currentLevel.planetHasTime)
        {
            return;
        }
        StartCoroutine(SpawnNodes());
    }

    private IEnumerator SpawnNodes()
    {
        yield return new WaitForSeconds(5f);
        Dungeon dungeonData = FindObjectOfType<Dungeon>();
        if (dungeonData == null)
        {
            Logger.LogDebug("WARNING!! did not find dungeonData, breaking");
            yield break;
        }
        int nodesSpawned = 0;
        int minNodes = GetAmountOfNodes(true);
        minNodes = RarityManager.CurrentlyGoldFever() ? minNodes * 2 : minNodes;
        int maxNodes = GetAmountOfNodes(false);
        maxNodes = RarityManager.CurrentlyGoldFever() ? maxNodes * 2 : maxNodes;
        Logger.LogDebug($"min: {minNodes} | max: {maxNodes}");
        foreach (Tile tile in dungeonData.AllTiles)
        {
            if (tile == null)
            {
                continue;
            }
            yield return null;
            int nodeType = GetNode(tile.gameObject.name);
            if (nodeType != -1)
            {
                if (Random.Range(1, 100) > GetChanceToSkip(nodeType, nodesSpawned < minNodes))
                {
                    GameObject instantiatedNode = Instantiate(goldNodePrefab);
                    MoveNodeBasedOnTile(tile, instantiatedNode, nodeType);
                    instantiatedNode.GetComponent<NetworkObject>().Spawn();
                    nodesSpawned++;
                    if (nodesSpawned >= maxNodes)
                    {
                        break;
                    }
                }
            }
        }
        Logger.LogDebug($"nodes spawned: {nodesSpawned}");
    }

    private int GetNode(string nodeName)
    {
        switch (nodeName)
        {
            default:
                return -1;
            case "CaveSmallIntersectTile(Clone)":
                return 0;
            case "CaveCrampedIntersectTile(Clone)":
                return 1;
            case "CaveYTile(Clone)":
                return 2;
            case "CaveLongRampTile(Clone)":
                return 3;
            case "CaveForkStairTile(Clone)":
                return 4;
        }
    }

    private int GetChanceToSkip(int nodeType, bool belowMinNodes)
    {
        switch (nodeType)
        {
            default:
                return belowMinNodes ? 100 : 95;
            case 1:
                return belowMinNodes ? 90 : 90;
            case 2:
                return belowMinNodes ? 75 : 85;
            case 3:
                return belowMinNodes ? 0 : 75;
            case 4:
                return belowMinNodes ? 25 : 60;
        }
    }

    private int GetAmountOfNodes(bool forMin)
    {
        int ID = StartOfRound.Instance.currentLevelID;
        if (ID == 4 || ID == 5 || ID == 8)
        {
            return forMin ? 2 : 6;
        }
        if (ID == 6 || ID == 7 || ID == 9 || ID == 10 || ID == 12)
        {
            return forMin ? 4 : 7;
        }
        return forMin ? 1 : 4;
    }

    private void MoveNodeBasedOnTile(Tile tile, GameObject instantiatedNode, int nodeType)
    {
        instantiatedNode.transform.SetParent(tile.gameObject.transform, false);
        int offsetSeed = Random.Range(0, nrOfOffsetsMadeSoFar);

        switch (nodeType)
        {
            case 0:
                MoveNodeZero(instantiatedNode, offsetSeed);
                break;
            case 1:
                MoveNodeOne(instantiatedNode, offsetSeed);
                break;
            case 2:
                MoveNodeTwo(instantiatedNode, offsetSeed);
                break;
            case 3:
                MoveNodeThree(instantiatedNode, offsetSeed);
                break;
            case 4:
                MoveNodeFour(instantiatedNode, offsetSeed);
                break;
        }

        Logger.LogDebug($"{instantiatedNode.name} #{nodeType} ({offsetSeed}): spawned at {instantiatedNode.transform.position} on server");
    }

    private void MoveNodeZero(GameObject node, int offsetSeed)
    {
        switch (offsetSeed)
        {
            default:
                node.transform.localPosition = new Vector3(1.5f, 2.5f, 2.5f);
                return;
            case 1:
                node.transform.localPosition = new Vector3(4.75f, 3.75f, 3f);
                return;
            case 2:
                node.transform.localPosition = new Vector3(1.5f, 5f, 3.5f);
                return;
        }
    }

    private void MoveNodeOne(GameObject node, int offsetSeed)
    {
        switch (offsetSeed)
        {
            default:
                node.transform.localPosition = new Vector3(5f, 5f, 0.25f);
                return;
            case 1:
                node.transform.localPosition = new Vector3(3.5f, 4.25f, 5.5f);
                return;
            case 2:
                node.transform.localPosition = new Vector3(6.5f, 3.25f, 2.5f);
                return;
        }
    }

    private void MoveNodeTwo(GameObject node, int offsetSeed)
    {
        switch (offsetSeed)
        {
            default:
                node.transform.localPosition = new Vector3(2f, 4.75f, 2.5f);
                return;
            case 1:
                node.transform.localPosition = new Vector3(9.5f, 15.2f, 2f);
                return;
            case 2:
                node.transform.localPosition = new Vector3(2.75f, 21f, 5.5f);
                return;
        }
    }

    private void MoveNodeThree(GameObject node, int offsetSeed)
    {
        switch (offsetSeed)
        {
            default:
                node.transform.localPosition = new Vector3(0.75f, 11f, 3f);
                return;
            case 1:
                node.transform.localPosition = new Vector3(14.5f, 18f, 9.5f);
                return;
            case 2:
                node.transform.localPosition = new Vector3(10f, 18f, 1.65f);
                return;
        }
    }

    private void MoveNodeFour(GameObject node, int offsetSeed)
    {
        switch (offsetSeed)
        {
            default:
                node.transform.localPosition = new Vector3(10f, 2.25f, -8f);
                return;
            case 1:
                node.transform.localPosition = new Vector3(12.25f, 8, -8.75f);
                return;
            case 2:
                node.transform.localPosition = new Vector3(2.25f, 8f, -6.5f);
                return;
        }
    }
}
