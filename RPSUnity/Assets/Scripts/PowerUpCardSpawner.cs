using UnityEngine;
using System.Collections.Generic;

public class PowerUpCardSpawner : MonoBehaviour
{
    [Header("Prefab & Container")]
    [SerializeField] private GameObject powerUpCardPrefab;
    [SerializeField] private Transform cardContainer;

    [Header("Available PowerUps")]
    [SerializeField] private PowerUpData[] availablePowerUps;

    public void PopulatePowerUpPanel()
    {
        // Clear existing cards
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Shuffle the availablePowerUps list
        List<PowerUpData> shuffledList = new List<PowerUpData>(availablePowerUps);
        for (int i = 0; i < shuffledList.Count; i++)
        {
            PowerUpData temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }

        // Determine how many to spawn (up to 3 or fewer if less available)
        int numberToSpawn = Mathf.Min(3, shuffledList.Count);

        int currentFavor = RunProgressManager.Instance.currentFavor;

        // Spawn the selected cards
        for (int i = 0; i < numberToSpawn; i++)
        {
            GameObject cardInstance = Instantiate(powerUpCardPrefab, cardContainer);
            PowerUpCardDisplay display = cardInstance.GetComponent<PowerUpCardDisplay>();

            if (display != null)
            {
                display.SetData(shuffledList[i], currentFavor);
            }
            else
            {
                Debug.LogWarning("PowerUpCard prefab is missing PowerUpCardDisplay script!");
            }
        }
    }
}
