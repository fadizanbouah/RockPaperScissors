using UnityEngine;

public static class PassivePowerUpHandler
{
    public static void ApplyAllPersistentPowerUps()
    {
        if (RunProgressManager.Instance == null || PlayerProgressData.Instance == null) return;

        Debug.Log($"[PassivePowerUpHandler] ApplyAllPersistentPowerUps called");
        Debug.Log($"[PassivePowerUpHandler] BEFORE reset - bonusBaseDamage: {PlayerProgressData.Instance.bonusBaseDamage}, bonusRockDamage: {PlayerProgressData.Instance.bonusRockDamage}, bonusPaperDamage: {PlayerProgressData.Instance.bonusPaperDamage}, bonusScissorsDamage: {PlayerProgressData.Instance.bonusScissorsDamage}");

        // Reset passive bonuses before applying new ones
        PlayerProgressData.Instance.bonusBaseDamage = 0;
        PlayerProgressData.Instance.bonusRockDamage = 0;
        PlayerProgressData.Instance.bonusPaperDamage = 0;
        PlayerProgressData.Instance.bonusScissorsDamage = 0;

        Debug.Log($"[PassivePowerUpHandler] AFTER reset - bonusBaseDamage: {PlayerProgressData.Instance.bonusBaseDamage}, bonusRockDamage: {PlayerProgressData.Instance.bonusRockDamage}, bonusPaperDamage: {PlayerProgressData.Instance.bonusPaperDamage}, bonusScissorsDamage: {PlayerProgressData.Instance.bonusScissorsDamage}");
        Debug.Log($"[PassivePowerUpHandler] Processing {RunProgressManager.Instance.persistentPowerUps.Count} persistent power-ups");

        foreach (PowerUpData data in RunProgressManager.Instance.persistentPowerUps)
        {
            Debug.Log($"[PassivePowerUpHandler] Processing: {data.powerUpName}");

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

                Debug.Log($"[PassivePowerUpHandler] About to call OnRoomStart for {data.powerUpName}");
                effect.OnRoomStart(); // Apply effect immediately (e.g., stat boost)
                Debug.Log($"[PassivePowerUpHandler] OnRoomStart completed for {data.powerUpName}");

                // SPECIAL CASE: Register effects that need ongoing callbacks with PowerUpEffectManager
                // These effects need OnRoundEnd, ModifyIncomingDamage, etc.
                if (data.powerUpName == "The Gambler" ||
                    effect is GamblerEffect ||
                    effect is IGamblerEffect ||
                    effect is RockDRStackEffect) // NEW: Add RockDRStackEffect
                {
                    if (PowerUpEffectManager.Instance != null)
                    {
                        PowerUpEffectManager.Instance.RegisterEffect(effect);
                        Debug.Log($"[PassivePowerUpHandler] Registered {data.powerUpName} with PowerUpEffectManager (needs callbacks)");
                    }
                    else
                    {
                        Debug.LogWarning($"[PassivePowerUpHandler] PowerUpEffectManager not found! {data.powerUpName} won't get callbacks");
                    }
                }
                else
                {
                    // For regular passive effects, destroy the GameObject after applying the effect
                    // since they don't need ongoing callbacks
                    Debug.Log($"[PassivePowerUpHandler] {data.powerUpName} is a regular passive - destroying GameObject after applying effect");
                    GameObject.Destroy(instance);
                }
            }
            else
            {
                Debug.LogWarning($"[PassivePowerUpHandler] No PowerUpEffectBase found on prefab for {data.powerUpName}");
                GameObject.Destroy(instance); // Clean up failed instantiation
            }
        }

        Debug.Log($"[PassivePowerUpHandler] FINAL VALUES - bonusBaseDamage: {PlayerProgressData.Instance.bonusBaseDamage}, bonusRockDamage: {PlayerProgressData.Instance.bonusRockDamage}, bonusPaperDamage: {PlayerProgressData.Instance.bonusPaperDamage}, bonusScissorsDamage: {PlayerProgressData.Instance.bonusScissorsDamage}");
    }
}