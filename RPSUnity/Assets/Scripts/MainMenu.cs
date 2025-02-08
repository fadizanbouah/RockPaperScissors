using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PanelType
{
    Main,
    Settings,
    Credits
}

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        InitializeSettings();
        ShowMainMenu();
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

    public void ShowPanel(PanelType panelType)
    {
        if (mainMenuPanel == null)
        {
            Debug.LogError("MainMenuPanel reference is missing!");
            return;
        }

        if (settingsPanel == null)
        {
            Debug.LogWarning("SettingsPanel reference is missing!");
        }

        if (creditsPanel == null)
        {
            Debug.LogWarning("CreditsPanel reference is missing!");
        }

        mainMenuPanel.gameObject.SetActive(panelType == PanelType.Main);
        settingsPanel?.SetActive(panelType == PanelType.Settings);
        creditsPanel?.SetActive(panelType == PanelType.Credits);
    }

    public void ShowMainMenu()
    {
        ShowPanel(PanelType.Main);
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

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
