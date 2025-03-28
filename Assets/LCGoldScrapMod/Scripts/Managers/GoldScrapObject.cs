using UnityEngine;
using BepInEx.Logging;
using System.Collections;

public class GoldScrapObject : MonoBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public GrabbableObject item;
    public ItemData data;

    //DropStop
    private float dropStopTimer;
    private float lastDropTime;
    private int weightBeforeIncreasing = 40;
    private float powerMultiplier = 2.25f;
    private float repeatDropTimeframe = 1.5f;
    private float maxWaitTimer = 3.5f;
    private float inStepsOf = 0.1f;

    void Start()
    {
        item = GetComponent<GrabbableObject>();
        if (gameObject.layer != 5)
        {
            StartCoroutine(SetConfigDependencies());
        }
    }

    private IEnumerator SetConfigDependencies()
    {
        yield return new WaitUntil(() => Plugin.appliedHostConfigs);
        if (item != null)
        {
            dropStopTimer = Mathf.Clamp(Mathf.Pow(item.itemProperties.weight - (weightBeforeIncreasing / 100f), powerMultiplier), 0, maxWaitTimer);
            if (!HoarderBugAI.grabbableObjectsInMap.Contains(item.gameObject))
            {
                HoarderBugAI.grabbableObjectsInMap.Add(item.gameObject);
            }
        }
        if (Plugin.specialDateCase >= 0 && data != null)
        {
            Logger.LogDebug($"calling runtime change from {gameObject}");
            RuntimeChanges.UpdatePrefabForSpecialDate(Plugin.specialDateCase, data, gameObject);
        }
    }

    public void StartDropStop()
    {
        if (!item.isInShipRoom && Time.realtimeSinceStartup - lastDropTime < dropStopTimer + repeatDropTimeframe)
        {
            StartCoroutine(DisableGrab());
        }
        lastDropTime = Time.realtimeSinceStartup;
    }

    private IEnumerator DisableGrab()
    {
        float timer = dropStopTimer;
        item.grabbable = false;
        while (timer > 0)
        {
            string charge = null;
            for (int i = 0; i < 100; i += 10)
            {
                float percentage = 100 * timer / dropStopTimer;
                if (i < percentage)
                {
                    charge += "|";
                }
                else
                {
                    charge += " ";
                }
            }
            item.customGrabTooltip = $"[{charge}]";
            timer -= inStepsOf;
            yield return new WaitForSeconds(inStepsOf);
        }
        item.grabbable = true;
        item.customGrabTooltip = null;
    }
}
