using UnityEngine;

public class StarterPackDamageEffect : PowerUpEffectBase
{
    [Header("Damage Configuration")]
    [SerializeField] private float damagePercentage = 10f; // 10% damage increase

    [Header("Visual Feedback")]
    [SerializeField] private bool showNotifications = true;

    private static bool hasShownNotification = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[StarterPackDamageEffect] Initialized for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        Debug.Log("[StarterPackDamageEffect] Applying damage bonus...");

        // Get the appropriate percentage based on upgrade level
        float currentDamagePercentage = damagePercentage;

        if (sourceData != null && sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            currentDamagePercentage = sourceData.GetValueForLevel(currentLevel);
            Debug.Log($"[StarterPackDamageEffect] Using level {currentLevel} value: {currentDamagePercentage}%");
        }

        ApplyDamageBonus(currentDamagePercentage);
    }

    private void ApplyDamageBonus(float percentage)
    {
        if (PlayerProgressData.Instance == null)
        {
            Debug.LogWarning("[StarterPackDamageEffect] PlayerProgressData is null!");
            return;
        }

        // Get player reference
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackDamageEffect] Cannot apply damage bonus - player is null!");
            return;
        }

        // Calculate damage increase based on BASE values (before any upgrades or bonuses)
        int rockDamageIncrease = Mathf.RoundToInt(activePlayer.baseRockDamage * (percentage / 100f));
        int paperDamageIncrease = Mathf.RoundToInt(activePlayer.basePaperDamage * (percentage / 100f));
        int scissorsDamageIncrease = Mathf.RoundToInt(activePlayer.baseScissorsDamage * (percentage / 100f));

        // Apply the increases to the appropriate bonus fields
        PlayerProgressData.Instance.bonusRockDamage += rockDamageIncrease;
        PlayerProgressData.Instance.bonusPaperDamage += paperDamageIncrease;
        PlayerProgressData.Instance.bonusScissorsDamage += scissorsDamageIncrease;

        Debug.Log($"[StarterPackDamageEffect] Applied damage bonuses ({percentage}% of BASE damage):");
        Debug.Log($"  Rock: +{rockDamageIncrease} ({percentage}% of base {activePlayer.baseRockDamage})");
        Debug.Log($"  Paper: +{paperDamageIncrease} ({percentage}% of base {activePlayer.basePaperDamage})");
        Debug.Log($"  Scissors: +{scissorsDamageIncrease} ({percentage}% of base {activePlayer.baseScissorsDamage})");

        // Only show notification once per run
        if (showNotifications && !hasShownNotification)
        {
            ShowNotification($"+{percentage}% All Damage!", activePlayer);
            hasShownNotification = true;
        }
    }

    private void ShowNotification(string message, HandController targetPlayer)
    {
        Debug.Log($"[StarterPackDamageEffect Notification] {message}");

        if (targetPlayer != null && targetPlayer.combatTextPrefab != null)
        {
            GameObject textInstance = Instantiate(targetPlayer.combatTextPrefab, targetPlayer.transform.position + Vector3.up, Quaternion.identity);
            var textComponent = textInstance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = Color.yellow;
                textComponent.fontSize = 24;
            }
        }
    }

    public override void Cleanup()
    {
        hasShownNotification = false;
        Debug.Log("[StarterPackDamageEffect] Cleanup - reset notification flag");
    }

    public static void ResetForNewRun()
    {
        hasShownNotification = false;
        Debug.Log("[StarterPackDamageEffect] Reset for new run");
    }
}