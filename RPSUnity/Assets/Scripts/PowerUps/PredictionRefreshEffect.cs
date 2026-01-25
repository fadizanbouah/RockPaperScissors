using UnityEngine;

public class PredictionRefreshEffect : PowerUpEffectBase
{
    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[PredictionRefreshEffect] Initialized for owner: {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        // NEW: Add null check for enemy before applying refresh
        if (enemy == null || enemy.gameObject == null)
        {
            Debug.LogWarning("[PredictionRefreshEffect] Enemy is null or destroyed - removing effect");
            RemoveEffect();
            return;
        }

        // Apply the refresh immediately when activated
        ApplyRefresh();
    }

    private void ApplyRefresh()
    {
        // NEW: Double-check enemy still exists
        if (enemy == null || enemy.gameObject == null)
        {
            Debug.LogWarning("[PredictionRefreshEffect] Enemy destroyed during refresh - aborting");
            RemoveEffect();
            return;
        }

        // NEW: Instead of always refreshing the enemy, refresh whoever the TARGET is
        // If player owns this effect (normal case), target is the enemy
        // If enemy owns this effect (stolen by Robin Good), target is also the enemy (which is the actual player)
        // So we need to refresh based on who is NOT the owner

        HandController targetToRefresh = enemy;

        // However, if Robin Good steals this, we want to refresh Robin Good's OWN signs
        // So if the owner (player field) is an enemy, we should refresh the owner instead
        if (player != null && !player.isPlayer)
        {
            // Owner is an enemy (Robin Good stole it), so refresh the enemy's signs
            targetToRefresh = player;
            Debug.Log($"[PredictionRefreshEffect] Owner is enemy, refreshing owner's signs: {player.name}");
        }
        else
        {
            // Normal case - player owns it, refresh the enemy
            Debug.Log($"[PredictionRefreshEffect] Owner is player, refreshing enemy's signs: {enemy?.name ?? "null"}");
        }

        if (targetToRefresh == null || targetToRefresh.gameObject == null)
        {
            Debug.LogWarning("[PredictionRefreshEffect] No valid target to refresh!");
            RemoveEffect();
            return;
        }

        // Check if target uses prediction system
        if (!targetToRefresh.UsesPredictionSystem())
        {
            Debug.Log($"[PredictionRefreshEffect] {targetToRefresh.name} doesn't use prediction system - no effect");
            RemoveEffect();
            return;
        }

        Debug.Log($"[PredictionRefreshEffect] Refreshing prediction for {targetToRefresh.name}");

        // Force target to generate a new sequence
        targetToRefresh.ForceNewSequence();

        // If target uses sign shuffle, reset the shuffle counter
        if (targetToRefresh.UsesSignShuffle())
        {
            targetToRefresh.ResetSignShuffle();
            Debug.Log("[PredictionRefreshEffect] Reset sign shuffle counter");
        }

        // Update the Prediction UI
        PredictionUI predictionUI = Object.FindObjectOfType<PredictionUI>();
        if (predictionUI != null)
        {
            predictionUI.SetupPrediction(targetToRefresh);
            Debug.Log("[PredictionRefreshEffect] Updated Prediction UI");
        }

        // Update the Sign Shuffle UI
        SignShuffleUI shuffleUI = Object.FindObjectOfType<SignShuffleUI>();
        if (shuffleUI != null)
        {
            shuffleUI.UpdateEnemyReference(targetToRefresh);
            Debug.Log("[PredictionRefreshEffect] Updated Sign Shuffle UI");
        }

        Debug.Log("[PredictionRefreshEffect] Prediction refresh complete!");

        // Remove the effect after applying (one-time use)
        RemoveEffect();
    }

    private void RemoveEffect()
    {
        // Remove icon from the appropriate tracker based on who owns this effect
        if (player != null && player.isPlayer)
        {
            PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
            if (tracker != null)
            {
                tracker.RemoveActiveEffect(this);
            }
        }
        else if (player != null && !player.isPlayer)
        {
            EnemyCombatTracker tracker = Object.FindObjectOfType<EnemyCombatTracker>();
            if (tracker != null)
            {
                tracker.RemoveActiveEffect(this);
            }
        }

        // CRITICAL: Remove from PowerUpEffectManager to prevent it from being called again
        PowerUpEffectManager.Instance?.RemoveEffect(this);
    }

    public override void Cleanup()
    {
        Debug.Log("[PredictionRefreshEffect] Cleanup called");
    }

    public override bool IsEffectActive()
    {
        return false; // One-time effect, never persists
    }
}