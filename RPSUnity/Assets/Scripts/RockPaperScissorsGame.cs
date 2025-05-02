using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    private enum GameSubstate
    {
        Idle,
        Selecting,
        Resolving,
        Transitioning
    }

    private GameSubstate currentSubstate = GameSubstate.Idle;

    private HandController playerInstance;
    private HandController enemyHandController;

    public TextMeshProUGUI resultText;
    public UnityEngine.UI.Button rockButton;
    public UnityEngine.UI.Button paperButton;
    public UnityEngine.UI.Button scissorsButton;
    public GameObject roomClearedTextObject;  // Reference for RoomCleared text object

    private string[] choices = { "Rock", "Paper", "Scissors" };

    private bool playerSignDone = false;
    private bool enemySignDone = false;

    public void InitializeGame(HandController player, HandController enemy)
    {
        Debug.Log("Initializing game...");

        playerInstance = player;
        enemyHandController = enemy;

        if (playerInstance == null || enemyHandController == null)
        {
            Debug.LogError("Player or Enemy instance is missing!");
            return;
        }

        playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
        enemyHandController.OnDeath -= OnEnemyDefeated;

        playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
        enemyHandController.OnDeath += OnEnemyDefeated;

        resultText.text = "";
        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();

        Debug.Log("Game successfully initialized.");
    }

    public void StartGame()
    {
        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();
        Debug.Log("Game started!");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (currentSubstate != GameSubstate.Idle) return;

        currentSubstate = GameSubstate.Selecting;
        DisableButtons();

        playerSignDone = false;
        enemySignDone = false;

        Debug.Log($"Player selected: {playerChoice}");
        playerInstance.StartShaking(playerChoice);

        string enemyChoice = choices[Random.Range(0, choices.Length)];
        enemyHandController.StartShaking(enemyChoice);
        Debug.Log($"Enemy has pre-selected: {enemyChoice}");

        StartCoroutine(ResolveRound(playerChoice, enemyChoice));
    }

    private IEnumerator ResolveRound(string playerChoice, string enemyChoice)
    {
        yield return new WaitForSeconds(1.0f);

        if (enemyHandController == null) yield break;

        currentSubstate = GameSubstate.Resolving;
        Debug.Log($"Resolving round: {playerChoice} vs {enemyChoice}");

        DetermineOutcome(playerChoice, enemyChoice);

        yield return new WaitUntil(() => playerSignDone && enemySignDone);
        Debug.Log("Both sign animations finished.");

        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();
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
            int damage = playerInstance.GetEffectiveDamage();
            result = "You Win!";
            enemyHandController?.TakeDamage(damage);
            Debug.Log($"Player dealt {damage} damage to {enemyHandController?.name}");
        }
        else
        {
            int damage = enemyHandController.GetEffectiveDamage();
            result = "You Lose!";
            playerInstance.TakeDamage(damage);
            Debug.Log($"Enemy {enemyHandController?.name} dealt {damage} damage to Player");
        }

        resultText.text = result;
    }

    private void DisableButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
        Debug.Log("Buttons disabled.");
    }

    private void AllowPlayerInput()
    {
        if (currentSubstate == GameSubstate.Idle && enemyHandController != null)
        {
            rockButton.interactable = true;
            paperButton.interactable = true;
            scissorsButton.interactable = true;
            Debug.Log("Buttons enabled.");
        }
    }

    public void UpdateEnemyReference(HandController newEnemy)
    {
        if (enemyHandController != null)
        {
            enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
            enemyHandController.OnDeath -= OnEnemyDefeated;
        }

        enemyHandController = newEnemy;

        if (enemyHandController != null)
        {
            enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
            enemyHandController.OnDeath += OnEnemyDefeated;
        }

        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();
    }

    private void OnPlayerSignAnimationFinished(HandController hand)
    {
        playerSignDone = true;
    }

    private void OnEnemySignAnimationFinished(HandController hand)
    {
        enemySignDone = true;
    }

    private void OnEnemyDefeated(HandController hand)
    {
        if (!hand.isPlayer)
        {
            PlayerProgressData.Instance.coins += hand.coinReward;
            PlayerProgressData.Save();
            Debug.Log($"Gained {hand.coinReward} coins! Total: {PlayerProgressData.Instance.coins}");

            RunProgressManager.Instance.AddFavor(hand.favorReward);
            Debug.Log($"Gained {hand.favorReward} favor! Total: {RunProgressManager.Instance.currentFavor}");
        }
    }
}
