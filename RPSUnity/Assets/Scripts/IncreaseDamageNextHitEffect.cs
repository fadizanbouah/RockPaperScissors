using UnityEngine;

public class IncreaseDamageNextHitEffect : PowerUpEffectBase
{
    private bool used = false;

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (used || result != RoundResult.Win)
            return;

        // Apply extra damage to the player's next attack
        player.ApplyTemporaryDamageBoost(Mathf.RoundToInt(source.effectValue));

        used = true;
        Debug.Log($"[Effect] Applied {source.effectValue} bonus damage to next hit (one-time use)");

        // Remove from active power-ups after use
        RunProgressManager.Instance.activePowerUps.Remove(source);
    }
}
