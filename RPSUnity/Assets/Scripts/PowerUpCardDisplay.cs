using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpCardDisplay : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    private PowerUpData data;
    private PowerUpPanelManager panelManager; // only needed in PowerUpPanel context

    public void SetData(PowerUpData newData, int currentFavor, PowerUpPanelManager manager = null)
    {
        data = newData;
        panelManager = manager;

        if (backgroundImage != null && data.icon != null)
            backgroundImage.sprite = data.icon;

        if (nameText != null)
            nameText.text = data.powerUpName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        UpdateAffordability(currentFavor);
    }

    public void UpdateAffordability(int currentFavor)
    {
        if (costText != null && data != null)
        {
            if (data.favorCost > 0)
            {
                costText.text = $"Cost: {data.favorCost}";
                costText.color = currentFavor >= data.favorCost ? Color.white : Color.red;
            }
            else
            {
                costText.text = "";
            }
        }
    }

    public void OnCardClicked()
    {
        if (data == null) return;

        int currentFavor = RunProgressManager.Instance.currentFavor;

        if (currentFavor >= data.favorCost)
        {
            RunProgressManager.Instance.favor -= data.favorCost;
            Debug.Log($"[PowerUpCardDisplay] Purchased {data.powerUpName} for {data.favorCost} Favor!");

            // Register acquired powerup for gameplay usage
            RunProgressManager.Instance.AddAcquiredPowerUp(data);

            // Hide or disable the card after purchase
            gameObject.SetActive(false);

            // Refresh UI favor & affordability
            if (panelManager != null)
            {
                panelManager.RefreshFavorDisplay();
                panelManager.RefreshCardAffordability();
            }
        }
        else
        {
            Debug.Log($"[PowerUpCardDisplay] Not enough favor to buy {data.powerUpName}!");
        }
    }
}
