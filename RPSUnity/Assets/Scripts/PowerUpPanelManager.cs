using UnityEngine;
using TMPro;

public class PowerUpPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI favorText;

    private void OnEnable()
    {
        RefreshFavorDisplay();
        RefreshCardAffordability();
    }

    public void RefreshFavorDisplay()
    {
        if (favorText != null)
        {
            favorText.text = "Favor: " + RunProgressManager.Instance.currentFavor;
        }
    }

    public void RefreshCardAffordability()
    {
        int currentFavor = RunProgressManager.Instance.currentFavor;

        PowerUpCardDisplay[] cards = GetComponentsInChildren<PowerUpCardDisplay>(true);

        foreach (var card in cards)
        {
            card.UpdateAffordability(currentFavor);
        }
    }
}
