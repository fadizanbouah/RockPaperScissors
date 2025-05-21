using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PowerUpCardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    private PowerUpData data;
    private PowerUpPanelManager panelManager; // only needed in PowerUpPanel context
    private bool isGameplayCard = false;

    private Vector3 originalLocalPosition; // Stores the fan layout position

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
            if (isGameplayCard)
            {
                costText.text = "";
                costText.gameObject.SetActive(false);
            }
            else
            {
                costText.gameObject.SetActive(true);

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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isGameplayCard)
        {
            // Save the position only if it's not already raised
            if (Mathf.Approximately(originalLocalPosition.y, 0f))
                originalLocalPosition = transform.localPosition;

            transform.localPosition = originalLocalPosition + Vector3.up * 20f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isGameplayCard)
        {
            transform.localPosition = originalLocalPosition;
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

    public void ResetToFanPosition()
    {
        transform.localPosition = originalLocalPosition;
    }

    public void ResetHoverPosition()
    {
        transform.localPosition = originalLocalPosition;
    }

    public bool IsGameplayCard()
    {
        return isGameplayCard;
    }

}
