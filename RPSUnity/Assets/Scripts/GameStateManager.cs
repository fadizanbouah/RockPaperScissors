using UnityEngine;
using System.Collections;

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
    [SerializeField] private Transform playerSpawnPoint; // New: Assign the player spawn location
    [SerializeField] private GameObject gameplayCanvas; // Assign the Gameplay Canvas here in the Inspector
    [SerializeField] private GameObject mainMenuCanvas; // New Main Menu Canvas

    private HandController playerInstance;
    private HandController currentEnemyInstance;

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
        // Ensure canvases are properly set up
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuCanvas reference is missing in GameStateManager!");
        }

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

        ChangeState(GameState.MainMenu); // Default state
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
                break;

            case GameState.Gameplay:
                Debug.Log("Entering Gameplay State");
                SetCanvasVisibility(mainMenuCanvas, false);
                SetCanvasVisibility(gameplayCanvas, true);

                if (rockPaperScissorsGame != null)
                {
                    InitializePlayer();

                    // Select a new room BEFORE waiting for the enemy
                    if (RoomManager.Instance != null)
                    {
                        RoomManager.Instance.StartRoomSequence();
                        StartCoroutine(WaitForRoomAndInitializeGame());
                    }
                    else
                    {
                        Debug.LogError("RoomManager instance is missing!");
                    }
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

        // Wait until RoomManager has an enemy
        while (RoomManager.Instance.GetCurrentEnemy() == null)
        {
            yield return null; // Wait 1 frame
        }

        Debug.Log("Room and enemy are ready. Initializing game...");
        currentEnemyInstance = RoomManager.Instance.GetCurrentEnemy();
        rockPaperScissorsGame.InitializeGame(playerInstance, currentEnemyInstance);
        rockPaperScissorsGame.StartGame();
    }

    // New method to update enemy reference dynamically
    public void UpdateEnemy(HandController newEnemy)
    {
        Debug.Log($"Updating GameStateManager enemy reference: {newEnemy.name}");
        currentEnemyInstance = newEnemy;
        rockPaperScissorsGame.InitializeGame(playerInstance, currentEnemyInstance);
    }
}
