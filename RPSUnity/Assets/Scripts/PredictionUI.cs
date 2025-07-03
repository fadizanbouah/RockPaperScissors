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
    private List<bool> usedSlots = new List<bool>();
    private int lastProcessedIndex = -1;

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
        lastProcessedIndex = -1;

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
        usedSlots.Clear();  // ADD THIS LINE

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
            usedSlots.Add(false);  // ADD THIS LINE
        }
    }

    private void UpdateUsedSigns(int currentIndex)
    {
        if (currentEnemy == null) return;

        // Get the actual sequence from the enemy
        List<string> actualSequence = currentEnemy.GetCurrentPredictionSequence();
        if (actualSequence == null) return;

        // Only process newly used signs (from lastProcessedIndex+1 to currentIndex-1)
        for (int i = lastProcessedIndex + 1; i < currentIndex && i < actualSequence.Count; i++)
        {
            string usedSign = actualSequence[i];
            Debug.Log($"[PredictionUI] Processing used sign at index {i}: {usedSign}");

            // Find a matching sign in our display that hasn't been grayed out yet
            for (int j = 0; j < displayedSequence.Count; j++)
            {
                if (displayedSequence[j] == usedSign && !usedSlots[j])
                {
                    // Gray out this slot
                    Image slotImage = currentSlots[j].GetComponent<Image>();
                    if (slotImage != null)
                    {
                        slotImage.color = usedColor;
                    }
                    usedSlots[j] = true;
                    Debug.Log($"[PredictionUI] Grayed out slot {j} ({displayedSequence[j]})");
                    break; // Only gray out one instance of this sign
                }
            }
        }

        lastProcessedIndex = currentIndex - 1;
    }

    private void ClearSlots()
    {
        foreach (GameObject slot in currentSlots)
        {
            Destroy(slot);
        }
        currentSlots.Clear();
        displayedSequence.Clear();
        usedSlots.Clear();
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