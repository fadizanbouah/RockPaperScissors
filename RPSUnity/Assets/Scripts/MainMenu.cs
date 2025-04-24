using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PanelType
{
    Main,
    Settings,
    Credits,
    Upgrades
}

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private CreditsPanel creditsPanel;
    [SerializeField] private GameObject upgradesPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Coin Display")]
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        InitializeSettings();
        UpdateCoinDisplay();
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
        settingsPanel?.gameObject.SetActive(panelType == PanelType.Settings);
        creditsPanel?.gameObject.SetActive(panelType == PanelType.Credits);
        upgradesPanel?.gameObject.SetActive(panelType == PanelType.Upgrades);
    }

    public void ShowMainMenu()
    {
        ShowPanel(PanelType.Main);
        UpdateCoinDisplay();
    }

    public void RefreshCoinDisplay()
    {
        UpdateCoinDisplay();
    }

    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + PlayerProgressData.Instance.coins;
            Debug.Log($"[MainMenu] Refreshed coin text: {PlayerProgressData.Instance.coins}");
        }
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
