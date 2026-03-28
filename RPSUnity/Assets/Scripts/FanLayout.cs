using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanLayout : MonoBehaviour
{
    [SerializeField] private float angleRange = 1000f;
    [SerializeField] private float spacing = 150f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float hoverSpacingMultiplier = 1.6f; // How much wider the fan spreads on hover

    // SINGLE SOURCE OF TRUTH for card positions
    private Dictionary<Transform, Vector3> canonicalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> canonicalRotations = new Dictionary<Transform, Quaternion>();

    // Hover spread state
    private Transform _hoveredCard = null;
    private Transform _draggedCard = null;
    private Coroutine _pendingUnspread = null;
    private Coroutine _runningAnimation = null;

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
        if (_runningAnimation != null) StopCoroutine(_runningAnimation);
        _runningAnimation = StartCoroutine(AnimateToNewLayout());
    }

    // Called by PowerUpCardDisplay when cursor enters a card
    public void OnHoverEnter(Transform card)
    {
        if (_pendingUnspread != null) { StopCoroutine(_pendingUnspread); _pendingUnspread = null; }
        if (_runningAnimation != null) { StopCoroutine(_runningAnimation); _runningAnimation = null; }

        _hoveredCard = card;

        // Bring hovered card to front visually without touching other siblings
        card.SetAsLastSibling();

        // Recalculate canonical positions with spread spacing.
        // Uses stable sort-by-X ordering so SetAsLastSibling() doesn't remap other cards' slots.
        CleanupDictionaries();
        RecalculateCanonicalPositions();

        // Apply spread positions INSTANTLY to all non-hovered cards.
        // No animation here = no card moves through the cursor = no phantom hover events.
        foreach (var kvp in canonicalPositions)
        {
            if (kvp.Key == null || kvp.Key == card) continue;
            kvp.Key.localPosition = kvp.Value;
            kvp.Key.localRotation = canonicalRotations[kvp.Key];
        }
        // The hovered card's position (canonical + lift) is applied by PowerUpCardDisplay.OnPointerEnter
    }

    // Called by PowerUpCardDisplay when cursor exits a card
    public void OnHoverExit(Transform card)
    {
        if (_hoveredCard == card)
        {
            if (_pendingUnspread != null) StopCoroutine(_pendingUnspread);
            _pendingUnspread = StartCoroutine(UnspreadAfterDelay());
        }
    }

    private IEnumerator UnspreadAfterDelay()
    {
        // Small delay prevents flicker when cursor moves directly between cards
        yield return new WaitForSeconds(0.06f);
        _hoveredCard = null;
        _pendingUnspread = null;

        // Restore natural left-to-right sibling order before snapping back
        RestoreNaturalSiblingOrder();

        // Snap instantly — no ease when hover ends, only drag-return uses an ease
        SnapToLayout();
    }

    // Instantly repositions all non-hovered, non-dragged cards to their canonical positions.
    // Used when hover ends so there is no easing animation on un-spread.
    private void SnapToLayout()
    {
        if (_runningAnimation != null) { StopCoroutine(_runningAnimation); _runningAnimation = null; }

        CleanupDictionaries();
        RecalculateCanonicalPositions();

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || child == _hoveredCard || child == _draggedCard) continue;
            if (canonicalPositions.ContainsKey(child))
            {
                child.localPosition = canonicalPositions[child];
                child.localRotation = canonicalRotations[child];
            }
        }
    }

    // Returns the natural (left-to-right) sibling index for a card, based on canonical X order.
    // Use this instead of transform.GetSiblingIndex() when sibling order may have been modified by hover.
    public int GetNaturalSiblingIndex(Transform card)
    {
        var sorted = new List<KeyValuePair<Transform, Vector3>>(canonicalPositions);
        sorted.Sort((a, b) => a.Value.x.CompareTo(b.Value.x));
        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].Key == card) return i;
        }
        return card.GetSiblingIndex(); // fallback
    }

    // Resets sibling indices to match the canonical left-to-right (sort-by-X) order
    private void RestoreNaturalSiblingOrder()
    {
        var sorted = new List<KeyValuePair<Transform, Vector3>>(canonicalPositions);
        sorted.Sort((a, b) => a.Value.x.CompareTo(b.Value.x));
        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].Key != null)
                sorted[i].Key.SetSiblingIndex(i);
        }
    }

    private IEnumerator AnimateToNewLayout()
    {
        // Snapshot at the start so mid-animation changes don't affect this run
        Transform hoveredSnapshot = _hoveredCard;
        Transform draggedSnapshot = _draggedCard;

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

        // Animate to new positions (skip hovered card — its position is managed by hover lift)
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child == null || child == hoveredSnapshot || child == draggedSnapshot) continue;

                child.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                child.localRotation = Quaternion.Lerp(startRotations[i], targetRotations[i], t);
            }

            yield return null;
        }

        // Ensure final positions are exact (skip hovered card)
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null || child == hoveredSnapshot || child == draggedSnapshot) continue;
            if (canonicalPositions.ContainsKey(child))
            {
                child.localPosition = canonicalPositions[child];
                child.localRotation = canonicalRotations[child];
            }
        }

        _runningAnimation = null;
    }

    public void ApplyFanLayout()
    {
        CleanupDictionaries();
        RecalculateCanonicalPositions();
        ResetAllCardsToCanonicalPositions();
    }

    private void RecalculateCanonicalPositions()
    {
        // Collect valid cards and their previous canonical X (for stable left-to-right ordering).
        // We sort by previous X instead of sibling index so that SetAsLastSibling() on the hovered
        // card cannot remap other cards to different slots and cause phantom hover events.
        var cards = new List<Transform>();
        var previousX = new Dictionary<Transform, float>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null) continue;
            cards.Add(child);
            // Use existing canonical X if available; otherwise fall back to sibling index
            previousX[child] = canonicalPositions.ContainsKey(child)
                ? canonicalPositions[child].x
                : i * spacing; // fallback for first layout when all cards are at origin
        }

        // Clear AFTER saving old data
        canonicalPositions.Clear();
        canonicalRotations.Clear();

        if (cards.Count == 0) return;

        // Sort by previous canonical X → stable left-to-right order regardless of sibling changes
        cards.Sort((a, b) => previousX[a].CompareTo(previousX[b]));

        // Calculate and store positions using the sorted (stable) order
        Vector3[] positions = new Vector3[cards.Count];
        Quaternion[] rotations = new Quaternion[cards.Count];
        CalculateFanPositions(positions, rotations, cards.Count);

        for (int i = 0; i < cards.Count; i++)
        {
            canonicalPositions[cards[i]] = positions[i];
            canonicalRotations[cards[i]] = rotations[i];
        }
    }

    private void CalculateFanPositions(Vector3[] positions, Quaternion[] rotations, int count = -1)
    {
        if (count < 0) count = positions.Length;
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

        float effectiveSpacing = (_hoveredCard != null) ? spacing * hoverSpacingMultiplier : spacing;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + angleStep * i;
            float xOffset = effectiveSpacing * (i - middleIndex);

            // FIXED: Reverse the calculation so rightmost is highest
            float yOffset = -(count - 1 - i) * 3f; // Rightmost (highest index) = 0, leftmost = lowest

            positions[i] = new Vector3(xOffset, yOffset, 0f);
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
        _draggedCard = draggedCard.transform;
        draggedCard.transform.SetAsLastSibling();
    }

    // Call this when a drag ends and the card returns to the hand (not activated)
    public void OnCardDragEnd(Transform card)
    {
        if (_draggedCard == card) _draggedCard = null;
    }

    // Call this when a card is permanently removed (used)
    public void OnCardRemoved()
    {
        _draggedCard = null;
        RefreshLayout();
    }
}