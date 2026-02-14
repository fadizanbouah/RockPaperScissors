using System.Collections;
using UnityEngine;

public class HoundMasterBehavior : MonoBehaviour, IEnemyBehavior
{
    private MinionController minionController;
    private HandController enemyHand;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        Debug.Log($"[HoundMasterBehavior] Initialize called with enemy: {enemy.name}");

        minionController = enemy.GetComponent<MinionController>();

        if (minionController == null)
        {
            Debug.LogError($"[HoundMasterBehavior] ERROR: No MinionController found on {enemy.name}!");
        }
        else
        {
            Debug.Log($"[HoundMasterBehavior] MinionController found successfully");
        }

        Debug.Log("[HoundMasterBehavior] Initialized");
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        Debug.Log($"[HoundMasterBehavior] OnAfterDamageResolved called. Result: {result}");

        if (minionController == null)
        {
            Debug.LogError("[HoundMasterBehavior] ERROR: minionController is NULL in OnAfterDamageResolved!");
            yield break;
        }

        // NEW: Don't attack if enemy is already dead
        if (enemyHand == null || enemyHand.health <= 0)
        {
            Debug.Log("[HoundMasterBehavior] Enemy is dead, hounds will not attack");
            yield break;
        }

        // Only trigger if player didn't already lose this round
        if (result == RoundResult.Win || result == RoundResult.Draw)
        {
            var minions = minionController.GetActiveMinions();

            Debug.Log($"[HoundMasterBehavior] Player survived round, {minions.Count} hounds will attempt to attack");

            // Each hound tries to attack sequentially
            foreach (var minionObj in minions)
            {
                if (minionObj == null)
                {
                    Debug.LogWarning("[HoundMasterBehavior] Minion object is null, skipping");
                    continue;
                }

                HoundMinion hound = minionObj.GetComponent<HoundMinion>();
                if (hound != null)
                {
                    Debug.Log($"[HoundMasterBehavior] Triggering attack for: {minionObj.name}");
                    yield return hound.TryAttack(player);
                }
                else
                {
                    Debug.LogWarning($"[HoundMasterBehavior] No HoundMinion component found on {minionObj.name}");
                }
            }
        }
        else
        {
            Debug.Log("[HoundMasterBehavior] Player lost round, hounds do not attack");
        }
    }

    private void Awake()
    {
        Debug.Log("[HoundMasterBehavior] Awake called");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        yield break;
    }
}