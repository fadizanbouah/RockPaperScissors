using UnityEngine;

public class StarterPackHPEffect : PowerUpEffectBase
{
    [Header("Health Configuration")]
    [SerializeField] private float healthPercentage = 20f; // 20% max health increase

    [Header("Visual Feedback")]
    [SerializeField] private bool showNotifications = true;

    private static bool hasAppliedHealthBonus = false;
    private static bool hasShownNotification = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[StarterPackHPEffect] Initialized for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        // Only apply health bonus once per run
        if (hasAppliedHealthBonus)
        {
            Debug.Log("[StarterPackHPEffect] Health bonus already applied this run - skipping");
            return;
        }

        Debug.Log("[StarterPackHPEffect] Applying health bonus...");

        // Get the appropriate percentage based on upgrade level
        float currentHealthPercentage = healthPercentage;

        if (sourceData != null && sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            currentHealthPercentage = sourceData.GetValueForLevel(currentLevel);
            Debug.Log($"[StarterPackHPEffect] Using level {currentLevel} value: {currentHealthPercentage}%");
        }

        ApplyHealthBonus(currentHealthPercentage);
        hasAppliedHealthBonus = true;
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

        // Show notification once
        if (showNotifications && !hasShownNotification)
        {
            ShowNotification($"+{percentage}% Max HP!", activePlayer);
            hasShownNotification = true;
        }
    }

    private void ShowNotification(string message, HandController targetPlayer)
    {
        Debug.Log($"[StarterPackHPEffect Notification] {message}");

        if (targetPlayer != null && targetPlayer.combatTextPrefab != null)
        {
            GameObject textInstance = Instantiate(targetPlayer.combatTextPrefab, targetPlayer.transform.position + Vector3.up, Quaternion.identity);
            var textComponent = textInstance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = Color.green; // Green for health
                textComponent.fontSize = 24;
            }
        }
    }

    public override void Cleanup()
    {
        hasAppliedHealthBonus = false;
        hasShownNotification = false;
        Debug.Log("[StarterPackHPEffect] Cleanup - reset flags");
    }

    public static void ResetForNewRun()
    {
        hasAppliedHealthBonus = false;
        hasShownNotification = false;
        Debug.Log("[StarterPackHPEffect] Reset for new run");
    }
}