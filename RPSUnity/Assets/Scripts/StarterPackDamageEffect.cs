using UnityEngine;

public class StarterPackDamageEffect : PowerUpEffectBase
{
    [Header("Damage Configuration")]
    [SerializeField] private float damagePercentage = 10f; // 10% damage increase

    private static int storedBaseRockDamage = -1;
    private static int storedBasePaperDamage = -1;
    private static int storedBaseScissorsDamage = -1;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[StarterPackDamageEffect] Initialized for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        Debug.Log("[StarterPackDamageEffect] Applying damage bonus...");

        // Get the current level
        float currentDamagePercentage = damagePercentage;

        if (sourceData != null && sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            currentDamagePercentage = sourceData.GetValueForLevel(currentLevel);
            Debug.Log($"[StarterPackDamageEffect] Current level: {currentLevel}, percentage: {currentDamagePercentage}%");
        }

        ApplyDamageBonus(currentDamagePercentage);
    }

    private void ApplyDamageBonus(float percentage)
    {
        if (PlayerProgressData.Instance == null)
        {
            Debug.LogWarning("[StarterPackDamageEffect] PlayerProgressData is null!");
            return;
        }

        // Get player reference
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackDamageEffect] Cannot apply damage bonus - player is null!");
            return;
        }

        // Store base damage values on FIRST application (just like HP does)
        if (storedBaseRockDamage == -1)
        {
            storedBaseRockDamage = activePlayer.rockDamage;
            storedBasePaperDamage = activePlayer.paperDamage;
            storedBaseScissorsDamage = activePlayer.scissorsDamage;
            Debug.Log($"[StarterPackDamageEffect] Stored base damage: Rock={storedBaseRockDamage}, Paper={storedBasePaperDamage}, Scissors={storedBaseScissorsDamage}");
        }

        // CRITICAL: Since PassivePowerUpHandler resets bonuses to 0 before calling OnRoomStart,
        // we need to add the FULL current bonus amount (not the difference)
        // The "stored baseline" approach means we always calculate the full bonus from the stored values
        int rockBonus = Mathf.RoundToInt(storedBaseRockDamage * (percentage / 100f));
        int paperBonus = Mathf.RoundToInt(storedBasePaperDamage * (percentage / 100f));
        int scissorsBonus = Mathf.RoundToInt(storedBaseScissorsDamage * (percentage / 100f));

        // Apply the FULL bonus (bonuses were already reset to 0 by PassivePowerUpHandler)
        PlayerProgressData.Instance.bonusRockDamage += rockBonus;
        PlayerProgressData.Instance.bonusPaperDamage += paperBonus;
        PlayerProgressData.Instance.bonusScissorsDamage += scissorsBonus;

        Debug.Log($"[StarterPackDamageEffect] Applied damage bonuses ({percentage}% of stored baseline):");
        Debug.Log($"  Rock: {storedBaseRockDamage} × {percentage}% = +{rockBonus}");
        Debug.Log($"  Paper: {storedBasePaperDamage} × {percentage}% = +{paperBonus}");
        Debug.Log($"  Scissors: {storedBaseScissorsDamage} × {percentage}% = +{scissorsBonus}");
        Debug.Log($"[StarterPackDamageEffect] Total bonuses now: Rock +{PlayerProgressData.Instance.bonusRockDamage}, Paper +{PlayerProgressData.Instance.bonusPaperDamage}, Scissors +{PlayerProgressData.Instance.bonusScissorsDamage}");
    }

    public override void Cleanup()
    {
        storedBaseRockDamage = -1;
        storedBasePaperDamage = -1;
        storedBaseScissorsDamage = -1;
        Debug.Log("[StarterPackDamageEffect] Cleanup - reset flags");
    }

    public static void ResetForNewRun()
    {
        storedBaseRockDamage = -1;
        storedBasePaperDamage = -1;
        storedBaseScissorsDamage = -1;
        Debug.Log("[StarterPackDamageEffect] Reset for new run");
    }
}