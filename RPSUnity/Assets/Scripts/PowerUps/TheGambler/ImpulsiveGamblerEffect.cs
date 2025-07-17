using UnityEngine;

public class ImpulsiveGamblerEffect : PowerUpEffectBase, IGamblerEffect
{
    [Header("Impulsive Gambler Configuration")]
    [SerializeField] protected float betPercentageLimit = 0.5f; // 50% of max HP
    [SerializeField] protected int hpCost = 2;        // HP required
    [SerializeField] protected int damageGained = 1;  // Damage received for that HP

    [Header("High Bet Tracking")]
    [SerializeField] protected float highBetThreshold = 0.2f; // 20% of max HP
    [SerializeField] protected int requiredHighBets = 3;      // Need 3 in a row
    [SerializeField] protected float damageReduction = 0.5f;  // 50% reduction

    // State tracking
    private int consecutiveHighBets = 0;
    private bool hasDamageReductionNextRound = false;
    private bool damageReductionActive = false;

    private int currentBetAmount = 0;
    private bool hasBetThisRound = false;
    private GamblerUI gamblerUI;
    private bool uiCreated = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[ImpulsiveGamblerEffect] Initialize - Player: {player?.name ?? "NULL"}, Enemy: {enemy?.name ?? "NULL"}");

        // Don't create UI yet if player is null - wait for player reference update
        if (player != null)
        {
            CreateGamblerUI();
        }
        else
        {
            Debug.Log("[ImpulsiveGamblerEffect] Player is null, delaying UI creation until player reference is updated");
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
        Debug.Log($"[ImpulsiveGamblerEffect] UpdatePlayerReference - Player: {newPlayer?.name ?? "NULL"}, Enemy: {newEnemy?.name ?? "NULL"}");

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
        Debug.Log("[ImpulsiveGamblerEffect] CreateGamblerUI called - looking for existing UI");
        Debug.Log("[ImpulsiveGamblerEffect] *** THIS SHOULD NOT HAPPEN IF GAMBLER WASN'T PURCHASED ***");
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
            Debug.Log("[ImpulsiveGamblerEffect] Found existing GamblerUI in scene - activating it");
            gamblerUI.gameObject.SetActive(true);
            gamblerUI.Initialize(this, player);
            Debug.Log("[ImpulsiveGamblerEffect] GamblerUI activated and initialized successfully");

            // NEW: Set up UI for impulsive variant
            gamblerUI.SetupForImpulsiveGambler(true);
            UpdateUIProgress(); // Initial update
        }
        else
        {
            Debug.LogError("[ImpulsiveGamblerEffect] No GamblerUI found in scene! Make sure you have one placed in GameplayCanvas.");
        }
    }

    public void SetBetAmount(int amount)
    {
        Debug.Log($"[ImpulsiveGamblerEffect] SetBetAmount called with amount: {amount} (previous amount was: {currentBetAmount})");

        // Check if player is null
        if (player == null)
        {
            Debug.LogError("[ImpulsiveGamblerEffect] Player is null! Cannot set bet amount.");
            return;
        }

        // Ensure bet doesn't exceed current HP - 1
        int maxBet = Mathf.Max(0, player.CurrentHealth - 1);
        int newBetAmount = Mathf.Clamp(amount, 0, maxBet);

        // Only update if the amount actually changed
        if (newBetAmount != currentBetAmount)
        {
            currentBetAmount = newBetAmount;
            Debug.Log($"[ImpulsiveGamblerEffect] Bet amount updated to: {currentBetAmount} (max was {maxBet})");
        }
        else
        {
            Debug.Log($"[ImpulsiveGamblerEffect] Bet amount unchanged: {currentBetAmount}");
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
            Debug.LogWarning("[ImpulsiveGamblerEffect] Player is null in GetMaxBet!");
            return 0;
        }

        // Calculate based on max HP, but can't bet more than current HP - 1
        int percentageBasedMax = Mathf.FloorToInt(player.maxHealth * betPercentageLimit);
        int currentHPLimit = player.CurrentHealth - 1;
        int actualMax = Mathf.Min(currentHPLimit, percentageBasedMax);

        //Debug.Log($"[GamblerEffect] GetMaxBet - MaxHP: {player.maxHealth}, CurrentHP: {player.CurrentHealth}, " +
                  //$"PercentageLimit: {percentageBasedMax}, CurrentHPLimit: {currentHPLimit}, Final: {actualMax}");

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
        // Check if we should activate damage reduction this round
        if (hasDamageReductionNextRound)
        {
            damageReductionActive = true;
            hasDamageReductionNextRound = false;
            Debug.Log("[ImpulsiveGamblerEffect] Damage reduction ACTIVE this round!");

            // Keep the counter at max while active
            consecutiveHighBets = requiredHighBets; // This keeps it at 2/2 or 3/3

            // Update UI to show active reduction
            UpdateUIProgress();
        }

        Debug.Log($"[ImpulsiveGamblerEffect] OnRoundStart - Player: {player?.name ?? "NULL"}, hasBetThisRound: {hasBetThisRound}, currentBetAmount: {currentBetAmount}");
        if (player == null)
        {
            Debug.LogError("[ImpulsiveGamblerEffect] Player is null in OnRoundStart!");
            return;
        }
        // Only deduct HP if we haven't already bet this round AND there's a bet amount set
        if (currentBetAmount > 0 && !hasBetThisRound)
        {
            // Deduct the bet HP at round start
            player.health -= currentBetAmount;
            player.UpdateHealthBar();
            hasBetThisRound = true;
            Debug.Log($"[ImpulsiveGamblerEffect] Bet {currentBetAmount} HP for +{GetBonusDamage()} damage");
        }
        else if (hasBetThisRound)
        {
            Debug.Log($"[ImpulsiveGamblerEffect] Already bet this round, skipping HP deduction");
        }
        else
        {
            Debug.Log($"[ImpulsiveGamblerEffect] No bet amount set, skipping");
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        Debug.Log($"[ImpulsiveGamblerEffect] OnRoundEnd called - Result: {result}, hasBetThisRound: {hasBetThisRound}, currentBetAmount: {currentBetAmount}");

        if (player == null)
        {
            Debug.LogError("[ImpulsiveGamblerEffect] Player is null in OnRoundEnd!");
            return;
        }

        // Only do HP restoration if there was an actual bet
        if (hasBetThisRound && currentBetAmount > 0)
        {
            switch (result)
            {
                case RoundResult.Win:
                    // Restore HP on win
                    player.health += currentBetAmount;
                    player.UpdateHealthBar();
                    Debug.Log($"[ImpulsiveGamblerEffect] Won! Restored {currentBetAmount} HP");
                    break;
                case RoundResult.Draw:
                    // Restore HP on draw
                    player.health += currentBetAmount;
                    player.UpdateHealthBar();
                    Debug.Log($"[ImpulsiveGamblerEffect] Draw! Restored {currentBetAmount} HP");
                    break;
                case RoundResult.Lose:
                    // HP stays lost
                    Debug.Log($"[ImpulsiveGamblerEffect] Lost! {currentBetAmount} HP remains lost");
                    break;
            }
        }

        // ALWAYS process bet tracking (even with 0 bets)
        // Only process bet tracking if damage reduction is not active or pending
        if (!damageReductionActive && !hasDamageReductionNextRound)
        {
            // Check if this was a high bet
            float highBetMinimum = player.maxHealth * highBetThreshold;
            bool wasHighBet = currentBetAmount >= highBetMinimum;
            Debug.Log($"[ImpulsiveGamblerEffect] Bet {currentBetAmount} vs threshold {highBetMinimum}. High bet: {wasHighBet}");

            if (wasHighBet)
            {
                consecutiveHighBets++;
                Debug.Log($"[ImpulsiveGamblerEffect] High bet! Progress: {consecutiveHighBets}/{requiredHighBets}");
                // Check if we've reached the target
                if (consecutiveHighBets >= requiredHighBets)
                {
                    hasDamageReductionNextRound = true;
                    Debug.Log("[ImpulsiveGamblerEffect] Target reached! Damage reduction next round!");
                    // Keep counter at max - don't reset
                }
            }
            else // ANY non-high bet (including 0) breaks the streak
            {
                consecutiveHighBets = 0; // Reset streak
                Debug.Log("[ImpulsiveGamblerEffect] Low/no bet broke the streak. Reset to 0.");
            }
        }

        // Handle damage reduction consumption separately
        if (damageReductionActive)
        {
            damageReductionActive = false;
            consecutiveHighBets = 0; // Reset counter AFTER using the reduction
            Debug.Log("[ImpulsiveGamblerEffect] Damage reduction consumed. Counter reset to 0.");
        }

        UpdateUIProgress();

        // IMPORTANT: Reset the bet state for the next round
        hasBetThisRound = false;
        // Keep currentBetAmount so player can see their last bet, but reset the "used" flag
        Debug.Log($"[ImpulsiveGamblerEffect] Round complete. Bet state reset for next round.");
    }

    public override int GetFlatDamageBonus(string signUsed)
    {
        if (hasBetThisRound && currentBetAmount > 0)
        {
            int bonus = GetBonusDamage();
            Debug.Log($"[ImpulsiveGamblerEffect] Providing {bonus} flat damage bonus from bet");
            return bonus;
        }
        return 0;
    }

    public override void OnRoomStart()
    {
        Debug.Log($"[ImpulsiveGamblerEffect] OnRoomStart called - Resetting bet state for new room");

        // Reset bet state completely when entering a new room
        currentBetAmount = 0;
        hasBetThisRound = false;

        // Reset all streak progress and clear any queued/active damage reduction
        consecutiveHighBets = 0;
        hasDamageReductionNextRound = false;
        damageReductionActive = false;

        // Update the UI to reflect the reset streak and cleared buffs
        UpdateUIProgress();

        // Reset the UI slider to match the reset bet amount
        if (gamblerUI != null && gamblerUI.gameObject.activeInHierarchy)
        {
            gamblerUI.ResetSlider();
            Debug.Log("[ImpulsiveGamblerEffect] Reset GamblerUI slider to 0");
        }

        Debug.Log($"[ImpulsiveGamblerEffect] Room reset complete - currentBetAmount: {currentBetAmount}, hasBetThisRound: {hasBetThisRound}, consecutiveHighBets: {consecutiveHighBets}, damageReductionActive: {damageReductionActive}");
    }

    public override void Cleanup()
    {
        Debug.Log("[ImpulsiveGamblerEffect] Cleanup called");
        if (gamblerUI != null)
        {
            gamblerUI.gameObject.SetActive(false);
            Debug.Log("[ImpulsiveGamblerEffect] GamblerUI deactivated");
        }
    }

    public int GetSnappedBetAmount(int rawAmount)
    {
        // Round down to nearest multiple of hpCost
        int snappedAmount = (rawAmount / hpCost) * hpCost;
        return Mathf.Min(snappedAmount, GetMaxBet());
    }

    public override void ModifyIncomingDamage(ref int damage, HandController source)
    {
        if (damageReductionActive && source != player)
        {
            int originalDamage = damage;
            damage = Mathf.RoundToInt(damage * (1f - damageReduction));
            Debug.Log($"[ImpulsiveGamblerEffect] Reduced incoming damage from {originalDamage} to {damage} ({damageReduction * 100}% reduction)");
        }
    }

    private void UpdateUIProgress()
    {
        if (gamblerUI != null)
        {
            // This will call the new method we'll add to GamblerUI
            gamblerUI.UpdateImpulsiveProgress(consecutiveHighBets, requiredHighBets);
        }
    }

    public void CheckAndUpdateHighBetProgress()
    {
        // Only update if damage reduction is not active or pending
        if (!damageReductionActive && !hasDamageReductionNextRound)
        {
            float highBetMinimum = player.maxHealth * highBetThreshold;
            bool isHighBet = currentBetAmount >= highBetMinimum;

            if (isHighBet)
            {
                // Temporarily increment to show progress
                int tempCount = consecutiveHighBets + 1;
                if (tempCount > requiredHighBets)
                    tempCount = requiredHighBets;

                // Update UI to show projected progress
                if (gamblerUI != null)
                {
                    gamblerUI.UpdateImpulsiveProgress(tempCount, requiredHighBets);
                }
            }
        }
    }
}