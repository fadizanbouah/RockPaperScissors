using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditsPanel : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI creditsText;  // Optional: for scrolling credits

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
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        mainMenu.ShowPanel(PanelType.Main);
    }
} 