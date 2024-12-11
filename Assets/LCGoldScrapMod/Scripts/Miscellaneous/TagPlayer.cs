using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using GameNetcodeStuff;

public class TagPlayer : NetworkBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GameObject tagContainer;
    public ScanNodeProperties scanNode;
    public StringList allPlayerDescriptions;
    public Collider colliderToToggle;
    private int onPlayerIndex = -1;

    public static void PutTagOnPlayers()
    {
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];
            TagPlayer tag = Instantiate(tagContainer).GetComponent<TagPlayer>();
            tag.onPlayerIndex = i;
            tag.NetworkObject.Spawn();
        }
    }

    void Start()
    {
        colliderToToggle.enabled = false;
        if (IsServer)
        {
            SendDescriptionClientRpc(RerollSubText(), onPlayerIndex);
        }
        else
        {
            StartCoroutine(WaitForLocalPlayerController());
        }
    }

    void Update()
    {
        if (onPlayerIndex != -1 && colliderToToggle.enabled)
        {
            transform.position = StartOfRound.Instance.allPlayerScripts[onPlayerIndex].playerEye.position;
        }
    }

    private string RerollSubText()
    {
        return allPlayerDescriptions.allStrings[Random.Range(0, allPlayerDescriptions.allStrings.Length)];
    }

    private IEnumerator WaitForLocalPlayerController()
    {
        yield return new WaitUntil(() => GameNetworkManager.Instance.localPlayerController != null);
        RequestDescriptionServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDescriptionServerRpc(int playerID)
    {
        string subText = playerID == onPlayerIndex ? RerollSubText() : scanNode.subText;
        SendDescriptionClientRpc(subText, onPlayerIndex, true);
    }

    [ClientRpc]
    private void SendDescriptionClientRpc(string subText, int representingPlayer, bool newJoiningPlayer = false)
    {
        onPlayerIndex = representingPlayer;
        scanNode.headerText = StartOfRound.Instance.allPlayerScripts[representingPlayer].playerUsername;
        scanNode.subText = subText;
        Logger.LogDebug($"{scanNode.headerText}, {scanNode.subText}");
    }



    void IGoldenGlassSecret.BeginReveal()
    {
        if (StartOfRound.Instance.allPlayerScripts[onPlayerIndex] != StartOfRound.Instance.localPlayerController)
        {
            colliderToToggle.enabled = true;
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        colliderToToggle.enabled = false;
    }
}
