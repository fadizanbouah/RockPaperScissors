// Replace your current RockPaperScissorsGame.cs with this updated version:

using UnityEngine;
using TMPro;
using System.Collections;

public class RockPaperScissorsGame : MonoBehaviour
{
    private enum GameSubstate
    {
        Idle,
        Selecting,
        Resolving,
        HitAnimation,
        Dying,
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
    private bool playerHitDone = true;
    private bool enemyHitDone = true;

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

        // Unsubscribe old
        playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
        enemyHandController.OnDeath -= OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;
        playerInstance.OnHitAnimationFinished -= OnPlayerHitAnimationFinished;
        enemyHandController.OnHitAnimationFinished -= OnEnemyHitAnimationFinished;

        // Subscribe new
        playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
        enemyHandController.OnDeath += OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;
        playerInstance.OnHitAnimationFinished += OnPlayerHitAnimationFinished;
        enemyHandController.OnHitAnimationFinished += OnEnemyHitAnimationFinished;

        PowerUpEffectManager.Instance?.Initialize(player, enemy);

        resultText.text = "";
        currentSubstate = GameSubstate.Idle;

        UpdateInputStates();

        Debug.Log("Game successfully initialized.");
    }

    public void StartGame()
    {
        currentSubstate = GameSubstate.Idle;
        UpdateInputStates();
        Debug.Log("Game started!");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (currentSubstate != GameSubstate.Idle) return;

        currentSubstate = GameSubstate.Selecting;
        UpdateInputStates();

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
        if (enemyHandController == null) yield break;

        currentSubstate = GameSubstate.Resolving;
        UpdateInputStates();

        Debug.Log($"Resolving round: {playerChoice} vs {enemyChoice}");

        yield return new WaitUntil(() => playerSignDone && enemySignDone);
        Debug.Log("Both sign animations finished.");

        RoundResult result = DetermineOutcome(playerChoice, enemyChoice);

        currentSubstate = GameSubstate.HitAnimation;
        UpdateInputStates();

        // Reset hit animation flags depending on who took damage
        playerHitDone = result != RoundResult.Lose;
        enemyHitDone = result != RoundResult.Win;

        yield return new WaitUntil(() => playerHitDone && enemyHitDone);

        currentSubstate = GameSubstate.Idle;
        UpdateInputStates();
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

    private void UpdateInputStates()
    {
        bool allow = (currentSubstate == GameSubstate.Idle);

        rockButton.interactable = allow;
        paperButton.interactable = allow;
        scissorsButton.interactable = allow;

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
        {
            spawner.SetAllCardsInteractable(allow);
        }

        Debug.Log($"Input {(allow ? "ENABLED" : "DISABLED")} - Current Substate: {currentSubstate}");
    }

    public void UpdateEnemyReference(HandController newEnemy)
    {
        if (enemyHandController != null)
        {
            enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
            enemyHandController.OnDeath -= OnEnemyDefeated;
            enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;
            enemyHandController.OnHitAnimationFinished -= OnEnemyHitAnimationFinished;
        }

        enemyHandController = newEnemy;

        if (enemyHandController != null)
        {
            enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
            enemyHandController.OnDeath += OnEnemyDefeated;
            enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;
            enemyHandController.OnHitAnimationFinished += OnEnemyHitAnimationFinished;
        }

        currentSubstate = GameSubstate.Idle;
        UpdateInputStates();
    }

    private void OnPlayerSignAnimationFinished(HandController hand) => playerSignDone = true;
    private void OnEnemySignAnimationFinished(HandController hand) => enemySignDone = true;
    private void OnPlayerHitAnimationFinished(HandController hand) => playerHitDone = true;
    private void OnEnemyHitAnimationFinished(HandController hand) => enemyHitDone = true;

    private void OnEnemyDefeated(HandController hand)
    {
        if (!hand.isPlayer)
        {
            PlayerProgressData.Instance.coins += hand.coinReward;
            PlayerProgressData.Save();
            Debug.Log($"Gained {hand.coinReward} coins! Total: {PlayerProgressData.Instance.coins}");

            RunProgressManager.Instance.AddFavor(hand.favorReward);
            Debug.Log($"Gained {hand.favorReward} favor! Total: {RunProgressManager.Instance.currentFavor}");

            currentSubstate = GameSubstate.Dying;
            UpdateInputStates();
        }
    }

    private void OnEnemyDeathAnimationFinished(HandController hand)
    {
        Debug.Log($"Enemy death animation finished for {hand.name}");
        currentSubstate = GameSubstate.Idle;
        UpdateInputStates();
    }
}
