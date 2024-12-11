using System.Collections;
using UnityEngine;
using Unity.Netcode;
using BepInEx.Logging;
using GameNetcodeStuff;

public class GoldmineScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("Functionality")]
    public GoldmineTrigger trigger;
    public bool hasExploded = false;
    public bool activated = false;

    public bool willExplodeImmediately = false;
    public bool willExplodeOnDelay = false;

    private float timeAtLastLightToggle;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource audioSource;
    public AudioClip triggerClip;
    public AudioClip beepClip;
    public AudioClip onClip;
    public AudioClip offClip;
    public Light triggerLight;

    public override void Start()
    {
        base.Start();
        triggerLight.enabled = false;
        if (IsServer && !isInShipRoom && Random.Range(1, 101) <= 25)
        {
            ToggleActiveClientRpc(true);
        }
    }

    public override void Update()
    {
        base.Update();
        if (hasExploded)
        {
            return;
        }
        if (willExplodeOnDelay && Time.realtimeSinceStartup - timeAtLastLightToggle > 0.075)
        {
            ToggleLight();
        }
        if (activated)
        {
            if (Time.realtimeSinceStartup - timeAtLastLightToggle > 5)
            {
                audioSource.PlayOneShot(beepClip);
                ToggleLight();
            }
            if (triggerLight.enabled && Time.realtimeSinceStartup - timeAtLastLightToggle > 0.1f)
            {
                ToggleLight();
            }
        }
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (StartOfRound.Instance.shipDoorsEnabled && !StartOfRound.Instance.inShipPhase)
        {
            ImmediateExplosionServerRpc((int)playerHeldBy.playerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ImmediateExplosionServerRpc(int playerID)
    {
        ImmediateExplosionClientRpc(playerID);
    }

    [ClientRpc]
    private void ImmediateExplosionClientRpc(int playerID)
    {
        if (!hasExploded && !willExplodeImmediately && !willExplodeOnDelay)
        {
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: call to IMMEDIATELY explode {StartOfRound.Instance.allPlayerScripts[playerID].gameObject.name}");
            StartCoroutine(ExplodeImmediately(playerID));
        }
    }

    private IEnumerator ExplodeImmediately(int playerID)
    {
        willExplodeImmediately = true;
        yield return new WaitForEndOfFrame();
        ExplodeAndSetValues(2.5f, 5f, 50, 15f);
    }

    public override void EquipItem()
    {
        base.EquipItem();
        playerHeldBy.equippedUsableItemQE = true;
    }

    public override void PocketItem()
    {
        base.PocketItem();
        playerHeldBy.equippedUsableItemQE = false;
    }

    public override void DiscardItem()
    {
        playerHeldBy.equippedUsableItemQE = false;
        base.DiscardItem();
    }

    public override void ItemInteractLeftRight(bool right)
    {
        base.ItemInteractLeftRight(right);
        if (right && !Config.hostToolRebalance)
        {
            ToggleActiveServerRpc();
        }
        if (!right && StartOfRound.Instance.shipDoorsEnabled && !StartOfRound.Instance.inShipPhase)
        {
            playerHeldBy.DiscardHeldObject();
            DelayedExplosionServerRpc(0.8f, true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DelayedExplosionServerRpc(float delay, bool waitToHitGround)
    {
        DelayedExplosionClientRpc(delay, waitToHitGround);
    }

    [ClientRpc]
    public void DelayedExplosionClientRpc(float delay, bool waitToHitGround)
    {
        if (!hasExploded && !willExplodeOnDelay && !willExplodeImmediately)
        {
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId}: call to DELAY explode {delay} with wait {waitToHitGround}");
            StartCoroutine(ExplodeOnDelay(delay, waitToHitGround));
        }
    }

    private IEnumerator ExplodeOnDelay(float delay, bool waitToHitGround)
    {
        willExplodeOnDelay = true;
        activated = false;
        grabbable = false;
        customGrabTooltip = " ";
        if (waitToHitGround)
        {
            yield return new WaitUntil(() => hasHitGround);
        }
        audioSource.PlayOneShot(triggerClip);
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
        ExplodeAndSetValues(5f, 10f, 33, 30f);
    }

    private void ExplodeAndSetValues(float killRange, float damageRange, int nonLethalDamage, float physicsForce)
    {
        hasExploded = true;
        trigger.enabled = false;
        triggerLight.enabled = false;
        if (playerHeldBy != null)
        {
            playerHeldBy.KillPlayer(Vector3.up * 25f, true, CauseOfDeath.Blast, 7, playerHeldBy.transform.position + Vector3.up * 10);
        }
        Landmine.SpawnExplosion(transform.position, true, killRange, damageRange, nonLethalDamage, physicsForce);
        DestroyObjectInHand(playerHeldBy);
        StartCoroutine(DelaySettingObjectAway());
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

    [ServerRpc(RequireOwnership = false)]
    private void ToggleActiveServerRpc()
    {
        activated = !activated;
        ToggleActiveClientRpc(activated);
    }

    [ClientRpc]
    private void ToggleActiveClientRpc(bool hostActivated, int playerID = -1)
    {
        if (playerID == -1 || playerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            activated = hostActivated;
            AudioClip clipToPlay = activated ? onClip : offClip;
            audioSource.PlayOneShot(clipToPlay);
            Logger.LogDebug($"{gameObject.name} #{NetworkObjectId} activated: {activated}");
        }
    }

    private void ToggleLight()
    {
        timeAtLastLightToggle = Time.realtimeSinceStartup;
        triggerLight.enabled = !triggerLight.enabled;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        ToggleActiveClientRpc(activated, playerID);
    }
}
