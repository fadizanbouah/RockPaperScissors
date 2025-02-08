using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        InitializeButtons();
        InitializeSettings();
        ShowMainMenu();
    }

    private void InitializeButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        else
            Debug.LogError("Play Button reference is missing in MainMenu!");

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));

        if (creditsButton != null)
            creditsButton.onClick.AddListener(() => ShowPanel(creditsPanel));

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void InitializeSettings()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    private void StartGame()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Gameplay);
    }

    private void ShowPanel(GameObject panel)
    {
        mainMenuPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);

        panel?.SetActive(true);
    }

    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }

    private void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        // Add your audio manager implementation here
    }

    private void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        // Add your audio manager implementation here
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
