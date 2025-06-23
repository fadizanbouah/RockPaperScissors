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

    [SerializeField] public RockPaperScissorsGame rockPaperScissorsGame;
    [SerializeField] public HandController playerPrefab;
    [SerializeField] public Transform playerSpawnPoint;
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject mainMenuCanvas;

    //private HandController playerInstance;
    //private HandController currentEnemyInstance;

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
                    rockPaperScissorsGame.InitializeGame();
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

    public void HandlePlayerDeathAnimationFinished(HandController player)
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
        if (rockPaperScissorsGame.playerInstance != null)
        {
            rockPaperScissorsGame.playerInstance.OnDeathAnimationFinished -= HandlePlayerDeathAnimationFinished;
            Destroy(rockPaperScissorsGame.playerInstance.gameObject);
        }
    }

    private void SetCanvasVisibility(GameObject canvas, bool isVisible)
    {
        if (canvas != null)
        {
            canvas.SetActive(isVisible);
            Debug.Log($"[Canvas Visibility] {canvas.name} active = {isVisible} from GameState: {currentState}");
        }
        else
        {
            Debug.LogError("Canvas reference is missing in GameStateManager!");
        }
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

        //RoomManager.Instance.SelectNextRoom();

        yield return StartCoroutine(fader.FadeInRoutine());

        ChangeState(GameState.Gameplay);
    }
}
