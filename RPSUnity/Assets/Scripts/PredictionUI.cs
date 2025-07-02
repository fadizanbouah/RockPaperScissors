using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PredictionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject predictionPanel;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Sign Sprites")]
    [SerializeField] private Sprite rockSprite;
    [SerializeField] private Sprite paperSprite;
    [SerializeField] private Sprite scissorsSprite;

    [Header("Visual Settings")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color usedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private List<GameObject> currentSlots = new List<GameObject>();
    private List<string> displayedSequence = new List<string>();
    private HandController currentEnemy;
    private int lastKnownIndex = 0;

    private void Awake()
    {
        if (predictionPanel != null)
            predictionPanel.SetActive(false);
    }

    public void SetupPrediction(HandController enemy)
    {
        ClearSlots();

        if (enemy == null || !enemy.UsesPredictionSystem())
        {
            predictionPanel.SetActive(false);
            currentEnemy = null;
            return;
        }

        currentEnemy = enemy;
        List<string> sequence = enemy.GetCurrentPredictionSequence();

        if (sequence == null || sequence.Count == 0)
        {
            predictionPanel.SetActive(false);
            return;
        }

        // Shuffle the sequence for display
        displayedSequence = sequence.ToList();
        ShuffleList(displayedSequence);

        // Create slots and populate them
        CreateSlots(displayedSequence);
        predictionPanel.SetActive(true);
        lastKnownIndex = 0;

        Debug.Log($"[PredictionUI] Set up prediction for {enemy.name} with {displayedSequence.Count} signs");
    }

    private void Update()
    {
        if (currentEnemy != null && currentEnemy.UsesPredictionSystem())
        {
            int currentIndex = currentEnemy.GetCurrentSequenceIndex();

            // Check if we need to update the UI
            if (currentIndex != lastKnownIndex)
            {
                UpdateUsedSigns(currentIndex);
                lastKnownIndex = currentIndex;
            }

            // Check if we need to refresh (all signs used)
            if (currentIndex >= displayedSequence.Count)
            {
                SetupPrediction(currentEnemy);
            }
        }
    }

    private void CreateSlots(List<string> sequence)
    {
        foreach (string sign in sequence)
        {
            GameObject slot = Instantiate(slotPrefab, slotsContainer);
            Image slotImage = slot.GetComponent<Image>();

            if (slotImage != null)
            {
                slotImage.sprite = GetSpriteForSign(sign);
                slotImage.color = activeColor;
            }

            currentSlots.Add(slot);
        }
    }

    private void UpdateUsedSigns(int usedCount)
    {
        // Gray out the appropriate number of signs
        // Note: We don't know which specific signs were used since the display is shuffled
        // So we just gray out 'usedCount' number of signs from the display
        for (int i = 0; i < currentSlots.Count; i++)
        {
            Image slotImage = currentSlots[i].GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = i < usedCount ? usedColor : activeColor;
            }
        }
    }

    private void ClearSlots()
    {
        foreach (GameObject slot in currentSlots)
        {
            Destroy(slot);
        }
        currentSlots.Clear();
        displayedSequence.Clear();
    }

    private Sprite GetSpriteForSign(string sign)
    {
        return sign switch
        {
            "Rock" => rockSprite,
            "Paper" => paperSprite,
            "Scissors" => scissorsSprite,
            _ => null
        };
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void ClearPrediction()
    {
        ClearSlots();
        predictionPanel.SetActive(false);
        currentEnemy = null;
    }
}