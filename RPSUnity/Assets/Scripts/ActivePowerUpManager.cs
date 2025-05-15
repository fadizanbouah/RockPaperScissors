using UnityEngine;
using System.Collections.Generic;

public class ActivePowerUpManager : MonoBehaviour
{
    [SerializeField] private GameObject powerUpCardPrefab;
    [SerializeField] private Transform cardContainer;

    private List<GameObject> activeCards = new List<GameObject>();

    public void AddPowerUpCard(PowerUpData data)
    {
        GameObject cardInstance = Instantiate(powerUpCardPrefab, cardContainer);
        PowerUpCardDisplay display = cardInstance.GetComponent<PowerUpCardDisplay>();

        if (display != null)
        {
            display.SetData(data, RunProgressManager.Instance.currentFavor);
            activeCards.Add(cardInstance);
        }
        else
        {
            Debug.LogWarning("[ActivePowerUpManager] Spawned card missing PowerUpCardDisplay component!");
        }
    }
}
