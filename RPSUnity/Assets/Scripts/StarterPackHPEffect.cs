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

        // If we're upgrading, remove the old bonus first
        if (hasAppliedHealthBonus && lastAppliedLevel < currentLevel)
        {
            Debug.Log($"[StarterPackHPEffect] Upgrading from level {lastAppliedLevel} to {currentLevel}");
            RemovePreviousHealthBonus();
        }

        ApplyHealthBonus(currentHealthPercentage);
        hasAppliedHealthBonus = true;
        lastAppliedLevel = currentLevel;
    }

    private void RemovePreviousHealthBonus()
    {
        // Get player reference
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackHPEffect] Cannot remove previous bonus - player is null!");
            return;
        }

        // Get the previous level's percentage
        float previousHealthPercentage = healthPercentage;
        if (sourceData != null && sourceData.isUpgradeable && lastAppliedLevel >= 0)
        {
            previousHealthPercentage = sourceData.GetValueForLevel(lastAppliedLevel);
        }

        // Calculate and remove the previous bonus
        int previousHealthIncrease = Mathf.RoundToInt(activePlayer.baseMaxHealth * (previousHealthPercentage / 100f));

        activePlayer.maxHealth -= previousHealthIncrease;
        activePlayer.health = Mathf.Min(activePlayer.health, activePlayer.maxHealth); // Clamp current health
        activePlayer.UpdateHealthBar();

        Debug.Log($"[StarterPackHPEffect] Removed previous bonus: -{previousHealthIncrease} max health");
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

        // Calculate health increase based on base max health
        int healthIncrease = Mathf.RoundToInt(activePlayer.baseMaxHealth * (percentage / 100f));

        // Increase max health
        activePlayer.maxHealth += healthIncrease;

        // Also heal the player by the same amount (so they get the benefit immediately)
        activePlayer.health = Mathf.Min(activePlayer.health + healthIncrease, activePlayer.maxHealth);
        activePlayer.UpdateHealthBar();

        Debug.Log($"[StarterPackHPEffect] Applied +{healthIncrease} max health ({percentage}% of {activePlayer.baseMaxHealth})");
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