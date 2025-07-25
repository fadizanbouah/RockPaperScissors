using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

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
        Debug.Log($"[PredictionUI] SetupPrediction called with enemy: {(enemy != null ? enemy.name : "null")}");

        ClearSlots();

        if (enemy == null || !enemy.UsesPredictionSystem())
        {
            Debug.Log($"[PredictionUI] Enemy null or doesn't use prediction system. UsesPrediction: {enemy?.UsesPredictionSystem()}");
            predictionPanel.SetActive(false);
            currentEnemy = null;
            return;
        }

        currentEnemy = enemy;
        List<string> sequence = enemy.GetCurrentPredictionSequence();

        Debug.Log($"[PredictionUI] Got sequence: {(sequence != null ? string.Join(", ", sequence) : "null")}");

        if (sequence == null || sequence.Count == 0)
        {
            Debug.Log("[PredictionUI] Sequence is null or empty");
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
        lastProcessedIndex = -1;  // Make sure this is reset

        Debug.Log($"[PredictionUI] Set up prediction for {enemy.name} with {displayedSequence.Count} signs");
    }

    private void Update()
    {
        // Empty - all updates now happen through UpdateAfterSignRevealed()
        // which is called after the enemy's sign animation finishes
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

    private IEnumerator DelayedRefresh()
    {
        // Wait for the current round to fully complete
        yield return new WaitForSeconds(0.5f);

        // Force the enemy to generate a new sequence if needed
        if (currentEnemy != null && currentEnemy.UsesPredictionSystem())
        {
            // Tell the enemy to generate a new sequence
            currentEnemy.ForceNewSequenceIfNeeded();

            // Now refresh the UI with the new sequence
            SetupPrediction(currentEnemy);
        }
    }

    public void UpdateAfterSignRevealed()
    {
        if (currentEnemy != null && currentEnemy.UsesPredictionSystem())
        {
            int currentIndex = currentEnemy.GetCurrentSequenceIndex();

            // Update the used signs now that the animation has finished
            UpdateUsedSigns(currentIndex);
            lastKnownIndex = currentIndex;

            // Check if all signs have been used
            if (currentIndex >= displayedSequence.Count)
            {
                Debug.Log($"[PredictionUI] All signs used ({currentIndex}/{displayedSequence.Count}). Scheduling refresh...");
            }
        }
    }

    public bool NeedsRefresh()
    {
        if (currentEnemy == null || !currentEnemy.UsesPredictionSystem())
            return false;

        return currentEnemy.GetCurrentSequenceIndex() >= displayedSequence.Count;
    }

    public void RefreshIfNeeded()
    {
        if (NeedsRefresh() && currentEnemy != null)
        {
            Debug.Log("[PredictionUI] Refreshing prediction display for next round");
            currentEnemy.ForceNewSequenceIfNeeded();
            SetupPrediction(currentEnemy);
        }
    }
}