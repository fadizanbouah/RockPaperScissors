using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("Play Button reference is missing in MainMenu!");
        }
    }

    private void StartGame()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Gameplay);
    }
}
