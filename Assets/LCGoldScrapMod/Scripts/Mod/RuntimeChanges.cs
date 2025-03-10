using UnityEngine;
using BepInEx.Logging;
using static AssetsCollection;
using static RegisterGoldScrap;

public class RuntimeChanges
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static void SyncHostConfigs(bool rebalance, float multiplierWeight, float multiplierPrice)
    {
        Logger.LogInfo($"Applying values of host's config [Multiplier Weight]. Value: {multiplierWeight}");
        foreach (ItemData itemWithWeight in Plugin.allGoldGrabbableObjects)
        {
            itemWithWeight.itemProperties.weight = Configs.CalculateWeightCustom(itemWithWeight.defaultWeight, multiplierWeight);
        }

        Logger.LogInfo($"Applying values of host's config [Multiplier Price]. Value: {multiplierPrice}");
        Configs.SetCustomGoldStorePrices(multiplierPrice);

        Logger.LogInfo($"Applying values of host's config [Other Tools Balance]. Value: {Configs.hostToolRebalance}");
        Configs.hostToolRebalance = rebalance;
        
        GoldenBell.RebalanceTool();
        GoldenGlass.RebalanceTool();
        GoldSign.RebalanceTool(multiplierWeight);
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
        GoldScrapObject[] allGoldScrapObjects = Object.FindObjectsByType<GoldScrapObject>(FindObjectsSortMode.None);
        Logger.LogDebug($"goldScrapItem length: {allGoldScrapObjects.Length}");
        foreach (GoldScrapObject scrapScript in allGoldScrapObjects)
        {
            if (scrapScript.item == null)
            {
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenBell.itemName)
            {
                AnimatedItem itemScript = scrapScript.GetComponent<AnimatedItem>();
                if (Configs.hostToolRebalance)
                {
                    itemScript.noiseRange = 128;
                }
                else
                {
                    itemScript.noiseRange = 64;
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenGlass.itemName)
            {
                if (Configs.hostToolRebalance)
                {
                    Material[] newMats = { defaultMaterialGold, defaultMaterialGold };
                    scrapScript.GetComponent<MeshRenderer>().materials = newMats;
                }
                else
                {
                    Material[] newMatsAlt = { defaultMaterialGold, defaultMaterialGoldTransparent };
                    scrapScript.GetComponent<MeshRenderer>().materials = newMatsAlt;
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldSign.itemName)
            {
                if (!Configs.sillyScrap.Value && Configs.hostToolRebalance && goldSignMeshAlt != null)
                {
                    scrapScript.GetComponent<MeshFilter>().mesh = goldSignMeshAlt;
                }
                else if (!Configs.sillyScrap.Value && !Configs.hostToolRebalance && goldSignMesh != null)
                {
                    scrapScript.GetComponent<MeshFilter>().mesh = goldSignMesh;
                }
                else
                {
                    scrapScript.GetComponent<MeshFilter>().mesh = LoadSillyMesh(GoldSign.itemFolder);
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldRemote.itemName)
            {
                if (Configs.hostToolRebalance)
                {
                    scrapScript.item.useCooldown = 1.5f;
                }
                else
                {
                    scrapScript.item.useCooldown = 0f;
                }
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenGrenade.itemName)
            {
                MeshFilter bodyMesh = scrapScript.transform.GetChild(2).GetComponent<MeshFilter>();
                MeshFilter pinMesh = scrapScript.transform.GetChild(2).GetChild(0).GetComponent<MeshFilter>();
                MeshFilter bodyMeshAlt = scrapScript.transform.GetChild(3).GetComponent<MeshFilter>();
                MeshFilter pinMeshAlt = scrapScript.transform.GetChild(3).GetChild(0).GetComponent<MeshFilter>();
                StunGrenadeItem itemScript = scrapScript.GetComponent<StunGrenadeItem>();

                if (Configs.hostToolRebalance)
                {
                    bodyMesh.mesh = null;
                    pinMesh.mesh = null;
                    bodyMeshAlt.mesh = null;
                    pinMeshAlt.mesh = null;
                    if (Configs.hostToolRebalance)
                    {
                        itemScript.TimeToExplode = 0.18f;
                        itemScript.playerAnimation = "PullGrenadePin2";
                        if (Configs.sillyScrap.Value)
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
                            if (Configs.replaceSFX.Value || sharedSFXgrenadePullPinAlt == null || sharedSFXgrenadeExplode == null)
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
                        if (Configs.sillyScrap.Value)
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
                            if (Configs.replaceSFX.Value || sharedSFXgrenadePullPin == null || sharedSFXgrenadeExplode == null)
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
                Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                continue;
            }
            if (scrapScript.item.itemProperties == GoldenGlove.itemName)
            {
                if (Configs.hostToolRebalance)
                {
                    GoldenGloveScript itemScript = scrapScript.GetComponent<GoldenGloveScript>();
                    itemScript.RebalanceTool();
                    Logger.LogDebug($"Updated prefab {scrapScript.name} #{scrapScript.item.NetworkObjectId} with rebalance {Configs.hostToolRebalance}");
                }
                continue;
            }
        }
    }
}
