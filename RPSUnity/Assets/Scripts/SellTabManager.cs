using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI; // Add this line

public class SellTabManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform sellCardsContainer;
    [SerializeField] private GameObject powerUpCardPrefab; // Same PowerUpCard prefab
    [SerializeField] private GameObject emptyStateMessage; // "No cards to sell"
    [SerializeField] private ScrollRect scrollRect; // Reference to the ScrollView's ScrollRect

    [Header("References")]
    [SerializeField] private PowerUpPanelManager panelManager;

    private List<GameObject> spawnedCards = new List<GameObject>();

    public void PopulateSellTab()
    {
        Debug.Log("[SellTabManager] Populating sell tab");

        // Clear existing cards
        ClearSellCards();

        // Get only active (non-passive) power-ups that player owns
        List<PowerUpData> sellablePowerUps = new List<PowerUpData>();
        foreach (PowerUpData powerUp in RunProgressManager.Instance.acquiredPowerUps)
        {
            if (!powerUp.isPassive)
            {
                sellablePowerUps.Add(powerUp);
            }
        }

        // Show/hide empty state
        if (emptyStateMessage != null)
        {
            emptyStateMessage.SetActive(sellablePowerUps.Count == 0);
        }

        // Spawn cards
        foreach (PowerUpData powerUp in sellablePowerUps)
        {
            SpawnSellCard(powerUp);
        }

        if (scrollRect != null)
        {
            // Force immediate layout rebuild
            Canvas.ForceUpdateCanvases();

            // Set scroll to top (1 = top, 0 = bottom)
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void SpawnSellCard(PowerUpData powerUpData)
    {
        if (powerUpCardPrefab == null || sellCardsContainer == null) return;

        GameObject cardInstance = Instantiate(powerUpCardPrefab, sellCardsContainer);
        PowerUpCardDisplay display = cardInstance.GetComponent<PowerUpCardDisplay>();

        if (display != null)
        {
            display.SetDataForSellMode(powerUpData, this);
        }

        spawnedCards.Add(cardInstance);
    }

    public void SellCard(PowerUpData powerUp)
    {
        // Add favor
        RunProgressManager.Instance.AddFavor(powerUp.sellValue);

        // Remove from inventory
        RunProgressManager.Instance.RemoveAcquiredPowerUp(powerUp);

        Debug.Log($"[SellTabManager] Sold {powerUp.powerUpName} for {powerUp.sellValue} favor");

        // Refresh the entire tab
        PopulateSellTab();

        // Update main panel favor display
        if (panelManager != null)
        {
            panelManager.RefreshFavorDisplay();
            panelManager.RefreshCardAffordability();
        }
    }

    private void ClearSellCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
                Destroy(card);
        }
        spawnedCards.Clear();
    }
}