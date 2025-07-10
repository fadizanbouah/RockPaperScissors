using UnityEngine;

public class PassiveIncreaseDamageEffect : PowerUpEffectBase
{
    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null || sourceData == null) return;

        int amount;

        // Check if this is an upgradeable power-up
        if (sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            Debug.Log($"[PassiveIncreaseDamage] Current level: {currentLevel}");
            amount = Mathf.RoundToInt(sourceData.GetValueForLevel(currentLevel));
            Debug.Log($"[PassiveIncreaseDamage] Using level {currentLevel} value: {amount}");
        }
        else
        {
            amount = Mathf.RoundToInt(sourceData.value);
        }

        PlayerProgressData.Instance.bonusBaseDamage += amount;
        Debug.Log($"[Passive] Base damage permanently increased by {amount} ({sourceData.powerUpName})");
    }
}
