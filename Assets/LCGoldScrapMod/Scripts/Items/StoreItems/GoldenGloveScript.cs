using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx.Logging;
using HarmonyLib;

public class GoldenGloveScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    [Space(3f)]
    [Header("GOLDEN GLOVE")]
    [Header("General")]
    public GameObject box;
    public GameObject glove;
    public GameObject extendo;
    public Transform collisionCheck;
    public Transform grabPositionItems;
    public Transform grabPositionPlayers;
    public Collider rpsCollider;
    public int checkEveryXFrames;
    private Coroutine extendCoroutine;
    private Coroutine retractCoroutine;
    private int stateIndex;
    private bool canExtend;
    private bool extending;
    private bool startedExtendHolding;
    private float currentSpeed;

    private static int gloveMask = -1;
    private static int layerDefault;
    private static int layerRoom;
    private static int layerColliders;
    private static int layerEnemies;
    private static int layerInteractableObject;
    private static int layerTerrain;
    private static int layerRailing;

    private GrabbableObject holdingItem;
    private PlayerControllerB holdingPlayer;
    private bool holdingLocalPlayer;
    private List<int> hitPlayerIDs = new List<int>();
    private bool hasHitDoor;
    private Vector3 hitTreeAt;
    private string hitRPS;

    private float timeLastSlide;
    private float timeLastStruggle;
    private int heldPlayerStruggles;

    [Space(3f)]
    [Header("Parameters")]
    public float maxExtendDistance;
    public float rpsTieWait;
    [Space]
    public float rockExtendSpeed;
    public float rockRetractSpeed;
    public float rockExtendedWait;
    public float rockKnockbackForce;
    public AnimationCurve rockDamagePlayers;
    public AnimationCurve rockDamageEnemies;
    public float rockJumpBatteryDrain;
    public AnimationCurve rockJumpDropOff;
    [Space]
    public float paperExtendSpeed;
    public float paperRetractSpeedDefault;
    private float paperRetractSpeedEvaluation;
    public AnimationCurve paperRetractDropOff;
    public float paperExtendedWait;
    public float paperKnockbackForce;
    public float paperLaunchForce;
    public AnimationCurve paperLaunchDropOff;
    [Space]
    public float scissorsExtendSpeed;
    public float scissorsRetractSpeed;
    public float scissorsExtendedWait;
    public float scissorsKnockbackForce;
    public float scissorsStunEnemies;
    public float scissorsStunPlayers;
    public float scissorsStunHazards;

    [Space(3f)]
    [Header("Meshes")]
    public MeshFilter gloveMeshFilter;
    public Animator gloveAnimator;
    public MeshRenderer[] enableItemMeshes;
    public MeshRenderer[] toggleShadowsOf;
    [Space]
    public Mesh rockMesh;
    public Mesh paperMesh;
    public Mesh paperHoldingMesh;
    public Mesh scissorsMesh;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource boxAudio;
    public AudioSource gloveAudio;
    public AudioSource loopAudio;
    [Space]
    public AudioClip extendStartClip;
    public AudioClip extendingLoopClip;
    public AudioClip extendStopClip;
    [Space]
    public AudioClip retractingLoopClip;
    public AudioClip retractStopClip;
    [Space]
    public AudioClip switchClip;
    public AudioClip struggleClip;
    public AudioClip slideLoopClip;
    [Space]
    public AudioClip rockPunchClip;
    public AudioClip rockJumpClip;
    public AudioClip paperGrabClip;
    public AudioClip paperSaveClip;
    public AudioClip scissorsStunClip;
    
    private List<AudioClip> syncedClips;



    //Overrides
    public override void Start()
    {
        base.Start();
        canExtend = true;
        rpsCollider.enabled = false;
        SetUpSyncedClips();
        if (gloveMask == -1)
        {
            GetLayers();
        }
        if (Configs.hostToolRebalance)
        {
            RebalanceTool();
        }
    }

    public override void Update()
    {
        base.Update();
        if (holdingItem != null && holdingItem.isInFactory != isInFactory)
        {
            holdingItem.isInFactory = isInFactory;
        }
        if (holdingPlayer != null && holdingLocalPlayer)
        {
            PerformGlobalHoldingPlayerUpdate();
            if (!holdingLocalPlayer)
            {
                return;
            }
            if (Time.realtimeSinceStartup - timeLastStruggle > 0.67f)
            {
                PerformLocalHoldingPlayerUpdate();
                timeLastStruggle = Time.realtimeSinceStartup;
            }
        }
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (holdingPlayer != null && !holdingLocalPlayer)
        {
            PerformGlobalHoldingPlayerUpdate();
        }
    }

    private void PerformGlobalHoldingPlayerUpdate()
    {
        if (holdingPlayer.teleportedLastFrame || holdingPlayer.inAnimationWithEnemy != null)
        {
            Logger.LogDebug("E");
            DiscardHoldingPlayer(false);
            return;
        }
        if (playerHeldBy == null || !playerHeldBy.isPlayerControlled || !holdingPlayer.isPlayerControlled)
        {
            Logger.LogDebug("F");
            DiscardHoldingPlayer();
            return;
        }
        holdingPlayer.transform.position = grabPositionPlayers.position;
        holdingPlayer.transform.rotation = grabPositionPlayers.rotation;
        if (holdingPlayer.isInsideFactory != playerHeldBy.isInsideFactory)
        {
            holdingPlayer.isInsideFactory = playerHeldBy.isInsideFactory;
            int presetToUse = -1;
            EntranceTeleport[] allEntrances = FindObjectsByType<EntranceTeleport>(FindObjectsSortMode.None);
            foreach (EntranceTeleport entrance in allEntrances)
            {
                if (holdingPlayer.isInsideFactory == entrance.isEntranceToBuilding)
                {
                    presetToUse = entrance.audioReverbPreset;
                    break;
                }
            }
            if (presetToUse == -1)
            {
                Logger.LogDebug("failed to find AudioReverbPreset");
            }
            else
            {
                FindObjectOfType<AudioReverbPresets>()?.audioPresets[presetToUse].ChangeAudioReverbForPlayer(holdingPlayer);
            }
        }
        if (Time.realtimeSinceStartup - timeLastSlide > 1f)
        {
            timeLastSlide = Time.realtimeSinceStartup;
            if (playerHeldBy == null || playerHeldBy.currentlyHeldObjectServer != this)
            {
                return;
            }
            float fastestAngle = !IsOwner || playerHeldBy.isCrouching ? 10f : 45f;
            float angleToDown = Vector3.Angle(playerHeldBy.playerEye.forward, Vector3.down);
            if (angleToDown <= 70f)
            {
                if (!loopAudio.isPlaying)
                {
                    loopAudio.volume = 0.0f;
                    loopAudio.clip = slideLoopClip;
                    loopAudio.Play();
                }
                float highestPossibleDistance = Mathf.Max(Mathf.Abs(fastestAngle - 70), Mathf.Abs(fastestAngle - 10));
                float distanceFromFastest = Mathf.Abs(angleToDown - fastestAngle);
                loopAudio.volume = Mathf.Lerp(loopAudio.volume, 1f - distanceFromFastest / highestPossibleDistance, 0.6f);
                if (IsOwner)
                {
                    RoundManager.Instance.PlayAudibleNoise(playerHeldBy.transform.position, loopAudio.volume * 17.5f, loopAudio.volume);
                }
            }
            else
            {
                loopAudio.Stop();
            }
        }
    }

    private void PerformLocalHoldingPlayerUpdate()
    {
        QuickMenuManager menu = FindObjectOfType<QuickMenuManager>();
        if (menu != null && menu.isMenuOpen)
        {
            return;
        }
        Vector2 lookInput = holdingPlayer.playerActions.Movement.Look.ReadValue<Vector2>();
        float totalLook = Mathf.Abs(lookInput.x) + Mathf.Abs(lookInput.y);
        if (totalLook > 55)
        {
            heldPlayerStruggles++;
            if (totalLook > 250)
            {
                heldPlayerStruggles++;
            }
            if (heldPlayerStruggles > 3)
            {
                Logger.LogDebug($"{name} #{NetworkObjectId} holdingPlayer SENDING custom call to discard holdingPlayer by ESCAPING");
                SyncDiscardHoldingPlayerServerRpc((int)DiscardHoldingPlayer().playerClientId);
            }
            else
            {
                PlaySFXOneShot(holdingPlayer.movementAudio, struggleClip, true, true, 0.3f, 5f, 0.75f + 0.25f * heldPlayerStruggles);
                PlaySyncedClipServerRpc((int)holdingPlayer.playerClientId, GetSourceIndex(gloveAudio), GetClipIndex(struggleClip), true, true, 0.3f, 5f, 0.75f + 0.25f * heldPlayerStruggles);
            }
        }
        else
        {
            heldPlayerStruggles = 0;
        }
    }

    public override void EquipItem()
    {
        base.EquipItem();
        if (playerHeldBy != null)
        {
            playerHeldBy.equippedUsableItemQE = true;
        }
    }

    public override void PocketItem()
    {
        rpsCollider.enabled = false;
        loopAudio.Stop();
        if (playerHeldBy != null)
        {
            playerHeldBy.equippedUsableItemQE = false;
            playerHeldBy.disableLookInput = false;
            playerHeldBy.twoHanded = false;
        }
        if (holdingItem != null)
        {
            OwnerDiscardHoldingItem();
        }
        if (holdingPlayer != null)
        {
            Logger.LogDebug("A");
            DiscardHoldingPlayer();
        }
        if (extendCoroutine != null || retractCoroutine != null)
        {
            StopExtendAndRetract(-1, false, false, true);
        }
        base.PocketItem();
    }

    public override void DiscardItem()
    {
        rpsCollider.enabled = false;
        loopAudio.Stop();
        if (playerHeldBy != null)
        {
            playerHeldBy.equippedUsableItemQE = false;
            playerHeldBy.disableLookInput = false;
            playerHeldBy.twoHanded = false;
        }
        if (playerHeldBy != null)
        {
            if (holdingItem != null)
            {
                playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (holdingItem.itemProperties.weight - 1), 1f, 10f);
            }
            if (holdingPlayer != null)
            {
                playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (holdingPlayer.carryWeight - 0.7f), 1f, 10f);
            }
        }
        if (extendCoroutine != null || retractCoroutine != null)
        {
            StopExtendAndRetract(-1, false, false, true);
        }
        base.DiscardItem();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (!extending && canExtend)
        {
            StartExtendGlove();
            ExtendGloveServerRpc((int)playerHeldBy.playerClientId);
        }
    }

    public override void ItemInteractLeftRight(bool right)
    {
        base.ItemInteractLeftRight(right);
        if (!right)
        {
            if (canExtend)
            {
                if (holdingItem != null)
                {
                    OwnerDiscardHoldingItem();
                }
                else if (holdingPlayer != null)
                {
                    Logger.LogDebug("B");
                    Logger.LogDebug($"{name} #{NetworkObjectId} owner SENDING custom call to discard holdingPlayer by LEFT/RIGHT");
                    DiscardHoldingPlayer();
                    SyncDiscardHoldingPlayerServerRpc((int)playerHeldBy.playerClientId);
                }
                else
                {
                    stateIndex++;
                    if (stateIndex == 3)
                    {
                        stateIndex = 0;
                    }
                    Logger.LogDebug($"{name} #{NetworkObjectId} OWNER locally set new state to [{stateIndex}]");
                    SwitchStateLocal(stateIndex);
                    SyncStateServerRpc((int)playerHeldBy.playerClientId, stateIndex);
                }
            }
            else if (holdingItem != null)
            {
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER performing interactLeftRight to discard holdingItem with EXTENDING {extending}");
                if (extending)
                {
                    StopExtendAndRetract();
                    RetractGloveServerRpc((int)playerHeldBy.playerClientId);
                }
                else
                {
                    OwnerDiscardHoldingItem();
                }
            }
            else if (extending)
            {
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER performing interactLeftRight while extending to StopExtendAndRetract()!");
                StopExtendAndRetract();
                RetractGloveServerRpc((int)playerHeldBy.playerClientId);
            }
        }
    }

    public override void OnHitGround()
    {
        base.OnHitGround();
        if (holdingItem != null)
        {
            OwnerDiscardHoldingItem();
        }
        if (holdingPlayer != null)
        {
            Logger.LogDebug("C");
            DiscardHoldingPlayer();
        }
    }

    public override void EnableItemMeshes(bool enable)
    {
        for (int i = 0; i < enableItemMeshes.Length; i++)
        {
            enableItemMeshes[i].enabled = enable;
        }
    }



    //Coroutines
    private void StartExtendGlove()
    {
        if (retractCoroutine != null)
        {
            StopCoroutine(retractCoroutine);
            retractCoroutine = null;
        }
        if (extendCoroutine == null)
        {
            Logger.LogDebug($"starting Extend GoldenGlove for #{NetworkObjectId} on client");
            extendCoroutine = StartCoroutine(ExtendGlove());
        }
    }

    private IEnumerator ExtendGlove()
    {
        if (IsOwner && playerHeldBy != null)
        {
            playerHeldBy.disableLookInput = true;
            float knockbackForce = GetKnockbackForce();
            playerHeldBy.externalForceAutoFade += transform.up * knockbackForce;
        }
        extending = true;
        canExtend = false;
        isBeingUsed = true;
        paperRetractSpeedEvaluation = 0f;
        hitPlayerIDs.Clear();
        hasHitDoor = false;
        hitRPS = "";
        rpsCollider.enabled = true;
        currentSpeed = GetCurrentSpeed(true);
        startedExtendHolding = holdingItem != null || holdingPlayer != null;
        if (startedExtendHolding && holdingPlayer != null)
        {
            Logger.LogDebug("D");
            PlayerControllerB playerToLaunch = DiscardHoldingPlayer(true, playerHeldBy.transform.position);
            hitPlayerIDs.Add((int)playerToLaunch.playerClientId);
            float dropOffMultiplier = paperLaunchDropOff.Evaluate(playerToLaunch.carryWeight - 1);
            Logger.LogDebug($"should launch with force: {paperLaunchForce * dropOffMultiplier}");
            playerToLaunch.externalForceAutoFade += playerHeldBy.playerEye.forward * (paperLaunchForce * dropOffMultiplier) + Vector3.up * 2.5f;
            playerHeldBy.externalForceAutoFade += transform.up * rockKnockbackForce * -1;
            SetToolTips();
            AudioSource launchAudio;
            if (playerToLaunch == GameNetworkManager.Instance.localPlayerController)
            {
                launchAudio = gloveAudio;
            }
            else
            {
                launchAudio = playerToLaunch.movementAudio;
            }
            PlaySFXOneShot(launchAudio, rockJumpClip, true, true, 1f, 20f);
        }
        else
        {
            float audibleDistance = stateIndex == 0 ? 20f : stateIndex == 1 ? 2f : 10f;
            float sfxVolume = stateIndex == 0 ? 1f : stateIndex == 1 ? 0.1f : 0.25f;
            PlaySFXOneShot(boxAudio, extendStartClip, true, true, sfxVolume, audibleDistance, 1f, sfxVolume);
        }
        loopAudio.Stop();
        if (!isPocketed)
        {
            loopAudio.volume = 1.0f;
            loopAudio.clip = extendingLoopClip;
            loopAudio.Play();
        }
        int framesChecked = checkEveryXFrames - 1;
        ToggleShadowsMeshRenders(true);
        yield return new WaitForEndOfFrame();
        while (glove.transform.localPosition.z < maxExtendDistance)
        {
            glove.transform.localPosition = new Vector3(0, 0, Mathf.Clamp(glove.transform.localPosition.z + currentSpeed * Time.deltaTime * 60, 0, maxExtendDistance));
            extendo.transform.localScale = new Vector3(Mathf.Max(glove.transform.localPosition.z / 7f, 0.01f), 1f, 1f);
            if (IsOwner && playerHeldBy != null)
            {
                framesChecked++;
                if (framesChecked == checkEveryXFrames)
                {
                    if (Physics.Raycast(collisionCheck.position, transform.up, out var hitInfo, 2.5f, gloveMask, QueryTriggerInteraction.Collide))
                    {
                        CollideWhileExtending(hitInfo);
                    }
                    framesChecked = 0;
                }
            }
            yield return null;
        }
        glove.transform.localPosition = new Vector3(glove.transform.localPosition.x, glove.transform.localPosition.y, maxExtendDistance);
        StopExtendAndRetract();
    }

    private void CollideWhileExtending(RaycastHit hitInfo)
    {
        bool stopExtending = false;
        int playSyncClip = -1;
        bool syncClipAudibleNoise = true;
        bool playPoofParticle = false;
        GameObject collidedObject = hitInfo.collider.gameObject;
        PlayerControllerB hitPlayer;
        GrabbableObject hitItem;
        EnemyAICollisionDetect hitEnemy;
        IHittable hitInterface;
        if (!startedExtendHolding)
        {
            switch (stateIndex)
            {
                default:
                    hitPlayer = collidedObject.GetComponent<PlayerControllerB>();
                    if (hitPlayer != null && hitInfo.distance < 1.5f)
                    {
                        int hitPlayerID = (int)hitPlayer.playerClientId;
                        if (PerformHitPlayerCheck(hitPlayerID, hitPlayer))
                        {
                            RockCollidePlayerServerRpc(hitPlayerID, GetRockHitForce(true), hitPlayerIDs.Count);
                            break;
                        }
                    }
                    hitEnemy = collidedObject.GetComponent<EnemyAICollisionDetect>();
                    if (hitEnemy != null && hitEnemy.mainScript != null && !hitEnemy.mainScript.isEnemyDead && hitEnemy.mainScript.enemyType != null && hitEnemy.mainScript.enemyType.canBeStunned && hitInfo.distance < 1.25f)
                    {
                        hitEnemy.mainScript.HitEnemyOnLocalClient(GetRockHitForce(false), glove.transform.forward, playerHeldBy, true, -1);
                        playSyncClip = GetClipIndex(rockPunchClip);
                        stopExtending = true;
                        break;
                    }
                    hitInterface = collidedObject.GetComponent<IHittable>();
                    if (hitInterface != null && hitEnemy == null && hitPlayer == null && hitInfo.distance < 1.5f)
                    {
                        hitInterface.Hit(GetRockHitForce(false), glove.transform.forward, playerHeldBy, true, -1);
                        playSyncClip = GetClipIndex(rockPunchClip);
                        stopExtending = true;
                        break;
                    }
                    hitItem = collidedObject.GetComponent<GrabbableObject>();
                    if (hitItem != null && hitItem.grabbable && !hitItem.isHeld && !hitItem.isPocketed && !hitItem.deactivated && hitInfo.distance < 1.25f)
                    {
                        GrabbableObjectPhysicsTrigger hitItemPhysics = hitItem.GetComponentInChildren<GrabbableObjectPhysicsTrigger>();
                        if (hitItemPhysics != null)
                        {
                            hitItem.ActivatePhysicsTrigger(playerHeldBy.playerCollider);
                            stopExtending = true;
                            break;
                        }
                    }
                    break;
                case 1:
                    hitItem = collidedObject.GetComponent<GrabbableObject>();
                    if (hitItem != null && hitItem.grabbable && !hitItem.deactivated && !hitItem.isHeld && !hitItem.isPocketed && hitItem.itemProperties != null && hitInfo.distance < 1.25f)
                    {
                        float retractDropOffPoint = hitItem.itemProperties.weight - 1;
                        PaperCollideItemLocal((int)playerHeldBy.playerClientId, hitItem, retractDropOffPoint);
                        PaperCollideItemServerRpc((int)playerHeldBy.playerClientId, hitItem.NetworkObject, retractDropOffPoint);
                        stopExtending = true;
                        syncClipAudibleNoise = false;
                        break;
                    }
                    hitPlayer = collidedObject.GetComponent<PlayerControllerB>();
                    if (hitPlayer != null && !hitPlayer.inVehicleAnimation && hitInfo.distance < 1f)
                    {
                        int hitPlayerID = (int)hitPlayer.playerClientId;
                        if (PerformHitPlayerCheck(hitPlayerID, hitPlayer))
                        {
                            float retractDropOffPoint;
                            if (hitPlayer.currentlyHeldObjectServer != null && hitPlayer.currentlyHeldObjectServer.itemProperties != null)
                            {
                                hitItem = hitPlayer.currentlyHeldObjectServer;
                                retractDropOffPoint = hitItem.itemProperties.weight - 1;
                                PaperCollideItemLocal((int)playerHeldBy.playerClientId, hitItem, retractDropOffPoint, hitPlayerID);
                                PaperCollideItemServerRpc((int)playerHeldBy.playerClientId, hitItem.NetworkObject, retractDropOffPoint, hitPlayerID);
                                stopExtending = true;
                                syncClipAudibleNoise = false;
                                break;
                            }
                            retractDropOffPoint = hitPlayer.carryWeight - 0.75f;
                            PaperCollidePlayerLocal(hitPlayerID, retractDropOffPoint);
                            PaperCollidePlayerServerRpc((int)playerHeldBy.playerClientId, hitPlayerID, retractDropOffPoint);
                            stopExtending = true;
                            syncClipAudibleNoise = false;
                            break;
                        }
                    }
                    break;
                case 2:
                    hitPlayer = collidedObject.GetComponent<PlayerControllerB>(); 
                    if (hitPlayer != null && hitInfo.distance < 2f)
                    {
                        int hitPlayerID = (int)hitPlayer.playerClientId;
                        if (PerformHitPlayerCheck(hitPlayerID, hitPlayer))
                        {
                            ScissorsCollidePlayerServerRpc(hitPlayerID, GetRockHitForce(true), hitPlayerIDs.Count);
                            break;
                        }
                    }
                    hitEnemy = collidedObject.GetComponent<EnemyAICollisionDetect>();
                    if (hitEnemy != null && hitEnemy.mainScript != null && !hitEnemy.mainScript.isEnemyDead && hitEnemy.mainScript.enemyType != null && hitEnemy.mainScript.enemyType.canBeStunned && hitInfo.distance < 1.25f)
                    {
                        ScissorsCollideEnemyServerRpc(hitEnemy.mainScript.NetworkObject);
                        stopExtending = true;
                        break;
                    }
                    hitInterface = collidedObject.GetComponent<IHittable>();
                    if (hitInterface != null && hitPlayer == null && hitEnemy == null && hitInfo.distance < 1.5f)
                    {
                        Landmine hitMine = collidedObject.GetComponent<Landmine>();
                        if (hitMine != null && !hitMine.hasExploded)
                        {
                            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER interacting with MINE {hitMine.name} #{hitMine.NetworkObjectId}");
                            StartCoroutine(ReactivateHazardOnDelay(hitMine));
                            playSyncClip = GetClipIndex(scissorsStunClip); 
                            stopExtending = true;
                            break;
                        }
                        Turret hitTurret = collidedObject.GetComponent<Turret>();
                        if (hitTurret != null && hitTurret.turretActive && hitTurret.turretMode != TurretMode.Firing && hitTurret.turretMode != TurretMode.Berserk)
                        {
                            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER interacting with TURRET {hitTurret.name} #{hitTurret.NetworkObjectId}");
                            StartCoroutine(ReactivateHazardOnDelay(null, hitTurret));
                            playSyncClip = GetClipIndex(scissorsStunClip); 
                            stopExtending = true;
                            break;
                        }
                    }
                    if (collidedObject.layer == layerColliders && collidedObject.transform.parent?.parent?.parent?.name == "SpikeRoofTrapHazard(Clone)")
                    {
                        SpikeRoofTrap hitSpikes = collidedObject.transform.parent.parent.GetChild(0).GetChild(4).GetComponent<SpikeRoofTrap>();
                        if (hitSpikes != null && hitSpikes.trapActive)
                        {
                            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER interacting with SPIKES {hitSpikes.name} #{hitSpikes.NetworkObjectId}");
                            StartCoroutine(ReactivateHazardOnDelay(null, null, hitSpikes));
                            playSyncClip = GetClipIndex(scissorsStunClip);
                            stopExtending = true;
                            break;
                        }
                    }
                    break;
            }
        }
        if (!stopExtending)
        {
            GoldenGloveScript hitGlove = collidedObject.GetComponentInParent<GoldenGloveScript>();
            if (collidedObject.layer == layerColliders && hitGlove != null && hitGlove != this && hitInfo.distance < 1.75f)
            {
                hitGlove.rpsCollider.enabled = false;
                hitRPS = GetRPSWithOtherGlove(hitGlove.stateIndex);
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER hit ROCK PAPER SCISSORS on {hitGlove.name} #{hitGlove.NetworkObjectId} [{hitGlove.stateIndex}] with result: {hitRPS}");
                switch (hitRPS)
                {
                    default:
                        playSyncClip = GetClipIndex(extendStopClip);
                        stopExtending = true;
                        playPoofParticle = true;
                        syncClipAudibleNoise = false;
                        break;
                    case "Win":
                        break;
                    case "Tie":
                        playSyncClip = GetClipIndex(scissorsStunClip);
                        stopExtending = true;
                        break;
                }
            }
            DoorLock hitDoor = collidedObject.GetComponent<DoorLock>();
            if (!stopExtending && hitDoor != null && !hasHitDoor)
            {
                AnimatedObjectTrigger doorTrigger = hitDoor.gameObject.GetComponentInChildren<AnimatedObjectTrigger>();
                switch (stateIndex)
                {
                    default:
                        if (hitInfo.distance < 1.25f)
                        {
                            if (hitDoor.isLocked)
                            {
                                stopExtending = true;
                                playSyncClip = GetClipIndex(extendStopClip);
                            }
                            else if (doorTrigger != null && !doorTrigger.boolValue)
                            {
                                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] client sending OPEN on DOOR {hitDoor.name} #{hitDoor.NetworkObjectId}");
                                hitDoor.SetDoorAsOpen(true);
                                doorTrigger.TriggerAnimationNonPlayer(true);
                                CollideDoorServerRpc((int)playerHeldBy.playerClientId, hitDoor.NetworkObject, true, false);
                                RoundManager.Instance.PlayAudibleNoise(hitDoor.transform.position, 33, 1f);
                            }
                            hasHitDoor = true;
                        }
                        break;
                    case 1:
                        if (hitInfo.distance < 1f)
                        {
                            if (doorTrigger == null || !doorTrigger.boolValue || hitDoor.isLocked)
                            {
                                hasHitDoor = true;
                                stopExtending = true;
                                playSyncClip = GetClipIndex(extendStopClip);
                            }
                            else if (doorTrigger.boolValue && !hitDoor.isLocked && !startedExtendHolding)
                            {
                                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] client sending CLOSE on DOOR {hitDoor.name} #{hitDoor.NetworkObjectId}");
                                hitDoor.SetDoorAsOpen(false);
                                doorTrigger.TriggerAnimationNonPlayer();
                                CollideDoorServerRpc((int)playerHeldBy.playerClientId, hitDoor.NetworkObject, false, false);
                                hasHitDoor = true;
                                stopExtending = true;
                            }
                        }
                        break;
                    case 2:
                        if (hitInfo.distance < 1f && doorTrigger != null && !doorTrigger.boolValue)
                        {
                            if (hitDoor.isLocked)
                            {
                                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] client sending UNLOCK on DOOR {hitDoor.name} #{hitDoor.NetworkObjectId}");
                                hitDoor.UnlockDoorSyncWithServer();
                            }
                            else
                            {
                                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] client sending LOCK on DOOR {hitDoor.name} #{hitDoor.NetworkObjectId}");
                                hitDoor.LockDoor(5f);
                                if (hitDoor.doorLockSFX != null)
                                {
                                    PlaySFXOneShot(hitDoor.doorLockSFX, hitDoor.unlockSFX);
                                }
                                CollideDoorServerRpc((int)playerHeldBy.playerClientId, hitDoor.NetworkObject, false, true);
                            }
                            hasHitDoor = true;
                            stopExtending = true;
                        }
                        break;
                }
            }
            VehicleController hitVehicle = collidedObject.GetComponentInParent<VehicleController>();
            if (!stopExtending && hitVehicle != null)
            {
                if (stateIndex == 0 && hitInfo.distance < 1.5f && !hitVehicle.carDestroyed)
                {
                    hitVehicle.PushTruckWithArms();
                    stopExtending = true;
                }
                else if (startedExtendHolding || hitInfo.distance < 0.75f)
                {
                    stopExtending = true;
                    playSyncClip = GetClipIndex(extendStopClip);
                }
            }
            TerrainObstacleTrigger hitTree = collidedObject.GetComponent<TerrainObstacleTrigger>();
            if (!stopExtending && stateIndex == 0 && hitTree != null && hitInfo.distance < 1.25f)
            {
                Vector3 newHitTree = hitTree.transform.position;
                Logger.LogDebug($"{name} {NetworkObjectId} [{stateIndex}] hit tree: {newHitTree} | previous hitTreeID: #{hitTreeAt}");
                if ((GetRockHitForce(true) == 100 || newHitTree == hitTreeAt) && RoundManager.Instance.DestroyTreeAtPosition(newHitTree, 1f))
                {
                    PlaySFXOneShot(gloveAudio, rockPunchClip, true, true, 1f, 7f);
                    CollideTreeServerRpc((int)playerHeldBy.playerClientId, newHitTree, true);
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER sending hit TREE at {newHitTree} with breakTree True");
                }
                else
                {
                    PlaySFXOneShot(gloveAudio, rockPunchClip, true, true, 1f, 7f);
                    CollideTreeServerRpc((int)playerHeldBy.playerClientId, newHitTree, false);
                    stopExtending = true;
                    playPoofParticle = true;
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER sending hit TREE at {newHitTree} with breakTree False");
                }
                hitTreeAt = newHitTree;
            }
            SteamValveHazard hitValve = collidedObject.GetComponent<SteamValveHazard>();
            if (!stopExtending && !startedExtendHolding && hitValve != null && hitInfo.distance < 1f)
            {
                if (stateIndex == 1 && hitValve.triggerScript.interactable)
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER sending FIX on VALVE {hitValve.name} #{hitValve.NetworkObjectId}");
                    hitValve.FixValve();
                }
                else if (stateIndex == 0 && !hitValve.triggerScript.interactable)
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER sending BURST on VALVE {hitValve.name} #{hitValve.NetworkObjectId}");
                    float backUpFogMultiplier = Random.Range(0.6f, 0.98f);
                    if (hitValve.valveBurstTime == 0 || hitValve.valveBurstTime > TimeOfDay.Instance.normalizedTimeOfDay)
                    {
                        playSyncClip = GetClipIndex(rockPunchClip);
                        playPoofParticle = true;
                    }
                    else
                    {
                        playSyncClip = GetClipIndex(extendStopClip);
                    }
                    CollideValveLocal(hitValve, backUpFogMultiplier);
                    CollideValveServerRpc((int)playerHeldBy.playerClientId, hitValve.NetworkObject, backUpFogMultiplier);
                }
                stopExtending = true;
            }
            BreakerBox hitBreaker = collidedObject.GetComponentInParent<BreakerBox>();
            if (!stopExtending && hitBreaker != null && hitInfo.distance < 1.25f)
            {
                if (stateIndex == 0 && !RoundManager.Instance.powerOffPermanently)
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER interacting with BREAKER BOX {hitBreaker.name} #{hitBreaker.NetworkObjectId} to turn POWER OFF");
                    StartCoroutine(CollideBreakerBoxLocal(hitBreaker, true));
                    CollideBreakerBoxServerRpc((int)playerHeldBy.playerClientId, hitBreaker.NetworkObject, true);
                    playPoofParticle = true;
                }
                else if (stateIndex == 2 && RoundManager.Instance.powerOffPermanently)
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER interacting with BREAKER BOX {hitBreaker.name} #{hitBreaker.NetworkObjectId} to turn POWER ON");
                    StartCoroutine(CollideBreakerBoxLocal(hitBreaker, false));
                    CollideBreakerBoxServerRpc((int)playerHeldBy.playerClientId, hitBreaker.NetworkObject, false);
                }
                stopExtending = true;
            }
            if (!stopExtending && (startedExtendHolding || hitInfo.distance < 0.75f || (stateIndex == 0 && !playerHeldBy.thisController.isGrounded)) && (!hitInfo.collider.isTrigger || GetItemDropRegionElevator(collidedObject, hitInfo.distance)) && GetValidLayerToRetract(collidedObject, hitInfo.distance, hitDoor == null, hitVehicle == null, hitTree == null, hitBreaker == null, hitValve == null, hitGlove == null))
            {
                stopExtending = true;
                float rockJumpMultiplier = 1;
                if (stateIndex == 0)
                {
                    float angle = Vector3.Angle(playerHeldBy.playerEye.forward, Vector3.down);
                    Logger.LogDebug($"angle: {angle} | distance: {glove.transform.localPosition.z} | grounded: {playerHeldBy.thisController.isGrounded}");
                    rockJumpMultiplier = rockJumpDropOff.Evaluate(angle);
                    int multiplyNegative = !playerHeldBy.thisController.isGrounded ? -1 : 1;
                    playerHeldBy.ResetFallGravity();
                    if (angle < 20f)
                    {
                        playSyncClip = GetClipIndex(rockJumpClip);
                        insertedBattery.charge -= rockJumpBatteryDrain / itemProperties.batteryUsage;
                    }
                    else
                    {
                        playSyncClip = GetClipIndex(extendStopClip);
                    }
                    float addFallValue = Mathf.Clamp(rockJumpMultiplier - 10, 0f, 75f) * multiplyNegative;
                    playerHeldBy.fallValueUncapped += addFallValue * 0.08f;

                    if (collidedObject.layer == layerColliders)
                    {
                        BridgeTrigger hitWalkBridge = collidedObject.transform.parent?.parent?.parent?.GetChild(0).GetComponent<BridgeTrigger>();
                        BridgeTriggerType2 hitJumpBridge = collidedObject.transform.parent?.parent?.parent?.GetChild(0).GetComponent<BridgeTriggerType2>();
                        if (hitWalkBridge != null)
                        {
                            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT sending hit WALK BRIDGE with angle {angle} at position {hitWalkBridge.transform.position}");
                            playSyncClip = GetClipIndex(rockPunchClip);
                            CollideBridgeServerRpc(angle, hitWalkBridge.transform.position);
                            playPoofParticle = true;
                        }
                        else if (hitJumpBridge != null && GetAdamanceCollapsingColliders(collidedObject.name))
                        {
                            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT sending hit JUMP BRIDGE");
                            playSyncClip = GetClipIndex(rockPunchClip);
                            hitJumpBridge.AddToBridgeInstabilityServerRpc();
                            playPoofParticle = true;
                        }
                    }
                }
                else if (stateIndex == 1 && RoundManager.Instance.currentDungeonType == 4 && playerHeldBy.isInsideFactory && collidedObject.CompareTag("Rock"))
                {
                    GoldenPickaxeNode[] allGoldNodes = FindObjectsByType<GoldenPickaxeNode>(FindObjectsSortMode.None);
                    if (allGoldNodes.Length > 0)
                    {
                        float smallestDistance = 999f;
                        foreach (GoldenPickaxeNode node in allGoldNodes)
                        {
                            if (node.nodeExhausted)
                            {
                                continue;
                            }
                            float distance = Vector3.Distance(hitInfo.point, node.transform.position);
                            if (distance < smallestDistance)
                            {
                                smallestDistance = distance;
                            }
                        }
                        Logger.LogDebug(smallestDistance);
                        playSyncClip = GetPickaxeHotColdByDistance(smallestDistance);
                        syncClipAudibleNoise = false;
                    }
                }
                else
                {
                    playSyncClip = GetClipIndex(extendStopClip);
                    if (stateIndex == 1 && StartOfRound.Instance.currentLevelID == 3)
                    {
                        syncClipAudibleNoise = false;
                    }
                    else
                    {
                        syncClipAudibleNoise = true;
                    }
                }
                playerHeldBy.externalForceAutoFade += transform.up * -1f * rockJumpMultiplier * paperLaunchDropOff.Evaluate(playerHeldBy.carryWeight - itemProperties.weight);
            }
        }
        if (stopExtending)
        {
            Logger.LogDebug($"other object: {collidedObject.name} | layer: {collidedObject.layer} | distance: {hitInfo.distance}");
            StopExtendAndRetract(playSyncClip, syncClipAudibleNoise, playPoofParticle);
            RetractGloveServerRpc((int)playerHeldBy.playerClientId, playSyncClip, syncClipAudibleNoise, playPoofParticle, hitRPS);
        }
    }

    private void StopExtendAndRetract(int playSyncClip = -1, bool audibleNoise = false, bool playParticle = false, bool skipRetractWait = false)
    {
        if (extendCoroutine != null)
        {
            StopCoroutine(extendCoroutine);
            extendCoroutine = null;
        }
        if (playSyncClip != -1)
        {
            PlaySFXOneShot(gloveAudio, syncedClips[playSyncClip], true, audibleNoise);
        }
        if (playParticle)
        {
            Instantiate(AssetsCollection.poofParticle, glove.transform.position + glove.transform.forward * 0.15f, new Quaternion(0, 0, 0, 0)).GetComponent<ParticleSystem>().Play();
        }
        if (retractCoroutine == null)
        {
            Logger.LogDebug($"retracting {name} #{NetworkObjectId} on client");
            retractCoroutine = StartCoroutine(RetractGlove(skipRetractWait));
        }
        else if (skipRetractWait)
        {
            currentSpeed = -2f;
        }
    }

    private IEnumerator RetractGlove(bool skipWait = false)
    {
        extending = false;
        loopAudio.Stop();
        if (!skipWait)
        {
            yield return new WaitForSeconds(GetCurrentWait());
        }
        currentSpeed = GetCurrentSpeed(false, skipWait) * -1f;
        yield return new WaitForEndOfFrame();
        if (startedExtendHolding && holdingItem != null)
        {
            yield return new WaitForSeconds(paperExtendedWait);
            OwnerDiscardHoldingItem();
        }
        loopAudio.Stop();
        if (!isPocketed)
        {
            loopAudio.volume = 1.0f;
            loopAudio.clip = retractingLoopClip;
            loopAudio.Play();
        }
        while (glove.transform.localPosition.z > 0)
        {
            glove.transform.localPosition = new Vector3(0, 0, Mathf.Clamp(glove.transform.localPosition.z + currentSpeed * Time.deltaTime * 60, 0, maxExtendDistance));
            extendo.transform.localScale = new Vector3(Mathf.Max(glove.transform.localPosition.z / 7f, 0.01f), 1f, 1f);
            yield return null;
        }
        rpsCollider.enabled = false;
        loopAudio.Stop();
        PlaySFXOneShot(boxAudio, retractStopClip, true, true, 0.33f, 10f);
        glove.transform.localPosition = new Vector3(glove.transform.localPosition.x, glove.transform.localPosition.y, 0);
        ToggleShadowsMeshRenders(false);
        if (playerHeldBy != null)
        {
            playerHeldBy.disableLookInput = false;
        }
        if (holdingPlayer != null)
        {
            holdingPlayer.thisController.detectCollisions = true;
        }
        yield return new WaitForSeconds(0.1f);
        isBeingUsed = false;
        canExtend = true;
        retractCoroutine = null;
    }

    private bool PerformHitPlayerCheck(int hitPlayerID, PlayerControllerB hitPlayerObject)
    {
        bool hitPlayerCollider = hitPlayerID != (int)playerHeldBy.playerClientId && !hitPlayerIDs.Contains(hitPlayerID);
        if (hitPlayerCollider)
        {
            float distanceAboveHead = glove.transform.position.y - hitPlayerObject.playerEye.transform.position.y;
            Logger.LogDebug($"above head: {distanceAboveHead}");
            if (distanceAboveHead > 0.5f)
            {
                hitPlayerCollider = false;
            }
            else
            {
                hitPlayerIDs.Add(hitPlayerID);
            }
        }
        return hitPlayerCollider;
    }

    private void ToggleShadowsMeshRenders(bool shadowsOn)
    {
        ShadowCastingMode castShadows = shadowsOn ? ShadowCastingMode.On : ShadowCastingMode.Off;
        foreach (MeshRenderer renderer in toggleShadowsOf)
        {
            renderer.shadowCastingMode = castShadows;
        }
    }

    private float GetCurrentSpeed(bool forExtend, bool fromSkipWait = false)
    {
        if (fromSkipWait || !isHeld || isPocketed)
        {
            return 2f;
        }
        switch (stateIndex)
        {
            default:
                if (forExtend)
                {
                    return rockExtendSpeed;
                }
                else
                {
                    return rockRetractSpeed;
                }
            case 1:
                if (forExtend)
                {
                    float multiplier = holdingPlayer != null ? 5f : holdingItem != null ? Mathf.Clamp(1.5f - (holdingItem.itemProperties.weight - 1.25f), 0.5f, 2f) : 1f;
                    return paperExtendSpeed * multiplier;
                }
                else
                {
                    return paperRetractSpeedDefault * paperRetractDropOff.Evaluate(paperRetractSpeedEvaluation);
                }
            case 2:
                if (forExtend)
                {
                    return scissorsExtendSpeed;
                }
                else
                {
                    return scissorsRetractSpeed;
                }
        }
    }

    private float GetCurrentWait()
    {
        if (hitRPS == "Tie")
        {
            return rpsTieWait;
        }
        else if (hitRPS == "Lose")
        {
            return paperExtendedWait;
        }
        switch (stateIndex)
        {
            default:
                return rockExtendedWait;
            case 1:
                return paperExtendedWait;
            case 2:
                return scissorsExtendedWait;
        }
    }

    private int GetRockHitForce(bool damagePlayer)
    {
        Logger.LogDebug($"evaluating at: {glove.transform.localPosition.z}");
        if (damagePlayer)
        {
            return (int)rockDamagePlayers.Evaluate(glove.transform.localPosition.z);
        }
        else
        {
            return (int)rockDamageEnemies.Evaluate(glove.transform.localPosition.z);
        }
    }

    private float GetKnockbackForce()
    {
        float force;
        switch (stateIndex)
        {
            default:
                force = rockKnockbackForce;
                break;
            case 1:
                force = paperKnockbackForce;
                break;
            case 2:
                force = scissorsKnockbackForce;
                break;
        }
        return force * -1;
    }

    private void GetLayers()
    {
        layerDefault = LayerMask.NameToLayer("Default");
        layerRoom = LayerMask.NameToLayer("Room");
        layerColliders = LayerMask.NameToLayer("Colliders");
        layerEnemies = LayerMask.NameToLayer("Enemies");
        layerInteractableObject = LayerMask.NameToLayer("InteractableObject");
        layerTerrain = LayerMask.NameToLayer("Terrain");
        layerRailing = LayerMask.NameToLayer("Railing");

        gloveMask = 1084754249;
        gloveMask |= 1 << layerInteractableObject;
        gloveMask |= 1 << layerInteractableObject;
        gloveMask |= 1 << layerTerrain;
        gloveMask |= 1 << layerRailing;
        gloveMask |= 1 << LayerMask.NameToLayer("Vehicle");
    }

    private bool GetValidLayerToRetract(GameObject collidedObject, float distance, bool hitNoDoor, bool hitNoVehicle, bool hitNoTree, bool hitNoBreaker, bool hitNoValve, bool hitNoGlove)
    {
        int collidedLayer = collidedObject.layer;
        return collidedLayer == layerRoom || collidedLayer == layerDefault || collidedLayer == layerColliders && hitNoGlove || (collidedLayer == layerEnemies && distance < 1.75f && collidedObject.GetComponent<EnemyAICollisionDetect>() == null) || (collidedLayer == layerInteractableObject && hitNoDoor && hitNoVehicle && hitNoBreaker && hitNoValve && (collidedObject.GetComponentInParent<DepositItemsDesk>() == null || distance < 0.5f)) || (collidedLayer == layerTerrain && hitNoTree) || (collidedLayer == layerRailing && collidedObject.GetComponentInParent<MineshaftElevatorController>() != null);
    }

    private bool GetItemDropRegionElevator(GameObject collidedObject, float distance)
    {
        if (collidedObject.layer != layerDefault || RoundManager.Instance.currentDungeonType != 4)
        {
            return false;
        }
        MineshaftElevatorController hitElevator = collidedObject.GetComponentInParent<MineshaftElevatorController>();
        if (hitElevator == null)
        {
            return false;
        }
        float minDistance = hitElevator.elevatorDoorOpen ? (startedExtendHolding ? 0.5f : 0f) : 2f;
        return distance < minDistance;
    }

    private bool GetAdamanceCollapsingColliders(string colliderName)
    {
        if (StartOfRound.Instance == null || StartOfRound.Instance.currentLevel == null || StartOfRound.Instance.currentLevel.name != "AdamanceLevel")
        {
            return true;
        }
        switch (colliderName)
        {
            case "Cube (7)":
                return true;
            case "Cube (11)":
                return true;
            case "Cube (12)":
                return true;
            case "Cube (9)":
                return true;
            case "Walkway2":
                return true;
            case "Walkway2 (1)":
                return true;
            case "Walkway2 (2)":
                return true;
            case "Walkway2 (3)":
                return true;
            default:
                return false;
        }
    }

    private int GetPickaxeHotColdByDistance(float smallestDistance)
    {
        if (smallestDistance < 1f)
        {
            return 8;
        }
        else if (smallestDistance < 2f)
        {
            return 7;
        }
        else if (smallestDistance < 3f)
        {
            return 6;
        }
        else if (smallestDistance < 5f)
        {
            return 5;
        }
        else if (smallestDistance < 7.5f)
        {
            return 4;
        }
        else if (smallestDistance < 10f)
        {
            return 3;
        }
        else if (smallestDistance < 15f)
        {
            return 2;
        }
        else if (smallestDistance < 30f)
        {
            return 1;
        }
        return 0;
    }

    private string GetRPSWithOtherGlove(int otherGlove)
    {
        int myGlove = stateIndex;
        if (myGlove == otherGlove)
        {
            return "Tie";
        }
        int thisGloveToWin = myGlove - 1;
        if (thisGloveToWin < 0)
        {
            thisGloveToWin = 2;
        }
        return thisGloveToWin == otherGlove ? "Win" : "Lose";
    }

    private int GetSourceIndex(AudioSource source)
    {
        if (source == boxAudio)
        {
            return 1;
        }
        if (source == loopAudio)
        {
            return 2;
        }
        return 0;
    }

    private int GetClipIndex(AudioClip clip)
    {
        for (int i = 0; i < syncedClips.Count; i++)
        {
            AudioClip syncedClip = syncedClips[i];
            if (syncedClip == clip)
            {
                return i;
            }
        }
        return 0;
    }

    private void GrabHoldingItem(int stolenFromPlayer = -1)
    {
        if (holdingItem == null)
        {
            Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] called GrabHoldingItem() with NULL item!!!");
            return;
        }
        holdingItem.parentObject = grabPositionItems;
        holdingItem.grabbable = false;
        holdingItem.grabbableToEnemies = false;
        holdingItem.isHeld = true;
        holdingItem.isPocketed = false;
        holdingItem.hasHitGround = false;
        holdingItem.heldByPlayerOnServer = true;
        holdingItem.transform.SetParent(grabPositionItems);
        holdingItem.transform.localPosition = Vector3.zero;
        holdingItem.playerHeldBy = playerHeldBy;
        holdingItem.GrabItem();
        holdingItem.EquipItem();
        holdingItem.EnablePhysics(false);
        SpecialHoldingItemFunctionality(true);
        if (playerHeldBy != null)
        {
            playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight + (holdingItem.itemProperties.weight - 1), 1f, 10f);
            playerHeldBy.SetItemInElevator(playerHeldBy.isInHangarShipRoom, playerHeldBy.isInElevator, holdingItem);
            holdingItem.isInFactory = playerHeldBy.isInsideFactory;
            playerHeldBy.equippedUsableItemQE = true;
            SetToolTips("Put down : [Q]");
        }
        PlaySFXOneShot(gloveAudio, paperGrabClip, true, false, 0.25f);
        if (holdingItem.itemProperties != null && holdingItem.itemProperties.grabSFX != null)
        {
            PlaySFXOneShot(gloveAudio, holdingItem.itemProperties.grabSFX);
        }
        gloveMeshFilter.mesh = paperHoldingMesh;
        GoldenGloveScript stolenGlove = holdingItem.GetComponent<GoldenGloveScript>();
        if (stolenGlove != null && stolenFromPlayer != -1 && stolenGlove.holdingItem != null)
        {
            stolenGlove.PaperCollideItemLocal((int)playerHeldBy.playerClientId, stolenGlove.holdingItem, paperRetractSpeedEvaluation, stolenFromPlayer);
        }
    }

    private void OwnerDiscardHoldingItem()
    {
        if (!IsOwner)
        {
            return;
        }
        if (holdingItem == null)
        {
            Logger.LogError($"{name} #{NetworkObjectId} OWNER called DiscardHoldingItem() with NULL item!!!");
            return;
        }
        holdingItem.SyncBatteryServerRpc((int)(holdingItem.insertedBattery.charge * 100f));
        Logger.LogDebug("I");
        if (glove.transform.localPosition.z < 2)
        {
            holdingItem.transform.position = collisionCheck.position;
        }
        Logger.LogDebug("II");
        Vector3 airPos = holdingItem.transform.position + Vector3.up * 0.75f;
        Vector3 floorPos;
        bool onShip = false;
        NetworkObject pRegionObject = holdingItem.GetPhysicsRegionOfDroppedObject(null, out floorPos);
        Logger.LogDebug("III");
        if (pRegionObject == null)
        {
            Logger.LogDebug("IV-1");
            floorPos = holdingItem.GetItemFloorPosition(airPos);
            if (StartOfRound.Instance.shipBounds.bounds.Contains(floorPos))
            {
                onShip = true;
                holdingItem.transform.SetParent(StartOfRound.Instance.elevatorTransform, true);
            }
            else
            {
                holdingItem.transform.SetParent(StartOfRound.Instance.propsContainer, true);
            }
            Logger.LogDebug("V-1");
        }
        else
        {
            Logger.LogDebug("IV-2");
            PlayerPhysicsRegion pRegionTransform = pRegionObject.GetComponentInChildren<PlayerPhysicsRegion>();
            if (pRegionTransform != null && pRegionTransform.physicsTransform != null)
            {
                holdingItem.transform.SetParent(pRegionTransform.physicsTransform, true);
            }
            else
            {
                holdingItem.transform.SetParent(pRegionObject.transform, true);
            }
            Logger.LogDebug("IV-2");
        }
        Logger.LogDebug("VI");
        if (pRegionObject == null)
        {
            Logger.LogDebug("VII-1");
            holdingItem.startFallingPosition = holdingItem.transform.parent.InverseTransformPoint(airPos);
            holdingItem.targetFloorPosition = holdingItem.transform.parent.InverseTransformPoint(floorPos);
            SetHoldingItemInElevator();
            bool parentedToCompanyDesk = OwnerDropItemOnCompanyDesk();
            SyncTargetFloorServerRpc((int)holdingItem.OwnerClientId, holdingItem.startFallingPosition, holdingItem.targetFloorPosition, onShip, parentedToCompanyDesk);
            Logger.LogDebug("VIII-1");
        }
        else
        {
            Logger.LogDebug("VII-2");
            holdingItem.startFallingPosition = holdingItem.transform.parent.InverseTransformPoint(airPos);
            holdingItem.targetFloorPosition = floorPos;
            SetHoldingItemInElevator();
            SyncPhysicsRegionServerRpc((int)holdingItem.OwnerClientId, pRegionObject, holdingItem.startFallingPosition, floorPos);
            Logger.LogDebug("VIII-2");
        }
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] LOCALLY discarded item to parent {holdingItem.transform.parent?.name} with physics OBJECT valid {pRegionObject != null}");
        GlobalDiscardHoldingItem();
    }

    private void ClientDiscardHoldingItem(Vector3 startPos, Vector3 targetPos)
    {
        if (holdingItem == null)
        {
            Logger.LogError($"{name} #{NetworkObjectId} CLIENT called DiscardHoldingItem() with NULL item!!!");
            return;
        }
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] discarding {holdingItem.name} #{holdingItem.NetworkObjectId} on CLIENT");
        holdingItem.startFallingPosition = startPos;
        holdingItem.targetFloorPosition = targetPos;
        GlobalDiscardHoldingItem();
    }

    private void GlobalDiscardHoldingItem()
    {
        holdingItem.parentObject = null;
        holdingItem.grabbable = true;
        holdingItem.grabbableToEnemies = true;
        holdingItem.isHeld = false;
        holdingItem.isPocketed = false;
        holdingItem.heldByPlayerOnServer = false;
        holdingItem.hasHitGround = false;
        holdingItem.reachedFloorTarget = false;
        holdingItem.fallTime = 0;
        holdingItem.floorYRot = -1;
        SpecialHoldingItemFunctionality(false);
        holdingItem.EnablePhysics(true);
        holdingItem.EnableItemMeshes(true);
        holdingItem.DiscardItem();
        SetHoldingItemInElevator();
        holdingItem.transform.localScale = holdingItem.originalScale;
        holdingItem.isInFactory = isInFactory;
        holdingItem.playerHeldBy = null;
        gloveMeshFilter.mesh = paperMesh;
        paperRetractSpeedEvaluation = 0f;
        currentSpeed = GetCurrentSpeed(false) * -1;
        if (playerHeldBy != null)
        {
            playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (holdingItem.itemProperties.weight - 1), 1f, 10f);
            playerHeldBy.equippedUsableItemQE = true;
            SetToolTips();
        }
        holdingItem = null;
    }

    private void SetHoldingItemInElevator()
    {
        bool willDropInShip = StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(holdingItem.transform.parent.TransformPoint(holdingItem.targetFloorPosition));
        bool parentIsElevator = holdingItem.transform.parent == StartOfRound.Instance.elevatorTransform;
        if (playerHeldBy != null)
        {
            playerHeldBy.SetItemInElevator(willDropInShip, parentIsElevator, holdingItem);
        }
        else
        {
            holdingItem.isInElevator = parentIsElevator;
            holdingItem.isInShipRoom = willDropInShip;
            if (!holdingItem.scrapPersistedThroughRounds)
            {
                if (holdingItem.isInShipRoom)
                {
                    RoundManager.Instance.scrapCollectedInLevel += holdingItem.scrapValue;
                    RoundManager.Instance.CollectNewScrapForThisRound(holdingItem);
                    holdingItem.OnBroughtToShip();
                }
                else
                {
                    if (holdingItem.scrapPersistedThroughRounds)
                    {
                        return;
                    }

                    RoundManager.Instance.scrapCollectedInLevel -= holdingItem.scrapValue;
                }

                HUDManager.Instance.SetQuota(RoundManager.Instance.scrapCollectedInLevel);
            }
            if (holdingItem.isInShipRoom)
            {
                StartOfRound.Instance.currentShipItemCount++;
            }
            else
            {
                StartOfRound.Instance.currentShipItemCount--;
            }
        }
    }

    private bool OwnerDropItemOnCompanyDesk()
    {
        bool parentedToCompanyDesk = false;
        if (StartOfRound.Instance.currentLevelID == 3)
        {
            DepositItemsDesk companyDesk = FindObjectOfType<DepositItemsDesk>();
            if (companyDesk != null && companyDesk.triggerCollider.bounds.Contains(holdingItem.transform.parent.TransformPoint(holdingItem.targetFloorPosition)))
            {
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] OWNER dropping item onto COMPANY DESK");
                parentedToCompanyDesk = true;
                companyDesk.AddObjectToDeskServerRpc(holdingItem.NetworkObject);
                holdingItem.startFallingPosition = companyDesk.deskObjectsContainer.transform.InverseTransformPoint(holdingItem.transform.parent.TransformPoint(holdingItem.startFallingPosition));
                holdingItem.targetFloorPosition = companyDesk.deskObjectsContainer.transform.InverseTransformPoint(holdingItem.transform.parent.TransformPoint(holdingItem.targetFloorPosition));
                holdingItem.transform.SetParent(companyDesk.deskObjectsContainer.transform, true);
                holdingItem.OnPlaceObject();
            }
        }
        return parentedToCompanyDesk;
    }

    private void SpecialHoldingItemFunctionality(bool onGrab)
    {
        if (onGrab)
        {
            if (holdingItem.name == "SoccerBall(Clone)")
            {
                holdingItem.transform.GetChild(0).GetComponent<Collider>().enabled = false;
                return;
            }
            FlashlightItem flashlight = holdingItem.GetComponent<FlashlightItem>();
            if (flashlight != null && flashlight.isBeingUsed)
            {
                flashlight.playerHeldBy.helmetLight.enabled = false;
                flashlight.usingPlayerHelmetLight = false;
                flashlight.playerHeldBy.ChangeHelmetLight(flashlight.flashlightTypeID, false);
                flashlight.flashlightBulb.enabled = true;
                flashlight.flashlightBulbGlow.enabled = true;
                return;
            }
        }
        else
        {
            if (holdingItem.name == "SoccerBall(Clone)")
            {
                holdingItem.transform.GetChild(0).GetComponent<Collider>().enabled = true;
                return;
            }
        }
    }

    private void GrabHoldingPlayer()
    {
        if (holdingPlayer == null)
        {
            Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] called GrabHoldingPlayer() with NULL player!!!");
            return;
        }
        if (holdingPlayer == GameNetworkManager.Instance.localPlayerController)
        {
            holdingLocalPlayer = true;
            holdingPlayer.CancelSpecialTriggerAnimations();
            HUDManager.Instance.ClearControlTips();
            string[] struggleLine = { "Struggle : [Mouse]" };
            HUDManager.Instance.ChangeControlTipMultiple(struggleLine);
        }
        holdingPlayer.enteringSpecialAnimation = true;
        holdingPlayer.isSprinting = false;
        holdingPlayer.isMovementHindered = 0;
        holdingPlayer.hinderedMultiplier = 1f;
        holdingPlayer.sourcesCausingSinking = 0;
        holdingPlayer.sinkingValue = 0f;
        holdingPlayer.isSinking = false;
        holdingPlayer.statusEffectAudio.Stop();
        holdingPlayer.externalForces = Vector3.zero;
        holdingPlayer.externalForceAutoFade = Vector3.zero;
        holdingPlayer.teleportedLastFrame = false;
        holdingPlayer.isCrouching = false;
        holdingPlayer.ResetFallGravity();
        holdingPlayer.playerBodyAnimator.SetBool("crouching", false);
        holdingPlayer.playerBodyAnimator.SetBool("Walking", false);
        holdingPlayer.playerBodyAnimator.SetBool("Sprinting", false);
        holdingPlayer.playerBodyAnimator.SetBool("Sideways", false);
        holdingPlayer.playerBodyAnimator.SetBool("hinderedMovement", false);
        holdingPlayer.playerBodyAnimator.SetBool("Jumping", false);
        holdingPlayer.playerBodyAnimator.SetBool("FallNoJump", false);
        holdingPlayer.playerBodyAnimator.SetBool("Limp", false);
        if (holdingPlayer.inAnimationWithEnemy != null)
        {
            PlaySFXOneShot(gloveAudio, paperSaveClip, true, false, 1f, 15f, 1f, 0.5f);
            RadMechAI torchedByMech = holdingPlayer.inAnimationWithEnemy.GetComponent<RadMechAI>();
            if (torchedByMech != null)
            {
                torchedByMech.CancelTorchPlayerAnimation();
            }
            else
            {
                holdingPlayer.inAnimationWithEnemy.CancelSpecialAnimationWithPlayer();
            }
            holdingPlayer.thisController.detectCollisions = false;
        }
        else
        {
            PlaySFXOneShot(gloveAudio, paperGrabClip, true, true, 0.25f, 5f);
        }
        if (playerHeldBy != null)
        {
            playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight + (holdingPlayer.carryWeight - 0.7f), 1f, 10f);
            playerHeldBy.twoHanded = true;
            if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.holdingTwoHandedItem.enabled = true;
            }
            SetToolTips("Gently put down : [Q]");
        }
        holdingPlayer.inSpecialInteractAnimation = true;
        heldPlayerStruggles = 0;
        timeLastStruggle = Time.realtimeSinceStartup;
        timeLastSlide = Time.realtimeSinceStartup;
        gloveMeshFilter.mesh = paperHoldingMesh;
    }

    private PlayerControllerB DiscardHoldingPlayer(bool discardFromLocation = true, Vector3 discardFrom = default)
    {
        if (holdingPlayer == null)
        {
            Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] called DiscardHoldingPlayer() with NULL player!!!");
            return null;
        }
        Logger.LogDebug($"discarding {holdingPlayer.playerUsername} from {name} #{NetworkObjectId}");
        holdingPlayer.inSpecialInteractAnimation = holdingPlayer.inAnimationWithEnemy != null;
        holdingPlayer.externalForces = Vector3.zero;
        holdingPlayer.externalForceAutoFade = Vector3.zero;
        holdingPlayer.ResetFallGravity();
        if (discardFromLocation)
        {
            if (discardFrom == default)
            {
                discardFrom = grabPositionItems.transform.position;
            }
            holdingPlayer.transform.position = discardFrom;
        }
        if (playerHeldBy != null)
        {
            playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (holdingPlayer.carryWeight - 0.7f), 1f, 10f);
            playerHeldBy.twoHanded = false;
            if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.holdingTwoHandedItem.enabled = false;
            }
            SetToolTips();
        }
        if (holdingLocalPlayer)
        {
            HUDManager.Instance.ClearControlTips();
        }
        holdingLocalPlayer = false;
        gloveMeshFilter.mesh = paperMesh;
        loopAudio.Stop();
        heldPlayerStruggles = 0;
        timeLastStruggle = Time.realtimeSinceStartup;
        timeLastSlide = Time.realtimeSinceStartup;
        PlayerControllerB heldPlayer = holdingPlayer;
        holdingPlayer = null;
        return heldPlayer;
    }

    private IEnumerator ReactivateHazardOnDelay(Landmine hitMine = null, Turret hitTurret = null, SpikeRoofTrap hitSpikes = null)
    {
        yield return new WaitForEndOfFrame();
        if (hitMine != null)
        {
            hitMine.ToggleMine(false);
            yield return new WaitForSeconds(scissorsStunHazards);
            hitMine.ToggleMine(true);
        }
        else if (hitTurret != null)
        {
            hitTurret.ToggleTurretEnabled(false);
            yield return new WaitForSeconds(scissorsStunHazards);
            hitTurret.ToggleTurretEnabled(true);
        }
        else if (hitSpikes != null)
        {
            hitSpikes.ToggleSpikesEnabled(false);
            yield return new WaitForSeconds(scissorsStunHazards);
            hitSpikes.ToggleSpikesEnabled(true);
        }
    }

    private void SwitchToMesh()
    {
        Mesh setMeshTo;
        switch (stateIndex)
        {
            default:
                setMeshTo = rockMesh;
                break;
            case 1:
                setMeshTo = paperMesh;
                break;
            case 2:
                setMeshTo = scissorsMesh;
                break;
        }
        gloveMeshFilter.mesh = setMeshTo;
        Logger.LogDebug($"switched to Mesh {gloveMeshFilter.mesh.name} with index {stateIndex}");
        canExtend = true;
        isBeingUsed = false;
    }

    private void PlaySFXOneShot(AudioSource soundSource, AudioClip clipToPlay, bool transmitOverWalkie = true, bool playAudibleNoise = false, float noiseLoudness = 0.5f, float noiseRange = 15f, float sourcePitch = 1f, float sourceVolume = 1f)
    {
        if (isPocketed)
        {
            return;
        }
        soundSource.pitch = Mathf.Clamp(sourcePitch, -3f, 3f);
        soundSource.volume = Mathf.Clamp(sourceVolume, 0f, 1f);
        soundSource.PlayOneShot(clipToPlay);
        if (transmitOverWalkie)
        {
            WalkieTalkie.TransmitOneShotAudio(soundSource, clipToPlay, noiseLoudness);
        }
        if (playAudibleNoise && IsServer)
        {
            RoundManager.Instance.PlayAudibleNoise(soundSource.transform.position, noiseRange, noiseLoudness);
        }
    }

    private void SetToolTips(string secondTip = "")
    {
        if (playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.ClearControlTips();
            string lineTwo = itemProperties.toolTips[1];
            if (secondTip != "")
            {
                lineTwo = secondTip;
            }
            string[] newLines = { itemProperties.toolTips[0], lineTwo };
            HUDManager.Instance.ChangeControlTipMultiple(newLines, playerHeldBy != null && isHeld && !isPocketed, itemProperties);
        }
    }

    private void SetUpSyncedClips()
    {
        syncedClips = new List<AudioClip>();
        syncedClips.AddRange(GoldenPickaxeScript.hotColdClips);
        syncedClips.Add(extendStartClip);
        syncedClips.Add(extendingLoopClip);
        syncedClips.Add(extendStopClip);
        syncedClips.Add(retractingLoopClip);
        syncedClips.Add(retractStopClip);
        syncedClips.Add(switchClip);
        syncedClips.Add(struggleClip);
        syncedClips.Add(slideLoopClip);
        syncedClips.Add(rockPunchClip);
        syncedClips.Add(rockJumpClip);
        syncedClips.Add(paperGrabClip);
        syncedClips.Add(paperSaveClip);
        syncedClips.Add(scissorsStunClip);
    }

    public void RebalanceTool()
    {
        if (!itemProperties.isConductiveMetal)
        {
            //CUSTOM PARAMETERS (the ones you don't have to worry about too much)
            maxExtendDistance = 13f;
            rpsTieWait = 3f;

            rockExtendSpeed = 1.25f;
            rockRetractSpeed = 0.25f;
            rockExtendedWait = 1f;
            rockKnockbackForce = 7f;
            rockJumpBatteryDrain = 3f;

            paperExtendSpeed = 0.125f;
            paperRetractSpeedDefault = 1f;
            paperExtendedWait = 0.1f;
            paperKnockbackForce = 1f;
            paperLaunchForce = 120f;

            scissorsExtendSpeed = 0.5f;
            scissorsRetractSpeed = 0.05f;
            scissorsExtendedWait = 0.1f;
            scissorsKnockbackForce = 3f;
            scissorsStunEnemies = 5f;
            scissorsStunPlayers = 1f;
            scissorsStunHazards = 3.33f;

            Logger.LogDebug($"{name} #{NetworkObjectId} using custom parameters!");
        }
        /*
        else
        {
            //DEFAULT PARAMETERS (should match Unity editor values!)
            maxExtendDistance = 15f;
            rpsTieWait = 1.5f;

            rockExtendSpeed = 1.1f;
            rockRetractSpeed = 0.3f;
            rockExtendedWait = 0.8f;
            rockKnockbackForce = 4f;
            rockJumpBatteryDrain = 2f;

            paperExtendSpeed = 0.17f;
            paperRetractSpeedDefault = 0.7f;
            paperExtendedWait = 0.02f;
            paperKnockbackForce = 0f;
            paperLaunchForce = 100f;

            scissorsExtendSpeed = 0.6f;
            scissorsRetractSpeed = 0.08f;
            scissorsExtendedWait = 0.33f;
            scissorsKnockbackForce = 2f;
            scissorsStunEnemies = 2.75f;
            scissorsStunPlayers = 0.9f;
            scissorsStunHazards = 2.5f;
        }
        */
    }



    //General syncing
    [ServerRpc(RequireOwnership = false)]
    private void ExtendGloveServerRpc(int playerID)
    {
        ExtendGloveClientRpc(playerID);
    }

    [ClientRpc]
    private void ExtendGloveClientRpc(int playerID)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            StartExtendGlove();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RetractGloveServerRpc(int playerID, int playSyncClip = -1, bool syncClipAudibleNoise = true, bool playPoofParticle = false, string rpsResult = null)
    {
        RetractGloveClientRpc(playerID, playSyncClip, syncClipAudibleNoise, playPoofParticle, rpsResult);
    }

    [ClientRpc]
    private void RetractGloveClientRpc(int playerID, int playSyncClip = -1, bool syncClipAudibleNoise = true, bool playPoofParticle = false, string rpsResult = null)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            if (rpsResult != null)
            {
                hitRPS = rpsResult;
            }
            StopExtendAndRetract(playSyncClip, syncClipAudibleNoise, playPoofParticle);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaySyncedClipServerRpc(int playerID, int sourceID, int clipID, bool walkie = true, bool audible = false, float loudness = 0.5f, float range = 15f, float pitch = 1f, float volume = 1f)
    {
        PlaySyncedClipClientRpc(playerID, sourceID, clipID, walkie, audible, loudness, range, pitch);
    }

    [ClientRpc]
    private void PlaySyncedClipClientRpc(int playerID, int sourceID, int clipID, bool walkie = true, bool audible = false, float loudness = 0.5f, float range = 15f, float pitch = 1f, float volume = 1f)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            AudioSource sourceToPlay = gloveAudio;
            switch (sourceID)
            {
                case 1:
                    sourceToPlay = boxAudio;
                    break;
                case 2:
                    sourceToPlay = loopAudio;
                    break;
            }
            PlaySFXOneShot(sourceToPlay, syncedClips[clipID], walkie, audible, loudness, range, pitch, volume);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollideDoorServerRpc(int playerID, NetworkObjectReference doorNOR, bool setOpenTo, bool lockDoor)
    {
        CollideDoorClientRpc(playerID, doorNOR, setOpenTo, lockDoor);
    }

    [ClientRpc]
    private void CollideDoorClientRpc(int playerID, NetworkObjectReference doorNOR, bool setOpenTo, bool lockDoor)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            if (doorNOR.TryGet(out var doorNO))
            {
                DoorLock door = doorNO.GetComponentInChildren<DoorLock>();
                if (door == null)
                {
                    Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get door SCRIPT!!!");
                    return;
                }
                if (lockDoor)
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] client receiving LOCK {lockDoor} on DOOR {door.name} #{door.NetworkObjectId}");
                    door.LockDoor(5f);
                    if (door.doorLockSFX != null)
                    {
                        PlaySFXOneShot(door.doorLockSFX, door.unlockSFX);
                    }
                }
                else
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] client receiving INTERACT {setOpenTo} on DOOR {door.name} #{door.NetworkObjectId}");
                    door.SetDoorAsOpen(setOpenTo);
                }
            }
            else
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get door NETWORK OBJECT!!!");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncTargetFloorServerRpc(int playerActualID,Vector3 startPos, Vector3 targetPos, bool elevatorTransform, bool parentedToDesk)
    {
        SyncTargetFloorClientRpc(playerActualID, startPos, targetPos, elevatorTransform, parentedToDesk);
    }

    [ClientRpc]
    private void SyncTargetFloorClientRpc(int playerActualID, Vector3 startPos, Vector3 targetPos, bool elevatorTransform, bool parentedToDesk)
    {
        if (playerActualID != (int)GameNetworkManager.Instance.localPlayerController.actualClientId)
        {
            if (holdingItem == null)
            {
                Logger.LogError($"{name} #{NetworkObjectId} CLIENT called DiscardHoldingItem() with NULl item!!!");
                return;
            }
            if (parentedToDesk)
            {
                DepositItemsDesk companyDesk = FindObjectOfType<DepositItemsDesk>();
                if (companyDesk != null)
                {
                    Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT dropping item onto COMPANY DESK");
                    holdingItem.transform.SetParent(companyDesk.deskObjectsContainer.transform, true);
                    holdingItem.OnPlaceObject();
                }
                else
                {
                    Logger.LogWarning($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT failed to find COMPANY DESK, dropping to propsContainer!");
                    holdingItem.startFallingPosition = StartOfRound.Instance.propsContainer.InverseTransformPoint(holdingItem.transform.position + Vector3.up * 0.75f);
                    holdingItem.targetFloorPosition = StartOfRound.Instance.propsContainer.InverseTransformPoint(holdingItem.GetItemFloorPosition());
                    holdingItem.transform.SetParent(StartOfRound.Instance.propsContainer, true);
                }
            }
            else if (elevatorTransform)
            {
                holdingItem.transform.SetParent(StartOfRound.Instance.elevatorTransform, true);
            }
            else
            {
                holdingItem.transform.SetParent(StartOfRound.Instance.propsContainer, true);
            }
            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] RECEIVED discarded item to parent {holdingItem.transform.parent?.name}");
            ClientDiscardHoldingItem(startPos, targetPos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncPhysicsRegionServerRpc(int playerActualID, NetworkObjectReference parentNOR, Vector3 startPos, Vector3 targetPos)
    {
        SyncPhysicsRegionClientRpc(playerActualID, parentNOR, startPos, targetPos);
    }

    [ClientRpc]
    private void SyncPhysicsRegionClientRpc(int playerActualID, NetworkObjectReference parentNOR, Vector3 startPos, Vector3 targetPos)
    {
        if (playerActualID != (int)GameNetworkManager.Instance.localPlayerController.actualClientId)
        {
            if (holdingItem == null)
            {
                Logger.LogError($"{name} #{NetworkObjectId} CLIENT called DiscardHoldingItem() with NULL item!!!");
                return;
            }
            if (parentNOR.TryGet(out var parentNO))
            {
                PlayerPhysicsRegion pRegionTransform = parentNO.GetComponentInChildren<PlayerPhysicsRegion>();
                if (pRegionTransform != null && pRegionTransform.physicsTransform != null)
                {
                    holdingItem.transform.SetParent(pRegionTransform.physicsTransform, true);
                }
                else
                {
                    holdingItem.transform.SetParent(parentNO.transform, true);
                }
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] RECEIVED discarded item to parent {holdingItem.transform.parent?.name} with physics TRANSFORM valid {pRegionTransform != null && pRegionTransform.physicsTransform != null}");
                ClientDiscardHoldingItem(startPos, targetPos);
            }
            else
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get NETWORK Object of physics region!!!");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncDiscardHoldingPlayerServerRpc(int localPlayerID)
    {
        SyncDiscardHoldingPlayerClientRpc(localPlayerID);
    }

    [ClientRpc]
    private void SyncDiscardHoldingPlayerClientRpc(int localPlayerID)
    {
        if (localPlayerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            Logger.LogDebug($"{name} #{NetworkObjectId} client RECEIVING custom call to discard holdingPlayer!");
            DiscardHoldingPlayer();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncStateServerRpc(int ownerID, int ownerState)
    {
        SyncStateClientRpc(ownerID, ownerState);
    }

    [ClientRpc]
    private void SyncStateClientRpc(int ownerID, int ownerState)
    {
        if (ownerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            stateIndex = ownerState;
            Logger.LogDebug($"{name} #{NetworkObjectId} CLIENT received new state [{stateIndex}]");
            SwitchStateLocal(stateIndex);
        }
    }

    private void SwitchStateLocal(int newState)
    {
        canExtend = false;
        isBeingUsed = true;
        gloveAnimator.SetTrigger("SwitchState");
        PlaySFXOneShot(gloveAudio, switchClip, true, true, 0.3f, 2f, 0.95f + stateIndex * 0.05f);
    }



    //Collision with map props
    [ServerRpc(RequireOwnership = false)]
    private void CollideTreeServerRpc(int playerID, Vector3 newHitTree, bool breakTree)
    {
        CollideTreeClientRpc(playerID, newHitTree, breakTree);
    }

    [ClientRpc]
    private void CollideTreeClientRpc(int playerID, Vector3 newHitTree, bool breakTree)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT received hit TREE at {newHitTree} with breakTree {breakTree}");
            PlaySFXOneShot(gloveAudio, rockPunchClip, true, true, 1f, 5f);
            if (breakTree)
            {
                RoundManager.Instance.DestroyTreeAtPosition(newHitTree, 1f);
            }
            hitTreeAt = newHitTree;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollideBreakerBoxServerRpc(int playerID, NetworkObjectReference breakerNOR, bool breakBreaker)
    {
        CollideBreakerBoxClientRpc(playerID, breakerNOR, breakBreaker);
    }

    [ClientRpc]
    private void CollideBreakerBoxClientRpc(int playerID, NetworkObjectReference breakerNOR, bool breakBreaker)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            if (breakerNOR.TryGet(out var breakerNO))
            {
                BreakerBox hitBreaker = breakerNO.GetComponent<BreakerBox>();
                if (hitBreaker == null)
                {
                    Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get SCRIPT on breaker box!!!");
                    return;
                }
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT interacting with BREAKER BOX {hitBreaker.name} #{hitBreaker.NetworkObjectId} to turn POWER {!breakBreaker}");
                StartCoroutine(CollideBreakerBoxLocal(hitBreaker, breakBreaker));
            }
            else
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get NETWORK Object on breaker box!!!");
            }
        }
    }

    private IEnumerator CollideBreakerBoxLocal(BreakerBox boxScript, bool breakBreaker)
    {
        RoundManager.Instance.FlickerLights(breakBreaker, breakBreaker);
        PlaySFXOneShot(gloveAudio, breakBreaker ? rockPunchClip : scissorsStunClip, true, breakBreaker);
        if (breakBreaker)
        {
            boxScript.breakerBoxHum.Stop();
        }
        else
        {
            boxScript.breakerBoxHum.Play();
        }
        yield return new WaitForSeconds(1.25f);
        RoundManager.Instance.powerOffPermanently = breakBreaker;
        RoundManager.Instance.SwitchPower(!breakBreaker);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollideValveServerRpc(int playerID, NetworkObjectReference valveNOR, float fogMultiplier)
    {
        CollideValveClientRpc(playerID, valveNOR, fogMultiplier);
    }

    [ClientRpc]
    private void CollideValveClientRpc(int playerID, NetworkObjectReference valveNOR, float fogMultiplier)
    {
        if (playerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            if (valveNOR.TryGet(out var valveNO))
            {
                SteamValveHazard hitValve = valveNO.GetComponent<SteamValveHazard>();
                if (hitValve == null)
                {
                    Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get SCRIPT on steam valve hazard!!!");
                    return;
                }
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] CLIENT interacting with VALVE {hitValve.name} #{hitValve.NetworkObjectId}");
                CollideValveLocal(hitValve, fogMultiplier);
            }
            else
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get NETWORK Object on steam valve hazard!!!");
            }
        }
    }

    private void CollideValveLocal(SteamValveHazard valveScript, float fogMultiplier)
    {
        if (valveScript.valveBurstTime != 0 && TimeOfDay.Instance.normalizedTimeOfDay > valveScript.valveBurstTime)
        {
            Logger.LogDebug($"{valveScript.name} #{valveScript.NetworkObjectId}: 0) valve has already burst, returning");
            return;
        }
        if (valveScript.valveCrackTime > TimeOfDay.Instance.normalizedTimeOfDay || valveScript.valveCrackTime == 0)
        {
            Logger.LogDebug($"{valveScript.name} #{valveScript.NetworkObjectId}: 1) valve has not yet cracked or will not crack");
            valveScript.valveCrackTime = TimeOfDay.Instance.normalizedTimeOfDay + 0.0001f;
            valveScript.valveBurstTime = TimeOfDay.Instance.normalizedTimeOfDay + 0.001f;
        }
        else if (TimeOfDay.Instance.normalizedTimeOfDay > valveScript.valveCrackTime)
        {
            Logger.LogDebug($"{valveScript.name} #{valveScript.NetworkObjectId}: 2) valve has already cracked");
            valveScript.valveBurstTime = TimeOfDay.Instance.normalizedTimeOfDay + 0.0001f;
        }
        if (valveScript.fogSizeMultiplier == 0)
        {
            Logger.LogDebug($"{valveScript.name} #{valveScript.NetworkObjectId}: 3) no fogSizeMultiplier, injecting custom {fogMultiplier}");
            valveScript.fogSizeMultiplier = fogMultiplier;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollideBridgeServerRpc(float angleHitAt, Vector3 bridgeAtPos)
    {
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] SERVER receiving hit BRIDGE with angle {angleHitAt} at position {bridgeAtPos}");
        foreach (BridgeTrigger walkBridge in FindObjectsByType<BridgeTrigger>(FindObjectsSortMode.None))
        {
            if (walkBridge.transform.position == bridgeAtPos)
            {
                CollideBridgeServer(walkBridge, angleHitAt);
                return;
            }
        }
    }

    private void CollideBridgeServer(BridgeTrigger bridge, float angle)
    {
        if (bridge.bridgeDurability <= 0)
        {
            Logger.LogDebug("walkBridge already collapsed");
            return;
        }
        float powerBridgeHitAt = Mathf.Clamp(90 - angle, 10, 70) / 100f;
        bridge.bridgeDurability -= powerBridgeHitAt;
        Logger.LogDebug($"hit bridge with power {powerBridgeHitAt} to new durability {bridge.bridgeDurability}");
    }



    //ROCK
    [ServerRpc(RequireOwnership = false)]
    private void RockCollidePlayerServerRpc(int hitPlayerID, int hitForce, int hitPlayersLength)
    {
        RockCollidePlayerClientRpc(hitPlayerID, hitForce, hitPlayersLength);
    }

    [ClientRpc]
    private void RockCollidePlayerClientRpc(int hitPlayerID, int hitForce, int hitPlayersLength)
    {
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to hit PLAYER [{hitPlayerID}] with force {hitForce}");
        PlaySFXOneShot(gloveAudio, rockPunchClip, true, false, hitForce / 100f, 20f, 1f + (0.1f * (hitPlayersLength - 1)));
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (hitPlayerID == (int)localPlayer.playerClientId)
        {
            localPlayer.DamagePlayer(hitForce, true, true, CauseOfDeath.Inertia, 0, false, glove.transform.forward * (hitForce / 1.5f) + Vector3.up * 0.75f);
            if (!localPlayer.isPlayerDead)
            {
                localPlayer.externalForceAutoFade += glove.transform.forward * (hitForce / 3f) + Vector3.up * (hitForce / 2f);
            }
        }
    }



    //PAPER
    [ServerRpc(RequireOwnership = false)]
    private void PaperCollideItemServerRpc(int heldPlayerID, NetworkObjectReference itemNOR, float evaluationPoint, int hitPlayerID = -1)
    {
        if (itemNOR.TryGet(out var itemNO))
        {
            try
            {
                itemNO.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[heldPlayerID].actualClientId);
            }
            catch
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to change OWNERSHIP on grabbed item!!!");
                return;
            }
        }
        else
        {
            Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get NETWORK Object on grabbed item!!!");
            return;
        }
        PaperCollideItemClientRpc(heldPlayerID, itemNOR, evaluationPoint, hitPlayerID);
    }

    [ClientRpc]
    private void PaperCollideItemClientRpc(int heldPlayerID, NetworkObjectReference itemNOR, float evaluationPoint, int hitPlayerID = -1)
    {
        if (heldPlayerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            if (itemNOR.TryGet(out var itemNO))
            {
                GrabbableObject item = itemNO.GetComponentInChildren<GrabbableObject>();
                if (item == null)
                {
                    Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get GRABBABLE Object on grabbed item!!!");
                    return;
                }
                PaperCollideItemLocal(heldPlayerID, item, evaluationPoint, hitPlayerID);
            }
            else
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get NETWORK Object on grabbed item!!!");
            }
        }
    }

    private void PaperCollideItemLocal(int heldPlayerID, GrabbableObject item, float evaluationPoint, int hitPlayerID = -1)
    {
        if (hitPlayerID != -1)
        {
            PaperCollideStolenItemLocal(hitPlayerID, item);
        }
        if (IsServer)
        {
            try
            {
                item.NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[heldPlayerID].actualClientId);
            }
            catch
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to change OWNERSHIP on grabbed item!!!");
                return;
            }
        }
        holdingItem = item;
        paperRetractSpeedEvaluation = evaluationPoint;
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to grab ITEM {holdingItem.name} #{holdingItem.NetworkObjectId} with evaluationPoint {evaluationPoint}");
        GrabHoldingItem(hitPlayerID);
    }

    private void PaperCollideStolenItemLocal(int hitPlayerID, GrabbableObject item)
    {
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to STEAL item {item.name} #{item.NetworkObjectId} from player [{hitPlayerID}]");
        item.DiscardItem();
        PlayerControllerB hitPlayer = StartOfRound.Instance.allPlayerScripts[hitPlayerID];
        int itemSlot = -1;
        for (int i = 0; i < hitPlayer.ItemSlots.Length; i++)
        {
            if (hitPlayer.ItemSlots[i] == item)
            {
                itemSlot = i;
                break;
            }
        }
        if (itemSlot != -1)
        {
            hitPlayer.ItemSlots[itemSlot] = null;
            if (hitPlayer == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
                HUDManager.Instance.holdingTwoHandedItem.enabled = false;
                HUDManager.Instance.ClearControlTips();
            }
        }
        hitPlayer.twoHanded = false;
        hitPlayer.twoHandedAnimation = false;
        hitPlayer.activatingItem = false;
        hitPlayer.IsInspectingItem = false;
        hitPlayer.isHoldingObject = false;
        hitPlayer.currentlyHeldObject = null;
        hitPlayer.currentlyHeldObjectServer = null;
        hitPlayer.equippedUsableItemQE = false;
        hitPlayer.carryWeight = Mathf.Clamp(hitPlayer.carryWeight - (item.itemProperties.weight - 1), 1f, 10f);
        hitPlayer.playerBodyAnimator.SetBool("cancelHolding", true);
        hitPlayer.playerBodyAnimator.SetTrigger("Throw");
        hitPlayer.playerBodyAnimator.SetBool("Grab", false);
        try
        {
            hitPlayer.playerBodyAnimator.SetBool(item.itemProperties.grabAnim, false);
        }
        catch
        {
            Logger.LogWarning($"error when trying to steal item and setting playerBodyAnimator with grabAnim {item.itemProperties.grabAnim}!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PaperCollidePlayerServerRpc(int heldPlayerID, int hitPlayerID, float evaluationPoint)
    {
        PaperCollidePlayerClientRpc(heldPlayerID, hitPlayerID, evaluationPoint);
    }

    [ClientRpc]
    private void PaperCollidePlayerClientRpc(int heldPlayerID, int hitPlayerID, float evaluationPoint)
    {
        if (heldPlayerID != (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            PaperCollidePlayerLocal(hitPlayerID, evaluationPoint);
        }
    }

    private void PaperCollidePlayerLocal(int hitPlayerID, float evaluationPoint)
    {
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to grab PLAYER [{hitPlayerID}] with evaluationPoint {evaluationPoint}");
        PlayerControllerB hitPlayer = StartOfRound.Instance.allPlayerScripts[hitPlayerID];
        if (hitPlayer.isPlayerControlled)
        {
            GoldenGloveScript gloveHoldingPlayer = PlayerAlreadyHeldByGlove(hitPlayer);
            if (gloveHoldingPlayer != null)
            {
                Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to STEAL player {hitPlayer.playerUsername}");
                gloveHoldingPlayer.DiscardHoldingPlayer(false);
            }
            holdingPlayer = hitPlayer;
            paperRetractSpeedEvaluation = evaluationPoint;
            GrabHoldingPlayer();
        }
    }

    private GoldenGloveScript PlayerAlreadyHeldByGlove(PlayerControllerB hitPlayer)
    {
        GoldenGloveScript gloveHoldingPlayer = null;
        foreach (GoldenGloveScript glove in FindObjectsByType<GoldenGloveScript>(FindObjectsSortMode.None))
        {
            if (glove != this && glove.holdingPlayer == hitPlayer)
            {
                gloveHoldingPlayer = glove;
                break;
            }
        }
        return gloveHoldingPlayer;
    }



    //SCISSORS
    [ServerRpc(RequireOwnership = false)]
    private void ScissorsCollideEnemyServerRpc(NetworkObjectReference enemyNOR)
    {
        ScissorsCollideEnemyClientRpc(enemyNOR);
    }

    [ClientRpc]
    private void ScissorsCollideEnemyClientRpc(NetworkObjectReference enemyNOR)
    {
        if (enemyNOR.TryGet(out var enemyNO))
        {
            EnemyAI hitEnemy = enemyNO.GetComponent<EnemyAI>();
            if (hitEnemy == null)
            {
                Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get ENEMY AI on hit enemy!!!");
                return;
            }
            Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to stun ENEMY {hitEnemy.name}");
            hitEnemy.SetEnemyStunned(true, scissorsStunEnemies, playerHeldBy);
            PlaySFXOneShot(gloveAudio, scissorsStunClip, true, true, 0.66f, 7.5f);
        }
        else
        {
            Logger.LogError($"{name} #{NetworkObjectId} [{stateIndex}] failed to get NETWORK Object on hit enemy!!!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ScissorsCollidePlayerServerRpc(int hitPlayerID, int hitForce, int hitPlayersLength)
    {
        ScissorsCollidePlayerClientRpc(hitPlayerID, hitForce, hitPlayersLength);
    }

    [ClientRpc]
    private void ScissorsCollidePlayerClientRpc(int hitPlayerID, int hitForce, int hitPlayersLength)
    {
        Logger.LogDebug($"{name} #{NetworkObjectId} [{stateIndex}] trying to stun PLAYER [{hitPlayerID}]");
        PlaySFXOneShot(gloveAudio, scissorsStunClip, true, false, 0.5f, 20f, 1f + (0.1f * (hitPlayersLength - 1)));
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (hitPlayerID == (int)localPlayer.playerClientId && !localPlayer.isPlayerDead)
        {
            float distanceFromEyes = Mathf.Clamp(Mathf.Abs(glove.transform.position.y - localPlayer.playerEye.position.y), 0.3f, 0.9f);
            float finalStun = scissorsStunPlayers - (-0.3f + distanceFromEyes);
            Logger.LogDebug($"distance: {distanceFromEyes} | final: {finalStun}");
            HUDManager.Instance.flashFilter += finalStun;
            Mathf.Clamp(HUDManager.Instance.flashFilter, 0.0f, 1f);
            SoundManager.Instance.earsRingingTimer += finalStun * 0.75f;
            Mathf.Clamp(SoundManager.Instance.earsRingingTimer, 0.0f, 1f);
            localPlayer.externalForceAutoFade += transform.up * (hitForce / 10f) + Vector3.up * (hitForce / 5f);
        }
    }



    //SyncUponJoin
    [ServerRpc(RequireOwnership = false)]
    public void SyncUponJoinServerRpc(int playerID)
    {
        int holdingItemID = -1;
        if (holdingItem != null)
        {
            holdingItemID = (int)holdingItem.NetworkObjectId;
        }
        int holdingPlayerID = -1;
        if (holdingPlayer != null)
        {
            holdingPlayerID = (int)holdingPlayer.playerClientId;
        }
        SyncUponJoinClientRpc(playerID, stateIndex, holdingItemID, holdingPlayerID);
    }

    [ClientRpc]
    private void SyncUponJoinClientRpc(int playerID, int hostStateIndex, int hostHoldingItem, int hostHoldingPlayer)
    {
        if (playerID == (int)GameNetworkManager.Instance.localPlayerController.playerClientId)
        {
            if (hostStateIndex != 0)
            {
                stateIndex = hostStateIndex;
                SwitchToMesh();
            }
            if (hostHoldingItem != -1)
            {
                foreach (GrabbableObject item in FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None))
                {
                    if ((int)item.NetworkObjectId == hostHoldingItem)
                    {
                        holdingItem = item;
                        GrabHoldingItem();
                        break;
                    }
                }
            }
            if (hostHoldingPlayer != -1)
            {
                holdingPlayer = StartOfRound.Instance.allPlayerScripts[hostHoldingPlayer];
                GrabHoldingPlayer();
            }
        }
    }



    //HarmonyPatch
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerPatch
    {
        [HarmonyPrefix, HarmonyPatch("KillPlayer")]
        public static void KillPlayerPrefix(PlayerControllerB __instance, bool spawnBody)
        {
            if (!spawnBody && __instance == GameNetworkManager.Instance.localPlayerController && !__instance.isPlayerDead && __instance.AllowPlayerDeath() && __instance.currentlyHeldObjectServer != null)
            {
                GoldenGloveScript goldenGlove = __instance.currentlyHeldObjectServer.GetComponent<GoldenGloveScript>();
                if (goldenGlove != null && goldenGlove.holdingPlayer != null)
                {
                    Logger.LogDebug($"{goldenGlove.name} #{goldenGlove.NetworkObjectId} local player SENDING custom call to discard holdingPlayer by DEATH");
                    goldenGlove.DiscardHoldingPlayer();
                    goldenGlove.SyncDiscardHoldingPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                }
            }
        }
    }

    [HarmonyPatch(typeof(CaveDwellerAI))]
    public class ManeaterPatch
    {
        [HarmonyPrefix, HarmonyPatch("DropBabyLocalClient")]
        public static bool DropBabyLocalClientPrefix(CaveDwellerAI __instance)
        {
            GoldenGloveScript heldByGlove = __instance.GetComponentInParent<GoldenGloveScript>();
            if (__instance.currentBehaviourStateIndex == 0 && heldByGlove != null)
            {
                Logger.LogDebug($"{heldByGlove.name} #{heldByGlove.NetworkObjectId} [{heldByGlove.stateIndex}] NOT DROPPING BABY {__instance.name} #{__instance.NetworkObjectId} as it is still held by a glove!!");
                return false;
            }
            return true;
        }
    }
}
