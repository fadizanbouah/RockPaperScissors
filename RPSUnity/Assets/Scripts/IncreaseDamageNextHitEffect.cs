using UnityEngine;

public class IncreaseDamageNextHitEffect : PowerUpEffectBase
{
    private bool used = false;

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (used || result != RoundResult.Win)
            return;

        int bonusAmount = Mathf.RoundToInt(sourceData.value);
        player.ApplyTemporaryDamageBoost(bonusAmount);

        used = true;

        Debug.Log($"[Effect] Applied {bonusAmount} bonus damage to next hit (one-time use) ({sourceData.powerUpName})");
    }

    public override void ModifyDamage(ref int damage, string signUsed)
    {
        Debug.Log($"[IncreaseDamageNextHitEffect] Modifying damage. Bonus applied: {sourceData.value}. Player is null? {player == null}");

        if (used) return;

        int bonus = Mathf.RoundToInt(sourceData.value);
        damage += bonus;
        Debug.Log($"[Effect] Applied {bonus} bonus damage (NextHit).");

        used = true;
    }
}
