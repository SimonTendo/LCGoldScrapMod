using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;

public class CrownScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource crownAudio;
    public AudioSource farAudio;
    public AudioSource loopAudio;
    public AudioClip launchClip;
    public AudioClip wearClip;
    public AudioClip landClip;
    public Light launchLight;
    public Collider mainCollider;
    public Collider scanNodeCollider;

    [Space(3f)]
    [Header("Wearer")]
    public PlayerControllerB playerWornBy;
    public PlayerControllerB playerWearingDayStart;
    public Vector3 wearRotationOffset;
    public Vector3 wearPositionOffset;
    public Vector3 wearRotationOffsetLocal;
    public Vector3 wearPositionOffsetLocal;
    public int samePlayerStreak;
    private bool wearerLocal;

    [Space(3f)]
    [Header("Knock off by")]
    public bool knockDeath;
    /*public bool knockDamage;
    public bool knockFallDamage;
    public bool knockCriticalInjury;
    public bool knockBlastForce;
    public bool knockTeleporting;
    public bool knockStunning;*/

    [Space(3f)]
    [Header("Launch")]
    public int maxVertical;
    public int maxHorizontal;
    public AnimationCurve curveUp;
    public AnimationCurve curveDown;
    private bool activatedLaunch;
    private bool launchGoingUp;
    private float launchHeight;
    private float riseTime;
    private float multiplierRiseTime;
    private float multiplierFallTime;

    [Space(3f)]
    [Header("Monetary value")]
    public int startingValue;
    public int startingValueFever;
    public int itemValuePercentage;
    public int consecutiveIncreaseDropoff;
    private int previousAddedAmount;

    public override void Start()
    {
        base.Start();
        launchLight.enabled = false;
        scrapPersistedThroughRounds = true;
        loopAudio.Stop();
        if (IsServer && !isInShipRoom)
        {
            int valueToSend = RarityManager.CurrentlyGoldFever() ? startingValueFever : startingValue;
            valueToSend = (int)((float)valueToSend * Configs.hostPriceMultiplier);
            SyncValueClientRpc(valueToSend, 0);
        }
    }

    //Launch functionality
    public override void LateUpdate()
    {
        if (playerWornBy != null)
        {
            isInFactory = playerWornBy.isInsideFactory;
            isInShipRoom = playerWornBy.isInHangarShipRoom;
            if (!playerWornBy.isPlayerControlled)
            {
                parentObject = null;
                if (playerWornBy.isPlayerDead)
                {
                    CheckLaunchCondition(0);
                    return;
                }
                ReplicateNormalDrop();
                return;
            }
            if (parentObject != null)
            {
                transform.rotation = parentObject.rotation;
                Vector3 rotationOffset = wearerLocal ? wearRotationOffsetLocal : wearRotationOffset;
                transform.Rotate(rotationOffset);
                transform.position = parentObject.position;
                Vector3 positionOffset = wearerLocal ? wearPositionOffsetLocal : wearPositionOffset;
                positionOffset = parentObject.rotation * positionOffset;
                transform.position += positionOffset;
                return;
            }
        }
        base.LateUpdate();
    }

    public override void EquipItem()
    {
        base.EquipItem();
        riseTime = 0f;
        activatedLaunch = false;
        loopAudio.Stop();
        launchLight.enabled = false;
    }

    private void CheckLaunchCondition(int launchType)
    {
        Logger.LogDebug($"checking with launchType {launchType}");
        switch (launchType)
        {
            case 0:
                if (knockDeath)
                {
                    StartLaunch();
                }
                return;
        }
    }

    private void StartLaunch()
    {
        if (activatedLaunch)
        {
            return;
        }
        Logger.LogDebug($"#{NetworkObjectId}: started launch");
        activatedLaunch = true;
        launchGoingUp = true;
        hasHitGround = false;
        launchLight.enabled = true;
        wearerLocal = false;
        grabbable = false;
        isHeld = false;
        isPocketed = false;
        parentObject = null;
        riseTime = 0f;
        fallTime = 0f;
        customGrabTooltip = " ";
        playerWornBy.carryWeight = Mathf.Clamp(playerWornBy.carryWeight - (itemProperties.weight - 1f), 1f, 10f);
        Transform playerParent = playerWornBy.parentedToElevatorLastFrame ? StartOfRound.Instance.elevatorTransform : StartOfRound.Instance.propsContainer;
        transform.SetParent(playerParent, true);
        startFallingPosition = transform.parent.InverseTransformPoint(transform.position);
        targetFloorPosition = new Vector3(3000f, -400f, 3000f);
        launchHeight = General.GetClosestCollision(transform.position, Vector3.up, maxVertical);
        multiplierRiseTime = Mathf.Sqrt(maxVertical / (launchHeight + 1));
        if (!farAudio.isPlaying)
        {
            farAudio.PlayOneShot(launchClip);
            WalkieTalkie.TransmitOneShotAudio(farAudio, launchClip);
        }
        if (radarIcon == null)
        {
            radarIcon = Instantiate(StartOfRound.Instance.itemRadarIconPrefab, RoundManager.Instance.mapPropsContainer.transform).transform;
        }
        if (IsServer)
        {
            SyncTargetFloorPositionClientRpc(GetTargetFloorPositionInRange());
        }
        playerWornBy = null;
    }

    public override void FallWithCurve()
    {
        if (activatedLaunch)
        {
            transform.Rotate(720f * Time.deltaTime, 0f, 0f, Space.Self);
            if (launchGoingUp)
            {
                transform.localPosition = Vector3.Lerp(startFallingPosition, new Vector3(startFallingPosition.x, startFallingPosition.y + launchHeight, startFallingPosition.z), curveUp.Evaluate(riseTime));
                riseTime += Time.deltaTime * multiplierRiseTime * 0.7f;
                if (riseTime > 1)
                {
                    launchGoingUp = false;
                    grabbable = true;
                    customGrabTooltip = null;
                    mainCollider.enabled = true;
                    scanNodeCollider.enabled = true;
                }
            }
            else
            {
                transform.localPosition = Vector3.Lerp(new Vector3(startFallingPosition.x, startFallingPosition.y + launchHeight, startFallingPosition.z), new Vector3(targetFloorPosition.x, Mathf.Lerp(startFallingPosition.y + launchHeight, targetFloorPosition.y, curveDown.Evaluate(fallTime)), targetFloorPosition.z), curveDown.Evaluate(fallTime));
                fallTime += Time.deltaTime * multiplierFallTime * multiplierRiseTime * 0.6f;
            }
        }
        else
        {
            base.FallWithCurve();
        }
    }

    private Vector3 GetTargetFloorPositionInRange()
    {
        Vector3 startingTarget = transform.position + Vector3.down * 2.5f;
        Vector3 newTargetFloorPosition = Vector3.zero;
        Vector3 randomDir = Vector3.zero;
        switch (Random.Range(0, 4))
        {
            case 0:
                randomDir = Vector3.right;
                break;
            case 1:
                randomDir = Vector3.left;
                break;
            case 2:
                randomDir = Vector3.forward;
                break;
            case 3:
                randomDir = Vector3.back;
                break;
        }
        float maxRange = 10f;
        float maxUp = Mathf.Clamp(launchHeight, 0, maxRange);
        for (int i = (int)General.GetClosestCollision(transform.position, randomDir, maxHorizontal); i >= 0; i--)
        {
            RaycastHit[] allHits = Physics.RaycastAll(transform.position + Vector3.up * maxUp + randomDir * i, Vector3.down, maxRange * 2, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
            Logger.LogDebug($"checking from {transform.position + Vector3.up * launchHeight + randomDir * i}");
            Vector3 highestPoint = new Vector3(0, -9999, 0);
            for (int j = 0; j < allHits.Length; j++)
            {
                Logger.LogDebug($"hit {i}:{j} hit point {allHits[j].point}");
                if (allHits[j].point.y > highestPoint.y)
                {
                    Logger.LogDebug($"hit new high point at {allHits[j].point}");
                    highestPoint = allHits[j].point;
                }
            }
            if (highestPoint.y != -9999)
            {
                newTargetFloorPosition = highestPoint;
                break;
            }
        }
        if (newTargetFloorPosition == Vector3.zero)
        {
            newTargetFloorPosition = startingTarget;
        }
        return newTargetFloorPosition;
    }

    public override void PlayDropSFX()
    {
        if (activatedLaunch)
        {
            if (!launchGoingUp)
            {
                crownAudio.PlayOneShot(landClip);
                WalkieTalkie.TransmitOneShotAudio(crownAudio, landClip);
                RoundManager.Instance.PlayAudibleNoise(transform.position, (float)maxHorizontal / 2, (float)itemValuePercentage / 100);
                loopAudio.Play();
            }
        }
        else
        {
            base.PlayDropSFX();
        }
    }

    public override void OnHitGround()
    {
        base.OnHitGround();
        if (activatedLaunch && !launchGoingUp)
        {
            grabbable = true;
            isInShipRoom = StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(transform.position);
            Logger.LogDebug($"launch active; landed in ship room = {isInShipRoom}");
            CrownInPhysicsRegion();
        }
        if (!mainCollider.enabled || !scanNodeCollider.enabled)
        {
            mainCollider.enabled = true;
            scanNodeCollider.enabled = true;
        }
    }

    private void CrownInPhysicsRegion()
    {
        NetworkObject pRegionObject = GetPhysicsRegionOfDroppedObject(null, out var hitPoint);
        if (pRegionObject != null)
        {
            targetFloorPosition = hitPoint;
            PlayerPhysicsRegion pRegionTransform = pRegionObject.GetComponentInChildren<PlayerPhysicsRegion>();
            if (pRegionTransform != null && pRegionTransform.physicsTransform != null)
            {
                transform.SetParent(pRegionTransform.physicsTransform, true);
            }
            else
            {
                transform.SetParent(pRegionObject.transform, true);
            }
            Logger.LogDebug($"{name} #{NetworkObjectId} hit PhysicsRegion! physicsTransform valid: {pRegionTransform != null && pRegionTransform.physicsTransform != null} | parent: {transform.parent.name}");
        }
    }

    private IEnumerator SyncTargetFloorPosition(Vector3 hostPosition)
    {
        yield return new WaitUntil(() => !launchGoingUp);
        Transform parentTransform = StartOfRound.Instance.shipBounds.bounds.Contains(hostPosition) ? StartOfRound.Instance.elevatorTransform : StartOfRound.Instance.propsContainer;
        if (parentTransform != transform.parent)
        {
            Logger.LogDebug($"parent not old parent, reparenting and resetting startFallingPosition");
            startFallingPosition = parentTransform.InverseTransformPoint(transform.parent.TransformPoint(startFallingPosition));
            transform.SetParent(parentTransform, true);
        }
        Logger.LogDebug($"pos: {hostPosition} | parent: {transform.parent.name}");
        targetFloorPosition = transform.parent.InverseTransformPoint(hostPosition) + Vector3.up * itemProperties.verticalOffset;
        multiplierFallTime = Mathf.Sqrt(maxHorizontal / Vector3.Distance(startFallingPosition, targetFloorPosition));
    }

    private void ReplicateNormalDrop()
    {
        Logger.LogDebug("ReplicateNormalDrop() called, likely due to disconnect");
        playerWornBy = null;
        playerHeldBy = null;
        isHeld = false;
        isPocketed = false;
        wearerLocal = false;
        grabbable = true;
        heldByPlayerOnServer = false;
        launchLight.enabled = false;
        mainCollider.enabled = true;
        scanNodeCollider.enabled = true;
        customGrabTooltip = null;
        parentObject = null;
        loopAudio.Stop();
        startFallingPosition = transform.parent.InverseTransformPoint(transform.position);
        Vector3 vector = GetItemFloorPosition();
        isInShipRoom = StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(vector);
        if (StartOfRound.Instance.shipBounds.bounds.Contains(vector))
        {
            transform.SetParent(StartOfRound.Instance.elevatorTransform, true);
            targetFloorPosition = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(vector);
        }
        else
        {
            transform.SetParent(StartOfRound.Instance.propsContainer, true);
            targetFloorPosition = StartOfRound.Instance.propsContainer.InverseTransformPoint(vector);
        }
    }



    //Wear functionality
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (PlayerAlreadyWearingCrown(playerHeldBy))
        {
            return;
        }
        if (buttonDown)
        {
            StartWearingCrown(playerHeldBy);
        }
    }

    private void StartWearingCrown(PlayerControllerB playerWearing)
    {
        Logger.LogDebug($"started wearing {gameObject.name} #{NetworkObjectId} on {playerWearing.playerUsername}");
        playerWornBy = playerWearing;
        playerHeldBy = null;
        wearerLocal = playerWornBy == StartOfRound.Instance.localPlayerController;
        parentObject = wearerLocal ? playerWornBy.gameplayCamera.transform : playerWornBy.playerGlobalHead;
        heldByPlayerOnServer = false;
        grabbable = false;
        isHeld = false;
        isPocketed = false;
        mainCollider.enabled = false;
        scanNodeCollider.enabled = !wearerLocal;
        fallTime = 0f;
        int itemSlot = -1;
        for (int i = 0; i < playerWornBy.ItemSlots.Length; i++)
        {
            if (playerWornBy.ItemSlots[i] == this)
            {
                itemSlot = i;
                break;
            }
        }
        if (itemSlot != -1)
        {
            playerWornBy.ItemSlots[itemSlot] = null;
            if (wearerLocal)
            {
                HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
                HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                HUDManager.Instance.ClearControlTips();
            }
        }
        if (radarIcon != null)
        {
            Destroy(radarIcon.gameObject);
        }
        playerWornBy.twoHanded = false;
        playerWornBy.twoHandedAnimation = false;
        playerWornBy.isHoldingObject = false;
        playerWornBy.currentlyHeldObject = null;
        playerWornBy.currentlyHeldObjectServer = null;
        playerWornBy.equippedUsableItemQE = false;
        playerWornBy.playerBodyAnimator.SetBool("cancelHolding", true);
        playerWornBy.playerBodyAnimator.SetTrigger("Throw");
        playerWornBy.playerBodyAnimator.SetBool("Grab", false);
        playerWornBy.playerBodyAnimator.SetBool(itemProperties.grabAnim, false);
        crownAudio.PlayOneShot(wearClip);
        WalkieTalkie.TransmitOneShotAudio(crownAudio, wearClip, 0.25f);
    }

    private bool PlayerAlreadyWearingCrown(PlayerControllerB playerTryingToWear)
    {
        foreach (CrownScript crown in FindObjectsByType<CrownScript>(FindObjectsSortMode.None))
        {
            if (crown.playerWornBy == playerTryingToWear)
            {
                return true;
            }
        }
        return false;
    }



    //Monetary value
    public void RegisterPlayerWearingDayStart()
    {
        playerWearingDayStart = playerWornBy;
        if (playerWearingDayStart != null)
        {
            Logger.LogDebug($"#{NetworkObjectId}: {playerWearingDayStart.playerUsername}");
        }
    }

    public void CalculateStreakValueIncrease()
    {
        if (playerWornBy == null || !StartOfRound.Instance.currentLevel.planetHasTime)
        {
            return;
        }
        int wearingPlayerTotalProfit = StartOfRound.Instance.gameStats.allPlayerStats[(int)playerWornBy.playerClientId].profitable;
        if (playerWornBy == playerWearingDayStart)
        {
            samePlayerStreak++;
        }
        else
        {
            samePlayerStreak = 0;
            previousAddedAmount = wearingPlayerTotalProfit - wearingPlayerTotalProfit / Mathf.Max(1, StartOfRound.Instance.gameStats.daysSpent / 2);
            Logger.LogDebug($"removed from this players totalProfit {wearingPlayerTotalProfit / Mathf.Max(1, StartOfRound.Instance.gameStats.daysSpent)} with days {StartOfRound.Instance.gameStats.daysSpent}");
        }
        float newPercentage;
        if (RarityManager.CurrentlyGoldFever())
        {
            newPercentage = (float)itemValuePercentage / 100;
        }
        else
        {
            newPercentage = (float)Mathf.Clamp(itemValuePercentage - consecutiveIncreaseDropoff * samePlayerStreak, 10, itemValuePercentage) / 100;
        }
        int amountToAdd = (int)((wearingPlayerTotalProfit - previousAddedAmount) * newPercentage);
        if (playerWornBy == playerWearingDayStart)
        {
            previousAddedAmount += amountToAdd;
        }
        int newValue = scrapValue + amountToAdd;
        newValue = (int)((float)newValue * Configs.hostPriceMultiplier);
        Logger.LogDebug($"totalProfit: {wearingPlayerTotalProfit} | streak: {samePlayerStreak} | percentage: {newPercentage} | previous: {previousAddedAmount} | add: {amountToAdd} | new: {newValue} | config: {Configs.hostPriceMultiplier}");
        SyncValueClientRpc(newValue, samePlayerStreak);
    }



    //Rpc's
    [ClientRpc]
    private void SyncTargetFloorPositionClientRpc(Vector3 hostPosition)
    {
        StartCoroutine(SyncTargetFloorPosition(hostPosition));
    }

    [ClientRpc]
    private void SyncValueClientRpc(int value, int hostStreak)
    {
        SetScrapValue(value);
        samePlayerStreak = hostStreak;
        Logger.LogDebug($"trying to set value of {gameObject.name} #{NetworkObjectId} on local client, value: {scrapValue}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        if (playerWornBy != null)
        {
            SyncUponJoinClientRpc(playerID, (int)playerWornBy.playerClientId);
        }
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(int newPlayerID, int wearingPlayerID)
    {
        if (newPlayerID == (int)StartOfRound.Instance.localPlayerController.playerClientId)
        {
            StartWearingCrown(StartOfRound.Instance.allPlayerScripts[wearingPlayerID]);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), "WritePlayerNotes")]
    public class NewStartOfRoundWriteNotes
    {
        [HarmonyPrefix]
        public static void WritePlayerNotesPostfix(StartOfRound __instance)
        {
            List<PlayerControllerB> writtenToPlayers = new List<PlayerControllerB>();
            CrownScript[] allCrowns = FindObjectsByType<CrownScript>(FindObjectsSortMode.None);
            foreach (CrownScript crown in allCrowns)
            {
                if (writtenToPlayers.Contains(crown.playerWornBy) || crown.playerWornBy == null)
                {
                    continue;
                }
                if (crown.playerWornBy == crown.playerWearingDayStart)
                {
                    string textToAdd = "Maintained their royalty.";
                    if (crown.samePlayerStreak >= 2)
                    {
                        textToAdd = $"{crown.samePlayerStreak + 1}-day crown streak.";
                    }
                    __instance.gameStats.allPlayerStats[(int)crown.playerWornBy.playerClientId].playerNotes.Add(textToAdd);
                    writtenToPlayers.Add(crown.playerWornBy);
                    continue;
                }
                else if (crown.playerWearingDayStart != null && crown.playerWornBy != crown.playerWearingDayStart)
                {
                    __instance.gameStats.allPlayerStats[(int)crown.playerWornBy.playerClientId].playerNotes.Add($"Dethroned {crown.playerWearingDayStart.playerUsername}.");
                    __instance.gameStats.allPlayerStats[(int)crown.playerWearingDayStart.playerClientId].playerNotes.Add($"Lost crown to {crown.playerWornBy.playerUsername}.");
                    writtenToPlayers.Add(crown.playerWornBy);
                    writtenToPlayers.Add(crown.playerWearingDayStart);
                    continue;
                }
                else if (crown.playerWearingDayStart == null && crown.playerWornBy != null)
                {
                    __instance.gameStats.allPlayerStats[(int)crown.playerWornBy.playerClientId].playerNotes.Add("Started their reign.");
                    writtenToPlayers.Add(crown.playerWornBy);
                    continue;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), "DamagePlayer")]
    public class NewPlayerDamage
    {
        [HarmonyPostfix]
        public static void DamagePlayerPostfix(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.connectedPlayersAmount == 0 && __instance.IsOwner && !__instance.isPlayerDead && __instance.AllowPlayerDeath() && FindAnyObjectByType<CrownScript>() != null)
            {
                foreach (CrownScript crown in FindObjectsByType<CrownScript>(FindObjectsSortMode.None))
                {
                    if (crown.playerWornBy != null && crown.playerWornBy == __instance)
                    {
                        Logger.LogDebug($"DamagePlayer Patch setting {crown.name} #{crown.NetworkObjectId} to launch!!");
                        crown.StartLaunch();
                    }
                }
            }
        }
    }
}
