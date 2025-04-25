using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;
    [HideInInspector] public UpgradesPanel upgradesPanel;

    [Header("Upgrade Settings")]
    public string upgradeName;
    public UpgradeType upgradeType;
    public int maxLevel = 5;
    public int[] levelCosts;

    private int currentLevel;

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        RefreshUI();
    }

    private int GetCurrentLevel()
    {
        return upgradeType switch
        {
            UpgradeType.MaxHealth => PlayerProgressData.Instance.maxHealthLevel,
            UpgradeType.BaseDamage => PlayerProgressData.Instance.baseDamageLevel,
            _ => 0
        };
    }

    private void SetCurrentLevel(int newLevel)
    {
        switch (upgradeType)
        {
            case UpgradeType.MaxHealth:
                PlayerProgressData.Instance.maxHealthLevel = newLevel;
                break;
            case UpgradeType.BaseDamage:
                PlayerProgressData.Instance.baseDamageLevel = newLevel;
                break;
        }
    }

    private int GetNextLevelCost()
    {
        int level = GetCurrentLevel();
        if (level >= maxLevel || level >= levelCosts.Length)
            return -1;
        return levelCosts[level];
    }

    private void OnUpgradeClicked()
    {
        int level = GetCurrentLevel();
        int cost = GetNextLevelCost();

        if (level >= maxLevel || cost == -1)
            return;

        if (PlayerProgressData.Instance.coins >= cost)
        {
            PlayerProgressData.Instance.coins -= cost;
            SetCurrentLevel(level + 1);
            PlayerProgressData.Save();
            RefreshUI();

            if (upgradesPanel != null)
            {
                upgradesPanel.RefreshCoinDisplay(); // Notify panel to update the coin display
            }
        }
        else
        {
            Debug.Log("Not enough coins to upgrade.");
        }
    }

    public void RefreshUI()
    {
        currentLevel = GetCurrentLevel();
        int nextCost = GetNextLevelCost();

        if (upgradeLabel != null)
            upgradeLabel.text = upgradeName;

        if (levelText != null)
            levelText.text = $"Level: {currentLevel}/{maxLevel}";

        if (costText != null)
        {
            if (currentLevel >= maxLevel)
                costText.text = "Maxed Out";
            else
                costText.text = $"Cost: {nextCost}";
        }

        if (upgradeButton != null)
            upgradeButton.interactable = currentLevel < maxLevel;
    }

    public int GetTotalSpentCoins()
    {
        int total = 0;
        int level = GetCurrentLevel();
        for (int i = 0; i < level; i++)
        {
            if (i < levelCosts.Length)
                total += levelCosts[i];
        }
        return total;
    }
}

public enum UpgradeType
{
    MaxHealth,
    BaseDamage
}
