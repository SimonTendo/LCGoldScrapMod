using System.Collections;
using UnityEngine;
using BepInEx.Logging;

public class GoldenGlassScript : GrabbableObject
{
    private static ManualLogSource Logger = Plugin.Logger;

    private Coroutine beginRevealCoroutine;
    private Coroutine endRevealCoroutine;

    [Space(3f)]
    [Header("Audiovisual")]
    public AudioSource audio2D;
    public AudioClip beginRevealClip;
    public AudioClip endRevealClip;
    public string inspectingTooltip;
    [Tooltip("The fewer reveals per frame, the better performance, but the slower things will appear/disappear on-screen when using the Glass.")]
    public int revealPerFrame;

    public override void InspectItem()
    {
        base.InspectItem();
        if (playerHeldBy.IsInspectingItem)
        {
            StartAndStopCoroutine(true);
        }
        else
        {
            StartAndStopCoroutine(false);
        }
    }

    public override void PocketItem()
    {
        if (playerHeldBy != null && playerHeldBy == GameNetworkManager.Instance.localPlayerController)
        {
            StartAndStopCoroutine(false, playerHeldBy.IsInspectingItem);
        }
        base.PocketItem();
    }

    public override void DiscardItem()
    {
        StartAndStopCoroutine(false, playerHeldBy.IsInspectingItem);
        base.DiscardItem();
    }

    private void StartAndStopCoroutine(bool beginReveal, bool playClip = true)
    {
        if (beginReveal)
        {
            if (endRevealCoroutine != null)
            {
                Logger.LogDebug("!!!STOPPING END REVEAL COROUTINE!!!");
                StopCoroutine(endRevealCoroutine);
                endRevealCoroutine = null;
            }
            beginRevealCoroutine = StartCoroutine(BeginRevealLocal(playClip));
        }
        else
        {
            if (beginRevealCoroutine != null)
            {
                Logger.LogDebug("!!!STOPPING BEGIN REVEAL COROUTINE!!!");
                StopCoroutine(beginRevealCoroutine);
                beginRevealCoroutine = null;
            }
            endRevealCoroutine = StartCoroutine(EndRevealLocal(playClip));
        }
    }

    private IEnumerator BeginRevealLocal(bool playClip = true)
    {
        if (!playClip)
        {
            yield break;
        }

        audio2D.Stop();
        audio2D.PlayOneShot(beginRevealClip);
        UpdateControlTipManually();
        int revealedThisFrame = 0;

        GoldScrapObject[] allGoldScrap = FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None);
        foreach (GoldScrapObject goldScrap in allGoldScrap)
        {
            if (RarityManager.CurrentlyGoldFever() && goldScrap.item != null)
            {
                SetAllItemsScanNodes(goldScrap.item, true);
            }

            IGoldenGlassSecret secret = goldScrap.GetComponent<IGoldenGlassSecret>();
            if (secret != null)
            {
                Logger.LogDebug($"BEGIN revealing {goldScrap.gameObject.name}");
                secret.BeginReveal();
                revealedThisFrame++;
                if (revealedThisFrame >= revealPerFrame)
                {
                    yield return null;
                    revealedThisFrame = 0;
                }
            }
        }
        beginRevealCoroutine = null;
    }

    private IEnumerator EndRevealLocal(bool playClip = true)
    {
        if (!playClip)
        {
            yield break;
        }

        audio2D.Stop();
        audio2D.PlayOneShot(endRevealClip);
        UpdateControlTipManually(false);
        int revealedThisFrame = 0;

        GoldScrapObject[] allGoldScrap = FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None);
        foreach (GoldScrapObject goldScrap in allGoldScrap)
        {
            if ((StartOfRound.Instance.inShipPhase || RarityManager.CurrentlyGoldFever()) && goldScrap.item != null)
            {
                SetAllItemsScanNodes(goldScrap.item, false);
            }

            IGoldenGlassSecret secret = goldScrap.GetComponent<IGoldenGlassSecret>();
            if (secret != null)
            {
                Logger.LogDebug($"STOP revealing {goldScrap.gameObject.name}");
                secret.EndReveal();
                revealedThisFrame++;
                if (revealedThisFrame >= revealPerFrame)
                {
                    yield return null;
                    revealedThisFrame = 0;
                }
            }
        }
        endRevealCoroutine = null;
    }

    private void SetAllItemsScanNodes(GrabbableObject item, bool setTo)
    {
        ScanNodeProperties scanNode = item.gameObject.GetComponentInChildren<ScanNodeProperties>();
        if (scanNode != null)
        {
            if (setTo && !item.isInShipRoom)
            {
                scanNode.maxRange = Configs.hostToolRebalance ? 128 : 256;
                scanNode.requiresLineOfSight = false;
            }
            else if (!setTo)
            {
                scanNode.maxRange = 13;
                scanNode.requiresLineOfSight = true;
            }
        }
    }

    private void UpdateControlTipManually(bool addTooltip = true)
    {
        for (int i = 0; i < HUDManager.Instance.controlTipLines.Length; i++)
        {
            string currentLine = HUDManager.Instance.controlTipLines[i].text;
            if (addTooltip && string.IsNullOrEmpty(currentLine))
            {
                Logger.LogDebug($"Found line '{currentLine}' at [{i}] with bool {addTooltip}");
                HUDManager.Instance.controlTipLines[i].text = inspectingTooltip;
                return;
            }
            else if (!addTooltip && currentLine == inspectingTooltip)
            {
                Logger.LogDebug($"Found line '{currentLine}' at [{i}] with bool {addTooltip}");
                HUDManager.Instance.controlTipLines[i].text = "";
                return;
            }
        }
        Logger.LogWarning($"UpdateControlTipManually({addTooltip}) did not find tooltip correctly");
    }
}
