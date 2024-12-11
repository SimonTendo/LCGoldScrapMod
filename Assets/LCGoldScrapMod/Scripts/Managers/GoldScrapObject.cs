using UnityEngine;
using BepInEx.Logging;
using System.Collections;

public class GoldScrapObject : MonoBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public GrabbableObject item;

    //DropStop
    private float dropStopTimer;
    private float lastDropTime;
    private int weightBeforeIncreasing = 40;
    private float powerMultiplier = 2.25f;
    private float repeatDropTimeframe = 1.5f;
    private float maxWaitTimer = 3.5f;

    void Start()
    {
        item = GetComponent<GrabbableObject>();
        if (item != null && item.gameObject.layer != 5)
        {
            StartCoroutine(SetDropStopTimer());
            if (!HoarderBugAI.grabbableObjectsInMap.Contains(item.gameObject))
            {
                HoarderBugAI.grabbableObjectsInMap.Add(item.gameObject);
            }
        }
    }

    private IEnumerator SetDropStopTimer()
    {
        yield return new WaitUntil(() => Plugin.appliedHostConfigs);
        dropStopTimer = Mathf.Clamp(Mathf.Pow(item.itemProperties.weight - (weightBeforeIncreasing / 100f), powerMultiplier), 0, maxWaitTimer);
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
        item.grabbable = false;
        item.customGrabTooltip = " ";
        yield return new WaitForSeconds(dropStopTimer);
        item.grabbable = true;
        item.customGrabTooltip = null;
    }
}
