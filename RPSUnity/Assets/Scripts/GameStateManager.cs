using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Gameplay,
        GameOver
    }

    public static GameStateManager Instance { get; private set; }

    public GameState currentState { get; private set; }

    [SerializeField] private RockPaperScissorsGame rockPaperScissorsGame; // Exposed field for assignment
    [SerializeField] private HandController playerPrefab; // Assign the Player prefab here
    [SerializeField] private GameObject gameplayCanvas; // Assign the Gameplay Canvas here in the Inspector

    private HandController playerInstance;

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
        // Ensure GameplayCanvas is hidden at start
        if (gameplayCanvas != null)
        {
            gameplayCanvas.SetActive(false);
            Debug.Log("GameplayCanvas is now HIDDEN at start.");
        }
        else
        {
            Debug.LogError("GameplayCanvas reference is missing in GameStateManager!");
        }

        ChangeState(GameState.Gameplay); // Default state
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
                SetGameplayCanvasVisibility(false);
                break;

            case GameState.Gameplay:
                Debug.Log("Entering Gameplay State");
                SetGameplayCanvasVisibility(true);

                if (rockPaperScissorsGame != null)
                {
                    InitializePlayer();
                    rockPaperScissorsGame.InitializeGame(playerInstance);
                    rockPaperScissorsGame.StartGame();
                }
                else
                {
                    Debug.LogError("RockPaperScissorsGame reference is missing in GameStateManager!");
                }
                break;

            case GameState.GameOver:
                Debug.Log("Entering Game Over State");
                SetGameplayCanvasVisibility(false);
                CleanupGame();
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
        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab);
            Debug.Log("Player initialized.");
        }
        else
        {
            Debug.LogWarning("Player already exists. Skipping instantiation.");
        }
    }

    private void CleanupGame()
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance.gameObject);
        }
    }

    private void SetGameplayCanvasVisibility(bool isVisible)
    {
        if (gameplayCanvas != null)
        {
            gameplayCanvas.SetActive(isVisible);
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
}
