using UnityEngine;

public class CheatDeathEffect : PowerUpEffectBase
{
    [Header("Cheat Death Configuration")]
    [SerializeField] private float healthRestorePercentage = 20f; // 20% of max health

    private static bool hasBeenUsed = false;
    private int pendingRestoreAmount = 0; // Cache the heal amount

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[CheatDeathEffect] Initialized for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        // Get player reference if we don't have it
        if (player == null && PowerUpEffectManager.Instance != null)
        {
            player = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (player == null)
        {
            Debug.LogWarning("[CheatDeathEffect] Cannot set up Cheat Death - player is null!");
            return;
        }

        // Only set up once per run
        if (!hasBeenUsed)
        {
            // Subscribe to the cheat death check
            player.OnCheatDeathCheck += OnCheatDeathCheck;
            Debug.Log("[CheatDeathEffect] Cheat Death is active and ready!");
        }
        else
        {
            Debug.Log("[CheatDeathEffect] Cheat Death already used this run - not activating");
        }
    }

    private bool OnCheatDeathCheck(HandController dyingPlayer)
    {
        // Check FIRST before logging anything
        if (hasBeenUsed)
        {
            Debug.Log("[CheatDeathEffect] Cheat Death already used - cannot trigger again");
            return false; // Don't save the player
        }

        Debug.Log("[CheatDeathEffect] CHEAT DEATH TRIGGERED! Saving player...");

        // Mark as used IMMEDIATELY to prevent double-triggering
        hasBeenUsed = true;

        // Unsubscribe IMMEDIATELY since it's one-time use
        player.OnCheatDeathCheck -= OnCheatDeathCheck;

        // Calculate heal amount but DON'T apply yet - wait for animation event
        pendingRestoreAmount = Mathf.RoundToInt(player.maxHealth * (healthRestorePercentage / 100f));
        pendingRestoreAmount = Mathf.Max(pendingRestoreAmount, 1); // At least 1 HP

        Debug.Log($"[CheatDeathEffect] Will restore {pendingRestoreAmount} HP when animation triggers");

        // Play the CheatDeath animation
        if (player.handAnimator != null && player.handAnimator.HasParameter("CheatDeath"))
        {
            player.handAnimator.SetTrigger("CheatDeath");
            Debug.Log("[CheatDeathEffect] Playing CheatDeath animation");
        }

        // Subscribe to heal event (mid-animation)
        player.CheatDeathHeal += OnCheatDeathHeal;

        // Subscribe to animation finished event to clean up
        player.CheatDeathAnimationFinished += OnCheatDeathAnimationComplete;

        // Remove icon from PlayerCombatTracker since it's been used
        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.RemoveActiveEffect(this);
        }

        // NEW: Remove from PassivePowerUpTracker as well
        if (sourceData != null && sourceData.isPassive)
        {
            // Remove from the persistent power-ups list
            if (RunProgressManager.Instance != null && RunProgressManager.Instance.persistentPowerUps.Contains(sourceData))
            {
                RunProgressManager.Instance.persistentPowerUps.Remove(sourceData);
                Debug.Log($"[CheatDeathEffect] Removed {sourceData.powerUpName} from persistent power-ups");
            }

            // Refresh the passive tracker UI to reflect the removal
            PassivePowerUpTracker passiveTracker = Object.FindObjectOfType<PassivePowerUpTracker>();
            if (passiveTracker != null)
            {
                passiveTracker.RefreshDisplay();
                Debug.Log("[CheatDeathEffect] Refreshed PassivePowerUpTracker UI");
            }
        }

        return true; // Tell TakeDamage that we handled it
    }

    private void OnCheatDeathHeal(HandController hand)
    {
        Debug.Log($"[CheatDeathEffect] Heal event triggered! Restoring {pendingRestoreAmount} HP");

        // Apply the heal NOW (timed with animation)
        player.health = pendingRestoreAmount;
        player.UpdateHealthBar();

        // Unsubscribe from heal event
        if (player != null)
        {
            player.CheatDeathHeal -= OnCheatDeathHeal;
        }

        Debug.Log($"[CheatDeathEffect] Player healed to {player.health} HP");
    }

    private void OnCheatDeathAnimationComplete(HandController hand)
    {
        Debug.Log("[CheatDeathEffect] CheatDeath animation complete");

        // Unsubscribe
        if (player != null)
        {
            player.CheatDeathAnimationFinished -= OnCheatDeathAnimationComplete;
        }

        // Remove from manager
        PowerUpEffectManager.Instance?.RemoveEffect(this);
    }

    public override void Cleanup()
    {
        // Unsubscribe from events
        if (player != null)
        {
            player.OnCheatDeathCheck -= OnCheatDeathCheck;
            player.CheatDeathHeal -= OnCheatDeathHeal;
            player.CheatDeathAnimationFinished -= OnCheatDeathAnimationComplete;
        }

        hasBeenUsed = false;
        Debug.Log("[CheatDeathEffect] Cleanup - reset flags");
    }

    public static void ResetForNewRun()
    {
        hasBeenUsed = false;
        Debug.Log("[CheatDeathEffect] Reset for new run");
    }

    public override bool IsEffectActive()
    {
        return !hasBeenUsed;
    }
}