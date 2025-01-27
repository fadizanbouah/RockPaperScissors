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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeState(GameState.Gameplay); // Default state
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        HandleStateChange();
    }

    private void HandleStateChange()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                Debug.Log("Entering Main Menu State");
                // Add logic to show the main menu later
                break;

            case GameState.Gameplay:
                Debug.Log("Entering Gameplay State");

                if (rockPaperScissorsGame != null)
                {
                    rockPaperScissorsGame.InitializeGame(); // Initialize before starting
                    rockPaperScissorsGame.StartGame();
                }
                else
                {
                    Debug.LogError("RockPaperScissorsGame reference is missing in GameStateManager!");
                }
                break;

            case GameState.GameOver:
                Debug.Log("Entering Game Over State");
                // Add logic for game over later
                break;
        }
    }
}
