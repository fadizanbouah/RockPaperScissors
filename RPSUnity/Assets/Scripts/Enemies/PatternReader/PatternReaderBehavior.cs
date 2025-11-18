using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternReaderBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Pattern Detection")]
    [SerializeField] private int requiredConsecutiveSigns = 3; // X times in a row

    [Header("Punishment Type")]
    [SerializeField] private PunishmentType punishmentType = PunishmentType.DamagePercent;

    [Header("Punishment Configuration")]
    [Tooltip("For DamagePercent: % of player's max HP (e.g., 20 = 20%)")]
    [SerializeField] private float punishmentValue = 20f;

    private HandController enemyHand;
    private HandController playerHand;

    // Tracking state
    private string lastPlayerSign = "";
    private int consecutiveCount = 0;

    public enum PunishmentType
    {
        DamagePercent,      // Deal X% of player's max HP as damage
        ReducePlayerDamage, // Player deals -X% damage next hit
        IncreaseEnemyDamage // Enemy deals +X% damage next hit
        // Add more types here later
    }

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        // configValues[0] = requiredConsecutiveSigns
        // configValues[1] = punishmentValue
        // configValues[2] = punishmentType (cast to int)
        if (configValues != null && configValues.Length > 0)
        {
            if (configValues.Length > 0)
                requiredConsecutiveSigns = Mathf.RoundToInt(configValues[0]);

            if (configValues.Length > 1)
                punishmentValue = configValues[1];

            if (configValues.Length > 2)
                punishmentType = (PunishmentType)Mathf.RoundToInt(configValues[2]);
        }

        Debug.Log($"[PatternReaderBehavior] Initialized with {requiredConsecutiveSigns} consecutive signs required, punishment: {punishmentType} ({punishmentValue})");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // Store player reference for later use
        if (playerHand == null)
        {
            playerHand = player;
        }

        // Track the player's sign pattern
        if (playerChoice == lastPlayerSign)
        {
            consecutiveCount++;
            Debug.Log($"[PatternReaderBehavior] Player repeated {playerChoice}! Count: {consecutiveCount}/{requiredConsecutiveSigns}");
        }
        else
        {
            // Different sign - reset the counter
            Debug.Log($"[PatternReaderBehavior] Player switched from {lastPlayerSign} to {playerChoice}. Resetting counter.");
            consecutiveCount = 1; // Start counting this new sign
            lastPlayerSign = playerChoice;
        }

        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        // Check if enemy is dead before punishing
        if (enemyHand == null || enemyHand.CurrentHealth <= 0)
        {
            Debug.Log("[PatternReaderBehavior] Enemy is dead - skipping punishment");
            yield break;
        }

        // Check if player has triggered the punishment
        if (consecutiveCount >= requiredConsecutiveSigns)
        {
            Debug.Log($"[PatternReaderBehavior] Pattern detected! Player used {lastPlayerSign} {consecutiveCount} times. Activating punishment: {punishmentType}");

            // Placeholder for punishment animation
            yield return PlayPunishmentAnimation();

            // Apply the punishment
            ApplyPunishment(player);

            // Reset counter after punishment
            consecutiveCount = 0;
            lastPlayerSign = "";
            Debug.Log("[PatternReaderBehavior] Counter reset after punishment");
        }

        yield return null;
    }

    private IEnumerator PlayPunishmentAnimation()
    {
        // TODO: Add animation similar to RobinGood's steal animation
        // For now, just a placeholder delay
        Debug.Log("[PatternReaderBehavior] Playing punishment animation (placeholder)");
        yield return new WaitForSeconds(0.5f);
    }

    private void ApplyPunishment(HandController player)
    {
        if (player == null)
        {
            Debug.LogWarning("[PatternReaderBehavior] Cannot apply punishment - player is null!");
            return;
        }

        switch (punishmentType)
        {
            case PunishmentType.DamagePercent:
                ApplyDamagePercentPunishment(player);
                break;

            case PunishmentType.ReducePlayerDamage:
                ApplyReducePlayerDamagePunishment(player);
                break;

            case PunishmentType.IncreaseEnemyDamage:
                ApplyIncreaseEnemyDamagePunishment(player);
                break;

            default:
                Debug.LogWarning($"[PatternReaderBehavior] Unknown punishment type: {punishmentType}");
                break;
        }
    }

    private void ApplyDamagePercentPunishment(HandController player)
    {
        // Calculate damage as percentage of player's max HP
        int damage = Mathf.RoundToInt(player.maxHealth * (punishmentValue / 100f));
        damage = Mathf.Max(1, damage); // At least 1 damage

        Debug.Log($"[PatternReaderBehavior] Dealing {damage} damage to player ({punishmentValue}% of {player.maxHealth} max HP)");

        player.TakeDamage(damage, enemyHand);
    }

    private void ApplyReducePlayerDamagePunishment(HandController player)
    {
        // TODO: Implement when you want this effect
        // This would require a temporary debuff system on the player
        Debug.Log($"[PatternReaderBehavior] TODO: Reduce player damage by {punishmentValue}% next hit");
    }

    private void ApplyIncreaseEnemyDamagePunishment(HandController player)
    {
        // TODO: Implement when you want this effect
        // This would require a temporary buff system on the enemy
        Debug.Log($"[PatternReaderBehavior] TODO: Increase enemy damage by {punishmentValue}% next hit");
    }

    private void OnDestroy()
    {
        // Clean up if needed
        Debug.Log("[PatternReaderBehavior] Destroyed");
    }
}