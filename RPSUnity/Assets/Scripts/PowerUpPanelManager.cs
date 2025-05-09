using UnityEngine;

public class PowerUpPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject cardContainer; // CardContainer with Horizontal Layout Group
    [SerializeField] private GameObject powerUpCardPrefab; // Your PowerUpCard prefab

    private void Start()
    {
        // Spawn 3 placeholder cards
        for (int i = 0; i < 3; i++)
        {
            GameObject card = Instantiate(powerUpCardPrefab, cardContainer.transform);
            card.name = "PowerUpCard_" + (i + 1);
        }
    }
}
