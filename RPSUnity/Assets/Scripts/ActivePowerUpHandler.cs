using UnityEngine;
using System.Collections.Generic;

public static class ActivePowerUpHandler
{
    public static int GetModifiedDamage(int baseDamage, string signUsed)
    {
        int modifiedDamage = baseDamage;
        List<PowerUp> toRemove = new List<PowerUp>();

        foreach (PowerUp powerUp in RunProgressManager.Instance.activePowerUps)
        {
            switch (powerUp.type)
            {
                case PowerUpType.IncreaseDamageThisRoom:
                    modifiedDamage += Mathf.RoundToInt(powerUp.effectValue);
                    break;

                case PowerUpType.IncreaseDamageNextHit:
                    modifiedDamage += Mathf.RoundToInt(powerUp.effectValue);
                    toRemove.Add(powerUp); // one-time use
                    break;
            }
        }

        // Remove one-time use power-ups after applying their effect
        foreach (var powerUp in toRemove)
        {
            RunProgressManager.Instance.activePowerUps.Remove(powerUp);
            Debug.Log($"[ActivePowerUpHandler] Consumed one-time power-up: {powerUp.powerUpName}");
        }

        return modifiedDamage;
    }

    public static void RemoveRoomScopedPowerUps()
    {
        if (RunProgressManager.Instance == null) return;

        RunProgressManager.Instance.activePowerUps.RemoveAll(p =>
            p.type == PowerUpType.IncreaseDamageThisRoom ||
            p.type == PowerUpType.IncreaseMaxHealthThisRoom);

        Debug.Log("[ActivePowerUpHandler] Removed room-scoped active power-ups.");
    }
}
