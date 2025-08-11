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
    private FanLayout fanLayout;
    private int originalSiblingIndex;

    private RectTransform activationZoneRect;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        cardDisplay = GetComponent<PowerUpCardDisplay>();

        // Find the FanLayout in parent
        fanLayout = GetComponentInParent<FanLayout>();

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

        // Store the original sibling index (layer order)
        originalSiblingIndex = transform.GetSiblingIndex();

        // Store the current anchored position (not world position)
        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        // Move card to top layer while dragging
        transform.SetAsLastSibling();

        // Notify FanLayout that we're dragging
        if (fanLayout != null)
        {
            fanLayout.OnCardDragStart(gameObject);
        }

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

    private void NotifyCardUsed()
    {
        // Notify FanLayout to re-center remaining cards
        if (fanLayout != null)
        {
            fanLayout.OnCardRemoved();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // Additional check during drag
        if (PowerUpUsageTracker.Instance != null && !PowerUpUsageTracker.Instance.CanUsePowerUp())
        {
            // Cancel the drag
            OnEndDrag(eventData);
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // Clear the drag state immediately
        eventData.pointerDrag = null;

        // Always hide the activation zone visual when drag ends
        CardActivationZone zone = FindObjectOfType<CardActivationZone>();
        if (zone != null)
        {
            zone.HideVisual();

            canvasGroup.blocksRaycasts = true;

            // Check if power-up can be used
            if (PowerUpUsageTracker.Instance != null && !PowerUpUsageTracker.Instance.CanUsePowerUp())
            {
                Debug.Log("[PowerUpCardDrag] Cannot activate - power-up already used this round!");
                StartCoroutine(SmoothReturnToOriginalPosition());
                return;
            }

            // NEW: Check if this is a Double Use card and if one is already active
            PowerUpCardDisplay display = GetComponent<PowerUpCardDisplay>();
            PowerUpData powerUpData = display?.GetPowerUpData();

            if (powerUpData != null && IsDoubleUsePowerUp(powerUpData))
            {
                if (IsDoubleUseEffectActive())
                {
                    Debug.Log("[PowerUpCardDrag] Cannot activate - Double Use effect already active!");
                    StartCoroutine(SmoothReturnToOriginalPosition());
                    return;
                }
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(zone.GetComponent<RectTransform>(), Input.mousePosition, eventData.enterEventCamera))
            {
                Debug.Log("[PowerUpCardDrag] Mouse released over CardActivationZone.");

                DisableInteraction();
                BeginActivationSequence(zone.activationAnimationTarget.position);

                // Trigger transition to PowerUpActivation substate
                zone.BeginPowerUpActivation(gameObject);
                return;
            }
        }

        Debug.Log("[PowerUpCardDrag] Drop not over activation zone. Returning to hand.");
        StartCoroutine(SmoothReturnToOriginalPosition());
    }

    private void DisableAllOtherPowerUpCards()
    {
        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
        {
            spawner.SetAllCardsInteractable(false);
        }
    }

    public void DisableInteraction()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void BeginActivationSequence(Vector3 targetPosition)
    {
        Debug.Log($"[DEBUG] BeginActivationSequence called for: {cardDisplay?.GetPowerUpData()?.powerUpName}");

        if (!isDraggable) return;

        Debug.Log("[PowerUpCardDrag] Begin activation sequence at " + targetPosition);
        transform.position = targetPosition;

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            Debug.Log("Animator trigger 'ActivePowerUpActivation' sent. Animator is: " + animator.name);
            animator.SetTrigger("ActivePowerUpActivation");
        }
        else
        {
            Debug.LogWarning("Animator not found on PowerUpCard or its children!");
        }

        // Logic for applying power-up effect has been moved to RockPaperScissorsGame (PowerUpActivation substate)
    }

    private IEnumerator SmoothReturnToOriginalPosition()
    {
        // Disable all interactions during the return animation
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Disable the EventSystem temporarily to prevent interference
        var eventSystem = EventSystem.current;
        bool wasEventSystemEnabled = false;
        if (eventSystem != null)
        {
            wasEventSystemEnabled = eventSystem.enabled;
            eventSystem.enabled = false;
        }

        float duration = 0.15f;
        float time = 0f;
        Vector2 start = rectTransform.anchoredPosition;

        // Get the stored fan position from the card display
        PowerUpCardDisplay display = GetComponent<PowerUpCardDisplay>();
        Vector2 targetPosition = originalAnchoredPosition;

        if (display != null)
        {
            // Use the fan layout position if available
            Vector3 fanPos = display.GetStoredFanPosition();
            targetPosition = new Vector2(fanPos.x, fanPos.y);
        }

        while (time < duration)
        {
            if (rectTransform == null) break; // Safety check

            rectTransform.anchoredPosition = Vector2.Lerp(start, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure we end at the exact target position
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = targetPosition;
        }

        // Reset rotation and scale as well
        if (display != null)
        {
            display.ResetToFanPosition();
        }

        // IMPORTANT: Restore the original sibling index (layer order)
        transform.SetSiblingIndex(originalSiblingIndex);

        // Re-enable the EventSystem
        if (eventSystem != null && wasEventSystemEnabled)
        {
            eventSystem.enabled = true;
        }

        // Re-enable interactions after animation completes
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private bool IsDoubleUsePowerUp(PowerUpData data)
    {
        // Check if this is a Double Use power-up
        if (data.powerUpName == "Double Activation" ||
            data.powerUpName == "Double Use" ||
            (data.effectPrefab != null && data.effectPrefab.GetComponent<DoubleUsePowerUpEffect>() != null))
        {
            return true;
        }
        return false;
    }

    private bool IsDoubleUseEffectActive()
    {
        // Check if a Double Use effect is currently active
        if (PowerUpEffectManager.Instance != null)
        {
            var effects = PowerUpEffectManager.Instance.GetActiveEffects();
            foreach (var effect in effects)
            {
                if (effect is DoubleUsePowerUpEffect doubleUse && doubleUse.IsEffectActive())
                {
                    return true;
                }
            }
        }
        return false;
    }
}