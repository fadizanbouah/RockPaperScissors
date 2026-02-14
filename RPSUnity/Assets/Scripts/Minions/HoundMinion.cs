using UnityEngine;
using System.Collections;

/// <summary>
/// Hound minion that can attack independently.
/// Example of a specific minion implementation.
/// </summary>
public class HoundMinion : MinionBase
{
    [Header("Hound Configuration")]
    [SerializeField] private float attackChance = 0.2f; // 20%
    [SerializeField] private float damagePercent = 0.5f; // 50% of parent's damage

    protected override void OnMinionInitialized()
    {
        Debug.Log($"[HoundMinion] Hound ready to attack! Attack chance: {attackChance * 100}%");
    }

    /// <summary>
    /// Attempt to attack the player.
    /// Returns true if attack happened.
    /// </summary>
    public IEnumerator TryAttack(HandController player)
    {
        float roll = Random.Range(0f, 1f);

        if (roll <= attackChance)
        {
            Debug.Log($"[HoundMinion] {gameObject.name} attacks! (rolled {roll})");

            // Play attack animation
            PlayAnimation("Attack");

            // Wait for animation
            yield return new WaitForSeconds(0.8f);

            // Calculate and deal damage
            if (parentEnemy != null)
            {
                int damage = Mathf.RoundToInt(parentEnemy.maxHealth * damagePercent);
                player.TakeDamage(damage);
                Debug.Log($"[HoundMinion] Dealt {damage} damage to player");
            }

            yield return new WaitForSeconds(0.3f); // Brief pause after attack
        }
        else
        {
            Debug.Log($"[HoundMinion] {gameObject.name} missed (rolled {roll})");
        }
    }
}