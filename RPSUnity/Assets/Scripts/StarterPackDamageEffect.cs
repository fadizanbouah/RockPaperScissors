using UnityEngine;

public class StarterPackDamageEffect : PowerUpEffectBase
{
    [Header("Damage Configuration")]
    [SerializeField] private float damagePercentage = 10f; // 10% damage increase

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

        // CHANGED: Calculate damage increase based on CURRENT values (which include upgrades)
        // instead of base values. This gives us the upgraded damage values.
        int rockDamageIncrease = Mathf.RoundToInt(activePlayer.rockDamage * (percentage / 100f));
        int paperDamageIncrease = Mathf.RoundToInt(activePlayer.paperDamage * (percentage / 100f));
        int scissorsDamageIncrease = Mathf.RoundToInt(activePlayer.scissorsDamage * (percentage / 100f));

        // Apply the increases to the appropriate bonus fields
        PlayerProgressData.Instance.bonusRockDamage += rockDamageIncrease;
        PlayerProgressData.Instance.bonusPaperDamage += paperDamageIncrease;
        PlayerProgressData.Instance.bonusScissorsDamage += scissorsDamageIncrease;

        Debug.Log($"[StarterPackDamageEffect] Applied damage bonuses ({percentage}% of CURRENT damage):");
        Debug.Log($"  Rock: +{rockDamageIncrease} ({percentage}% of current {activePlayer.rockDamage})");
        Debug.Log($"  Paper: +{paperDamageIncrease} ({percentage}% of current {activePlayer.paperDamage})");
        Debug.Log($"  Scissors: +{scissorsDamageIncrease} ({percentage}% of current {activePlayer.scissorsDamage})");
    }
}