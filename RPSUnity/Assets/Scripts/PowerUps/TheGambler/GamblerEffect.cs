using UnityEngine;

public class GamblerEffect : PowerUpEffectBase
{
    private int currentBetAmount = 0;
    private bool hasBetThisRound = false;
    private GamblerUI gamblerUI;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log("[GamblerEffect] The Gambler effect activated!");

        // Create the UI
        CreateGamblerUI();
    }

    private void CreateGamblerUI()
    {
        // Find or create the UI
        gamblerUI = FindObjectOfType<GamblerUI>();
        if (gamblerUI == null)
        {
            // Load the prefab from Resources
            GameObject uiPrefab = Resources.Load<GameObject>("GamblerUI");
            if (uiPrefab != null)
            {
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
            }
        }

        if (gamblerUI != null)
        {
            gamblerUI.Initialize(this, player);
            gamblerUI.Show();
            Debug.Log("[GamblerEffect] GamblerUI created and initialized successfully");
        }
    }

    public void SetBetAmount(int amount)
    {
        // Ensure bet doesn't exceed current HP - 1
        int maxBet = Mathf.Max(0, player.CurrentHealth - 1);
        currentBetAmount = Mathf.Clamp(amount, 0, maxBet);

        Debug.Log($"[GamblerEffect] Bet amount set to: {currentBetAmount}");
    }

    public int GetCurrentBet()
    {
        return currentBetAmount;
    }

    public int GetMaxBet()
    {
        return Mathf.Max(0, player.CurrentHealth - 1);
    }

    public int GetBonusDamage()
    {
        // 2:1 ratio - every 2 HP bet gives 1 damage
        return currentBetAmount / 2;
    }

    public override void OnRoundStart()
    {
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

    public override void ModifyDamage(ref int damage, string signUsed)
    {
        if (hasBetThisRound && currentBetAmount > 0)
        {
            damage += GetBonusDamage();
            Debug.Log($"[GamblerEffect] Added {GetBonusDamage()} bonus damage from bet");
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