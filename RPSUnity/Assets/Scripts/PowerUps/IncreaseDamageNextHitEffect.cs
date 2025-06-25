using UnityEngine;

public class IncreaseDamageNextHitEffect : PowerUpEffectBase
{
    private bool used = false;
    private float bonusPercentage;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        bonusPercentage = data.value / 100f; // Convert to decimal percentage, e.g., 20 -> 0.2 (20%)
        Debug.Log($"[IncreaseDamageNextHitEffect] Initialized with {data.value}% bonus damage for next hit.");
    }

    public override void ModifyDamageMultiplier(ref float multiplier, string signUsed)
    {
        if (used) return;

        multiplier += bonusPercentage;
        Debug.Log($"[IncreaseDamageNextHitEffect] Added {bonusPercentage * 100}% to multiplier. Total multiplier now: {multiplier}");

        used = true;
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (used || result != RoundResult.Win)
            return;

        // This effect is now fully handled via ModifyDamageMultiplier
        used = true;
        Debug.Log($"[IncreaseDamageNextHitEffect] Marked as used after round win ({sourceData.powerUpName})");
    }
}
