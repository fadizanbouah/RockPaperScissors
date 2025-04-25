using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesPanel : MonoBehaviour
{
    [Header("Upgrade Buttons")]
    [SerializeField] private UpgradeButtonController maxHealthUpgrade;
    [SerializeField] private UpgradeButtonController baseDamageUpgrade;
    [HideInInspector] public UpgradesPanel upgradesPanel;


    [Header("Refund")]
    [SerializeField] private Button refundButton;

    [Header("Main Menu Reference")]
    [SerializeField] private MainMenu mainMenu;

    [Header("Coin Display")]
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        if (refundButton != null)
            refundButton.onClick.AddListener(OnRefundClicked);

        // Set upgrade panel reference on each button
        maxHealthUpgrade.upgradesPanel = this;
        baseDamageUpgrade.upgradesPanel = this;

        ClampUpgradeLevels();
        RefreshAllUpgrades();
        RefreshCoinDisplay();
    }

    private void OnRefundClicked()
    {
        int refundAmount = 0;

        refundAmount += maxHealthUpgrade.GetTotalSpentCoins();
        refundAmount += baseDamageUpgrade.GetTotalSpentCoins();

        // Reset upgrade levels
        PlayerProgressData.Instance.maxHealthLevel = 0;
        PlayerProgressData.Instance.baseDamageLevel = 0;

        // Refund coins
        PlayerProgressData.Instance.coins += refundAmount;
        PlayerProgressData.Save();

        RefreshAllUpgrades();
    }

    public void OnBackButtonClicked()
    {
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        else
        {
            Debug.LogWarning("MainMenu reference not set on UpgradesPanel.");
        }
    }

    private void RefreshAllUpgrades()
    {
        maxHealthUpgrade.RefreshUI();
        baseDamageUpgrade.RefreshUI();
        RefreshCoinDisplay();
    }

    private void ClampUpgradeLevels()
    {
        PlayerProgressData.Instance.maxHealthLevel = Mathf.Clamp(PlayerProgressData.Instance.maxHealthLevel, 0, maxHealthUpgrade.maxLevel);
        PlayerProgressData.Instance.baseDamageLevel = Mathf.Clamp(PlayerProgressData.Instance.baseDamageLevel, 0, baseDamageUpgrade.maxLevel);
    }

    public void RefreshCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + PlayerProgressData.Instance.coins;
        }
    }
}
