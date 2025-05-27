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
            if (card != null && card != selectedCard)
            {
                Button button = card.GetComponent<Button>();
                CanvasGroup group = card.GetComponent<CanvasGroup>();

                if (button != null)
                    button.interactable = false;

                if (group != null)
                    group.alpha = 0.7f; // Visually gray it out
            }
        }

        // Play checkmark animation on the selected passive card
        if (selectedCard != null)
        {
            selectedCard.PlayCheckmarkAnimation();
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
}
