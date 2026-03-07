using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy attacks the player for a percentage of their average damage when reaching HP thresholds.
/// Triggers at 75%, 50%, and 25% HP.
/// </summary>
public class MurderousIntentBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Configuration")]
    [SerializeField] private float damagePercent = 100f; // Percentage of average damage to deal

    private HandController thisEnemy;
    private HandController player;

    // Track which thresholds have been triggered
    private bool triggered75 = false;
    private bool triggered50 = false;
    private bool triggered25 = false;

    // HP thresholds
    private const float THRESHOLD_75 = 0.75f;
    private const float THRESHOLD_50 = 0.50f;
    private const float THRESHOLD_25 = 0.25f;

    public void Initialize(HandController enemy, float[] configValues)
    {
        thisEnemy = enemy;

        // Get damage percentage from config if provided
        if (configValues != null && configValues.Length > 0)
        {
            damagePercent = configValues[0];
        }

        Debug.Log($"[MurderousIntent] Initialized with {damagePercent}% damage on thresholds");

        // Get player reference
        player = PowerUpEffectManager.Instance?.GetPlayer();
        if (player == null)
        {
            Debug.LogError("[MurderousIntent] Could not find player!");
        }
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        // Check HP thresholds after damage is dealt to enemy
        float hpPercent = (float)thisEnemy.health / (float)thisEnemy.maxHealth;

        Debug.Log($"[MurderousIntent] Enemy HP: {thisEnemy.health}/{thisEnemy.maxHealth} ({hpPercent * 100}%)");

        // Check each threshold (from highest to lowest to avoid double-triggering)
        if (!triggered75 && hpPercent <= THRESHOLD_75)
        {
            triggered75 = true;
            Debug.Log("[MurderousIntent] 75% threshold triggered!");
            yield return TriggerAttack();
        }
        else if (!triggered50 && hpPercent <= THRESHOLD_50)
        {
            triggered50 = true;
            Debug.Log("[MurderousIntent] 50% threshold triggered!");
            yield return TriggerAttack();
        }
        else if (!triggered25 && hpPercent <= THRESHOLD_25)
        {
            triggered25 = true;
            Debug.Log("[MurderousIntent] 25% threshold triggered!");
            yield return TriggerAttack();
        }

        yield return null;
    }

    private IEnumerator TriggerAttack()
    {
        if (player == null)
        {
            Debug.LogError("[MurderousIntent] Player is null, cannot attack!");
            yield break;
        }

        // Calculate average damage
        float averageDamage = (thisEnemy.rockDamage + thisEnemy.paperDamage + thisEnemy.scissorsDamage) / 3f;
        int attackDamage = Mathf.RoundToInt(averageDamage * (damagePercent / 100f));

        Debug.Log($"[MurderousIntent] Attacking for {attackDamage} damage (average: {averageDamage}, percent: {damagePercent}%)");

        // Apply damage (dodge is handled inside TakeDamage automatically)
        player.TakeDamage(attackDamage, thisEnemy);

        // Note: Combat text is shown by TakeDamage itself, and dodge animation plays if dodged
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        yield break;
    }

    private void OnDestroy()
    {
        Debug.Log("[MurderousIntent] Destroyed");
    }
}