using UnityEngine;

public class DoubleUsePowerUpEffect : PowerUpEffectBase
{
    [Header("Configuration")]
    public int extraUsesToGrant = 2;
    private bool effectApplied = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[DoubleUsePowerUpEffect] Initialized. Will grant {extraUsesToGrant} extra power-up uses.");

        // Apply effect immediately on initialization
        ApplyDoubleUseEffect();
    }

    public override void OnRoomStart()
    {
        // This effect is one-time use, so we don't need to do anything on room start
        // The effect was already applied during initialization
    }

    private void ApplyDoubleUseEffect()
    {
        if (effectApplied) return;

        if (PowerUpUsageTracker.Instance != null)
        {
            PowerUpUsageTracker.Instance.AddBonusUses(extraUsesToGrant);
            effectApplied = true;
            Debug.Log($"[DoubleUsePowerUpEffect] Granted {extraUsesToGrant} bonus power-up uses!");

            // Remove icon immediately since effect is consumed
            PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
            if (tracker != null)
            {
                tracker.RemoveActiveEffect(this);
            }

            // Remove from manager
            PowerUpEffectManager.Instance?.RemoveEffect(this);
        }
        else
        {
            Debug.LogError("[DoubleUsePowerUpEffect] PowerUpUsageTracker not found!");
        }
    }

    public override void Cleanup()
    {
        // Don't destroy here - let the PowerUpEffectManager handle cleanup
        Debug.Log("[DoubleUsePowerUpEffect] Cleanup called");
    }
}