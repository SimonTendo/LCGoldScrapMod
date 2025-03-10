using UnityEngine;
using BepInEx.Logging;

public class CatOGoldScript : MonoBehaviour, IGoldenGlassSecret
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static CatOGoldScript Instance;
    public Animator armAnimator;
    public float[] animSpeedPerDayToFever;

    [Space(3f)]
    [Header("Audiovisual")]
    public MeshRenderer meshRenderer;
    public Material[] matsNormal;
    public Material[] matsFever;
    public GameObject scanNodeObject;
    public ScanNodeProperties scanNode;


    private void Awake()
    {
        Instance = this;
        OnDayChange();
        scanNodeObject.SetActive(false);
    }

    public void OnDayChange(int daysUntilFever = -99)
    {
        if (daysUntilFever == -99 && RarityManager.instance != null)
        {
            daysUntilFever = RarityManager.instance.daysUntilNextFever;
        }
        float animSpeed = animSpeedPerDayToFever[animSpeedPerDayToFever.Length - 1];
        if (daysUntilFever >= 0 && daysUntilFever < animSpeedPerDayToFever.Length)
        {
            animSpeed = animSpeedPerDayToFever[daysUntilFever];
        }
        Logger.LogDebug($"AnimateArm finished with animSpeed {animSpeed} and daysUntilFever {daysUntilFever}");
        armAnimator.SetFloat("animSpeed", animSpeed);
        if (daysUntilFever == 0)
        {
            meshRenderer.materials = matsFever;
            scanNode.subText = "Gold Fever TODAY!";
        }
        else
        {
            meshRenderer.materials = matsNormal;
            scanNode.subText = $"Gold Fever in {daysUntilFever} days";
        }
    }

    void IGoldenGlassSecret.BeginReveal()
    {
        if (!Configs.hostToolRebalance)
        {
            scanNodeObject.SetActive(true);
        }
    }

    void IGoldenGlassSecret.EndReveal()
    {
        scanNodeObject.SetActive(false);
    }
}
