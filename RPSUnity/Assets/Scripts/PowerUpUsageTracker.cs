using UnityEngine;

public class PowerUpUsageTracker : MonoBehaviour
{
    public static PowerUpUsageTracker Instance { get; private set; }

    private int powerUpsUsedThisRound = 0;
    private int allowedPowerUpsPerRound = 1;
    private int bonusUsesRemaining = 0;  // Extra uses from special power-ups
    private bool bonusUsesAreTemporary = false;  // Track if bonus uses expire at round end

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("PowerUpUsageTracker initialized!");
    }

    public bool CanUsePowerUp()
    {
        bool canUse = false;

        // Check if we have bonus uses first
        if (bonusUsesRemaining > 0)
        {
            canUse = true;
            //Debug.Log($"[CanUsePowerUp] YES - Using bonus. Bonus remaining: {bonusUsesRemaining}");
        }
        else if (powerUpsUsedThisRound < allowedPowerUpsPerRound)
        {
            canUse = true;
            //Debug.Log($"[CanUsePowerUp] YES - Using normal allowance. Used: {powerUpsUsedThisRound}/{allowedPowerUpsPerRound}");
        }
        else
        {
            Debug.Log($"[CanUsePowerUp] NO - All uses exhausted. Used: {powerUpsUsedThisRound}/{allowedPowerUpsPerRound}, Bonus: {bonusUsesRemaining}");
        }

        return canUse;
    }

    public void MarkPowerUpUsed()
    {
        Debug.Log($"[MarkPowerUpUsed] BEFORE - Normal: {powerUpsUsedThisRound}/{allowedPowerUpsPerRound}, Bonus: {bonusUsesRemaining}");

        // IMPORTANT: Always increment the normal counter first
        powerUpsUsedThisRound++;

        // Then check if we should consume a bonus use
        if (powerUpsUsedThisRound > allowedPowerUpsPerRound && bonusUsesRemaining > 0)
        {
            bonusUsesRemaining--;
            Debug.Log($"[MarkPowerUpUsed] Consumed bonus use. Bonus remaining: {bonusUsesRemaining}");
        }

        Debug.Log($"[MarkPowerUpUsed] AFTER - Normal: {powerUpsUsedThisRound}/{allowedPowerUpsPerRound}, Bonus: {bonusUsesRemaining}");

        // If we just used up all temporary bonus uses, notify DoubleUse to remove its icon
        if (bonusUsesAreTemporary && bonusUsesRemaining == 0)
        {
            var effects = PowerUpEffectManager.Instance?.GetActiveEffects();
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    if (effect is DoubleUsePowerUpEffect doubleUse)
                    {
                        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
                        if (tracker != null)
                        {
                            tracker.RemoveActiveEffect(doubleUse);
                        }
                        PowerUpEffectManager.Instance?.RemoveEffect(doubleUse);
                        break;
                    }
                }
            }
        }
    }

    public void AddBonusUses(int count, bool temporary = false)
    {
        bonusUsesRemaining += count;
        if (temporary)
        {
            bonusUsesAreTemporary = true;
        }
        Debug.Log($"[AddBonusUses] Added {count} bonus uses. Total bonus: {bonusUsesRemaining}, Temporary: {temporary}");
    }

    public void ResetRoundUsage()
    {
        powerUpsUsedThisRound = 0;

        // Only clear bonus uses if they're marked as temporary (from DoubleUse)
        if (bonusUsesAreTemporary && bonusUsesRemaining > 0)
        {
            Debug.Log($"[ResetRoundUsage] Clearing {bonusUsesRemaining} temporary bonus uses");
            bonusUsesRemaining = 0;
            bonusUsesAreTemporary = false;
        }

        Debug.Log($"[ResetRoundUsage] Round reset. Bonus uses remaining: {bonusUsesRemaining}");
    }

    // For debugging
    public void DebugState()
    {
        Debug.Log($"[DEBUG STATE] Used: {powerUpsUsedThisRound}, Allowed: {allowedPowerUpsPerRound}, Bonus: {bonusUsesRemaining}, Can Use More: {CanUsePowerUp()}");
    }

    public int GetRemainingUses()
    {
        int normalRemaining = Mathf.Max(0, allowedPowerUpsPerRound - powerUpsUsedThisRound);
        return normalRemaining + bonusUsesRemaining;
    }

    public bool HasTemporaryBonusUses()
    {
        return bonusUsesAreTemporary && bonusUsesRemaining > 0;
    }
}