using UnityEngine;

public class IncreaseDamageNextHitEffect : PowerUpEffectBase
{
    private bool used = false;

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (used || result != RoundResult.Win)
            return;

        // Apply extra damage to the player's next attack
        player.ApplyTemporaryDamageBoost(Mathf.RoundToInt(sourceData.value));

        used = true;
        Debug.Log($"[Effect] Applied {sourceData.value} bonus damage to next hit (one-time use)");

        // No need to remove anything manually — manager will handle cleanup
    }
}
