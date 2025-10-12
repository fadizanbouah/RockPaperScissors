using UnityEngine;

public class PredictionRefreshEffect : PowerUpEffectBase
{
    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[PredictionRefreshEffect] Initialized");
    }

    public override void OnRoomStart()
    {
        // Apply the refresh immediately when activated
        ApplyRefresh();
    }

    private void ApplyRefresh()
    {
        if (enemy == null)
        {
            Debug.LogWarning("[PredictionRefreshEffect] No enemy reference!");
            return;
        }

        // Check if enemy uses prediction system
        if (!enemy.UsesPredictionSystem())
        {
            Debug.Log("[PredictionRefreshEffect] Enemy doesn't use prediction system - no effect");
            RemoveEffect();
            return;
        }

        Debug.Log($"[PredictionRefreshEffect] Refreshing prediction for {enemy.name}");

        // Force enemy to generate a new sequence
        enemy.ForceNewSequence();

        // If enemy uses sign shuffle, reset the shuffle counter
        if (enemy.UsesSignShuffle())
        {
            enemy.ResetSignShuffle();
            Debug.Log("[PredictionRefreshEffect] Reset sign shuffle counter");
        }

        // Update the Prediction UI
        PredictionUI predictionUI = Object.FindObjectOfType<PredictionUI>();
        if (predictionUI != null)
        {
            predictionUI.SetupPrediction(enemy);
            Debug.Log("[PredictionRefreshEffect] Updated Prediction UI");
        }

        // Update the Sign Shuffle UI
        SignShuffleUI shuffleUI = Object.FindObjectOfType<SignShuffleUI>();
        if (shuffleUI != null)
        {
            shuffleUI.UpdateEnemyReference(enemy);
            Debug.Log("[PredictionRefreshEffect] Updated Sign Shuffle UI");
        }

        Debug.Log("[PredictionRefreshEffect] Prediction refresh complete!");

        // Remove the effect after applying (one-time use)
        RemoveEffect();
    }

    private void RemoveEffect()
    {
        // Remove icon from tracker
        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.RemoveActiveEffect(this);
        }

        // Remove from manager
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