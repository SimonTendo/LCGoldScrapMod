using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;

public class TagDoor : MonoBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GameObject tagContainer;

    public ScanNodeProperties scanNode;
    public Collider colliderToToggle;

    void Start()
    {
        colliderToToggle.enabled = false;
        Logger.LogDebug($"spawned {name} at {transform.position}");
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!Config.hostToolRebalance)
        {
            colliderToToggle.enabled = true;
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        colliderToToggle.enabled = false;
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
