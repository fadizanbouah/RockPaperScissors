using UnityEngine;

public class StarterPackHPEffect : PowerUpEffectBase
{
    [Header("Health Configuration")]
    [SerializeField] private float healthPercentage = 20f; // 20% max health increase

    private static bool hasAppliedHealthBonus = false;
    private static int lastAppliedLevel = -1; // Track which level was last applied

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[StarterPackHPEffect] Initialized for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        Debug.Log("[StarterPackHPEffect] Checking if health bonus needs to be applied...");

        // Get the current level
        int currentLevel = 0;
        float currentHealthPercentage = healthPercentage;

        if (sourceData != null && sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            currentHealthPercentage = sourceData.GetValueForLevel(currentLevel);
            Debug.Log($"[StarterPackHPEffect] Current level: {currentLevel}, percentage: {currentHealthPercentage}%");
        }

        // Check if this level has already been applied
        if (hasAppliedHealthBonus && lastAppliedLevel >= currentLevel)
        {
            Debug.Log($"[StarterPackHPEffect] Level {currentLevel} already applied (last applied: {lastAppliedLevel}) - skipping");
            return;
        }

        // If we're upgrading, just apply the difference (handled in ApplyHealthBonus)
        if (hasAppliedHealthBonus && lastAppliedLevel < currentLevel)
        {
            Debug.Log($"[StarterPackHPEffect] Upgrading from level {lastAppliedLevel} to {currentLevel}");
        }

        ApplyHealthBonus(currentHealthPercentage);
        hasAppliedHealthBonus = true;
        lastAppliedLevel = currentLevel;
    }

    private void ApplyHealthBonus(float percentage)
    {
        // Get player reference
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackHPEffect] Cannot apply health bonus - player is null!");
            return;
        }

        // Calculate the NEW health increase
        int newHealthIncrease = Mathf.RoundToInt(activePlayer.baseMaxHealth * (percentage / 100f));

        // Calculate the OLD health increase (if upgrading)
        int oldHealthIncrease = 0;
        if (hasAppliedHealthBonus && lastAppliedLevel >= 0)
        {
            float previousHealthPercentage = healthPercentage;
            if (sourceData != null && sourceData.isUpgradeable)
            {
                previousHealthPercentage = sourceData.GetValueForLevel(lastAppliedLevel);
            }
            oldHealthIncrease = Mathf.RoundToInt(activePlayer.baseMaxHealth * (previousHealthPercentage / 100f));
        }

        // Calculate the DIFFERENCE (how much extra we're adding)
        int healthDifference = newHealthIncrease - oldHealthIncrease;

        // Increase max health by the difference
        activePlayer.maxHealth += healthDifference;

        // Heal the player ONLY by the difference
        activePlayer.health = Mathf.Min(activePlayer.health + healthDifference, activePlayer.maxHealth);

        activePlayer.UpdateHealthBar();

        Debug.Log($"[StarterPackHPEffect] Applied +{healthDifference} max health (difference from {oldHealthIncrease} to {newHealthIncrease})");
        Debug.Log($"[StarterPackHPEffect] New max health: {activePlayer.maxHealth}");
    }

    public override void Cleanup()
    {
        hasAppliedHealthBonus = false;
        lastAppliedLevel = -1;
        Debug.Log("[StarterPackHPEffect] Cleanup - reset flags");
    }

    public static void ResetForNewRun()
    {
        hasAppliedHealthBonus = false;
        lastAppliedLevel = -1;
        Debug.Log("[StarterPackHPEffect] Reset for new run");
    }
}