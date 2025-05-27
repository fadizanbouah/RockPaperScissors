using UnityEngine;

public static class ActivePowerUpHandler
{
    public static int GetModifiedDamage(int baseDamage, string signUsed)
    {
        int modifiedDamage = baseDamage;

        var effects = PowerUpEffectManager.Instance?.GetActiveEffects();
        if (effects == null) return modifiedDamage;

        foreach (var effect in effects)
        {
            Debug.Log($"[DEBUG] Modifying damage with effect: {effect.GetType().Name}");
            effect.ModifyDamage(ref modifiedDamage, signUsed);
        }

        return modifiedDamage;
    }

    // Removed RemoveRoomScopedPowerUps method entirely
}
