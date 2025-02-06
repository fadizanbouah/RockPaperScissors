using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    private HandController playerInstance;
    public TextMeshProUGUI resultText;
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;

    private string[] choices = { "Rock", "Paper", "Scissors" };
    private bool isRoundActive = false;
    private HandController enemyHandController;

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

        DisableButtons();
        resultText.text = "";
    }

    public void StartGame()
    {
        EnableButtons();
    }

    public void PlayerSelect(string playerChoice)
    {
        if (isRoundActive || enemyHandController == null) return; // Prevent actions if the enemy is null

        isRoundActive = true;
        DisableButtons();

        Debug.Log("Player selected: " + playerChoice);

        if (playerInstance != null)
        {
            playerInstance.StartShaking(playerChoice);
        }

        var storedEnemyChoice = choices[Random.Range(0, choices.Length)];

        if (enemyHandController != null)
        {
            enemyHandController.StartShaking(storedEnemyChoice);
            Debug.Log("Enemy has pre-selected: " + storedEnemyChoice);
            StartCoroutine(WaitForEnemyHand(playerChoice, storedEnemyChoice));
        }
        else
        {
            Debug.LogError("EnemyHandController is null! Cannot execute enemy actions.");
            isRoundActive = false;
            EnableButtons();
        }
    }

    private IEnumerator WaitForEnemyHand(string playerChoice, string enemyChoice)
    {
        yield return new WaitForSeconds(1.0f);

        if (enemyHandController == null) yield break; // Ensure enemy still exists

        Debug.Log("Updating enemy hand with choice: " + enemyChoice);
        DetermineOutcome(playerChoice, enemyChoice);

        yield return new WaitForSeconds(1.0f);
        EnableButtons();
        isRoundActive = false;
    }

    private void DetermineOutcome(string playerChoice, string enemyChoice)
    {
        if (enemyHandController == null) return; // Prevent further actions if the enemy no longer exists

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
            if (enemyHandController != null)
            {
                enemyHandController.TakeDamage(damage);
            }
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
