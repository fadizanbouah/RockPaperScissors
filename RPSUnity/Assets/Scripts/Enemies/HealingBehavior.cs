using System.Collections;
using UnityEngine;

/// <summary>
/// Healing fountain/object behavior.
/// When destroyed, heals the player by a percentage of their max health.
/// </summary>
public class HealingBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Healing Configuration")]
    [Tooltip("Percentage of player's max health to restore (0-100)")]
    [SerializeField] private float healPercentage = 50f; // 50% by default

    private HandController thisObject;
    private bool hasHealed = false;

    public void Initialize(HandController enemy, float[] configValues)
    {
        thisObject = enemy;

        // Get heal percentage from config if provided
        if (configValues != null && configValues.Length > 0)
        {
            healPercentage = configValues[0];
        }

        Debug.Log($"[HealingBehavior] Initialized with {healPercentage}% healing");

        // Subscribe to death event
        if (thisObject != null)
        {
            thisObject.OnDeath += OnObjectDestroyed;
        }
    }

    private void OnObjectDestroyed(HandController obj)
    {
        if (hasHealed) return;

        Debug.Log($"[HealingBehavior] Healing object destroyed!");

        // Get player reference
        HandController player = PowerUpEffectManager.Instance?.GetPlayer();
        if (player == null)
        {
            Debug.LogError("[HealingBehavior] Could not find player to heal!");
            return;
        }

        // Calculate heal amount
        int healAmount = Mathf.RoundToInt(player.maxHealth * (healPercentage / 100f));
        healAmount = Mathf.Max(healAmount, 1); // At least 1 HP

        // Heal the player
        int oldHealth = player.health;
        player.health = Mathf.Min(player.health + healAmount, player.maxHealth);
        int actualHealed = player.health - oldHealth;

        // Update health bar
        player.UpdateHealthBar();

        Debug.Log($"[HealingBehavior] Healed player for {actualHealed} HP ({healPercentage}% of {player.maxHealth})");

        hasHealed = true;

        // TODO: Play healing VFX/animation here if you want
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        yield return null;
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        yield break;
    }

    private void OnDestroy()
    {
        if (thisObject != null)
        {
            thisObject.OnDeath -= OnObjectDestroyed;
        }

        Debug.Log("[HealingBehavior] Destroyed");
    }
}