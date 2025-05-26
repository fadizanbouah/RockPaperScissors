using UnityEngine;

public static class PassivePowerUpHandler
{
    public static void ApplyAllPersistentPowerUps()
    {
        if (RunProgressManager.Instance == null || PlayerProgressData.Instance == null) return;

        // Reset passive bonuses before applying new ones
        PlayerProgressData.Instance.bonusBaseDamage = 0;
        PlayerProgressData.Instance.bonusRockDamage = 0;
        PlayerProgressData.Instance.bonusPaperDamage = 0;
        PlayerProgressData.Instance.bonusScissorsDamage = 0;

        foreach (PowerUp powerUp in RunProgressManager.Instance.persistentPowerUps)
        {
            switch (powerUp.type)
            {
                case PowerUpType.PassiveIncreaseDamage:
                    PlayerProgressData.Instance.bonusBaseDamage += Mathf.RoundToInt(powerUp.effectValue);
                    break;
                case PowerUpType.PassiveIncreaseRockDamage:
                    PlayerProgressData.Instance.bonusRockDamage += Mathf.RoundToInt(powerUp.effectValue);
                    break;
                case PowerUpType.PassiveIncreasePaperDamage:
                    PlayerProgressData.Instance.bonusPaperDamage += Mathf.RoundToInt(powerUp.effectValue);
                    break;
                case PowerUpType.PassiveIncreaseScissorsDamage:
                    PlayerProgressData.Instance.bonusScissorsDamage += Mathf.RoundToInt(powerUp.effectValue);
                    break;
                default:
                    Debug.LogWarning($"[PassivePowerUpHandler] Unsupported passive power-up type: {powerUp.type}");
                    break;
            }

            Debug.Log($"[PassivePowerUpHandler] Applied passive power-up: {powerUp.powerUpName} ({powerUp.type})");
        }
    }
}
