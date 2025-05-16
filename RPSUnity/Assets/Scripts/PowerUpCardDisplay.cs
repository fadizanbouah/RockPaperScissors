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
    private bool isGameplayCard = false;

    public void SetData(PowerUpData newData, int currentFavor, PowerUpPanelManager manager = null, bool isGameplay = false)
    {
        data = newData;
        panelManager = manager;
        isGameplayCard = isGameplay;

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
        if (isGameplayCard)
        {
            Debug.Log("[PowerUpCardDisplay] Card is in gameplay, ignoring click.");
            return;
        }

        if (data == null) return;

        int currentFavor = RunProgressManager.Instance.currentFavor;

        if (currentFavor >= data.favorCost)
        {
            RunProgressManager.Instance.favor -= data.favorCost;
            Debug.Log($"[PowerUpCardDisplay] Purchased {data.powerUpName} for {data.favorCost} Favor!");

            RunProgressManager.Instance.AddAcquiredPowerUp(data);
            gameObject.SetActive(false);

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
