using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;

public class GoldenPickaxeNode : NetworkBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public bool nodeExhausted = false;
    public Collider pickaxeCollider;
    private float dimSpeedMultiplier = 15f;
    private float maxDim = 40f;
    private float minDim = 5f;

    [Space(3f)]
    [Header("Durability")]
    public int durabilityBase;
    public int durabilityOffset;
    private int durability;

    [Space(3f)]
    [Header("Audiovisual")]
    public Light revealedLight;
    private bool dimmingLight = false;
    public AudioSource nodeAudio;
    public AudioClip[] hitClip;
    public AudioClip emptiedClip;
    public AudioClip allNodesEmptyJingle;
    public GameObject scanNodeObject;

    void Start()
    {
        revealedLight.enabled = false;
        scanNodeObject.SetActive(false);
        if (Config.hostToolRebalance)
        {
            revealedLight.shadows = LightShadows.Hard;
        }
        if (IsServer)
        {
            StartCoroutine(SetDurabilityAndSync());
        }
        else
        {
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: spawned at {transform.position} on client");
        }
    }

    void Update()
    {
        if (nodeExhausted || !revealedLight.enabled)
        {
            return;
        }
        if (revealedLight.intensity <= minDim)
        {
            dimmingLight = false;
        }
        else if (revealedLight.intensity >= maxDim)
        {
            dimmingLight = true;
        }
        if (dimmingLight && revealedLight.intensity > minDim)
        {
            revealedLight.intensity -= Time.deltaTime * dimSpeedMultiplier;
        }
        else if (!dimmingLight && revealedLight.intensity < maxDim)
        {
            revealedLight.intensity += Time.deltaTime * dimSpeedMultiplier;
        }
    }

    private IEnumerator SetDurabilityAndSync()
    {
        int minDurability = durabilityBase - durabilityOffset;
        int maxDurability = durabilityBase + durabilityOffset;
        durability = Random.Range(minDurability, maxDurability + 1);
        Logger.LogDebug($"#{NetworkObjectId}: host set durability to {durability}");
        yield return new WaitForSeconds(3f);
        SetDurabilityClientRpc(durability);
    }

    private void SetDurabilityLocally(int receivedDurability)
    {
        durability = receivedDurability;
        Logger.LogDebug($"#{NetworkObjectId}: locally set durability to {durability}");
    }

    [ClientRpc]
    private void SetDurabilityClientRpc(int receivedDurability)
    {
        if (!IsServer)
        {
            SetDurabilityLocally(receivedDurability);
        }
    }

    public void InteractWithNode(int performingPlayerID)
    {
        if (nodeExhausted)
        {
            return;
        }
        int localNewDurability = durability - 1;
        ExhaustNodeLocally(localNewDurability);
        ExhaustNodeServerRpc(performingPlayerID, localNewDurability);
    }

    private void ExhaustNodeLocally(int receivedDurability)
    {
        durability = receivedDurability;
        Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: {durability} left");
        if (durability <= 0)
        {
            nodeExhausted = true;
            pickaxeCollider.enabled = false;
            revealedLight.enabled = false;
            scanNodeObject.SetActive(false);
            nodeAudio.PlayOneShot(emptiedClip);
            WalkieTalkie.TransmitOneShotAudio(nodeAudio, emptiedClip);
            Logger.LogDebug($"Gold Node #{NetworkObjectId} exhausted!!!");
            StartCoroutine(CheckRemainingNodes());
        }
        else
        {
            int clampedSFX = Mathf.Clamp(hitClip.Length - durability, 0, hitClip.Length - 1);
            Logger.LogDebug($"Gold Node #{NetworkObjectId} playing SFX {clampedSFX}");
            nodeAudio.PlayOneShot(hitClip[clampedSFX]);
            WalkieTalkie.TransmitOneShotAudio(nodeAudio, hitClip[clampedSFX], 0.5f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExhaustNodeServerRpc(int performingPlayerID, int receivedDurability)
    {
        ExhaustNodeClientRpc(performingPlayerID, receivedDurability);
    }

    [ClientRpc]
    private void ExhaustNodeClientRpc(int performingPlayerID, int receivedDurability)
    {
        if (performingPlayerID != (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            ExhaustNodeLocally(receivedDurability);
        }
    }

    private IEnumerator CheckRemainingNodes()
    {
        GoldenPickaxeNode[] allNodes = FindObjectsOfType<GoldenPickaxeNode>();
        foreach (GoldenPickaxeNode node in allNodes)
        {
            if (!node.nodeExhausted)
            {
                yield break;
            }
        }
        yield return new WaitForSeconds(0.5f);
        nodeAudio.PlayOneShot(allNodesEmptyJingle);
        WalkieTalkie.TransmitOneShotAudio(nodeAudio, allNodesEmptyJingle);
        RoundManager.Instance.PlayAudibleNoise(transform.position, 25f, 0.25f);
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!nodeExhausted)
        {
            revealedLight.intensity = minDim;
            revealedLight.enabled = true;
            scanNodeObject.SetActive(true);
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        revealedLight.enabled = false;
        scanNodeObject.SetActive(false);
    }
}
