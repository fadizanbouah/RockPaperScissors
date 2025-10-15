using UnityEngine;

public class CheatDeathEffect : PowerUpEffectBase
{
    [Header("Cheat Death Configuration")]
    [SerializeField] private float healthRestorePercentage = 20f; // 20% of max health

    private static bool hasBeenUsed = false;

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
        if (hasBeenUsed)
        {
            Debug.Log("[CheatDeathEffect] Cheat Death already used - cannot trigger again");
            return false; // Don't save the player
        }

        Debug.Log("[CheatDeathEffect] CHEAT DEATH TRIGGERED! Saving player...");
        hasBeenUsed = true;

        // Unsubscribe since it's one-time use
        player.OnCheatDeathCheck -= OnCheatDeathCheck;

        // Calculate and restore health
        int restoreAmount = Mathf.RoundToInt(player.maxHealth * (healthRestorePercentage / 100f));
        restoreAmount = Mathf.Max(restoreAmount, 1); // At least 1 HP

        player.health = restoreAmount;
        // Don't update health bar here - TakeDamage will do it

        // Play the CheatDeath animation
        if (player.handAnimator != null && player.handAnimator.HasParameter("CheatDeath"))
        {
            player.handAnimator.SetTrigger("CheatDeath");
            Debug.Log("[CheatDeathEffect] Playing CheatDeath animation");
        }

        // Subscribe to animation finished event to clean up
        player.CheatDeathAnimationFinished += OnCheatDeathAnimationComplete;

        Debug.Log($"[CheatDeathEffect] Player saved with {restoreAmount} HP ({healthRestorePercentage}% of {player.maxHealth})");

        // Remove icon from tracker since it's been used
        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.RemoveActiveEffect(this);
        }

        return true; // Tell TakeDamage that we handled it
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