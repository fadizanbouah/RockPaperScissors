using UnityEngine;
using TMPro;

public class PowerUpPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI favorText;

    private void OnEnable()
    {
        RefreshFavorDisplay();
    }

    public void RefreshFavorDisplay()
    {
        if (favorText != null)
        {
            favorText.text = "Favor: " + RunProgressManager.Instance.currentFavor;
        }
    }
}
