using UnityEngine;
using TMPro;
using System.Collections;

public class RockPaperScissorsGame : MonoBehaviour
{
    public PlayerController player;     // Reference to the player script
    public EnemyBehavior enemyBehavior; // Reference to the enemy script
    public HealthBar playerHealthBar;   // Reference to the player's health bar
    public HealthBar enemyHealthBar;    // Reference to the enemy's health bar
    public TextMeshProUGUI playerChoiceText;
    public TextMeshProUGUI enemyChoiceText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public EnemyHandController enemyHandController;  // Reference to enemy hand animations
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;

    private string[] choices = { "Rock", "Paper", "Scissors" };
    private string storedEnemyChoice;
    private bool isRoundActive = false;

    void Start()
    {
        UpdateHealthUI();
    }

    public void PlayerSelect(string playerChoice)
    {
        if (isRoundActive) return; // Prevent multiple selections

        isRoundActive = true;  // Lock input until the round ends
        DisableButtons();

        Debug.Log("Player selected: " + playerChoice);
        playerChoiceText.text = "Player Chose: " + playerChoice;

        // Enemy chooses immediately
        storedEnemyChoice = enemyBehavior.MakeChoice();
        enemyChoiceText.text = "Enemy Chose: " + storedEnemyChoice;
        Debug.Log("Enemy has pre-selected: " + storedEnemyChoice);

        if (enemyHandController != null)
        {
            enemyHandController.StartShaking();
            StartCoroutine(WaitForEnemyHand(playerChoice));
        }
        else
        {
            Debug.LogError("EnemyHandController is not assigned!");
        }
    }

    private IEnumerator WaitForEnemyHand(string playerChoice)
    {
        yield return new WaitForSeconds(1.0f);  // Ensure animation finishes

        Debug.Log("Updating enemy hand with choice: " + storedEnemyChoice);
        enemyChoiceText.text = "Enemy Chose: " + storedEnemyChoice;

        if (enemyHandController != null)
        {
            enemyHandController.SetHandChoice(storedEnemyChoice);
        }

        DetermineOutcome(playerChoice, storedEnemyChoice);

        yield return new WaitForSeconds(1.0f);  // Placeholder for future animations
        EnableButtons();
        isRoundActive = false;  // Unlock input for next round
    }

    private void DetermineOutcome(string playerChoice, string enemyChoice)
    {
        string result = "";

        if (playerChoice == enemyChoice)
        {
            result = "It's a Draw!";
        }
        else if ((playerChoice == "Rock" && enemyChoice == "Scissors") ||
                 (playerChoice == "Paper" && enemyChoice == "Rock") ||
                 (playerChoice == "Scissors" && enemyChoice == "Paper"))
        {
            int damage = 0;
            switch (playerChoice)
            {
                case "Rock":
                    damage = player.rockDamage;
                    break;
                case "Paper":
                    damage = player.paperDamage;
                    break;
                case "Scissors":
                    damage = player.scissorsDamage;
                    break;
            }

            result = "You Win!";
            enemyBehavior.TakeDamage(damage);
        }
        else
        {
            int damage = 0;
            switch (enemyChoice)
            {
                case "Rock":
                    damage = enemyBehavior.rockDamage;
                    break;
                case "Paper":
                    damage = enemyBehavior.paperDamage;
                    break;
                case "Scissors":
                    damage = enemyBehavior.scissorsDamage;
                    break;
            }

            result = "You Lose!";
            player.TakeDamage(damage);
        }

        resultText.text = result;
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        playerHealthBar.SetHealth(player.health, player.maxHealth);
        enemyHealthBar.SetHealth(enemyBehavior.health, enemyBehavior.maxHealth);

        playerHealthText.text = player.health + " / " + player.maxHealth;
        enemyHealthText.text = enemyBehavior.health + " / " + enemyBehavior.maxHealth;
    }

    private void DisableButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
    }

    private void EnableButtons()
    {
        rockButton.interactable = true;
        paperButton.interactable = true;
        scissorsButton.interactable = true;
    }
}
