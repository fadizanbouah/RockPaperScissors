using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    public List<HandController> EnemiesList; // This is a list with enemies
    public HandController player;     // Reference to the player script
    public TextMeshProUGUI playerChoiceText;
    public TextMeshProUGUI enemyChoiceText;
    public TextMeshProUGUI resultText;
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;

    private string[] choices = { "Rock", "Paper", "Scissors" };
    private bool isRoundActive = false;
    private HandController enemyHandController;  // Reference to enemy hand animations

    // New method to initialize the game
    public void InitializeGame()
    {
        Debug.Log("Initializing game...");

        // Ensure UI elements and gameplay objects are properly set up before starting
        if (player == null)
        {
            Debug.LogError("Player reference is missing!");
        }

        if (EnemiesList == null || EnemiesList.Count == 0)
        {
            Debug.LogError("No enemies assigned to the list!");
        }

        DisableButtons();  // Disable buttons until the game starts
    }

    public void StartGame()
    {
        if (enemyHandController == null)
        {
            if (EnemiesList != null && EnemiesList.Count > 0)
            {
                enemyHandController = Instantiate(EnemiesList[Random.Range(0, EnemiesList.Count)]);
            }
            else
            {
                Debug.LogError("No enemies assigned to the list!");
            }
        }
        else
        {
            Debug.LogWarning("Enemy already exists. Skipping instantiation.");
        }

        EnableButtons();  // Enable buttons after game starts
    }

    public void PlayerSelect(string playerChoice)
    {
        if (isRoundActive) return; // Prevent multiple selections

        isRoundActive = true;  // Lock input until the round ends
        DisableButtons();

        Debug.Log("Player selected: " + playerChoice);
        playerChoiceText.text = "Player Chose: " + playerChoice;

        // Enemy chooses immediately
        var storedEnemyChoice = choices[Random.Range(0, choices.Length)];
        enemyHandController.StartShaking(storedEnemyChoice);
        enemyChoiceText.text = "Enemy Chose: " + storedEnemyChoice;
        Debug.Log("Enemy has pre-selected: " + storedEnemyChoice);

        StartCoroutine(WaitForEnemyHand(playerChoice, storedEnemyChoice));
    }

    private IEnumerator WaitForEnemyHand(string playerChoice, string enemyChoice)
    {
        yield return new WaitForSeconds(1.0f);  // Ensure animation finishes

        Debug.Log("Updating enemy hand with choice: " + enemyChoice);
        enemyChoiceText.text = "Enemy Chose: " + enemyChoice;

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
            player.TakeDamage(damage);
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
