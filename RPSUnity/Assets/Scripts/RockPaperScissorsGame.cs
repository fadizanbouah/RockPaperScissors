using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RockPaperScissorsGame : MonoBehaviour
{
    public static RockPaperScissorsGame Instance { get; private set; }

    private GameObject activePowerUpCardGO;
    private System.Action onPowerUpAnimationDoneCallback;

    private enum GameSubstate
    {
        Selecting,
        Resolving_EvaluateOutcome,
        Resolving_TakeDamage,
        PowerUpActivation,
        Dying,
        EnemySpawn,
        Transitioning
    }

    private GameSubstate currentSubstate;

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

        playerInstance.SignAnimationFinished -= OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished -= OnEnemySignAnimationFinished;
        enemyHandController.OnDeath -= OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished -= OnEnemyDeathAnimationFinished;
        playerInstance.OnDeathAnimationFinished -= OnPlayerDeathAnimationFinished;
        playerInstance.HitAnimationFinished -= OnPlayerHitAnimationFinished;
        enemyHandController.HitAnimationFinished -= OnEnemyHitAnimationFinished;

        playerInstance.SignAnimationFinished += OnPlayerSignAnimationFinished;
        enemyHandController.SignAnimationFinished += OnEnemySignAnimationFinished;
        enemyHandController.OnDeath += OnEnemyDefeated;
        enemyHandController.OnDeathAnimationFinished += OnEnemyDeathAnimationFinished;
        playerInstance.OnDeathAnimationFinished += OnPlayerDeathAnimationFinished;
        playerInstance.HitAnimationFinished += OnPlayerHitAnimationFinished;
        enemyHandController.HitAnimationFinished += OnEnemyHitAnimationFinished;

        PowerUpEffectManager.Instance?.Initialize(player, enemy);

        resultText.text = "";
    }

    public void StartGame()
    {
        GameplayStateMachine.Instance.ChangeState(new IdleState());
        Debug.Log("Game started!");
    }

    private void SetSubstate(GameSubstate newSubstate)
    {
        currentSubstate = newSubstate;
        Debug.Log($"[GAMESUBSTATE] Entering {newSubstate.ToString().ToUpper()} state.");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (!GameplayStateMachine.Instance.IsCurrentState<IdleState>()) return;

        GameplayStateMachine.Instance.ChangeState(new SelectingState(playerChoice));
    }

    public IEnumerator ResolveRound(string playerChoice, string enemyChoice)
    {
        SetSubstate(GameSubstate.Resolving_EvaluateOutcome);
        yield return new WaitUntil(() => playerSignDone && enemySignDone);

        RoundResult result = DetermineOutcome(playerChoice, enemyChoice);

        SetSubstate(GameSubstate.Resolving_TakeDamage);
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
            SetSubstate(GameSubstate.Dying);
        }
        else if (playerInstance != null && playerInstance.CurrentHealth <= 0)
        {
            SetSubstate(GameSubstate.Dying);
        }
        else
        {
            GameplayStateMachine.Instance.ChangeState(new IdleState());
        }
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
        SetSubstate(GameSubstate.PowerUpActivation);
        DisableButtons();

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);

        activePowerUpCardGO = cardGO;

        StartCoroutine(HandlePowerUpActivation());
    }

    private IEnumerator HandlePowerUpActivation()
    {
        Debug.Log("[GameSubstate] HandlePowerUpActivation: Waiting for power-up animation to finish...");
        bool animationDone = false;
        onPowerUpAnimationDoneCallback = () => animationDone = true;

        yield return new WaitUntil(() => animationDone);

        Debug.Log("[GameSubstate] Power-up animation finished. Applying effect...");

        if (activePowerUpCardGO != null)
        {
            PowerUpCardDisplay cardDisplay = activePowerUpCardGO.GetComponent<PowerUpCardDisplay>();
            PowerUpData data = cardDisplay?.GetPowerUpData();

            if (data != null)
            {
                RunProgressManager.Instance.ApplyPowerUpEffect(data);
                RunProgressManager.Instance.RemoveAcquiredPowerUp(data);
                Debug.Log($"[PowerUp] Applied effect from card: {data.powerUpName}");
            }
            else
            {
                Debug.LogWarning("[PowerUp] No PowerUpData found on activated card!");
            }

            activePowerUpCardGO = null;
        }

        Debug.Log("[GameSubstate] Power-up handling complete. Returning to Idle.");
        GameplayStateMachine.Instance.ChangeState(new IdleState());
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
        if (currentSubstate == GameSubstate.Dying)
        {
            SetSubstate(GameSubstate.EnemySpawn);
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

    public bool IsInPowerUpActivationSubstate()
    {
        return currentSubstate == GameSubstate.PowerUpActivation;
    }

    // --- Public methods for state machine usage (SelectingState) ---

    public void DisablePlayerInput()
    {
        DisableButtons();

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);
    }

    public void SetPlayerSign(string sign)
    {
        playerSignDone = false;
        playerInstance.StartShaking(sign);
    }

    public void SetEnemySign(string sign)
    {
        enemySignDone = false;
        enemyHandController.StartShaking(sign);
    }

    public string GetRandomSign()
    {
        return choices[Random.Range(0, choices.Length)];
    }

    public HandController GetPlayer()
    {
        return playerInstance;
    }

    public HandController GetEnemy()
    {
        return enemyHandController;
    }

    public Coroutine ResolveRoundCoroutine(string playerChoice, string enemyChoice)
    {
        return StartCoroutine(ResolveRound(playerChoice, enemyChoice));
    }
}
