using UnityEngine;

public static class ActivePowerUpHandler
{
    public static void GetModifiedMultiplier(ref float multiplier, string signUsed)
    {
        var effects = PowerUpEffectManager.Instance?.GetActiveEffects();
        if (effects == null) return;

        foreach (var effect in effects)
        {
            //Debug.Log($"[DEBUG] Modifying multiplier with effect: {effect.GetType().Name}");
            effect.ModifyDamageMultiplier(ref multiplier, signUsed);
        }
    }
}
