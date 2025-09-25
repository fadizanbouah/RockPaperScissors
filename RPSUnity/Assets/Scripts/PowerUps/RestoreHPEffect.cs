using UnityEngine;

public class RestoreHPEffect : PowerUpEffectBase
{
    [Header("Configuration")]
    [SerializeField] private float healPercentage = 30f; // Default 30% heal
    [SerializeField] private bool healBasedOnMax = true; // true = % of max HP, false = % of current HP
    [SerializeField] private int minimumHeal = 5; // Minimum HP to restore
    [SerializeField] private bool showHealingVFX = true;

    private bool effectUsed = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // Use the value from ScriptableObject if provided
        if (data.value > 0)
        {
            healPercentage = data.value;
        }

        Debug.Log($"[RestoreHPEffect] Initialized with {healPercentage}% healing");
    }

    public override void OnRoomStart()
    {
        // Apply healing immediately when activated
        ApplyHealing();
    }

    private void ApplyHealing()
    {
        if (effectUsed || player == null) return;

        // Calculate heal amount
        int healAmount;
        if (healBasedOnMax)
        {
            healAmount = Mathf.RoundToInt(player.maxHealth * (healPercentage / 100f));
        }
        else
        {
            healAmount = Mathf.RoundToInt(player.CurrentHealth * (healPercentage / 100f));
        }

        // Apply minimum heal threshold
        healAmount = Mathf.Max(healAmount, minimumHeal);

        // Calculate actual healing (can't exceed max health)
        int previousHealth = player.CurrentHealth;
        int newHealth = Mathf.Min(player.CurrentHealth + healAmount, player.maxHealth);
        int actualHealing = newHealth - previousHealth;

        // Apply the healing
        player.health = newHealth;
        player.UpdateHealthBar();

        Debug.Log($"[RestoreHPEffect] Healed player for {actualHealing} HP (from {previousHealth} to {newHealth})");

        // Show floating text for healing
        if (showHealingVFX && actualHealing > 0)
        {
            SpawnHealingText(actualHealing);
        }

        effectUsed = true;

        // Remove icon from tracker since it's a one-time effect
        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.RemoveActiveEffect(this);
        }

        // Remove from manager
        PowerUpEffectManager.Instance?.RemoveEffect(this);
    }

    private void SpawnHealingText(int amount)
    {
        if (player == null) return;

        // Try to find combat text prefab on player
        GameObject combatTextPrefab = player.combatTextPrefab;

        if (combatTextPrefab != null)
        {
            GameObject instance = Instantiate(combatTextPrefab, player.transform.position, Quaternion.identity);
            var textComponent = instance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "+" + amount.ToString();
                textComponent.color = Color.green; // Green for healing
            }
        }
    }

    public override void Cleanup()
    {
        Debug.Log("[RestoreHPEffect] Cleanup called");
    }

    public override bool IsEffectActive()
    {
        return !effectUsed;
    }
}