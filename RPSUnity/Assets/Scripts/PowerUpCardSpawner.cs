using UnityEngine;
using System.Collections.Generic;

public class PowerUpCardSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject powerUpCardPrefab;

    [Header("Card Slots")]
    [SerializeField] private Transform cardSlot1;
    [SerializeField] private Transform cardSlot2;
    [SerializeField] private Transform cardSlot3;

    [Header("Available PowerUps")]
    [SerializeField] private PowerUpData[] availablePowerUps;

    [Header("References")]
    [SerializeField] private PowerUpPanelManager panelManager;

    public void PopulatePowerUpPanel()
    {
        if (panelManager == null)
        {
            Debug.LogError("[PowerUpCardSpawner] PowerUpPanelManager reference not assigned!");
            return;
        }

        // Clear any existing cards in the fixed slots
        ClearSlot(cardSlot1);
        ClearSlot(cardSlot2);
        ClearSlot(cardSlot3);

        // Shuffle powerup list
        List<PowerUpData> shuffledList = new List<PowerUpData>(availablePowerUps);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledList.Count);
            (shuffledList[i], shuffledList[randomIndex]) = (shuffledList[randomIndex], shuffledList[i]);
        }

        int numberToSpawn = Mathf.Min(3, shuffledList.Count);
        int currentFavor = RunProgressManager.Instance.currentFavor;

        // Spawn into each fixed slot
        if (numberToSpawn >= 1) SpawnCardToSlot(cardSlot1, shuffledList[0], currentFavor);
        if (numberToSpawn >= 2) SpawnCardToSlot(cardSlot2, shuffledList[1], currentFavor);
        if (numberToSpawn >= 3) SpawnCardToSlot(cardSlot3, shuffledList[2], currentFavor);
    }

    private void ClearSlot(Transform slot)
    {
        if (slot == null) return;
        foreach (Transform child in slot)
        {
            Destroy(child.gameObject);
        }
    }

    private void SpawnCardToSlot(Transform slot, PowerUpData data, int currentFavor)
    {
        if (slot == null || data == null) return;

        GameObject cardInstance = Instantiate(powerUpCardPrefab, slot);
        PowerUpCardDisplay display = cardInstance.GetComponent<PowerUpCardDisplay>();

        if (display != null)
        {
            display.SetData(data, currentFavor, panelManager);
        }
        else
        {
            Debug.LogWarning("[PowerUpCardSpawner] Spawned card is missing PowerUpCardDisplay!");
        }
    }
}
