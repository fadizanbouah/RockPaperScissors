using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    private HandController playerInstance;   // Player instance
    public TextMeshProUGUI resultText;
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;

    private string[] choices = { "Rock", "Paper", "Scissors" };
    private bool isRoundActive = false;
    private HandController enemyHandController;  // Reference to enemy hand animations

    // Updated method to initialize the game with a player instance
    public void InitializeGame(HandController player, HandController enemy)
    {
        Debug.Log("Initializing game...");

        if (player != null)
        {
            playerInstance = player;
        }
        else
        {
            Debug.LogError("Player instance is missing!");
            return;
        }

        if (enemy != null)
        {
            enemyHandController = enemy;
        }
        else
        {
            Debug.LogError("Enemy instance is missing!");
            return;
        }

        DisableButtons();  // Disable buttons until the game starts
        resultText.text = "";
    }

    public void StartGame()
    {
        EnableButtons();  // Enable buttons after game starts
    }

    public void PlayerSelect(string playerChoice)
    {
        if (isRoundActive) return; // Prevent multiple selections

        isRoundActive = true;  // Lock input until the round ends
        DisableButtons();

        Debug.Log("Player selected: " + playerChoice);

        // Start player hand animation first
        if (playerInstance != null)
        {
            playerInstance.StartShaking(playerChoice);
        }
        else
        {
            Debug.LogError("Player instance is null when trying to shake!");
        }

        // Enemy chooses immediately
        var storedEnemyChoice = choices[Random.Range(0, choices.Length)];
        if (enemyHandController != null)
        {
            enemyHandController.StartShaking(storedEnemyChoice);
            Debug.Log("Enemy has pre-selected: " + storedEnemyChoice);
        }
        else
        {
            Debug.LogError("EnemyHandController is null! Cannot execute enemy actions.");
        }

        StartCoroutine(WaitForEnemyHand(playerChoice, storedEnemyChoice));
    }

    private IEnumerator WaitForEnemyHand(string playerChoice, string enemyChoice)
    {
        yield return new WaitForSeconds(1.0f);  // Ensure animation finishes

        Debug.Log("Updating enemy hand with choice: " + enemyChoice);

        DetermineOutcome(playerChoice, enemyChoice);

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
                    damage = playerInstance.rockDamage;
                    break;
                case "Paper":
                    damage = playerInstance.paperDamage;
                    break;
                case "Scissors":
                    damage = playerInstance.scissorsDamage;
                    break;
            }

            result = "You Win!";
            enemyHandController.TakeDamage(damage);
        }
        else
        {
            int damage = 0;
            switch (enemyChoice)
            {
                case "Rock":
                    damage = enemyHandController.rockDamage;
                    break;
                case "Paper":
                    damage = enemyHandController.paperDamage;
                    break;
                case "Scissors":
                    damage = enemyHandController.scissorsDamage;
                    break;
            }

            result = "You Lose!";
            playerInstance.TakeDamage(damage);
        }

        resultText.text = result;
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
