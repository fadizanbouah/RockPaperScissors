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

    public void SetData(PowerUpData newData, int currentFavor)
    {
        data = newData;

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

            // TODO: Add power-up to active effects list here in the future

            // Hide or disable the card after purchase
            gameObject.SetActive(false);

            // Refresh UI favor display and affordability on all cards
            PowerUpPanelManager panelManager = GetComponentInParent<PowerUpPanelManager>();
            if (panelManager != null)
            {
                panelManager.RefreshFavorDisplay();

                // Go through sibling cards and update their affordability colors
                PowerUpCardDisplay[] cardDisplays = panelManager.GetComponentsInChildren<PowerUpCardDisplay>(true);
                foreach (PowerUpCardDisplay card in cardDisplays)
                {
                    card.UpdateAffordability(RunProgressManager.Instance.currentFavor);
                }
            }
        }
        else
        {
            Debug.Log($"[PowerUpCardDisplay] Not enough favor to buy {data.powerUpName}!");
        }
    }
}
