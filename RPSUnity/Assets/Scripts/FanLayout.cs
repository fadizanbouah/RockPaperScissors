using UnityEngine;
using System.Collections;

public class FanLayout : MonoBehaviour
{
    [SerializeField] private float angleRange = 30f;
    [SerializeField] private float spacing = 150f; // Horizontal distance between cards
    [SerializeField] private float animationDuration = 0.3f; // Smooth transition time

    private void Start()
    {
        ApplyFanLayout();
    }

    // Call this whenever cards change (added/removed)
    public void RefreshLayout()
    {
        StartCoroutine(AnimateToNewLayout());
    }

    private IEnumerator AnimateToNewLayout()
    {
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

        // Calculate new positions
        Vector3[] targetPositions = new Vector3[childCount];
        Quaternion[] targetRotations = new Quaternion[childCount];
        CalculateFanPositions(targetPositions, targetRotations);

        // Animate to new positions
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            t = Mathf.SmoothStep(0, 1, t); // Smooth interpolation

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
        ApplyFanLayout();
    }

    public void ApplyFanLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        Vector3[] positions = new Vector3[childCount];
        Quaternion[] rotations = new Quaternion[childCount];

        CalculateFanPositions(positions, rotations);

        for (int i = 0; i < childCount; i++)
        {
            Transform card = transform.GetChild(i);
            if (card != null)
            {
                card.localPosition = positions[i];
                card.localRotation = rotations[i];

                PowerUpCardDisplay display = card.GetComponent<PowerUpCardDisplay>();
                if (display != null)
                {
                    display.StoreFanLayoutState(positions[i], rotations[i]);
                }
            }
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

            // IMPORTANT: Always use relative positions from zero
            positions[i] = new Vector3(xOffset, 0f, 0f);
            rotations[i] = Quaternion.Euler(0f, 0f, -angle);
        }
    }

    // Call this when a card starts being dragged
    public void OnCardDragStart(GameObject draggedCard)
    {
        // Temporarily remove the dragged card from layout calculations
        draggedCard.transform.SetAsLastSibling(); // Move to end so it renders on top
    }

    // Call this when a card is permanently removed (used)
    public void OnCardRemoved()
    {
        RefreshLayout();
    }
}