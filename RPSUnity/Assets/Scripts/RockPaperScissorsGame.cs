using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    public static RockPaperScissorsGame Instance { get; private set; }

    private enum GameSubstate
    {
        Idle,
        Selecting,
        Resolving_EvaluateOutcome,
        Resolving_TakeDamage,
        PowerUpActivation,
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
        Debug.Log("[GameSubstate] Entering SELECTING state.");

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
        Debug.Log("[GameSubstate] Entering RESOLVING_EVALUATEOUTCOME state.");

        yield return new WaitUntil(() => playerSignDone && enemySignDone);
        Debug.Log($"[GameSubstate] Resolving_EvaluateOutcome: {playerChoice} vs {enemyChoice}");

        RoundResult result = DetermineOutcome(playerChoice, enemyChoice);

        currentSubstate = GameSubstate.Resolving_TakeDamage;
        Debug.Log("[GameSubstate] Entering RESOLVING_TAKEDAMAGE state.");

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
            Debug.Log("[GameSubstate] Entering DYING state (enemy dead)...");
        }
        else if (playerInstance != null && playerInstance.CurrentHealth <= 0)
        {
            currentSubstate = GameSubstate.Dying;
            Debug.Log("[GameSubstate] Entering DYING state (player dead)...");
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
        Debug.Log("[GameSubstate] Entering IDLE state.");
        AllowPlayerInput();
    }

    public void EnterPowerUpActivationState(System.Action onAnimationComplete)
    {
        currentSubstate = GameSubstate.PowerUpActivation;
        Debug.Log("[GameSubstate] Entering POWERUP ACTIVATION state.");

        DisableButtons();

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);

        StartCoroutine(HandlePowerUpActivation(onAnimationComplete));
    }

    private IEnumerator HandlePowerUpActivation(System.Action onAnimationComplete)
    {
        Debug.Log("[GameSubstate] HandlePowerUpActivation: Waiting for power-up animation to finish...");

        bool animationDone = false;
        onAnimationComplete += () => animationDone = true;

        yield return new WaitUntil(() => animationDone);

        Debug.Log("[GameSubstate] Power-up animation finished. Returning to Idle.");
        EnterIdleState();
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
            Debug.Log("[GameSubstate] Entering ENEMY SPAWN state (after enemy death).");
            RoomManager.Instance.OnEnemySpawned += OnEnemySpawned;
        }
    }

    private void OnPlayerDeathAnimationFinished(HandController hand)
    {
        if (currentSubstate == GameSubstate.Dying)
        {
            Debug.Log("[GameSubstate] Player death animation finished. Returning to main menu...");
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

    public void OnPowerUpActivationComplete()
    {
        Debug.Log("[PowerUp] Activation animation complete.");
        // This callback is called by DestroyOnAnimationEvent when the card's animation ends.

        EnterIdleState();
    }

    public bool IsInPowerUpActivationSubstate()
    {
        return currentSubstate == GameSubstate.PowerUpActivation;
    }
}
