using System.Collections;
using UnityEngine;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

public class GoldenTicketScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static int playersBeingTeleported;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource teleportAudio;
    public AudioClip teleportClip;
    public AudioSource inverseAudio;
    public AudioClip inverseClip;

    //GoldenTicket functionality
    [HarmonyPatch(typeof(PlayerControllerB), "DropAllHeldItems")]
    public class NewPlayerDropAllItems
    {
        [HarmonyPrefix]
        public static bool DontDropItemsWhenHoldingTicket(PlayerControllerB __instance)
        {
            //Player dropping items must be alive and connected, and if a Golden Ticket has fired, skip this functionality this frame just to be sure
            if (!__instance.isPlayerControlled)
            {
                return true;
            }
            Logger.LogDebug($"running DropAllHeldItems patch on {__instance.playerUsername}");

            //If there are other players left being teleported, do not destroy their items from this client
            if (playersBeingTeleported > 0)
            {
                playersBeingTeleported--;
                Logger.LogDebug($"not dropping {__instance.playerUsername}'s items | players left to teleport: {playersBeingTeleported}");
                return false;
            }

            //First check if there is any Golden Ticket in the world at all, this to prevent the entire calculation every time
            GoldenTicketScript[] goldenTickets = FindObjectsOfType<GoldenTicketScript>();
            bool foundValidTicket = false;
            foreach (GoldenTicketScript ticket in goldenTickets)
            {
                if (!ticket.deactivated)
                {
                    foundValidTicket = true;
                    break;
                }
            }
            Plugin.Logger.LogDebug($"foundValidTicket?: {foundValidTicket}");
            if (!foundValidTicket)
            {
                return true;
            }

            //Teleporter: you keep your own items when teleported back to the ship
            if (__instance.shipTeleporterId == 1)
            {
                for (int i = 0; i < __instance.ItemSlots.Length; i++)
                {
                    GrabbableObject currentItem = __instance.ItemSlots[i];
                    Plugin.Logger.LogDebug($"player: {__instance.playerUsername} | slot: {i} | holding: {currentItem}");
                    if (currentItem != null)
                    {
                        GoldenTicketScript ticket = currentItem.GetComponent<GoldenTicketScript>();
                        if (ticket != null)
                        {
                            Plugin.Logger.LogDebug($"found TELEPORTER and destroying {currentItem.name} #{currentItem.NetworkObjectId} at slot {i}");
                            ticket.DestroyGoldenTicket(i);
                            return false;
                        }
                    }
                }
                return true;
            }

            //Check if there is an inverse teleporter, should the normal teleporter check fail
            ShipTeleporter inverseTeleporter = null;
            ShipTeleporter[] allTeleporters = FindObjectsOfType<ShipTeleporter>();
            foreach (ShipTeleporter teleporter in allTeleporters)
            {
                if (teleporter.isInverseTeleporter)
                {
                    inverseTeleporter = teleporter;
                    break;
                }
            }
            Plugin.Logger.LogDebug($"found inverseTeleporter?: {inverseTeleporter != null}");
            if (inverseTeleporter == null)
            {
                return true;
            }

            //Inverse Teleporter: all players who will get teleported keep their items
            if (inverseTeleporter.shipTeleporterAudio.isPlaying)
            {
                GoldenTicketScript foundTicket = null;
                int foundSlot = 0;
                int playersInTeleporter = 0;
                for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];
                    if (!player.isPlayerControlled)
                    {
                        continue;
                    }
                    float distance = Vector3.Distance(player.transform.position, inverseTeleporter.teleportOutPosition.position);
                    Logger.LogDebug($"{player.playerUsername}: {distance}");
                    if (distance < 2f)
                    {
                        playersInTeleporter++;
                        for (int j = 0; j < player.ItemSlots.Length; j++)
                        {
                            GrabbableObject currentItem = player.ItemSlots[j];
                            Plugin.Logger.LogDebug($"player: {player.playerUsername} | slot: {j} | holding: {currentItem}");
                            if (foundTicket == null && currentItem != null)
                            {
                                GoldenTicketScript ticket = currentItem.GetComponent<GoldenTicketScript>();
                                if (ticket != null)
                                {
                                    foundTicket = ticket;
                                    foundSlot = j;
                                    Plugin.Logger.LogDebug($"found INVERSE and destroying {foundTicket.name} #{foundTicket.NetworkObjectId} at slot {foundSlot}");
                                    break;    
                                }
                            }
                        }
                    }
                }
                if (foundTicket != null)
                {
                    foundTicket.DestroyGoldenTicket(foundSlot, playersInTeleporter, false);
                    return false;
                }
                else
                {
                    playersBeingTeleported = 0;
                }
            }

            //If none of the above fires, nothing new should happen
            return true;
        }
    }

    public void DestroyGoldenTicket(int itemSlot, int amount = 1, bool normalTeleport = true)
    {
        playersBeingTeleported = amount - 1;
        Logger.LogDebug($"started DestroyGoldenTicket() with amount: {playersBeingTeleported} and normalTeleport {normalTeleport}");
        StartCoroutine(StartCooldown(itemSlot, normalTeleport));
    }

    private IEnumerator StartCooldown(int itemSlot, bool normalTeleport)
    {
        yield return new WaitForSeconds(0.05f);
        Logger.LogDebug("Ti-A");
        PlaySFX(normalTeleport);
        if (!RarityManager.CurrentlyGoldFever())
        {
            BreakItem(itemSlot);
            StartCoroutine(DelaySettingObjectAway());
        }
        yield return new WaitForSeconds(5f);
        Logger.LogDebug("Ti-B");
        if (playersBeingTeleported != 0)
        {
            Logger.LogDebug($"WARNING!! teleport coroutine finished while having {playersBeingTeleported} players unaccounted for, returning!");
            playersBeingTeleported = 0;
        }
    }

    private void PlaySFX(bool normalTeleport)
    {
        if (normalTeleport)
        {
            teleportAudio.PlayOneShot(teleportClip);
            WalkieTalkie.TransmitOneShotAudio(teleportAudio, teleportClip, 0.25f);
        }
        else
        {
            inverseAudio.PlayOneShot(inverseClip);
            WalkieTalkie.TransmitOneShotAudio(inverseAudio, inverseClip, 0.25f);
        }
    }

    private void BreakItem(int itemSlot)
    {
        DestroyObjectInHand(playerHeldBy);
        if (isPocketed)
        {
            if (StartOfRound.Instance.shipBounds.bounds.Contains(transform.position))
            {
                transform.SetParent(StartOfRound.Instance.elevatorTransform);
            }
            else
            {
                transform.SetParent(StartOfRound.Instance.propsContainer);
            }
            startFallingPosition = transform.parent.InverseTransformPoint(transform.position);
            targetFloorPosition = transform.parent.InverseTransformPoint(transform.position);
            fallTime = 1f;
            reachedFloorTarget = true;
            hasHitGround = true;
            parentObject = null;
            heldByPlayerOnServer = false;
            isHeld = false;
            isPocketed = false;
            playerHeldBy.carryWeight = Mathf.Clamp(playerHeldBy.carryWeight - (itemProperties.weight - 1), 1f, 10f);
            playerHeldBy.ItemSlots[itemSlot] = null;
            if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
            }
        }
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
}
