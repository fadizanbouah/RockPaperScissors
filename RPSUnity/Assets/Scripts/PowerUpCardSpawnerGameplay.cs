using UnityEngine;

public class PowerUpCardSpawnerGameplay : MonoBehaviour
{
    [Header("Prefab & References")]
    [SerializeField] private GameObject powerUpCardPrefab;

    [Header("Container where cards will be spawned (e.g., CardContainer child)")]
    [SerializeField] private Transform cardContainer;

    public void SpawnActivePowerUps()
    {
        if (cardContainer == null)
        {
            Debug.LogError("[PowerUpCardSpawnerGameplay] CardContainer reference is missing!");
            return;
        }

        // Clear existing cards (but leave background untouched)
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Spawn acquired powerups as cards
        foreach (PowerUpData powerUp in RunProgressManager.Instance.acquiredPowerUps)
        {
            GameObject cardInstance = Instantiate(powerUpCardPrefab, cardContainer);

            // Set display data
            PowerUpCardDisplay display = cardInstance.GetComponent<PowerUpCardDisplay>();
            if (display != null)
            {
                display.SetData(powerUp, int.MaxValue, null, true);
            }
            else
            {
                Debug.LogWarning("PowerUpCard prefab is missing PowerUpCardDisplay script!");
            }

            // Enable dragging for gameplay cards
            PowerUpCardDrag drag = cardInstance.GetComponent<PowerUpCardDrag>();
            if (drag != null)
            {
                drag.isDraggable = true;
            }
            else
            {
                Debug.LogWarning("PowerUpCard prefab is missing PowerUpCardDrag script!");
            }
        }

        // Apply fan layout if the container has it
        FanLayout fanLayout = cardContainer.GetComponent<FanLayout>();
        if (fanLayout != null)
        {
            fanLayout.ApplyFanLayout();
        }
    }
}
