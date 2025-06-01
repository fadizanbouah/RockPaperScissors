using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Gameplay,
        GameOver,
        Transition
    }

    public static GameStateManager Instance { get; private set; }

    public GameState currentState { get; private set; }

    [SerializeField] private RockPaperScissorsGame rockPaperScissorsGame;
    [SerializeField] private HandController playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject mainMenuCanvas;

    private HandController playerInstance;
    private HandController currentEnemyInstance;

    private bool hasStartedRoomSequence = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuCanvas reference is missing in GameStateManager!");
        }

        if (gameplayCanvas != null)
        {
            gameplayCanvas.SetActive(false);
            Debug.Log("GameplayCanvas is now HIDDEN at start.");
        }
        else
        {
            Debug.LogError("GameplayCanvas reference is missing in GameStateManager!");
        }

        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        HandleStateChange();
    }

    private void HandleStateChange()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                Debug.Log("Entering Main Menu State");
                SetCanvasVisibility(mainMenuCanvas, true);
                SetCanvasVisibility(gameplayCanvas, false);
                hasStartedRoomSequence = false;
                break;

            case GameState.Gameplay:
                Debug.Log("Entering Gameplay State");
                SetCanvasVisibility(mainMenuCanvas, false);
                SetCanvasVisibility(gameplayCanvas, true);

                if (rockPaperScissorsGame != null)
                {
                    InitializePlayer();

                    if (!hasStartedRoomSequence)
                    {
                        if (RoomManager.Instance != null)
                        {
                            RoomManager.Instance.StartRoomSequence();
                            hasStartedRoomSequence = true;
                        }
                        else
                        {
                            Debug.LogError("RoomManager instance is missing!");
                        }
                    }

                    StartCoroutine(WaitForRoomAndInitializeGame());
                }
                else
                {
                    Debug.LogError("RockPaperScissorsGame reference is missing in GameStateManager!");
                }
                break;

            case GameState.GameOver:
                Debug.Log("Entering Game Over State");
                SetCanvasVisibility(gameplayCanvas, false);
                CleanupGame();
                break;

            case GameState.Transition:
                Debug.Log("Entering Transition State");
                break;
        }
    }

    private void InitializePlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        if (playerSpawnPoint == null)
        {
            Debug.LogError("Player spawn point is not assigned!");
            return;
        }

        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            Debug.Log("Player initialized at spawn point.");

            playerInstance.OnDeathAnimationFinished += HandlePlayerDeathAnimationFinished;
        }
        else
        {
            Debug.LogWarning("Player already exists. Skipping instantiation.");
        }
    }

    private void HandlePlayerDeathAnimationFinished(HandController player)
    {
        Debug.Log("Player death animation finished. Returning to main menu...");
        StartCoroutine(FadeToMainMenu());
    }

    public IEnumerator FadeToMainMenu()
    {
        ChangeState(GameState.Transition);

        ScreenFader fader = FindObjectOfType<ScreenFader>();
        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeOutRoutine());
        }

        ChangeState(GameState.MainMenu);

        // Refresh the coin display now that we're back at the main menu
        MainMenu mainMenu = FindObjectOfType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.RefreshCoinDisplay();
            Debug.Log("[GameStateManager] Refreshed coin display after returning to main menu.");
        }
    }

    private void CleanupGame()
    {
        if (playerInstance != null)
        {
            playerInstance.OnDeathAnimationFinished -= HandlePlayerDeathAnimationFinished;
            Destroy(playerInstance.gameObject);
        }
    }

    private void SetCanvasVisibility(GameObject canvas, bool isVisible)
    {
        if (canvas != null)
        {
            canvas.SetActive(isVisible);
            if (isVisible)
            {
                Debug.Log("GameplayCanvas is now VISIBLE");
            }
        }
        else
        {
            Debug.LogError("GameplayCanvas reference is missing in GameStateManager!");
        }
    }

    private IEnumerator WaitForRoomAndInitializeGame()
    {
        Debug.Log("Waiting for the room to be ready...");

        while (RoomManager.Instance.GetCurrentEnemy() == null)
        {
            yield return null;
        }

        Debug.Log("Room and enemy are ready. Initializing game...");
        currentEnemyInstance = RoomManager.Instance.GetCurrentEnemy();
        rockPaperScissorsGame.InitializeGame(playerInstance, currentEnemyInstance);
        rockPaperScissorsGame.StartGame();
    }

    public void UpdateEnemy(HandController newEnemy)
    {
        Debug.Log($"Updating GameStateManager enemy reference: {newEnemy.name}");
        currentEnemyInstance = newEnemy;
        rockPaperScissorsGame.InitializeGame(playerInstance, currentEnemyInstance);
    }

    public void BeginRoomTransition()
    {
        ChangeState(GameState.Transition);

        ScreenFader fader = FindObjectOfType<ScreenFader>();
        if (fader != null)
        {
            StartCoroutine(HandleRoomTransitionSequence(fader));
        }
        else
        {
            Debug.LogWarning("ScreenFader not found! Skipping transition.");
            ChangeState(GameState.Gameplay);
        }
    }

    private IEnumerator HandleRoomTransitionSequence(ScreenFader fader)
    {
        yield return StartCoroutine(fader.FadeOutRoutine());

        RoomManager.Instance.SelectNextRoom();

        yield return StartCoroutine(fader.FadeInRoutine());

        ChangeState(GameState.Gameplay);
    }
}
