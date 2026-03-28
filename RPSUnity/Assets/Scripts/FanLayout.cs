using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanLayout : MonoBehaviour
{
    [SerializeField] private float angleRange = 1000f;
    [SerializeField] private float spacing = 150f;
    [SerializeField] private float animationDuration = 0.3f;

    // SINGLE SOURCE OF TRUTH for card positions
    private Dictionary<Transform, Vector3> canonicalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> canonicalRotations = new Dictionary<Transform, Quaternion>();

    private void Start()
    {
        ApplyFanLayout();
    }

    // PUBLIC: Cards query their canonical position from here
    public Vector3 GetCanonicalPosition(Transform card)
    {
        if (canonicalPositions.ContainsKey(card))
        {
            return canonicalPositions[card];
        }

        Debug.LogWarning($"[FanLayout] No canonical position found for {card.name}");
        return Vector3.zero;
    }

    public Quaternion GetCanonicalRotation(Transform card)
    {
        if (canonicalRotations.ContainsKey(card))
        {
            return canonicalRotations[card];
        }

        return Quaternion.identity;
    }

    // PUBLIC: Reset a specific card to its canonical position
    public void ResetCardToCanonicalPosition(Transform card)
    {
        if (canonicalPositions.ContainsKey(card))
        {
            card.localPosition = canonicalPositions[card];
            card.localRotation = canonicalRotations[card];
        }
    }

    // PUBLIC: Reset ALL cards to canonical positions (called when drag ends)
    public void ResetAllCardsToCanonicalPositions()
    {
        foreach (var kvp in canonicalPositions)
        {
            Transform card = kvp.Key;
            if (card != null)
            {
                card.localPosition = kvp.Value;
                card.localRotation = canonicalRotations[card];
            }
        }
    }

    // Call this whenever cards change (added/removed)
    public void RefreshLayout()
    {
        StartCoroutine(AnimateToNewLayout());
    }

    private IEnumerator AnimateToNewLayout()
    {
        // Clean up null entries
        CleanupDictionaries();

        // Get current positions
        int childCount = transform.childCount;
        Vector3[] startPositions = new Vector3[childCount];
        Quaternion[] startRotations = new Quaternion[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            startPositions[i] = child.localPosition;
            startRotations[i] = child.localRotation;
        }

        // Calculate new canonical positions
        RecalculateCanonicalPositions();

        // Get target positions from dictionaries
        Vector3[] targetPositions = new Vector3[childCount];
        Quaternion[] targetRotations = new Quaternion[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            targetPositions[i] = canonicalPositions[child];
            targetRotations[i] = canonicalRotations[child];
        }

        // Animate to new positions
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    child.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                    child.localRotation = Quaternion.Lerp(startRotations[i], targetRotations[i], t);
                }
            }

            yield return null;
        }

        // Ensure final positions are exact
        ResetAllCardsToCanonicalPositions();
    }

    public void ApplyFanLayout()
    {
        CleanupDictionaries();
        RecalculateCanonicalPositions();
        ResetAllCardsToCanonicalPositions();
    }

    private void RecalculateCanonicalPositions()
    {
        // Clear existing data
        canonicalPositions.Clear();
        canonicalRotations.Clear();

        int childCount = transform.childCount;
        if (childCount == 0) return;

        // Calculate positions
        Vector3[] positions = new Vector3[childCount];
        Quaternion[] rotations = new Quaternion[childCount];
        CalculateFanPositions(positions, rotations);

        // Store in dictionaries
        for (int i = 0; i < childCount; i++)
        {
            Transform card = transform.GetChild(i);
            canonicalPositions[card] = positions[i];
            canonicalRotations[card] = rotations[i];
        }
    }

    private void CalculateFanPositions(Vector3[] positions, Quaternion[] rotations)
    {
        int count = positions.Length;
        if (count == 0) return;

        if (count == 1)
        {
            positions[0] = Vector3.zero;
            rotations[0] = Quaternion.identity;
            return;
        }

        float angleStep = angleRange / (count - 1);
        float startAngle = -angleRange / 2f;
        float middleIndex = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            float xOffset = spacing * (i - middleIndex);

            positions[i] = new Vector3(xOffset, 0f, 0f);
            rotations[i] = Quaternion.Euler(0f, 0f, -angle);
        }
    }

    // Clean up null references in dictionaries
    private void CleanupDictionaries()
    {
        List<Transform> keysToRemove = new List<Transform>();

        foreach (var key in canonicalPositions.Keys)
        {
            if (key == null)
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            canonicalPositions.Remove(key);
            canonicalRotations.Remove(key);
        }
    }

    // Call this when a card starts being dragged
    public void OnCardDragStart(GameObject draggedCard)
    {
        draggedCard.transform.SetAsLastSibling();
    }

    // Call this when a card is permanently removed (used)
    public void OnCardRemoved()
    {
        RefreshLayout();
    }
}