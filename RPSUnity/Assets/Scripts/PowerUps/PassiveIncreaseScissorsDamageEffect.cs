using UnityEngine;

public class PassiveIncreaseScissorsDamageEffect : PowerUpEffectBase
{
    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null || sourceData == null) return;

        int amount;

        // Check if this is an upgradeable power-up
        if (sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            amount = Mathf.RoundToInt(sourceData.GetValueForLevel(currentLevel));
        }
        else
        {
            // Fallback to base value for non-upgradeable power-ups
            amount = Mathf.RoundToInt(sourceData.value);
        }

        // For Rock damage:
        PlayerProgressData.Instance.bonusScissorsDamage += amount;
        // For Paper: use bonusPaperDamage
        // For Scissors: use bonusScissorsDamage

        Debug.Log($"[Passive] Scissors damage permanently increased by {amount} ({sourceData.powerUpName})");
    }
}
