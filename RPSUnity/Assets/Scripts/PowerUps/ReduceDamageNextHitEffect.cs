using UnityEngine;

public class ReduceDamageNextHitEffect : PowerUpEffectBase
{
    [Header("Configuration")]
    [SerializeField] private float damageReductionPercentage = 50f; // Exposed for configuration

    private bool effectUsed = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // Use the value from ScriptableObject if provided, otherwise use default
        if (data.value > 0)
        {
            damageReductionPercentage = data.value;
        }

        Debug.Log($"[ReduceDamageNextHitEffect] Initialized with {damageReductionPercentage}% damage reduction for next enemy hit");
    }

    public override void ModifyIncomingDamage(ref int damage, HandController source)
    {
        // Only apply if:
        // 1. Effect hasn't been used yet
        // 2. The damage source is not the player (i.e., it's from an enemy)
        // 3. There's actually damage to reduce
        if (!effectUsed && source != player && damage > 0)
        {
            float reduction = damageReductionPercentage / 100f;
            int originalDamage = damage;
            damage = Mathf.RoundToInt(damage * (1f - reduction));

            Debug.Log($"[ReduceDamageNextHitEffect] Reduced incoming damage from {originalDamage} to {damage} ({damageReductionPercentage}% reduction)");

            effectUsed = true;

            // Optional: Add visual feedback here
            // You could trigger a shield effect or special animation
            // Remove icon from tracker
            PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
            if (tracker != null)
            {
                tracker.RemoveActiveEffect(this);
            }

            // Remove from manager
            PowerUpEffectManager.Instance?.RemoveEffect(this);
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        // If the player won or drew, the effect wasn't used, so it carries over
        // If the player lost and effect was used, it's already marked as used
        if (effectUsed)
        {
            Debug.Log($"[ReduceDamageNextHitEffect] Effect was used this round");
        }
    }

    public override void Cleanup()
    {
        Debug.Log("[ReduceDamageNextHitEffect] Cleanup called");
    }
}