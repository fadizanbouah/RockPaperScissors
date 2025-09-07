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

    [Header("Animation Settings")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float flipOutDuration = 0.3f;
    [SerializeField] private float flipInDuration = 0.3f;
    [SerializeField] private float delayBetweenFlips = 0.1f;

    private List<GameObject> currentSlots = new List<GameObject>();
    private List<string> displayedSequence = new List<string>();
    private HandController currentEnemy;
    private int lastKnownIndex = 0;
    private List<bool> usedSlots = new List<bool>();
    private int lastProcessedIndex = -1;
    private bool isAnimating = false;

    private void Awake()
    {
        if (predictionPanel != null)
            predictionPanel.SetActive(false);
    }

    public void SetupPrediction(HandController enemy)
    {
        Debug.Log($"[PredictionUI] SetupPrediction called with enemy: {(enemy != null ? enemy.name : "null")}");

        // Check if we should animate (only if signs are already showing)
        bool shouldAnimate = useAnimations && currentSlots.Count > 0 && predictionPanel.activeSelf && !isAnimating;

        if (enemy == null || !enemy.UsesPredictionSystem())
        {
            Debug.Log($"[PredictionUI] Enemy null or doesn't use prediction system. UsesPrediction: {enemy?.UsesPredictionSystem()}");
            ClearSlots();
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
            ClearSlots();
            predictionPanel.SetActive(false);
            return;
        }

        // Shuffle the sequence for display
        displayedSequence = sequence.ToList();
        ShuffleList(displayedSequence);

        // Animate the refresh if signs are already showing
        if (shouldAnimate)
        {
            StartCoroutine(AnimateRefresh(displayedSequence));
        }
        else
        {
            // First time setup - no animation
            ClearSlots(); // This clears the lists but we'll repopulate displayedSequence

            // IMPORTANT: Restore displayedSequence after ClearSlots cleared it
            displayedSequence = sequence.ToList();
            ShuffleList(displayedSequence);

            CreateSlots(displayedSequence);
            predictionPanel.SetActive(true);

            // IMPORTANT: Ensure slots start in Idle state
            foreach (GameObject slot in currentSlots)
            {
                if (slot != null)
                {
                    Animator anim = slot.GetComponent<Animator>();
                    if (anim != null)
                    {
                        anim.Play("Idle", 0, 0f);
                    }
                }
            }
        }

        lastKnownIndex = 0;
        lastProcessedIndex = -1;

        Debug.Log($"[PredictionUI] Set up prediction for {enemy.name} with {displayedSequence.Count} signs");
    }

    private IEnumerator AnimateRefresh(List<string> newSequence)
    {
        isAnimating = true;

        Debug.Log($"[AnimateRefresh] Starting with {newSequence.Count} signs: {string.Join(", ", newSequence)}");

        // Step 1: Animate all current slots out
        foreach (GameObject slot in currentSlots)
        {
            if (slot != null)
            {
                Animator anim = slot.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Play("SignSlotFlipOut");
                }
            }
        }

        // Wait for flip out animation to complete
        yield return new WaitForSeconds(flipOutDuration);

        // Step 2: Clear old slots BUT NOT displayedSequence yet
        foreach (GameObject slot in currentSlots)
        {
            Destroy(slot);
        }
        currentSlots.Clear();
        usedSlots.Clear();
        // DON'T clear displayedSequence here

        // Now set the new sequence
        displayedSequence = new List<string>(newSequence);
        Debug.Log($"[AnimateRefresh] After clear, displayedSequence has {displayedSequence.Count} signs");

        CreateSlots(displayedSequence);
        Debug.Log($"[AnimateRefresh] Created {currentSlots.Count} slots");

        // Start new slots in flipped state (scale Y = 0)
        foreach (GameObject slot in currentSlots)
        {
            if (slot != null)
            {
                Animator anim = slot.GetComponent<Animator>();
                if (anim != null)
                {
                    // Start at the end of flip out animation
                    anim.Play("SignSlotFlipOut", 0, 1f);
                }
            }
        }

        // Step 3: Small delay between animations
        yield return new WaitForSeconds(delayBetweenFlips);

        // Step 4: Animate new slots in
        foreach (GameObject slot in currentSlots)
        {
            if (slot != null)
            {
                Animator anim = slot.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Play("SignSlotFlipIn");
                }
            }
        }

        // Wait for flip in animation to complete
        yield return new WaitForSeconds(flipInDuration);

        // Ensure all slots end in Idle state
        foreach (GameObject slot in currentSlots)
        {
            if (slot != null)
            {
                Animator anim = slot.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Play("Idle");
                }
            }
        }

        isAnimating = false;
    }

    private void Update()
    {
        // Empty - all updates now happen through UpdateAfterSignRevealed()
        // which is called after the enemy's sign animation finishes
    }

    private void CreateSlots(List<string> sequence)
    {
        usedSlots.Clear();

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
            usedSlots.Add(false);
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

    public void ForceRefresh()
    {
        if (currentEnemy != null && currentEnemy.UsesPredictionSystem())
        {
            Debug.Log("[PredictionUI] Forcing refresh due to sign shuffle");
            SetupPrediction(currentEnemy);
        }
    }
}