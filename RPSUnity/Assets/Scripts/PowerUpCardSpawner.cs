using UnityEngine;
using System.Collections.Generic;

public class PowerUpCardSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject powerUpCardPrefab;

    [Header("Active Card Slots")]
    [SerializeField] private Transform cardSlot1;
    [SerializeField] private Transform cardSlot2;
    [SerializeField] private Transform cardSlot3;

    [Header("Passive Card Slots")]
    [SerializeField] private Transform passiveSlot1;
    [SerializeField] private Transform passiveSlot2;
    [SerializeField] private Transform passiveSlot3;

    [Header("Available PowerUps")]
    [SerializeField] private PowerUpData[] availablePowerUps;

    [Header("Available Passive PowerUps")]
    [SerializeField] private PowerUpData[] passivePowerUps;

    [Header("References")]
    [SerializeField] private PowerUpPanelManager panelManager;

    public void PopulatePowerUpPanel()
    {
        if (panelManager == null)
        {
            Debug.LogError("[PowerUpCardSpawner] PowerUpPanelManager reference not assigned!");
            return;
        }

        int currentFavor = RunProgressManager.Instance.currentFavor;

        // Spawn active power-ups
        SpawnCardsToFixedSlots(availablePowerUps, new[] { cardSlot1, cardSlot2, cardSlot3 }, currentFavor, false);

        // Spawn passive power-ups
        SpawnCardsToFixedSlots(passivePowerUps, new[] { passiveSlot1, passiveSlot2, passiveSlot3 }, currentFavor, true);
    }

    private void SpawnCardsToFixedSlots(PowerUpData[] pool, Transform[] slots, int favor, bool isPassive)
    {
        if (pool == null || slots == null || slots.Length == 0) return;

        List<PowerUpData> list = new List<PowerUpData>(pool);
        ShuffleList(list);

        for (int i = 0; i < Mathf.Min(slots.Length, list.Count); i++)
        {
            ClearSlot(slots[i]);
            SpawnCardToSlot(slots[i], list[i], favor);
        }
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

    private void ShuffleList(List<PowerUpData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
