using UnityEngine;

public class PowerUpUsageTracker : MonoBehaviour
{
    public static PowerUpUsageTracker Instance { get; private set; }

    private int powerUpsUsedThisRound = 0;
    private int allowedPowerUpsPerRound = 1;
    private int bonusUsesRemaining = 0;  // Extra uses from special power-ups

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
            Debug.Log($"[CanUsePowerUp] YES - Using bonus. Bonus remaining: {bonusUsesRemaining}");
        }
        else if (powerUpsUsedThisRound < allowedPowerUpsPerRound)
        {
            canUse = true;
            Debug.Log($"[CanUsePowerUp] YES - Using normal allowance. Used: {powerUpsUsedThisRound}/{allowedPowerUpsPerRound}");
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
    }

    public void AddBonusUses(int count)
    {
        bonusUsesRemaining += count;
        Debug.Log($"[AddBonusUses] Added {count} bonus uses. Total bonus: {bonusUsesRemaining}");
        DebugState();
    }

    public void ResetRoundUsage()
    {
        powerUpsUsedThisRound = 0;
        // Note: We do NOT reset bonusUsesRemaining - they carry over between rounds
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
}