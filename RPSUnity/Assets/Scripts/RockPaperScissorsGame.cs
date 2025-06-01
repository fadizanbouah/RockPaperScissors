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
        Resolving_EvaluateOutcome,
        Resolving_TakeDamage,
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
    public GameObject roomClearedTextObject;

    private string[] choices = { "Rock", "Paper", "Scissors" };

    private bool playerSignDone = false;
    private bool enemySignDone = false;
    private bool playerHitDone = false;
    private bool enemyHitDone = false;

    public void InitializeGame(HandController player, HandController enemy)
    {
        playerInstance = player;
        enemyHandController = enemy;

        if (playerInstance == null || enemyHandController == null)
        {
            Debug.LogError("Player or Enemy instance is missing!");
            return;
        }

        // Unsubscribe to prevent duplicate listeners
        playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
        enemyHandController.OnDeath -= OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;
        playerInstance.OnDeathAnimationFinished -= OnPlayerDeathAnimationFinished;
        playerInstance.HitAnimationFinished -= OnPlayerHitAnimationFinished;
        enemyHandController.HitAnimationFinished -= OnEnemyHitAnimationFinished;

        // Subscribe to necessary events
        playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
        enemyHandController.OnDeath += OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;
        playerInstance.OnDeathAnimationFinished += OnPlayerDeathAnimationFinished;
        playerInstance.HitAnimationFinished += OnPlayerHitAnimationFinished;
        enemyHandController.HitAnimationFinished += OnEnemyHitAnimationFinished;

        PowerUpEffectManager.Instance?.Initialize(player, enemy);

        resultText.text = "";
        currentSubstate = GameSubstate.EnemySpawn;
        Debug.Log("[GameSubstate] Entering ENEMY SPAWN state.");
    }

    public void StartGame()
    {
        EnterIdleState();
        Debug.Log("Game started!");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (currentSubstate != GameSubstate.Idle) return;

        currentSubstate = GameSubstate.Selecting;
        DisableButtons();

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);

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
        currentSubstate = GameSubstate.Resolving_EvaluateOutcome;
        yield return new WaitUntil(() => playerSignDone && enemySignDone);
        Debug.Log($"[GameSubstate] Resolving_EvaluateOutcome: {playerChoice} vs {enemyChoice}");

        RoundResult result = DetermineOutcome(playerChoice, enemyChoice);

        currentSubstate = GameSubstate.Resolving_TakeDamage;
        yield return StartCoroutine(HandleTakeDamage(result, playerChoice, enemyChoice));
    }

    private RoundResult DetermineOutcome(string playerChoice, string enemyChoice)
    {
        if (playerChoice == enemyChoice)
        {
            resultText.text = "It's a Draw!";
            return RoundResult.Draw;
        }
        else if ((playerChoice == "Rock" && enemyChoice == "Scissors") ||
                 (playerChoice == "Paper" && enemyChoice == "Rock") ||
                 (playerChoice == "Scissors" && enemyChoice == "Paper"))
        {
            resultText.text = "You Win!";
            return RoundResult.Win;
        }
        else
        {
            resultText.text = "You Lose!";
            return RoundResult.Lose;
        }
    }

    private IEnumerator HandleTakeDamage(RoundResult result, string playerChoice, string enemyChoice)
    {
        Debug.Log("[GameSubstate] Resolving_TakeDamage: Applying damage...");

        playerHitDone = true;
        enemyHitDone = true;

        if (result == RoundResult.Win)
        {
            int damage = playerInstance.GetEffectiveDamage(playerChoice);
            enemyHitDone = false;
            enemyHandController.TakeDamage(damage);
            Debug.Log($"Player dealt {damage} damage to {enemyHandController?.name}");
        }
        else if (result == RoundResult.Lose)
        {
            int damage = enemyHandController.GetEffectiveDamage(enemyChoice);
            playerHitDone = false;
            playerInstance.TakeDamage(damage);
            Debug.Log($"Enemy {enemyHandController?.name} dealt {damage} damage to Player");
        }
        else
        {
            EnterIdleState();
            yield break;
        }

        yield return new WaitUntil(() => playerHitDone && enemyHitDone);
        Debug.Log("Both hit animations finished.");

        if (enemyHandController != null && enemyHandController.CurrentHealth <= 0)
        {
            currentSubstate = GameSubstate.Dying;
            Debug.Log("Enemy defeated. Entering DYING state...");
        }
        else if (playerInstance != null && playerInstance.CurrentHealth <= 0)
        {
            currentSubstate = GameSubstate.Dying;
            Debug.Log("Player defeated. Entering DYING state...");
        }
        else
        {
            EnterIdleState();
        }
    }

    private void DisableButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
    }

    private void AllowPlayerInput()
    {
        if (currentSubstate == GameSubstate.Idle && enemyHandController != null)
        {
            rockButton.interactable = true;
            paperButton.interactable = true;
            scissorsButton.interactable = true;

            PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
            if (spawner != null)
                spawner.SetAllCardsInteractable(true);
        }
    }

    private void EnterIdleState()
    {
        currentSubstate = GameSubstate.Idle;
        AllowPlayerInput();
        Debug.Log("[GameSubstate] Entering IDLE state.");
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

        EnterIdleState();
    }

    private void OnPlayerSignAnimationFinished(HandController hand) => playerSignDone = true;
    private void OnEnemySignAnimationFinished(HandController hand) => enemySignDone = true;

    private void OnEnemyDefeated(HandController hand)
    {
        if (!hand.isPlayer)
        {
            PlayerProgressData.Instance.coins += hand.coinReward;
            PlayerProgressData.Save();
            RunProgressManager.Instance.AddFavor(hand.favorReward);
        }
    }

    private void OnEnemyDeathAnimationFinished(HandController hand)
    {
        if (currentSubstate == GameSubstate.Dying)
        {
            currentSubstate = GameSubstate.EnemySpawn;
            RoomManager.Instance.OnEnemySpawned += OnEnemySpawned;
        }
    }

    private void OnPlayerDeathAnimationFinished(HandController hand)
    {
        if (currentSubstate == GameSubstate.Dying)
        {
            Debug.Log("[Dying State] Player death animation finished. Returning to main menu...");
            StartCoroutine(GameStateManager.Instance.FadeToMainMenu());
        }
    }

    private void OnEnemySpawned()
    {
        RoomManager.Instance.OnEnemySpawned -= OnEnemySpawned;
        EnterIdleState();
    }

    private void OnPlayerHitAnimationFinished(HandController hand)
    {
        playerHitDone = true;
        Debug.Log("[Hit] Player hit animation finished.");
    }

    private void OnEnemyHitAnimationFinished(HandController hand)
    {
        enemyHitDone = true;
        Debug.Log("[Hit] Enemy hit animation finished.");
    }
}