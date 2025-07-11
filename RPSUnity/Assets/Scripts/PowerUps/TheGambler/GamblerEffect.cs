using UnityEngine;

public class GamblerEffect : PowerUpEffectBase
{
    [Header("Gambler Configuration")]
    [SerializeField] protected float betPercentageLimit = 0.5f; // 50% of max HP
    [SerializeField] protected int hpCost = 2;        // HP required
    [SerializeField] protected int damageGained = 1;  // Damage received for that HP

    private int currentBetAmount = 0;
    private bool hasBetThisRound = false;
    private GamblerUI gamblerUI;
    private bool uiCreated = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[GamblerEffect] Initialize - Player: {player?.name ?? "NULL"}, Enemy: {enemy?.name ?? "NULL"}");

        // Don't create UI yet if player is null - wait for player reference update
        if (player != null)
        {
            CreateGamblerUI();
        }
        else
        {
            Debug.Log("[GamblerEffect] Player is null, delaying UI creation until player reference is updated");
        }
    }

    // Override the base UpdateReferences to also update UI
    public override void UpdateReferences(HandController newPlayer, HandController newEnemy)
    {
        base.UpdateReferences(newPlayer, newEnemy);
        UpdatePlayerReference(newPlayer, newEnemy);
    }

    // NEW: Method to update player reference when it becomes available
    public void UpdatePlayerReference(HandController newPlayer, HandController newEnemy)
    {
        Debug.Log($"[GamblerEffect] UpdatePlayerReference - Player: {newPlayer?.name ?? "NULL"}, Enemy: {newEnemy?.name ?? "NULL"}");

        this.player = newPlayer;
        this.enemy = newEnemy;

        // Create UI now that we have a valid player reference
        if (player != null && !uiCreated)
        {
            CreateGamblerUI();
        }

        // Update UI to reflect current bet (don't reset the amount)
        if (gamblerUI != null && gamblerUI.gameObject.activeInHierarchy)
        {
            gamblerUI.UpdateBetDisplay(currentBetAmount); // Add this method to GamblerUI
        }
    }

    private void CreateGamblerUI()
    {
        Debug.Log("[GamblerEffect] CreateGamblerUI called - looking for existing UI");
        Debug.Log("[GamblerEffect] *** THIS SHOULD NOT HAPPEN IF GAMBLER WASN'T PURCHASED ***");

        // Find the existing GamblerUI in the scene (even if inactive)
        GamblerUI[] allGamblerUIs = Resources.FindObjectsOfTypeAll<GamblerUI>();

        foreach (var ui in allGamblerUIs)
        {
            // Make sure it's in the scene (not a prefab)
            if (ui.gameObject.scene.IsValid())
            {
                gamblerUI = ui;
                break;
            }
        }

        if (gamblerUI != null)
        {
            Debug.Log("[GamblerEffect] Found existing GamblerUI in scene - activating it");
            gamblerUI.gameObject.SetActive(true);
            gamblerUI.Initialize(this, player);
            Debug.Log("[GamblerEffect] GamblerUI activated and initialized successfully");
        }
        else
        {
            Debug.LogError("[GamblerEffect] No GamblerUI found in scene! Make sure you have one placed in GameplayCanvas.");
        }
    }

    public void SetBetAmount(int amount)
    {
        Debug.Log($"[GamblerEffect] SetBetAmount called with amount: {amount} (previous amount was: {currentBetAmount})");

        // Check if player is null
        if (player == null)
        {
            Debug.LogError("[GamblerEffect] Player is null! Cannot set bet amount.");
            return;
        }

        // Ensure bet doesn't exceed current HP - 1
        int maxBet = Mathf.Max(0, player.CurrentHealth - 1);
        int newBetAmount = Mathf.Clamp(amount, 0, maxBet);

        // Only update if the amount actually changed
        if (newBetAmount != currentBetAmount)
        {
            currentBetAmount = newBetAmount;
            Debug.Log($"[GamblerEffect] Bet amount updated to: {currentBetAmount} (max was {maxBet})");
        }
        else
        {
            Debug.Log($"[GamblerEffect] Bet amount unchanged: {currentBetAmount}");
        }
    }

    public int GetCurrentBet()
    {
        return currentBetAmount;
    }

    public int GetMaxBet()
    {
        if (player == null)
        {
            Debug.LogWarning("[GamblerEffect] Player is null in GetMaxBet!");
            return 0;
        }

        // Calculate based on max HP, but can't bet more than current HP - 1
        int percentageBasedMax = Mathf.FloorToInt(player.maxHealth * betPercentageLimit);
        int currentHPLimit = player.CurrentHealth - 1;
        int actualMax = Mathf.Min(currentHPLimit, percentageBasedMax);

        Debug.Log($"[GamblerEffect] GetMaxBet - MaxHP: {player.maxHealth}, CurrentHP: {player.CurrentHealth}, " +
                  $"PercentageLimit: {percentageBasedMax}, CurrentHPLimit: {currentHPLimit}, Final: {actualMax}");

        return Mathf.Max(0, actualMax);
    }

    public int GetBonusDamage()
    {
        // Calculate how many "sets" of the ratio we can afford
        int ratioSets = currentBetAmount / hpCost;
        return ratioSets * damageGained;
    }

    public override void OnRoundStart()
    {
        Debug.Log($"[GamblerEffect] OnRoundStart - Player: {player?.name ?? "NULL"}, hasBetThisRound: {hasBetThisRound}, currentBetAmount: {currentBetAmount}");

        if (player == null)
        {
            Debug.LogError("[GamblerEffect] Player is null in OnRoundStart!");
            return;
        }

        // Only deduct HP if we haven't already bet this round AND there's a bet amount set
        if (currentBetAmount > 0 && !hasBetThisRound)
        {
            // Deduct the bet HP at round start
            player.health -= currentBetAmount;
            player.UpdateHealthBar();
            hasBetThisRound = true;

            Debug.Log($"[GamblerEffect] Bet {currentBetAmount} HP for +{GetBonusDamage()} damage");
        }
        else if (hasBetThisRound)
        {
            Debug.Log($"[GamblerEffect] Already bet this round, skipping HP deduction");
        }
        else
        {
            Debug.Log($"[GamblerEffect] No bet amount set, skipping");
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        Debug.Log($"[GamblerEffect] OnRoundEnd called - Result: {result}, hasBetThisRound: {hasBetThisRound}, currentBetAmount: {currentBetAmount}");

        if (!hasBetThisRound || currentBetAmount == 0)
        {
            Debug.Log("[GamblerEffect] No active bet, skipping round end logic");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[GamblerEffect] Player is null in OnRoundEnd!");
            return;
        }

        switch (result)
        {
            case RoundResult.Win:
                // Restore HP on win
                player.health += currentBetAmount;
                player.UpdateHealthBar();
                Debug.Log($"[GamblerEffect] Won! Restored {currentBetAmount} HP");
                break;

            case RoundResult.Draw:
                // Restore HP on draw
                player.health += currentBetAmount;
                player.UpdateHealthBar();
                Debug.Log($"[GamblerEffect] Draw! Restored {currentBetAmount} HP");
                break;

            case RoundResult.Lose:
                // HP stays lost
                Debug.Log($"[GamblerEffect] Lost! {currentBetAmount} HP remains lost");
                break;
        }

        // IMPORTANT: Reset the bet state for the next round
        hasBetThisRound = false;
        // Keep currentBetAmount so player can see their last bet, but reset the "used" flag

        Debug.Log($"[GamblerEffect] Round complete. Bet state reset for next round.");
    }

    public override int GetFlatDamageBonus(string signUsed)
    {
        if (hasBetThisRound && currentBetAmount > 0)
        {
            int bonus = GetBonusDamage();
            Debug.Log($"[GamblerEffect] Providing {bonus} flat damage bonus from bet");
            return bonus;
        }
        return 0;
    }

    public override void OnRoomStart()
    {
        Debug.Log($"[GamblerEffect] OnRoomStart called - Resetting bet state for new room");

        // Reset bet state completely when entering a new room
        currentBetAmount = 0;
        hasBetThisRound = false;

        // Reset the UI slider to match the reset bet amount
        if (gamblerUI != null && gamblerUI.gameObject.activeInHierarchy)
        {
            gamblerUI.ResetSlider();
            Debug.Log("[GamblerEffect] Reset GamblerUI slider to 0");
        }

        Debug.Log($"[GamblerEffect] Room reset complete - currentBetAmount: {currentBetAmount}, hasBetThisRound: {hasBetThisRound}");
    }

    public override void Cleanup()
    {
        Debug.Log("[GamblerEffect] Cleanup called");
        if (gamblerUI != null)
        {
            gamblerUI.gameObject.SetActive(false);
            Debug.Log("[GamblerEffect] GamblerUI deactivated");
        }
    }
}