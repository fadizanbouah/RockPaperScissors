using UnityEngine;
using System.Collections;

public class HoundMinion : MinionBase
{
    [Header("Hound Configuration")]
    [SerializeField] private float attackChance = 0.2f; // 20%
    [SerializeField] private float damagePercent = 0.5f; // 50% of parent's damage

    [Header("Animation Timing")]
    [Tooltip("Total duration of attack animation in seconds")]
    [SerializeField] private float attackAnimationDuration = 2.5f;

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

            // Wait for full attack animation to complete
            yield return new WaitForSeconds(attackAnimationDuration);

            // Animation should automatically transition back to Idle via Animator

            targetPlayer = null;
            shouldDealDamage = false;
        }
        else
        {
            Debug.Log($"[HoundMinion] {gameObject.name} missed (rolled {roll})");
        }
    }

    public void OnDealDamage()
    {
        if (!shouldDealDamage || targetPlayer == null)
        {
            Debug.LogWarning("[HoundMinion] OnDealDamage called but no valid target!");
            return;
        }

        if (parentEnemy != null)
        {
            int damage = Mathf.RoundToInt(parentEnemy.maxHealth * damagePercent);
            targetPlayer.TakeDamage(damage);
            Debug.Log($"[HoundMinion] Dealt {damage} damage to player");
        }
    }
}