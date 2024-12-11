using UnityEngine;
using BepInEx.Logging;
using static AssetsCollection;
using static RegisterGoldScrap;

public class RuntimeChanges
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static void SyncHostConfigs(bool rebalance, float multiplier)
    {
        foreach (ItemData itemWithWeight in Plugin.allGoldScrap.allItemData)
        {
            itemWithWeight.itemProperties.weight = Config.CalculateWeightCustom(itemWithWeight.defaultWeight, multiplier);
        }
        Logger.LogInfo($"Applied values of host's config [Multiplier Weight]. Value: {multiplier}");

        Config.hostToolRebalance = rebalance;
        Logger.LogInfo($"Applied value of host's config [Other Tools Balance]. Value: {Config.hostToolRebalance}");
        
        GoldenBell.RebalanceTool();
        GoldenGlass.RebalanceTool();
        GoldSign.RebalanceTool(multiplier);
        GoldenGuardian.RebalanceTool();
        JacobsLadder.RebalanceTool();
        GoldRemote.RebalanceTool();
        Goldmine.RebalanceTool();
        GoldenGrenade.RebalanceTool();
        GoldenPickaxe.RebalanceTool();
        GoldenGlove.RebalanceTool();

        UpdateRebalancedScrapRuntime();

        Plugin.appliedHostConfigs = true;
    }

    public static void UpdateRebalancedScrapRuntime()
    {
        GoldScrapObject[] allGoldScrap = Object.FindObjectsOfType<GoldScrapObject>();
        Logger.LogDebug($"goldScrapItem length: {allGoldScrap.Length}");
        foreach (GoldScrapObject scrapScript in allGoldScrap)
        {
            if (scrapScript.item == null)
            {
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenBell.itemName)
            {
                AnimatedItem itemScript = scrapScript.GetComponent<AnimatedItem>();
                if (Config.hostToolRebalance)
                {
                    itemScript.noiseRange = 128;
                }
                else
                {
                    itemScript.noiseRange = 64;
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Config.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenGlass.itemName)
            {
                if (Config.hostToolRebalance)
                {
                    Material[] newMats = { defaultMaterialGold, defaultMaterialGold };
                    scrapScript.GetComponent<MeshRenderer>().materials = newMats;
                }
                else
                {
                    Material[] newMatsAlt = { defaultMaterialGold, defaultMaterialGoldTransparent };
                    scrapScript.GetComponent<MeshRenderer>().materials = newMatsAlt;
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Config.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldSign.itemName)
            {
                if (!Config.sillyScrap.Value && Config.hostToolRebalance && goldSignMeshAlt != null)
                {
                    scrapScript.GetComponent<MeshFilter>().mesh = goldSignMeshAlt;
                }
                else if (!Config.sillyScrap.Value && !Config.hostToolRebalance && goldSignMesh != null)
                {
                    scrapScript.GetComponent<MeshFilter>().mesh = goldSignMesh;
                }
                else
                {
                    scrapScript.GetComponent<MeshFilter>().mesh = LoadSillyMesh(GoldSign.itemFolder);
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Config.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldRemote.itemName)
            {
                if (Config.hostToolRebalance)
                {
                    scrapScript.item.useCooldown = 1.5f;
                }
                else
                {
                    scrapScript.item.useCooldown = 0f;
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Config.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenGrenade.itemName)
            {
                MeshFilter bodyMesh = scrapScript.transform.GetChild(2).GetComponent<MeshFilter>();
                MeshFilter pinMesh = scrapScript.transform.GetChild(2).GetChild(0).GetComponent<MeshFilter>();
                MeshFilter bodyMeshAlt = scrapScript.transform.GetChild(3).GetComponent<MeshFilter>();
                MeshFilter pinMeshAlt = scrapScript.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();
                StunGrenadeItem itemScript = scrapScript.GetComponent<StunGrenadeItem>();

                if (Config.hostToolRebalance)
                {
                    bodyMesh.mesh = null;
                    pinMesh.mesh = null;
                    bodyMeshAlt.mesh = null;
                    pinMeshAlt.mesh = null;
                    if (Config.hostToolRebalance)
                    {
                        itemScript.TimeToExplode = 0.18f;
                        itemScript.playerAnimation = "PullGrenadePin2";
                        if (Config.sillyScrap.Value)
                        {
                            bodyMesh.mesh = LoadSillyMesh(GoldenGrenade.itemFolder);
                            itemScript.pullPinSFX = LoadSillySFX($"{GoldenGrenade.itemFolder}PullPin");
                            itemScript.explodeSFX = LoadSillySFX($"{GoldenGrenade.itemFolder}Explode");
                        }
                        else
                        {
                            if (goldenGrenadeBodyMeshAlt != null && goldenGrenadePinMeshAlt != null)
                            {
                                bodyMeshAlt.mesh = goldenGrenadeBodyMeshAlt;
                                pinMeshAlt.mesh = goldenGrenadePinMeshAlt;
                            }
                            else
                            {
                                bodyMesh.mesh = LoadSillyMesh(GoldenGrenade.itemFolder);
                            }
                            if (Config.replaceSFX.Value || sharedSFXgrenadePullPinAlt == null || sharedSFXgrenadeExplode == null)
                            {
                                itemScript.pullPinSFX = LoadReplaceSFX("ExplosionAnticipationSFXShort");
                                itemScript.explodeSFX = LoadReplaceSFX("ExplosionSFX");
                            }
                            else
                            {
                                itemScript.pullPinSFX = sharedSFXgrenadePullPinAlt;
                                itemScript.explodeSFX = sharedSFXgrenadeExplode;
                            }
                        }
                    }
                    else
                    {
                        itemScript.TimeToExplode = 2.25f;
                        itemScript.playerAnimation = "PullGrenadePin";
                        if (Config.sillyScrap.Value)
                        {
                            bodyMesh.mesh = LoadSillyMesh(GoldenGrenade.itemFolder);
                            itemScript.pullPinSFX = LoadSillySFX($"{GoldenGrenade.itemFolder}PullPin");
                            itemScript.explodeSFX = LoadSillySFX($"{GoldenGrenade.itemFolder}Explode");
                        }
                        else
                        {
                            if (goldenGrenadeBodyMesh != null && goldenGrenadePinMesh != null)
                            {
                                bodyMesh.mesh = goldenGrenadeBodyMesh;
                                pinMesh.mesh = goldenGrenadePinMesh;
                            }
                            else
                            {
                                bodyMesh.mesh = LoadSillyMesh(GoldenGrenade.itemFolder);
                            }
                            if (Config.replaceSFX.Value || sharedSFXgrenadePullPin == null || sharedSFXgrenadeExplode == null)
                            {
                                itemScript.pullPinSFX = LoadReplaceSFX("ExplosionAnticipationSFX");
                                itemScript.explodeSFX = LoadReplaceSFX("ExplosionSFX");
                            }
                            else
                            {
                                itemScript.pullPinSFX = sharedSFXgrenadePullPin;
                                itemScript.explodeSFX = sharedSFXgrenadeExplode;
                            }
                        }
                    }
                }                
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Config.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenGlove.itemName)
            {
                if (Config.hostToolRebalance)
                {
                    GoldenGloveScript itemScript = scrapScript.GetComponent<GoldenGloveScript>();
                    itemScript.RebalanceTool();
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Config.hostToolRebalance}");
                }
                continue;
            }
        }
    }
}
