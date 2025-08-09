using UnityEngine;

public class IncreaseDamageNextHitEffect : PowerUpEffectBase
{
    private bool used = false;
    private float bonusPercentage;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        bonusPercentage = data.value / 100f;
        used = false; // Ensure it starts as unused
        //Debug.Log($"[IncreaseDamageNextHitEffect] Initialized with {data.value}% bonus damage. Used = {used}");
    }

    public override void ModifyDamageMultiplier(ref float multiplier, string signUsed)
    {
        //Debug.Log($"[IncreaseDamageNextHitEffect] ModifyDamageMultiplier called. Used = {used}, Multiplier before = {multiplier}");

        if (used)
        {
            //Debug.Log("[IncreaseDamageNextHitEffect] Effect already used, skipping");
            return;
        }

        float oldMultiplier = multiplier;
        multiplier += bonusPercentage;
        //Debug.Log($"[IncreaseDamageNextHitEffect] Applied bonus. Old multiplier: {oldMultiplier}, New multiplier: {multiplier}");
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        //Debug.Log($"[IncreaseDamageNextHitEffect] OnRoundEnd called. Result = {result}, Used = {used}");

        if (used)
        {
            //Debug.Log("[IncreaseDamageNextHitEffect] Already used, skipping");
            return;
        }

        if (result == RoundResult.Win)
        {
            used = true;
            //Debug.Log($"[IncreaseDamageNextHitEffect] Marking as used after win");

            // Remove icon from tracker
            PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
            if (tracker != null)
            {
                tracker.RemoveActiveEffect(this);
            }

            // Remove this effect from manager
            PowerUpEffectManager.Instance?.RemoveEffect(this);
        }
    }

    public override bool IsEffectActive()
    {
        return !used;
    }
}
