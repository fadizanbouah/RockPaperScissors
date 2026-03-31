using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesPanel : MonoBehaviour
{
    [Header("Upgrade Buttons")]
    [SerializeField] private UpgradeButtonController maxHealthUpgrade;
    [SerializeField] private UpgradeButtonController baseDamageUpgrade;
    [SerializeField] private UpgradeButtonController dodgeChanceUpgrade;
    [SerializeField] private UpgradeButtonController critChanceUpgrade;
    [SerializeField] private UpgradeButtonController startingCardsUpgrade;
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
        dodgeChanceUpgrade.upgradesPanel = this;
        critChanceUpgrade.upgradesPanel = this;
        startingCardsUpgrade.upgradesPanel = this;

        ClampUpgradeLevels();
        RefreshAllUpgrades();
        RefreshCoinDisplay();
    }

    private void OnRefundClicked()
    {
        int refundAmount = 0;

        refundAmount += maxHealthUpgrade.GetTotalSpentCoins();
        refundAmount += baseDamageUpgrade.GetTotalSpentCoins();
        refundAmount += dodgeChanceUpgrade.GetTotalSpentCoins();
        refundAmount += critChanceUpgrade.GetTotalSpentCoins();
        refundAmount += startingCardsUpgrade.GetTotalSpentCoins();

        // Reset upgrade levels
        PlayerProgressData.Instance.maxHealthLevel = 0;
        PlayerProgressData.Instance.baseDamageLevel = 0;
        PlayerProgressData.Instance.dodgeChanceLevel = 0;
        PlayerProgressData.Instance.critChanceLevel = 0;
        PlayerProgressData.Instance.startingCardsLevel = 0;

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
        dodgeChanceUpgrade.RefreshUI();
        critChanceUpgrade.RefreshUI();
        startingCardsUpgrade.RefreshUI();
        RefreshCoinDisplay();
    }

    private void ClampUpgradeLevels()
    {
        PlayerProgressData.Instance.maxHealthLevel = Mathf.Clamp(PlayerProgressData.Instance.maxHealthLevel, 0, maxHealthUpgrade.maxLevel);
        PlayerProgressData.Instance.baseDamageLevel = Mathf.Clamp(PlayerProgressData.Instance.baseDamageLevel, 0, baseDamageUpgrade.maxLevel);
        PlayerProgressData.Instance.dodgeChanceLevel = Mathf.Clamp(PlayerProgressData.Instance.dodgeChanceLevel, 0, dodgeChanceUpgrade.maxLevel);
        PlayerProgressData.Instance.critChanceLevel = Mathf.Clamp(PlayerProgressData.Instance.critChanceLevel, 0, critChanceUpgrade.maxLevel);
        PlayerProgressData.Instance.startingCardsLevel = Mathf.Clamp(PlayerProgressData.Instance.startingCardsLevel, 0, startingCardsUpgrade.maxLevel);
    }

    public void RefreshCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + PlayerProgressData.Instance.coins;
        }
    }
}
