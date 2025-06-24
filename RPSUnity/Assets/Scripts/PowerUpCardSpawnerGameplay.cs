using UnityEngine;
using System.Collections;

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

        // IMPORTANT: Reset the container's position before spawning
        cardContainer.localPosition = Vector3.zero;
        cardContainer.localRotation = Quaternion.identity;
        cardContainer.localScale = Vector3.one;

        // Clear existing cards (but leave background untouched)
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Wait a frame to ensure cleanup is complete
        StartCoroutine(SpawnCardsAfterCleanup());
    }

    private IEnumerator SpawnCardsAfterCleanup()
    {
        yield return null; // Wait one frame

        // Spawn acquired powerups as cards
        foreach (PowerUpData powerUp in RunProgressManager.Instance.acquiredPowerUps)
        {
            GameObject cardInstance = Instantiate(powerUpCardPrefab, cardContainer);

            // Ensure card starts at origin
            cardInstance.transform.localPosition = Vector3.zero;
            cardInstance.transform.localRotation = Quaternion.identity;
            cardInstance.transform.localScale = Vector3.one;

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

        // Apply fan layout after all cards are spawned
        FanLayout fanLayout = cardContainer.GetComponent<FanLayout>();
        if (fanLayout != null)
        {
            fanLayout.ApplyFanLayout();
        }
    }

    public void SetAllCardsInteractable(bool isInteractable)
    {
        foreach (Transform child in cardContainer)
        {
            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = isInteractable;
                cg.blocksRaycasts = isInteractable;
            }
        }
    }
}