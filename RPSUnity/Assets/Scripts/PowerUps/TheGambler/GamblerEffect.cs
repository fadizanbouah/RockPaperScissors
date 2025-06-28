using UnityEngine;

public class GamblerEffect : PowerUpEffectBase
{
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
    }

    private void CreateGamblerUI()
    {
        if (uiCreated)
        {
            Debug.Log("[GamblerEffect] UI already created, skipping");
            return;
        }

        Debug.Log("[GamblerEffect] CreateGamblerUI called");

        // Find or create the UI
        gamblerUI = FindObjectOfType<GamblerUI>();
        if (gamblerUI == null)
        {
            // Load the prefab from Resources
            GameObject uiPrefab = Resources.Load<GameObject>("GamblerUI");
            if (uiPrefab != null)
            {
                Debug.Log("[GamblerEffect] GamblerUI prefab loaded");

                // Find the GameplayCanvas to parent the UI
                Canvas gameplayCanvas = GameObject.Find("GameplayCanvas")?.GetComponent<Canvas>();
                if (gameplayCanvas == null)
                {
                    // Try to find any canvas tagged as gameplay
                    Canvas[] allCanvases = FindObjectsOfType<Canvas>();
                    foreach (var canvas in allCanvases)
                    {
                        if (canvas.gameObject.name.Contains("Gameplay") || canvas.gameObject.name.Contains("Game"))
                        {
                            gameplayCanvas = canvas;
                            break;
                        }
                    }
                }

                GameObject uiInstance = Instantiate(uiPrefab);

                // Parent to the gameplay canvas
                if (gameplayCanvas != null)
                {
                    uiInstance.transform.SetParent(gameplayCanvas.transform, false);
                    Debug.Log($"[GamblerEffect] UI parented to {gameplayCanvas.name}");
                }
                else
                {
                    Debug.LogWarning("[GamblerEffect] Could not find GameplayCanvas! UI may not display correctly.");
                }

                gamblerUI = uiInstance.GetComponent<GamblerUI>();
            }
            else
            {
                Debug.LogError("[GamblerEffect] GamblerUI prefab not found in Resources folder!");
                return;
            }
        }

        if (gamblerUI != null)
        {
            gamblerUI.Initialize(this, player);
            gamblerUI.Show();
            uiCreated = true;
            Debug.Log("[GamblerEffect] GamblerUI created and initialized successfully");
        }
    }

    public void SetBetAmount(int amount)
    {
        Debug.Log($"[GamblerEffect] SetBetAmount called with amount: {amount}");

        // Check if player is null
        if (player == null)
        {
            Debug.LogError("[GamblerEffect] Player is null! Cannot set bet amount.");
            return;
        }

        // Ensure bet doesn't exceed current HP - 1
        int maxBet = Mathf.Max(0, player.CurrentHealth - 1);
        currentBetAmount = Mathf.Clamp(amount, 0, maxBet);

        Debug.Log($"[GamblerEffect] Bet amount set to: {currentBetAmount} (max was {maxBet})");
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
        return Mathf.Max(0, player.CurrentHealth - 1);
    }

    public int GetBonusDamage()
    {
        // 2:1 ratio - every 2 HP bet gives 1 damage
        return currentBetAmount / 2;
    }

    public override void OnRoundStart()
    {
        Debug.Log($"[GamblerEffect] OnRoundStart - Player: {player?.name ?? "NULL"}");

        if (player == null)
        {
            Debug.LogError("[GamblerEffect] Player is null in OnRoundStart!");
            return;
        }

        if (currentBetAmount > 0 && !hasBetThisRound)
        {
            // Deduct the bet HP at round start
            player.health -= currentBetAmount;
            player.UpdateHealthBar();
            hasBetThisRound = true;

            Debug.Log($"[GamblerEffect] Bet {currentBetAmount} HP for +{GetBonusDamage()} damage");
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        if (!hasBetThisRound || currentBetAmount == 0) return;

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

        hasBetThisRound = false;
    }

    public override void ModifyDamageMultiplier(ref float multiplier, string signUsed)
    {
        if (hasBetThisRound && currentBetAmount > 0)
        {
            // Calculate how much to add to multiplier for flat damage bonus
            int bonusDamage = GetBonusDamage();

            // Add the bonus as additional multiplier
            // If base damage is 25 and we want +15 bonus, multiplier becomes (25+15)/25 = 1.6
            if (player != null && player.baseDamage > 0)
            {
                float bonusMultiplier = (float)bonusDamage / player.baseDamage;
                multiplier += bonusMultiplier;
                Debug.Log($"[GamblerEffect] Added {bonusMultiplier} to multiplier for +{bonusDamage} damage bonus");
            }
        }
    }

    public override void Cleanup()
    {
        if (gamblerUI != null)
        {
            gamblerUI.Hide();
            Destroy(gamblerUI.gameObject);
        }
    }
}