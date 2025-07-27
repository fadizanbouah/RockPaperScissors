using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Needed for List

public class PowerUpPanelManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI favorText;

    // Added: Keeps track of passive card instances
    private List<GameObject> passiveCards = new List<GameObject>();
    private List<GameObject> activeCards = new List<GameObject>();

    private void OnEnable()
    {
        RefreshFavorDisplay();
        RefreshCardAffordability();
    }

    public void RefreshFavorDisplay()
    {
        if (favorText != null)
        {
            favorText.text = "" + RunProgressManager.Instance.currentFavor;
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

    public void LockOutOtherPassiveChoices(PowerUpCardDisplay chosenCard)
    {
        foreach (Transform child in transform)
        {
            PowerUpCardDisplay card = child.GetComponent<PowerUpCardDisplay>();
            if (card != null && card != chosenCard)
            {
                // Disable interaction
                Button btn = card.GetComponent<Button>();
                if (btn != null)
                    btn.interactable = false;

                // Dim the card visually
                CanvasGroup group = card.GetComponent<CanvasGroup>();
                if (group != null)
                    group.alpha = 0.5f;
            }
        }
    }

    public void DisableOtherPassiveCards(PowerUpCardDisplay selectedCard)
    {
        foreach (var card in passiveCards)
        {
            if (card != null)
            {
                PowerUpCardDisplay cardDisplay = card.GetComponent<PowerUpCardDisplay>();

                if (card == selectedCard.gameObject)
                {
                    // For the selected card: play checkmark and disable interactions
                    // but DON'T set button.interactable = false to keep normal appearance
                    selectedCard.PlayCheckmarkAnimation();
                    selectedCard.DisableAllInteractions();
                }
                else
                {
                    // For other cards: make them dim by setting interactable = false
                    Button button = card.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = false; // This causes the dimming
                    }
                }
            }
        }
    }

    // Add this method so other scripts (like PowerUpCardSpawner) can register the cards
    public void RegisterPassiveCard(GameObject card)
    {
        if (!passiveCards.Contains(card))
        {
            passiveCards.Add(card);
        }
    }

    // Add this method to register active cards (similar to RegisterPassiveCard):
    public void RegisterActiveCard(GameObject card)
    {
        if (!activeCards.Contains(card))
        {
            activeCards.Add(card);
        }
    }
}
