using UnityEngine;
using BepInEx.Logging;
using HarmonyLib;

public class GoldmineTrigger : MonoBehaviour
{
    private static ManualLogSource Logger = Plugin.Logger;

    public GoldmineScript mainScript;

    private void OnTriggerEnter(Collider other)
    {
        if (!mainScript.IsServer)
        {
            return;
        }
        if (other.CompareTag("Enemy"))
        {
            EnemyAICollisionDetect enemy = other.gameObject.GetComponentInChildren<EnemyAICollisionDetect>();
            if (enemy != null && !enemy.mainScript.isEnemyDead && mainScript.activated && !mainScript.isHeld && mainScript.hasHitGround)
            {
                Logger.LogDebug($"trigger entered: {enemy.mainScript.gameObject.name} #{enemy.mainScript.NetworkObjectId}");
                SetOffMine(0.33f, false);
            }
        }
    }

    public void SetOffMine(float delay, bool waitToHitGround)
    {
        mainScript.DelayedExplosionClientRpc(delay, waitToHitGround);
    }

    [HarmonyPatch(typeof(Landmine), "SpawnExplosion")]
    public class NewLandmineExplosion
    {
        [HarmonyPostfix]
        public static void SpawnExplosionPostfix(Vector3 explosionPosition)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || !GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
            {
                return;
            }
            foreach (GoldmineTrigger goldmineTrigger in FindObjectsOfType<GoldmineTrigger>())
            {
                if (goldmineTrigger.mainScript.hasExploded || goldmineTrigger.mainScript.willExplodeImmediately || goldmineTrigger.mainScript.willExplodeOnDelay)
                {
                    continue;
                }
                float distance = Vector3.Distance(explosionPosition, goldmineTrigger.transform.position);
                Logger.LogDebug(distance);
                if (distance < 5f)
                {
                    bool lineIntersected = Physics.Linecast(explosionPosition + Vector3.up * 0.5f, goldmineTrigger.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
                    Logger.LogDebug(lineIntersected);
                    if (!lineIntersected)
                    {
                        Logger.LogDebug("OTHER EXPLOSION SET OFF GOLDMINE!!");
                        int tenthRandom = Random.Range(3, 10);
                        goldmineTrigger.SetOffMine((float)tenthRandom / 10, false);
                    }
                }
            }
        }
    }
}
