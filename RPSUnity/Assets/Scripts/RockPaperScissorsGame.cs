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
        //SetSubstate(GameSubstate.EnemySpawn);
    }

    public void StartGame()
    {
        EnterIdleState();
        Debug.Log("Game started!");
    }

    private void SetSubstate(GameSubstate newSubstate)
    {
        currentSubstate = newSubstate;
        Debug.Log($"[GAMESUBSTATE] Entering {newSubstate.ToString().ToUpper()} state.");
    }

    public void PlayerSelect(string playerChoice)
    {
        if (currentSubstate != GameSubstate.Idle) return;

        SetSubstate(GameSubstate.Selecting);

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
            enemyHandController.TakeDamage(damage, playerInstance);
            Debug.Log($"Player dealt {damage} damage to {enemyHandController?.name}");
        }
        else if (result == RoundResult.Lose)
        {
            int damage = enemyHandController.GetEffectiveDamage(enemyChoice);
            playerHitDone = false;
            playerInstance.TakeDamage(damage, enemyHandController);
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
            SetSubstate(GameSubstate.Dying);
        }
        else if (playerInstance != null && playerInstance.CurrentHealth <= 0)
        {
            SetSubstate(GameSubstate.Dying);
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
            {
                // MODIFY THIS PART: Check if power-ups can still be used
                bool canUsePowerUp = PowerUpUsageTracker.Instance == null || PowerUpUsageTracker.Instance.CanUsePowerUp();
                spawner.SetAllCardsInteractable(canUsePowerUp);

                if (!canUsePowerUp)
                {
                    Debug.Log("[RockPaperScissorsGame] Power-ups disabled - already used this round");
                }
            }
        }
    }

    private void EnterIdleState()
    {
        SetSubstate(GameSubstate.Idle);

        // ADD THIS LINE: Reset power-up usage for the new round
        if (PowerUpUsageTracker.Instance != null)
        {
            PowerUpUsageTracker.Instance.ResetRoundUsage();
            Debug.Log("[RockPaperScissorsGame] Power-up usage reset for new round");
        }

        AllowPlayerInput();
    }

    public void EnterPowerUpActivationState(System.Action unusedCallback, GameObject cardGO)
    {
        SetSubstate(GameSubstate.PowerUpActivation);
        DisableButtons();

        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);

        activePowerUpCardGO = cardGO;

        // Start the coroutine and store the internal callback to trigger it later
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
                // CHECK FIRST: Is this a Double Use power-up?
                bool isDoubleUseCard = data.powerUpName == "Double Activation" ||
                                      (data.effectPrefab != null && data.effectPrefab.GetComponent<DoubleUsePowerUpEffect>() != null);

                // Mark power-up as used BEFORE applying effect
                if (PowerUpUsageTracker.Instance != null)
                {
                    Debug.Log($"[HandlePowerUpActivation] About to mark {data.powerUpName} as used");
                    PowerUpUsageTracker.Instance.MarkPowerUpUsed();
                }

                // Apply the power-up effect
                RunProgressManager.Instance.ApplyPowerUpEffect(data);
                RunProgressManager.Instance.RemoveAcquiredPowerUp(data);
                Debug.Log($"[PowerUp] Applied effect from card: {data.powerUpName}");

                PowerUpCardSpawnerGameplay gameplaySpawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
                if (gameplaySpawner != null)
                {
                    Transform cardContainer = gameplaySpawner.transform.Find("CardContainer");
                    if (cardContainer != null)
                    {
                        FanLayout fanLayout = cardContainer.GetComponent<FanLayout>();
                        if (fanLayout != null)
                        {
                            // Wait a frame for the card to be destroyed, then refresh
                            StartCoroutine(RefreshFanLayoutNextFrame(fanLayout));
                        }
                    }
                }

                // Always check if more power-ups can be used after applying ANY effect
                if (PowerUpUsageTracker.Instance != null)
                {
                    PowerUpUsageTracker.Instance.DebugState();

                    PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
                    if (spawner != null)
                    {
                        bool canUseMore = PowerUpUsageTracker.Instance.CanUsePowerUp();
                        spawner.SetAllCardsInteractable(canUseMore);

                        if (canUseMore)
                        {
                            Debug.Log($"[HandlePowerUpActivation] Cards remain enabled - more uses available!");
                        }
                        else
                        {
                            Debug.Log($"[HandlePowerUpActivation] Cards disabled - no more uses this round");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[PowerUp] No PowerUpData found on activated card!");
            }

            activePowerUpCardGO = null;
        }

        Debug.Log("[GameSubstate] Power-up handling complete. Returning to Idle.");

        // IMPORTANT: Don't call EnterIdleState() here, just set the state
        SetSubstate(GameSubstate.Idle);

        // Re-enable RPS buttons
        rockButton.interactable = true;
        paperButton.interactable = true;
        scissorsButton.interactable = true;
    }

    private IEnumerator RefreshFanLayoutNextFrame(FanLayout fanLayout)
    {
        yield return null; // Wait one frame for card destruction
        fanLayout.RefreshLayout();
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

        if (onPowerUpAnimationDoneCallback != null)
        {
            onPowerUpAnimationDoneCallback.Invoke();
            onPowerUpAnimationDoneCallback = null;
        }
        else
        {
            Debug.LogWarning("[PowerUp] No callback registered for animation completion.");
        }
    }

    public bool IsInPowerUpActivationSubstate()
    {
        return currentSubstate == GameSubstate.PowerUpActivation;
    }

    public bool IsInCombat()
    {
        return currentSubstate != GameSubstate.Idle;
    }
}
