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

        foreach (PowerUpData data in RunProgressManager.Instance.persistentPowerUps)
        {
            if (data.effectPrefab == null)
            {
                Debug.LogWarning($"[PassivePowerUpHandler] Missing effectPrefab on PowerUpData: {data.powerUpName}");
                continue;
            }

            GameObject instance = GameObject.Instantiate(data.effectPrefab);
            PowerUpEffectBase effect = instance.GetComponent<PowerUpEffectBase>();

            if (effect != null)
            {
                effect.Initialize(data, null, null);
                effect.OnRoomStart(); // Apply effect immediately (e.g., stat boost)

                // IMPORTANT: Register with PowerUpEffectManager so it gets callbacks
                if (PowerUpEffectManager.Instance != null)
                {
                    PowerUpEffectManager.Instance.RegisterEffect(effect);
                    Debug.Log($"[PassivePowerUpHandler] Registered {data.powerUpName} with PowerUpEffectManager");
                }
                else
                {
                    Debug.LogWarning($"[PassivePowerUpHandler] PowerUpEffectManager not found! {data.powerUpName} won't get callbacks");
                }
            }
            else
            {
                Debug.LogWarning($"[PassivePowerUpHandler] No PowerUpEffectBase found on prefab for {data.powerUpName}");
                GameObject.Destroy(instance); // Clean up failed instantiation
            }
        }
    }
}