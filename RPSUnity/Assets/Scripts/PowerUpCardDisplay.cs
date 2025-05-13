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

        if (costText != null)
        {
            if (data.favorCost > 0)
            {
                costText.text = $"Cost: {data.favorCost}";

                // Set text color: white if affordable, red if not
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

            // TODO: Trigger UI refresh for Favor counter externally (next step)
        }
        else
        {
            Debug.Log($"[PowerUpCardDisplay] Not enough favor to buy {data.powerUpName}!");
        }
    }
}
