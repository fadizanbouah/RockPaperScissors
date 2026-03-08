using UnityEngine;
using System.Collections;

public class HoundMinion : MinionBase
{
    [Header("Hound Configuration")]
    [SerializeField] private float attackChance = 0.2f; // 20%
    [SerializeField] private float damagePercent = 0.5f; // 50% of parent's damage

    private HandController targetPlayer;
    private bool shouldDealDamage;

    protected override void OnMinionInitialized()
    {
        Debug.Log($"[HoundMinion] Hound ready to attack! Attack chance: {attackChance * 100}%");
    }

    public IEnumerator TryAttack(HandController player)
    {
        float roll = Random.Range(0f, 1f);

        if (roll <= attackChance)
        {
            Debug.Log($"[HoundMinion] {gameObject.name} attacks! (rolled {roll})");

            targetPlayer = player;
            shouldDealDamage = true;

            // Trigger attack animation
            PlayAnimation("Attack");

            // Subscribe to hit event (for damage timing)
            bool hitReceived = false;
            HandController.HoundAttackHitHandler hitCallback = (hand) =>
            {
                hitReceived = true;
                OnDealDamage();
            };
            parentEnemy.HoundAttackHit += hitCallback;

            // Subscribe to finished event (for animation completion)
            bool attackFinished = false;
            HandController.HoundAttackFinishedHandler finishedCallback = (hand) => attackFinished = true;
            parentEnemy.HoundAttackFinished += finishedCallback;

            // Wait for attack animation to complete
            yield return new WaitUntil(() => attackFinished);

            // Unsubscribe
            parentEnemy.HoundAttackHit -= hitCallback;
            parentEnemy.HoundAttackFinished -= finishedCallback;

            Debug.Log("[HoundMinion] Attack animation finished");

            targetPlayer = null;
            shouldDealDamage = false;
        }
        else
        {
            Debug.Log($"[HoundMinion] {gameObject.name} missed (rolled {roll})");
        }
    }

    private void OnDealDamage()
    {
        if (!shouldDealDamage || targetPlayer == null)
        {
            Debug.LogWarning("[HoundMinion] OnDealDamage called but no valid target!");
            return;
        }

        if (parentEnemy != null)
        {
            // Calculate average damage from rock, paper, scissors
            float averageDamage = (parentEnemy.rockDamage + parentEnemy.paperDamage + parentEnemy.scissorsDamage) / 3f;
            int damage = Mathf.RoundToInt(averageDamage * damagePercent);

            targetPlayer.TakeDamage(damage, parentEnemy);
            Debug.Log($"[HoundMinion] Dealt {damage} damage to player (based on average damage: {averageDamage})");
        }
    }
}