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

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (used || result != RoundResult.Win)
            return;

        // We keep this for compatibility, but no flat bonus applied here anymore
        used = true;
        Debug.Log($"[IncreaseDamageNextHitEffect] Marked as used after round win ({sourceData.powerUpName})");
    }

    public override void ModifyDamage(ref int damage, string signUsed)
    {
        if (used) return;

        int bonus = Mathf.RoundToInt(damage * bonusPercentage);
        damage += bonus;

        Debug.Log($"[IncreaseDamageNextHitEffect] Applied {bonusPercentage * 100}% bonus damage: +{bonus} (New damage: {damage})");

        used = true;
    }
}
