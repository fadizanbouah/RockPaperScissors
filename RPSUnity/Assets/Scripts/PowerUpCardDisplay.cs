using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpCardDisplay : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    public void SetData(PowerUpData data, int currentFavor)
    {
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
}
