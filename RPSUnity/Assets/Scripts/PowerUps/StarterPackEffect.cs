using UnityEngine;

public class StarterPackEffect : PowerUpEffectBase
{
    [Header("Starter Pack Configuration")]
    [SerializeField] private float damagePercentageIncrease = 10f;
    [SerializeField] private float healthPercentageIncrease = 10f;

    private PowerUpData myPowerUpData;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        myPowerUpData = data;
        Debug.Log($"[StarterPackEffect] Initialized for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null)
        {
            Debug.LogWarning("[StarterPackEffect] PlayerProgressData is null!");
            return;
        }

        // Check if already applied this run
        if (PlayerProgressData.Instance.HasAppliedOneTimeEffect(myPowerUpData.powerUpName))
        {
            Debug.Log($"[StarterPackEffect] {myPowerUpData.powerUpName} already applied, skipping");
            return;
        }

        // Get player
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackEffect] Cannot apply - player is null!");
            return;
        }

        // Calculate damage bonuses based on current values
        int rockBonus = Mathf.RoundToInt(activePlayer.rockDamage * (damagePercentageIncrease / 100f));
        int paperBonus = Mathf.RoundToInt(activePlayer.paperDamage * (damagePercentageIncrease / 100f));
        int scissorsBonus = Mathf.RoundToInt(activePlayer.scissorsDamage * (damagePercentageIncrease / 100f));

        // Apply to PlayerProgressData (just like PassiveIncreaseDamageEffect)
        PlayerProgressData.Instance.bonusRockDamage += rockBonus;
        PlayerProgressData.Instance.bonusPaperDamage += paperBonus;
        PlayerProgressData.Instance.bonusScissorsDamage += scissorsBonus;

        // Calculate and apply health bonus
        int healthBonus = Mathf.RoundToInt(activePlayer.maxHealth * (healthPercentageIncrease / 100f));
        PlayerProgressData.Instance.bonusMaxHealth += healthBonus;

        Debug.Log($"[StarterPackEffect] Applied {myPowerUpData.powerUpName}:");
        Debug.Log($"  Damage: Rock +{rockBonus}, Paper +{paperBonus}, Scissors +{scissorsBonus}");
        Debug.Log($"  Max Health: +{healthBonus}");

        // Mark as applied
        PlayerProgressData.Instance.MarkOneTimeEffectApplied(myPowerUpData.powerUpName);
    }
}