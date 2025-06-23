using UnityEngine;

// New script to track power-up usage per round
public class PowerUpUsageTracker : MonoBehaviour
{
    public static PowerUpUsageTracker Instance { get; private set; }

    private bool powerUpUsedThisRound = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanUsePowerUp()
    {
        return !powerUpUsedThisRound;
    }

    public void MarkPowerUpUsed()
    {
        powerUpUsedThisRound = true;
        Debug.Log("[PowerUpUsageTracker] Power-up used this round. No more power-ups allowed until next round.");
    }

    public void ResetRoundUsage()
    {
        powerUpUsedThisRound = false;
        Debug.Log("[PowerUpUsageTracker] Round reset. Power-ups can be used again.");
    }
}