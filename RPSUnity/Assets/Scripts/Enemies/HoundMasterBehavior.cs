using System.Collections;
using UnityEngine;

public class HoundMasterBehavior : MonoBehaviour, IEnemyBehavior
{
    private MinionController minionController;
    private HandController enemyHand;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;
        minionController = enemy.GetComponent<MinionController>();

        if (minionController == null)
        {
            Debug.LogError($"[HoundMasterBehavior] ERROR: No MinionController found on {enemy.name}!");
        }

        Debug.Log("[HoundMasterBehavior] Initialized");
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        if (minionController == null)
        {
            Debug.LogError("[HoundMasterBehavior] ERROR: minionController is NULL in OnAfterDamageResolved!");
            yield break;
        }

        // Don't attack if enemy is already dead
        if (enemyHand == null || enemyHand.health <= 0)
        {
            Debug.Log("[HoundMasterBehavior] Enemy is dead, hounds will not attack");
            yield break;
        }

        // NOTE: Hounds attack even if Cheat Death triggered this round
        // This is intentional - Cheat Death saves you but doesn't prevent the hounds from attacking
        // Adds strategic depth: even with Cheat Death, facing HoundMaster is risky

        var minions = minionController.GetActiveMinions();

        Debug.Log($"[HoundMasterBehavior] {minions.Count} hounds will attempt to attack");

        foreach (var minionObj in minions)
        {
            if (minionObj == null)
            {
                continue;
            }

            HoundMinion hound = minionObj.GetComponent<HoundMinion>();
            if (hound != null)
            {
                yield return hound.TryAttack(player);
            }
        }
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