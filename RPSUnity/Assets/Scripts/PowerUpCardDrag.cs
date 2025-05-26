using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PowerUpCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Should this card be draggable? Set true only for gameplay cards.")]
    public bool isDraggable = false;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private PowerUpCardDisplay cardDisplay;

    private RectTransform activationZoneRect;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        cardDisplay = GetComponent<PowerUpCardDisplay>();

        // Find the CardActivationZone once at runtime
        CardActivationZone zone = FindObjectOfType<CardActivationZone>();
        if (zone != null)
        {
            activationZoneRect = zone.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("[PowerUpCardDrag] CardActivationZone not found in scene!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        // Reset hover offset before dragging
        if (cardDisplay != null)
        {
            cardDisplay.ResetHoverPosition();

            // Show the activation zone visual when dragging starts
            CardActivationZone zone = FindObjectOfType<CardActivationZone>();
            if (zone != null)
            {
                zone.ShowVisual();
            }

            // Straighten the card by resetting rotation
            transform.localRotation = Quaternion.identity;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Always hide the activation zone visual when drag ends
        CardActivationZone zone = FindObjectOfType<CardActivationZone>();
        if (zone != null)
        {
            zone.HideVisual();

            if (!isDraggable) return;

            canvasGroup.blocksRaycasts = true;

            if (RectTransformUtility.RectangleContainsScreenPoint(zone.GetComponent<RectTransform>(), Input.mousePosition, eventData.enterEventCamera))
            {
                Debug.Log("[PowerUpCardDrag] Mouse released over CardActivationZone.");
                DisableInteraction();
                BeginActivationSequence(zone.activationAnimationTarget.position);
                return;
            }
        }

        Debug.Log("[PowerUpCardDrag] Drop not over activation zone. Returning to hand.");
        StartCoroutine(SmoothReturnToOriginalPosition());
    }

    public void DisableInteraction()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void BeginActivationSequence(Vector3 targetPosition)
    {
        Debug.LogWarning($"[DEBUG] BeginActivationSequence called for: {cardDisplay?.GetPowerUpData()?.powerUpName}");

        if (!isDraggable) return;

        Debug.Log("[PowerUpCardDrag] Begin activation sequence at " + targetPosition);
        transform.position = targetPosition;

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log("Animator trigger 'Activate' sent. Animator is: " + animator.name);
            animator.SetTrigger("Activate");
        }
        else
        {
            Debug.LogWarning("Animator not found on PowerUpCard or its children!");
        }

        // Apply the power-up effect
        if (cardDisplay != null && cardDisplay.IsGameplayCard())
        {
            PowerUpData data = cardDisplay.GetPowerUpData();
            RunProgressManager.Instance.ApplyPowerUpEffect(data);
        }

        // Remove visual card's data from acquired list if it was used
        PowerUpData powerUp = cardDisplay?.GetPowerUpData();
        if (powerUp != null)
        {
            RunProgressManager.Instance.RemoveAcquiredPowerUp(powerUp);
        }
    }

    private IEnumerator SmoothReturnToOriginalPosition()
    {
        float duration = 0.15f;
        float time = 0f;
        Vector2 start = rectTransform.anchoredPosition;

        while (time < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(start, originalAnchoredPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalAnchoredPosition;
        gameObject.GetComponent<PowerUpCardDisplay>().ResetToFanPosition();
    }
}
