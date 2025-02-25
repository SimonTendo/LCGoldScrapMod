using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;

public class TagDoor : MonoBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GameObject tagContainer;

    public GameObject scanNodeObject;

    void Start()
    {
        scanNodeObject.SetActive(false);
        Logger.LogDebug($"spawned {name} at {transform.position}");
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!Config.hostToolRebalance)
        {
            scanNodeObject.SetActive(true);
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        scanNodeObject.SetActive(false);
    }

    [HarmonyPatch(typeof(DoorLock), "Awake")]
    public class NewDoorLock
    {
        [HarmonyPostfix]
        public static void AwakePostfix(DoorLock __instance)
        {
            if (PrivateAccesser.GetPrivateField<bool>(__instance, "hauntedDoor"))
            {
                Instantiate(tagContainer, __instance.transform);
            }
        }
    }
}
