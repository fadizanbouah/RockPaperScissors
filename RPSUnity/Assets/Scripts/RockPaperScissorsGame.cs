using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    private enum GameSubstate
    {
        Idle,         // Waiting for player input
        Selecting,    // Player & Enemy choosing actions
        Resolving,    // Playing attack animations & applying damage
        Transitioning // Waiting for next round/enemy spawn
    }

    private GameSubstate currentSubstate = GameSubstate.Idle;

    private HandController playerInstance;
    public TextMeshProUGUI resultText;
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;

    private string[] choices = { "Rock", "Paper", "Scissors" };
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
        currentSubstate = GameSubstate.Idle;
        EnableButtons();

        Debug.Log("Game successfully initialized. Waiting for player input...");
    }

    public void StartGame()
    {
        currentSubstate = GameSubstate.Idle;
        EnableButtons();
        Debug.Log("Game started! Waiting for player to make a selection...");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (currentSubstate != GameSubstate.Idle) return; // Prevent actions if not in idle

        currentSubstate = GameSubstate.Selecting;
        DisableButtons();

        Debug.Log($"Player selected: {playerChoice}");

        if (playerInstance != null)
        {
            playerInstance.StartShaking(playerChoice);
        }

        string enemyChoice = choices[Random.Range(0, choices.Length)];

        if (enemyHandController != null)
        {
            enemyHandController.StartShaking(enemyChoice);
            Debug.Log($"Enemy has pre-selected: {enemyChoice}");
            StartCoroutine(ResolveRound(playerChoice, enemyChoice));
        }
        else
        {
            Debug.LogError("EnemyHandController is null! Cannot execute enemy actions.");
            currentSubstate = GameSubstate.Idle;
            EnableButtons();
        }
    }

    private IEnumerator ResolveRound(string playerChoice, string enemyChoice)
    {
        yield return new WaitForSeconds(1.0f); // Wait for animations

        if (enemyHandController == null) yield break; // Ensure enemy still exists

        Debug.Log($"Resolving round: {playerChoice} vs {enemyChoice}");
        currentSubstate = GameSubstate.Resolving;
        DetermineOutcome(playerChoice, enemyChoice);

        yield return new WaitForSeconds(1.5f); // Allow time for damage animations

        // Check if the enemy is still alive
        if (enemyHandController != null && enemyHandController.health <= 0)
        {
            currentSubstate = GameSubstate.Transitioning;
            yield return new WaitUntil(() => enemyHandController == null); // Wait for enemy to be destroyed
        }

        Debug.Log("Round complete. Returning to idle state.");
        currentSubstate = GameSubstate.Idle;
        EnableButtons();
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
            int damage = GetDamage(playerChoice, playerInstance);
            result = "You Win!";
            if (enemyHandController != null)
            {
                enemyHandController.TakeDamage(damage);
                Debug.Log($"Player dealt {damage} damage to Enemy {enemyHandController.name}.");
            }
        }
        else
        {
            int damage = GetDamage(enemyChoice, enemyHandController);
            result = "You Lose!";
            playerInstance.TakeDamage(damage);
            Debug.Log($"Enemy {enemyHandController.name} dealt {damage} damage to Player.");
        }

        resultText.text = result;
    }

    private int GetDamage(string choice, HandController hand)
    {
        switch (choice)
        {
            case "Rock": return hand.rockDamage;
            case "Paper": return hand.paperDamage;
            case "Scissors": return hand.scissorsDamage;
            default: return 0;
        }
    }

    private void DisableButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
        Debug.Log("Buttons disabled.");
    }

    private void EnableButtons()
    {
        if (currentSubstate == GameSubstate.Idle)
        {
            rockButton.interactable = true;
            paperButton.interactable = true;
            scissorsButton.interactable = true;
            Debug.Log("Buttons enabled. Ready for next selection.");
        }
    }
}
