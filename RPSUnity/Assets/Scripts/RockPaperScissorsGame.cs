using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    public static RockPaperScissorsGame Instance { get; private set; }

    private GameObject activePowerUpCardGO;
    private System.Action onPowerUpAnimationDoneCallback;

    public HandController playerInstance;
    public HandController enemyHandController;

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

    public void InitializeGame()
    {
        resultText.text = "";
        GameplayStateMachine.Instance.ChangeState(new InitializeMatchState());
    }

    public void UpdatePlayerReference(HandController player)
    {
        if (playerInstance != null)
        {
            playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
            playerInstance.OnDeathAnimationFinished -= OnPlayerDeathAnimationFinished;
            playerInstance.HitAnimationFinished -= OnPlayerHitAnimationFinished;
        }

        playerInstance = player;

        if (playerInstance != null)
        {
            playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
            playerInstance.OnDeathAnimationFinished += OnPlayerDeathAnimationFinished;
            playerInstance.HitAnimationFinished += OnPlayerHitAnimationFinished;
        }
    }

    //public void OnMatchInitialized()
    //{
    //    playerInstance = player;
    //    enemyHandController = enemy;

    //    if (playerInstance == null || enemyHandController == null)
    //    {
    //        Debug.LogError("Player or Enemy instance is missing!");
    //        return;
    //    }

    //    playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
    //    enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
    //    enemyHandController.OnDeath -= OnEnemyDefeated;
    //    enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;
    //    enemyHandController.HitAnimationFinished -= OnEnemyHitAnimationFinished;

    //    playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
    //    enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
    //    enemyHandController.OnDeath += OnEnemyDefeated;
    //    enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;
    //    enemyHandController.HitAnimationFinished += OnEnemyHitAnimationFinished;

    //    PowerUpEffectManager.Instance?.Initialize(player, enemy);
    //}

    public void StartGame()
    {
        GameplayStateMachine.Instance.ChangeState(new EnemySpawnState());
        Debug.Log("Game started!");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (!GameplayStateMachine.Instance.IsCurrentState<IdleState>()) return;

        GameplayStateMachine.Instance.ChangeState(new SelectingState(playerChoice));
    }

    public Coroutine ResolveRoundCoroutine(string playerChoice, string enemyChoice)
    {
        return StartCoroutine(GameplayStateMachine.Instance.StartResolvingEvaluateOutcome(playerChoice, enemyChoice));
    }

    public RoundResult DetermineOutcome(string playerChoice, string enemyChoice)
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

    public IEnumerator HandleTakeDamage(RoundResult result, string playerChoice, string enemyChoice, System.Action onComplete)
    {
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
            GameplayStateMachine.Instance.ChangeState(new IdleState());
            yield break;
        }

        yield return new WaitUntil(() => playerHitDone && enemyHitDone);
        Debug.Log("Both hit animations finished.");

        if (enemyHandController != null && enemyHandController.CurrentHealth <= 0)
        {
            GameplayStateMachine.Instance.ChangeState(new DyingState(enemyHandController)); // Enemy died
        }
        else if (playerInstance != null && playerInstance.CurrentHealth <= 0)
        {
            GameplayStateMachine.Instance.ChangeState(new DyingState(playerInstance)); // Player died
        }
        else
        {
            GameplayStateMachine.Instance.ChangeState(new IdleState());
        }

        onComplete?.Invoke();
    }

    public void DisableButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
    }

    public void AllowPlayerInput()
    {
        rockButton.interactable = true;
        paperButton.interactable = true;
        scissorsButton.interactable = true;

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(true);
    }

    public void EnterPowerUpActivationState(System.Action unusedCallback, GameObject cardGO)
    {
        activePowerUpCardGO = cardGO;
        GameplayStateMachine.Instance.ChangeState(new PowerUpActivationState(cardGO));
    }

    public void RegisterPowerUpAnimationCallback(System.Action callback)
    {
        onPowerUpAnimationDoneCallback = callback;
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
        if (RoomManager.Instance.HasMoreEnemiesInRoom())
        {
            GameplayStateMachine.Instance.ChangeState(new EnemySpawnState());
            RoomManager.Instance.OnEnemySpawned += OnEnemySpawned;
        }
        else
        {
            Debug.Log("[Room] All enemies defeated. Transitioning to next room...");
            GameStateManager.Instance.BeginRoomTransition();
        }
    }

    private void OnPlayerDeathAnimationFinished(HandController hand)
    {
        Debug.Log("[GameSubstate] Player death animation finished. Returning to main menu...");
        StartCoroutine(GameStateManager.Instance.FadeToMainMenu());
    }

    private void OnEnemySpawned()
    {
        RoomManager.Instance.OnEnemySpawned -= OnEnemySpawned;
        GameplayStateMachine.Instance.ChangeState(new IdleState());
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
        onPowerUpAnimationDoneCallback?.Invoke();
        onPowerUpAnimationDoneCallback = null;
    }

    public HandController GetPlayer() => playerInstance;
    public HandController GetEnemy() => enemyHandController;

    public bool IsInPowerUpActivationSubstate()
    {
        return GameplayStateMachine.Instance.IsCurrentState<PowerUpActivationState>();
    }

    public void ResetSignAnimationFlags()
    {
        playerSignDone = false;
        enemySignDone = false;
    }

    public bool BothSignAnimationsDone()
    {
        return playerSignDone && enemySignDone;
    }
}
