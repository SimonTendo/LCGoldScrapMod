using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using System.Collections;

public class GoldenGirlScript : GrabbableObject, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    private Mesh loadedMesh;
    private bool broughtToShip = false;
    private bool choseLocalPlayer = false;

    [Space(3f)]
    [Header("(In)visibility")]
    public MeshFilter meshToToggle;
    public MeshRenderer materialToToggle;
    public AudioSource audioToMute;

    [Space(3f)]
    [Header("Audiovisual feedback")]
    public AudioSource reappearSource;
    public AudioClip reappearClip;
    public Material appearMat;
    public Material invisibleMat;

    public override void Start()
    {
        base.Start();
        loadedMesh = GetComponent<MeshFilter>().mesh;
        if (IsServer)
        {
            if (isInShipRoom)
            {
                broughtToShip = true;
                return;
            }
            StartCoroutine(ChoosePlayer());
        }
    }

    private IEnumerator ChoosePlayer()
    {
        yield return new WaitForSeconds(1f);
        if (isInFactory && !isInShipRoom)
        {
            ChoosePlayerClientRpc((int)General.GetRandomPlayer().playerClientId);
        }
    }

    [ClientRpc]
    private void ChoosePlayerClientRpc(int playerClientId)
    {
        Logger.LogDebug($"#{NetworkObjectId}: [{playerClientId}]");
        if (StartOfRound.Instance.allPlayerScripts[playerClientId] == StartOfRound.Instance.localPlayerController)
        {
            choseLocalPlayer = true;
        }
        else
        {
            ToggleGirl(false);
        }
    }

    public override void OnHitGround()
    {
        base.OnHitGround();
        if (!broughtToShip && !choseLocalPlayer)
        {
            ToggleGirl(false);
        }
    }

    public override void OnBroughtToShip()
    {
        base.OnBroughtToShip();
        if (IsOwner && !broughtToShip)
        {
            ReappearForEveryoneServerRpc();
        }
    }

    private void ToggleGirl(bool enableGirl)
    {
        meshToToggle.mesh = enableGirl ? loadedMesh : null;
        audioToMute.volume = enableGirl ? 1 : 0;
        foreach (BoxCollider collider in propColliders)
        {
            collider.enabled = enableGirl;
        }
        if (enableGirl)
        {
            if (!StartOfRound.Instance.inShipPhase)
            {
                reappearSource.PlayOneShot(reappearClip);
                Instantiate(AssetsCollection.poofParticle, transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<ParticleSystem>().Play();
            }
            materialToToggle.material = appearMat;
            General.InstantiateSparklesOnTransform(transform);
        }
        else
        {
            if (radarIcon != null)
            {
                Destroy(radarIcon.gameObject);
            }
            General.DestroySparklesOnTransform(transform);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReappearForEveryoneServerRpc()
    {
        ReappearForEveryoneClientRpc();
    }

    [ClientRpc]
    private void ReappearForEveryoneClientRpc()
    {
        broughtToShip = true;
        if (!choseLocalPlayer)
        {
            ToggleGirl(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        SyncUponJoinClientRpc(broughtToShip, playerID);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(bool onShip, int playerID)
    {
        if (playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            broughtToShip = onShip;
        }
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!Configs.hostToolRebalance && !broughtToShip && !choseLocalPlayer)
        {
            meshToToggle.mesh = loadedMesh;
            materialToToggle.material = invisibleMat;
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        if (!broughtToShip && !choseLocalPlayer)
        {
            meshToToggle.mesh = null;
            materialToToggle.material = appearMat;
        }
    }
}
