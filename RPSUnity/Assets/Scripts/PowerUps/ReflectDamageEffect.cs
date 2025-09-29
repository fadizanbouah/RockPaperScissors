using UnityEngine;

public class ReflectDamageEffect : PowerUpEffectBase
{
    [Header("Configuration")]
    [SerializeField] private float reflectPercentage = 30f; // Default 30% reflection
    [SerializeField] private bool showReflectVFX = true;
    [SerializeField] private Color reflectTextColor = new Color(1f, 0.5f, 0f); // Orange for reflected damage

    private bool effectUsed = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // Use the value from ScriptableObject if provided
        if (data.value > 0)
        {
            reflectPercentage = data.value;
        }

        Debug.Log($"[ReflectDamageEffect] Initialized with {reflectPercentage}% damage reflection");
    }

    public override void ModifyIncomingDamage(ref int damage, HandController source)
    {
        // Only apply if:
        // 1. Effect hasn't been used yet
        // 2. The damage source is not the player (i.e., it's from an enemy)
        // 3. There's actually damage to reflect
        if (!effectUsed && source != player && source != null && damage > 0)
        {
            // Calculate reflected damage
            int reflectedDamage = Mathf.RoundToInt(damage * (reflectPercentage / 100f));

            // Ensure at least 1 damage is reflected if any damage was taken
            if (reflectedDamage < 1 && damage > 0)
            {
                reflectedDamage = 1;
            }

            Debug.Log($"[ReflectDamageEffect] Reflecting {reflectedDamage} damage back to {source.name} ({reflectPercentage}% of {damage})");

            effectUsed = true;

            // Deal damage back to the attacker
            if (reflectedDamage > 0)
            {
                // Use a coroutine to apply damage after a short delay for visual clarity
                // AND to handle cleanup after the damage is applied
                StartCoroutine(ApplyReflectedDamageAndCleanup(source, reflectedDamage));
            }
            else
            {
                // If no damage to reflect, just cleanup
                CleanupEffect();
            }
        }
    }

    private System.Collections.IEnumerator ApplyReflectedDamageAndCleanup(HandController target, int damage)
    {
        // Small delay so the reflected damage is visually distinct from the original hit
        yield return new WaitForSeconds(0.3f);

        if (target != null && target.CurrentHealth > 0)
        {
            // Apply the reflected damage
            target.TakeDamage(damage, player);

            // Show visual feedback if enabled
            if (showReflectVFX)
            {
                SpawnReflectText(target, damage);
            }

            Debug.Log($"[ReflectDamageEffect] Applied {damage} reflected damage to {target.name}");
        }

        // NOW cleanup after the damage has been applied
        CleanupEffect();
    }

    private void CleanupEffect()
    {
        // Remove icon from tracker
        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.RemoveActiveEffect(this);
        }

        // Remove from manager - this will destroy the GameObject
        PowerUpEffectManager.Instance?.RemoveEffect(this);
    }

    private void SpawnReflectText(HandController target, int damage)
    {
        if (target == null) return;

        // Try to find combat text prefab
        GameObject combatTextPrefab = target.combatTextPrefab;

        if (combatTextPrefab != null)
        {
            GameObject instance = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity);
            var textComponent = instance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "-" + damage.ToString() + " (Reflected)";
                textComponent.color = reflectTextColor;
            }
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        // Optional: You could reset the effect if the player wins without taking damage
        // This would make it persist until actually triggered
        // Currently it's one-time use regardless
    }

    public override void Cleanup()
    {
        Debug.Log("[ReflectDamageEffect] Cleanup called");
    }

    public override bool IsEffectActive()
    {
        return !effectUsed;
    }
}