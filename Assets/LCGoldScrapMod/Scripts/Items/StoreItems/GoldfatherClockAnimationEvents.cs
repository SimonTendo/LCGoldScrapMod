using UnityEngine;
using BepInEx.Logging;

public class GoldfatherClockAnimationEvents : MonoBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public GoldfatherClockScript mainScript;

    public void PendulumSway()
    {
        mainScript.AnimEventPendulumSway();
    }

    public void PendulumCrash()
    {
        mainScript.AnimEventPendulumCrash();
    }

    public void BirdhouseOpen()
    {
        mainScript.AnimEventBirdhouseOpen();
    }

    public void BirdScreech()
    {
        mainScript.AnimEventBirdScreech();
    }

    public void PunchBird()
    {
        mainScript.AnimEventPunchBird();
    }

    public void SlamShut()
    {
        mainScript.AnimEventSlamShut();
    }

    public void StartStressfulBells()
    {
        mainScript.StartStressfulLoop();
    }
}
