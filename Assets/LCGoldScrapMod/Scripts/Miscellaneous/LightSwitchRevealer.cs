using UnityEngine;
using BepInEx.Logging;


public class LightSwitchRevealer : MonoBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static GameObject lightRevealerPrefab;
    public Light lightSource;

    void Start()
    {
        lightSource.enabled = false;
        lightSource.intensity = 5;
        lightSource.range = 10;
        lightSource.color = Color.white;
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!Config.hostToolRebalance)
        {
            lightSource.enabled = true;
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        lightSource.enabled = false;
    }
}
