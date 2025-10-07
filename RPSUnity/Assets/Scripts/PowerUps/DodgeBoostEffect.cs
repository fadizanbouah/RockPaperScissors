using UnityEngine;

public class DodgeBoostEffect : PowerUpEffectBase, IDurationEffect
{
    [Header("Configuration")]
    [SerializeField] private float dodgeBoostAmount = 25f; // Add 25% dodge chance
    [SerializeField] private int duration = 3; // Lasts for 3 rounds
    [SerializeField] private bool isTemporary = true; // If false, lasts entire room

    private int roundsRemaining;
    private bool isActive = false;
    private float originalDodgeChance;
    private bool hasBeenApplied = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // Use the value from ScriptableObject if provided
        if (data.value > 0)
        {
            dodgeBoostAmount = data.value;
        }

        roundsRemaining = duration;
        Debug.Log($"[DodgeBoostEffect] Initialized with +{dodgeBoostAmount}% dodge for {duration} rounds");
    }

    public override void OnRoomStart()
    {
        // Only apply if not already active
        if (!isActive && !hasBeenApplied && player != null)
        {
            originalDodgeChance = player.dodgeChance;
            player.dodgeChance = Mathf.Min(player.dodgeChance + dodgeBoostAmount, 100f);
            isActive = true;
            hasBeenApplied = true;

            Debug.Log($"[DodgeBoostEffect] Dodge increased from {originalDodgeChance}% to {player.dodgeChance}%");

            player.OnDodge += OnPlayerDodged;
        }
        else if (isActive)
        {
            Debug.Log($"[DodgeBoostEffect] Already active with {roundsRemaining} rounds remaining - persisting to new room");
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (!isActive || !isTemporary) return;

        roundsRemaining--;
        Debug.Log($"[DodgeBoostEffect] {roundsRemaining} rounds remaining");

        UpdateCounterDisplay();

        if (roundsRemaining <= 0)
        {
            RemoveEffect();
        }
    }

    private void OnPlayerDodged(HandController hand)
    {
        Debug.Log($"[DodgeBoostEffect] Player successfully dodged with boosted dodge chance!");
    }

    private void RemoveEffect()
    {
        if (isActive && player != null)
        {
            // Restore original dodge chance
            player.dodgeChance = originalDodgeChance;
            player.OnDodge -= OnPlayerDodged;
            isActive = false;

            Debug.Log($"[DodgeBoostEffect] Effect expired. Dodge restored to {originalDodgeChance}%");

            // Remove icon from tracker
            PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
            if (tracker != null)
            {
                tracker.RemoveActiveEffect(this);
            }

            // Remove from manager
            PowerUpEffectManager.Instance?.RemoveEffect(this);
        }
    }

    public override void Cleanup()
    {
        RemoveEffect();
        Debug.Log("[DodgeBoostEffect] Cleanup called");
    }

    public override bool IsEffectActive()
    {
        return isActive && (roundsRemaining > 0 || !isTemporary);
    }

    public int GetRoundsRemaining()
    {
        return isTemporary ? roundsRemaining : -1; // -1 = no duration
    }

    private void UpdateCounterDisplay()
    {
        PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.UpdateEffectCounter(this);
        }
    }
}