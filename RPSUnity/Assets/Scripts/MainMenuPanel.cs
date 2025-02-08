using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuPanel : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    private MainMenu mainMenu;

    private void Awake()
    {
        mainMenu = GetComponentInParent<MainMenu>();
    }

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Gameplay);
    }

    private void OnSettingsClicked()
    {
        mainMenu.ShowPanel(PanelType.Settings);
    }

    private void OnCreditsClicked()
    {
        mainMenu.ShowPanel(PanelType.Credits);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
} 