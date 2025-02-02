using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    public List<HandController> EnemiesList; // List of enemies
    private HandController playerInstance;   // Player instance
    public TextMeshProUGUI playerChoiceText;
    public TextMeshProUGUI enemyChoiceText;
    public TextMeshProUGUI resultText;
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;

    private string[] choices = { "Rock", "Paper", "Scissors" };
    private bool isRoundActive = false;
    private HandController enemyHandController;  // Reference to enemy hand animations

    void Start()
    {
        // Hide buttons at the start
        DisableButtons();
    }

    // Updated method to initialize the game with a player instance
    public void InitializeGame(HandController player)
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

        if (EnemiesList == null || EnemiesList.Count == 0)
        {
            Debug.LogError("No enemies assigned to the list!");
        }

        InitializeButtons();  // Initialize buttons
    }

    // New method to initialize button functionality
    private void InitializeButtons()
    {
        if (rockButton == null || paperButton == null || scissorsButton == null)
        {
            Debug.LogError("One or more buttons are missing from the UI!");
            return;
        }

        rockButton.onClick.AddListener(() => PlayerSelect("Rock"));
        paperButton.onClick.AddListener(() => PlayerSelect("Paper"));
        scissorsButton.onClick.AddListener(() => PlayerSelect("Scissors"));

        Debug.Log("Rock, Paper, Scissors buttons have been initialized!");
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

        // Show buttons when gameplay starts
        EnableButtons();
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
        enemyHandController.StartShaking(storedEnemyChoice);
        Debug.Log("Enemy has pre-selected: " + storedEnemyChoice);

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
        rockButton.gameObject.SetActive(false);
        paperButton.gameObject.SetActive(false);
        scissorsButton.gameObject.SetActive(false);
    }

    private void EnableButtons()
    {
        rockButton.gameObject.SetActive(true);
        paperButton.gameObject.SetActive(true);
        scissorsButton.gameObject.SetActive(true);
    }
}
