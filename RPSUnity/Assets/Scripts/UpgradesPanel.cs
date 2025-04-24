using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesPanel : MonoBehaviour
{
    [Header("Upgrade Buttons")]
    [SerializeField] private UpgradeButtonController maxHealthUpgrade;
    [SerializeField] private UpgradeButtonController rockDamageUpgrade;
    [SerializeField] private UpgradeButtonController paperDamageUpgrade;
    [SerializeField] private UpgradeButtonController scissorsDamageUpgrade;
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
        rockDamageUpgrade.upgradesPanel = this;
        paperDamageUpgrade.upgradesPanel = this;
        scissorsDamageUpgrade.upgradesPanel = this;

        ClampUpgradeLevels();
        RefreshAllUpgrades();
        RefreshCoinDisplay();
    }

    private void OnRefundClicked()
    {
        int refundAmount = 0;

        refundAmount += maxHealthUpgrade.GetTotalSpentCoins();
        refundAmount += rockDamageUpgrade.GetTotalSpentCoins();
        refundAmount += paperDamageUpgrade.GetTotalSpentCoins();
        refundAmount += scissorsDamageUpgrade.GetTotalSpentCoins();

        // Reset upgrade levels
        PlayerProgressData.Instance.maxHealthLevel = 0;
        PlayerProgressData.Instance.rockDamageLevel = 0;
        PlayerProgressData.Instance.paperDamageLevel = 0;
        PlayerProgressData.Instance.scissorsDamageLevel = 0;

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
        rockDamageUpgrade.RefreshUI();
        paperDamageUpgrade.RefreshUI();
        scissorsDamageUpgrade.RefreshUI();
        RefreshCoinDisplay();
    }

    private void ClampUpgradeLevels()
    {
        PlayerProgressData.Instance.maxHealthLevel = Mathf.Clamp(PlayerProgressData.Instance.maxHealthLevel, 0, maxHealthUpgrade.maxLevel);
        PlayerProgressData.Instance.rockDamageLevel = Mathf.Clamp(PlayerProgressData.Instance.rockDamageLevel, 0, rockDamageUpgrade.maxLevel);
        PlayerProgressData.Instance.paperDamageLevel = Mathf.Clamp(PlayerProgressData.Instance.paperDamageLevel, 0, paperDamageUpgrade.maxLevel);
        PlayerProgressData.Instance.scissorsDamageLevel = Mathf.Clamp(PlayerProgressData.Instance.scissorsDamageLevel, 0, scissorsDamageUpgrade.maxLevel);
    }

    public void RefreshCoinDisplay()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + PlayerProgressData.Instance.coins;
        }
    }
}
