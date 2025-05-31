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
        Dying,
        EnemySpawn,
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

        // Unsubscribe first to avoid duplicate listeners
        playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
        enemyHandController.OnDeath -= OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;

        // Subscribe to necessary events
        playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
        enemyHandController.OnDeath += OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;

        PowerUpEffectManager.Instance?.Initialize(player, enemy);

        resultText.text = "";
        currentSubstate = GameSubstate.EnemySpawn;
        Debug.Log("[GameSubstate] Entering ENEMY SPAWN state.");
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

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
        {
            spawner.SetAllCardsInteractable(false);
        }

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
        //yield return new WaitForSeconds(1.0f); <- DON'T DELETE THIS PLEASE

        if (enemyHandController == null) yield break;

        currentSubstate = GameSubstate.Resolving;
        Debug.Log($"Resolving round: {playerChoice} vs {enemyChoice}");

        yield return new WaitUntil(() => playerSignDone && enemySignDone);
        Debug.Log("Both sign animations finished.");

        RoundResult result = DetermineOutcome(playerChoice, enemyChoice);

        yield return new WaitForSeconds(0.5f);

        if (enemyHandController != null && enemyHandController.CurrentHealth <= 0)
        {
            currentSubstate = GameSubstate.Dying;
            Debug.Log("Enemy defeated. Entering DYING state...");
        }
        else
        {
            currentSubstate = GameSubstate.Idle;
            Debug.Log("[GameSubstate] No one died. Returning to IDLE state.");
            AllowPlayerInput();
        }
    }

    private RoundResult DetermineOutcome(string playerChoice, string enemyChoice)
    {
        RoundResult result;

        if (playerChoice == enemyChoice)
        {
            result = RoundResult.Draw;
            resultText.text = "It's a Draw!";
        }
        else if ((playerChoice == "Rock" && enemyChoice == "Scissors") ||
                 (playerChoice == "Paper" && enemyChoice == "Rock") ||
                 (playerChoice == "Scissors" && enemyChoice == "Paper"))
        {
            int damage = playerInstance.GetEffectiveDamage(playerChoice);
            result = RoundResult.Win;
            resultText.text = "You Win!";
            enemyHandController?.TakeDamage(damage);
            Debug.Log($"Player dealt {damage} damage to {enemyHandController?.name}");
        }
        else
        {
            int damage = enemyHandController.GetEffectiveDamage(enemyChoice);
            result = RoundResult.Lose;
            resultText.text = "You Lose!";
            playerInstance.TakeDamage(damage);
            Debug.Log($"Enemy {enemyHandController?.name} dealt {damage} damage to Player");
        }

        return result;
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

            PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
            if (spawner != null)
            {
                spawner.SetAllCardsInteractable(true);
            }
        }
    }

    public void UpdateEnemyReference(HandController newEnemy)
    {
        if (enemyHandController != null)
        {
            enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
            enemyHandController.OnDeath -= OnEnemyDefeated;
            enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;
        }

        enemyHandController = newEnemy;

        if (enemyHandController != null)
        {
            enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
            enemyHandController.OnDeath += OnEnemyDefeated;
            enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;
        }

        currentSubstate = GameSubstate.EnemySpawn;
        Debug.Log("[GameSubstate] New enemy spawned. Entering ENEMY SPAWN state...");
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

    private void OnEnemyDeathAnimationFinished(HandController hand)
    {
        if (currentSubstate == GameSubstate.Dying)
        {
            Debug.Log("Enemy death animation completed. Entering ENEMYSPAWN state...");
            currentSubstate = GameSubstate.EnemySpawn;

            // Listen for when the next enemy is spawned
            RoomManager.Instance.OnEnemySpawned += OnEnemySpawned;
        }
    }

    private void OnEnemySpawned()
    {
        Debug.Log("EnemySpawn complete. Returning to IDLE and enabling input.");
        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();

        // Important: Unsubscribe so we don't trigger this multiple times
        RoomManager.Instance.OnEnemySpawned -= OnEnemySpawned;
    }

    private IEnumerator WaitAndEnableInputAfterEnemySpawn()
    {
        yield return new WaitForSeconds(0.1f); // Buffer in case new enemy takes a frame to initialize
        Debug.Log("[GameSubstate] Returning to IDLE after enemy spawn. Enabling input.");
        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();
    }
}
