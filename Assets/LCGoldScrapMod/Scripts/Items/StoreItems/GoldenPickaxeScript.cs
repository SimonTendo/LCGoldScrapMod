using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using HarmonyLib;

public class GoldenPickaxeScript : Shovel
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static AudioClip[] hotColdClips;
    private bool performedHitThisFrame = false;

    [Space(3f)]
    [Header("Durability")]
    public int durability = 50;
    public AudioClip breakClip;

    [Space(3f)]
    [Header("Damage modifiers")]
    public AudioClip critClip;
    private int lastNumber = 9;

    [Space(3f)]
    [Header("Gold spawning")]
    public GoldenPickaxeNode nodeScript;
    public AudioClip spawnClip;
    public Item goldNugget;
    public int nuggetSpawnNormalOneIn;
    public int nuggetSpawnBonusOneIn;
    public int nuggetSpawnFeverOneIn;
    public Item goldOre;
    public int oreSpawnNormalOneIn;
    public int oreSpawnBonusOneIn;
    public int oreSpawnFeverOneIn;
    private bool hitOreThisFrame;

    public override void Start()
    {
        base.Start();
        if (RarityManager.CurrentlyGoldFever())
        {
            durability *= 2;
            Material[] newMats = { AssetsCollection.defaultMaterialSilver, AssetsCollection.defaultMaterialGold };
            GetComponent<MeshRenderer>().materials = newMats;
            Logger.LogDebug($"Pickaxe #{NetworkObjectId} started during suspected GoldFever, doubled local durability to {durability}");
        }
    }

    [HarmonyPatch(typeof(Shovel), "HitShovelClientRpc")]
    public class NewShovelHit
    {
        [HarmonyPostfix]
        public static void GoldenPickaxeHit(Shovel __instance, int hitSurfaceID)
        {
            GoldenPickaxeScript goldenPickaxe = __instance.gameObject.GetComponent<GoldenPickaxeScript>();
            if (goldenPickaxe == null || !goldenPickaxe.IsOwner || goldenPickaxe.playerHeldBy == null)
            {
                return;
            }
            goldenPickaxe.HitGoldenPickaxe(hitSurfaceID);
        }
    }

    public void HitGoldenPickaxe(int hitSurfaceID)
    {
        if (performedHitThisFrame)
        {
            return;
        }
        Logger.LogDebug($"Performing HitGoldenPickaxe() with {gameObject.name} #{NetworkObjectId}");
        Logger.LogDebug($"A: hitSurfaceID {hitSurfaceID}");
        performedHitThisFrame = true;
        PerformRockCheck(hitSurfaceID);
        PerformDamageCheck(hitSurfaceID);
        SyncDurabilityServerRpc(true, lastNumber);
        StartCoroutine(NoMoreHitUntilNextFrame());
    }

    private void PerformDamageCheck(int hitSurfaceID)
    {
        if (shovelHitForce == 99 && hitSurfaceID == -1)
        {
            PlayCritClipServerRpc((int)playerHeldBy.playerClientId);
            PlayCritClipLocal();
        }
        if (hitOreThisFrame)
        {
            Logger.LogDebug("C0: hit ore this frame, not rolling for damage modifier");
            lastNumber = 0;
        }
        else
        {
            lastNumber = RollNextDamage();
        }
        Logger.LogDebug($"C2: {lastNumber == 9}");
    }

    private int RollNextDamage()
    {
        int newRolledNr = -1;
        while (newRolledNr == -1 || (lastNumber == 9 && newRolledNr == lastNumber))
        {
            newRolledNr = Random.Range(1, 10);
            Logger.LogDebug($"C1: {newRolledNr}");
        }
        return newRolledNr;
    }

    private void PerformRockCheck(int hitSurfaceID)
    {
        hitOreThisFrame = false;
        if (hitSurfaceID != -1 && RoundManager.Instance.currentDungeonType == 4 && playerHeldBy.isInsideFactory && StartOfRound.Instance.footstepSurfaces[hitSurfaceID].surfaceTag == "Rock")
        {
            Logger.LogDebug("B0: hit rock!");
            HitRock();
        }
    }

    private void HitRock()
    {
        Vector3 positionToSpawn = playerHeldBy.gameplayCamera.transform.position + playerHeldBy.gameplayCamera.transform.forward * 0.5f + Vector3.up * 0.25f;
        GoldenPickaxeNode nodeInRange = GetNodeInRange(positionToSpawn);
        bool useBonusSpawn = false;
        if (nodeInRange != null)
        {
            nodeInRange.InteractWithNode((int)playerHeldBy.playerClientId);
            useBonusSpawn = true;
            hitOreThisFrame = true;
        }
        int maxNuggetSpawn = useBonusSpawn ? (RarityManager.CurrentlyGoldFever() ? nuggetSpawnFeverOneIn : nuggetSpawnBonusOneIn) : nuggetSpawnNormalOneIn;
        int maxOreSpawn = useBonusSpawn ? (RarityManager.CurrentlyGoldFever() ? oreSpawnFeverOneIn : oreSpawnBonusOneIn) : oreSpawnNormalOneIn;
        int randomNrNugget = Random.Range(0, maxNuggetSpawn);
        int randomNrOre = Random.Range(0, maxOreSpawn);
        Logger.LogDebug($"B2: nugget chance: {randomNrNugget}/{maxNuggetSpawn} | ore chance: {randomNrOre}/{maxOreSpawn}");
        if (randomNrNugget == 0)
        {
            Logger.LogDebug("B3-I: spawn chance succeeded! for: nugget");
            SpawnGoldServerRpc(0, positionToSpawn);
        }
        else if (randomNrOre == 0)
        {
            Logger.LogDebug("B3-II: spawn chance succeeded! for: ore");
            SpawnGoldServerRpc(1, positionToSpawn);
        }
        else
        {
            Logger.LogDebug("B3-III: spawn chance failed");
        }
    }

    private GoldenPickaxeNode GetNodeInRange(Vector3 checkPosition)
    {
        GoldenPickaxeNode node = null;
        RaycastHit[] rays = Physics.SphereCastAll(checkPosition, 0.5f, playerHeldBy.gameplayCamera.transform.forward, 0.25f);
        foreach (RaycastHit ray in rays)
        {
            GoldenPickaxeNode foundNode = ray.collider.gameObject.GetComponent<GoldenPickaxeNode>();
            if (foundNode != null)
            {
                node = foundNode;
                Logger.LogDebug($"B1: found node!");
                break;
            }
        }
        return node;
    }

    private GameObject GetObjectToSpawn(int spawnType)
    {
        switch (spawnType)
        {
            default:
                return goldNugget.spawnPrefab;
            case 1:
                return goldOre.spawnPrefab;
        }
    }

    private IEnumerator ReplicateObjectSpawningFunctionality(GrabbableObject item)
    {
        yield return new WaitForEndOfFrame();
        item.reachedFloorTarget = false;
        item.hasHitGround = false;
        item.fallTime = 0f;
        item.isInFactory = true;
    }

    private IEnumerator ReplicateWaitForObjectFunctionality(NetworkObjectReference itemNOR, Vector3 positionToSpawn)
    {
        NetworkObject netObject = null;
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < 8f && !itemNOR.TryGet(out netObject))
        {
            yield return new WaitForSeconds(0.03f);
        }
        if (netObject == null)
        {
            Debug.Log("No network object found");
            yield break;
        }
        yield return new WaitForEndOfFrame();
        GrabbableObject item = netObject.GetComponent<GrabbableObject>();
        item.startFallingPosition = positionToSpawn;
        item.fallTime = 0f;
        item.hasHitGround = false;
        item.reachedFloorTarget = false;
        item.isInFactory = true;
        if (playerHeldBy != null)
        {
            playerHeldBy.SetItemInElevator(false, false, item);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnGoldServerRpc(int spawnType, Vector3 positionToSpawn)
    {
        Transform parent = (((!(playerHeldBy != null) || !playerHeldBy.isInElevator) && !StartOfRound.Instance.inShipPhase) || !(RoundManager.Instance.spawnedScrapContainer != null)) ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.spawnedScrapContainer;
        GameObject objectToInstantiate = GetObjectToSpawn(spawnType);
        GameObject spawnedObject = Instantiate(objectToInstantiate, positionToSpawn, Quaternion.identity, parent);
        GrabbableObject item = spawnedObject.GetComponent<GrabbableObject>();
        item.startFallingPosition = positionToSpawn;
        StartCoroutine(ReplicateObjectSpawningFunctionality(item));
        item.targetFloorPosition = item.GetItemFloorPosition(transform.position);
        if (playerHeldBy != null)
        {
            playerHeldBy.SetItemInElevator(false, false, item);
        }
        item.NetworkObject.Spawn();
        SpawnGoldClientRpc(spawnedObject.GetComponent<NetworkObject>(), positionToSpawn);
    }

    [ClientRpc]
    private void SpawnGoldClientRpc(NetworkObjectReference itemNOR, Vector3 positionToSpawn)
    {
        Instantiate(AssetsCollection.poofParticle, positionToSpawn, new Quaternion(0, 0, 0, 0)).GetComponent<ParticleSystem>().Play();
        shovelAudio.PlayOneShot(spawnClip);
        WalkieTalkie.TransmitOneShotAudio(shovelAudio, spawnClip);
        RoundManager.Instance.PlayAudibleNoise(positionToSpawn, 25, 0.1f);
        if (!IsServer)
        {
            StartCoroutine(ReplicateWaitForObjectFunctionality(itemNOR, positionToSpawn));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncDurabilityServerRpc(bool subtract = false, int receivedNumber = -1, int playerID = -1)
    {
        if (subtract)
        {
            durability--;
        }
        int rolledNr = receivedNumber != -1 ? receivedNumber : lastNumber;
        SyncDurabilityClientRpc(durability, rolledNr, playerID);
    }

    [ClientRpc]
    private void SyncDurabilityClientRpc(int hostDurability, int hostLastNumber, int playerID)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            durability = hostDurability;
            lastNumber = hostLastNumber;
            shovelHitForce = lastNumber == 9 && durability != 50 ? 99 : 1;
            Logger.LogDebug($"D: durability: {durability} | lastNumber: {lastNumber} | shovelHitForce {shovelHitForce}");
            if (durability <= 0)
            {
                BreakPickaxe();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayCritClipServerRpc(int playerID)
    {
        PlayCritClipClientRpc(playerID);
    }

    [ClientRpc]
    private void PlayCritClipClientRpc(int playerID)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            PlayCritClipLocal();
        }
    }

    private void PlayCritClipLocal()
    {
        shovelAudio.PlayOneShot(critClip);
        WalkieTalkie.TransmitOneShotAudio(shovelAudio, critClip);
        if (Config.hostToolRebalance)
        {
            BreakPickaxe();
        }
    }

    private void BreakPickaxe()
    {
        shovelAudio.PlayOneShot(breakClip);
        WalkieTalkie.TransmitOneShotAudio(shovelAudio, breakClip);
        RoundManager.Instance.PlayAudibleNoise(transform.position);
        DestroyObjectInHand(playerHeldBy);
        if (isPocketed)
        {
            General.BreakPocketedItem(this);
        }
        DelaySettingObjectAway();
        Logger.LogDebug($"Golden Pickaxe #{NetworkObjectId} broke!!!");
    }

    private IEnumerator DelaySettingObjectAway()
    {
        yield return new WaitForSeconds(5f);
        targetFloorPosition = new Vector3(3000f, -400f, 3000f);
        startFallingPosition = new Vector3(3000f, -400f, 3000f);
    }

    public override void PlayDropSFX()
    {
        if (!deactivated)
        {
            base.PlayDropSFX();
        }
    }

    private IEnumerator NoMoreHitUntilNextFrame()
    {
        yield return null;
        performedHitThisFrame = false;
    }
}
