using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class PowerUpCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Should this card be draggable? Set true only for gameplay cards.")]
    public bool isDraggable = false;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;
    private PowerUpCardDisplay cardDisplay;
    private FanLayout fanLayout;
    private int originalSiblingIndex;
    private bool isPressed = false;

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
        }

        // NEW: Straighten the card AFTER all other operations
        transform.localRotation = Quaternion.identity;
        transform.localEulerAngles = Vector3.zero; // Extra safety
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

        // NEW: Ensure card stays straight while dragging
        if (isPressed)
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        // Delay the reset until AFTER EventSystem processes this frame
        StartCoroutine(ResetCardsNextFrame());

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
                StartCoroutine(ResetCardsNextFrame()); // Reset when returning to hand
                StartCoroutine(SmoothReturnToOriginalPosition());
                return;
            }

            // Check if this is a Double Use card and if one is already active
            PowerUpCardDisplay display = GetComponent<PowerUpCardDisplay>();
            PowerUpData powerUpData = display?.GetPowerUpData();

            if (powerUpData != null && IsDoubleUsePowerUp(powerUpData))
            {
                if (IsDoubleUseEffectActive())
                {
                    Debug.Log("[PowerUpCardDrag] Cannot activate - Double Use effect already active!");
                    StartCoroutine(ResetCardsNextFrame()); // Reset when returning to hand
                    StartCoroutine(SmoothReturnToOriginalPosition());
                    return;
                }
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(zone.GetComponent<RectTransform>(), Input.mousePosition, eventData.enterEventCamera))
            {
                Debug.Log("[PowerUpCardDrag] Mouse released over CardActivationZone.");

                DisableInteraction();
                BeginActivationSequence(zone.activationAnimationTarget.position);

                // NEW: Reset OTHER cards, but NOT this one since it's being activated
                StartCoroutine(ResetCardsNextFrame());

                // Trigger transition to PowerUpActivation substate
                zone.BeginPowerUpActivation(gameObject);
                return;
            }
        }

        Debug.Log("[PowerUpCardDrag] Drop not over activation zone. Returning to hand.");
        StartCoroutine(ResetCardsNextFrame()); // Reset when returning to hand
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

        // NEW: Ensure card is straight before animation
        transform.localRotation = Quaternion.identity;
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

        float duration = 0.5f; // Slower for testing
        float time = 0f;

        // Use local position instead of anchored position
        Vector3 start = transform.localPosition;

        // Get the canonical position from FanLayout
        FanLayout fanLayout = GetComponentInParent<FanLayout>();
        Vector3 targetPosition = start;

        if (fanLayout != null)
        {
            targetPosition = fanLayout.GetCanonicalPosition(transform);

            // Also get canonical rotation
            Quaternion targetRotation = fanLayout.GetCanonicalRotation(transform);
            Quaternion startRotation = transform.localRotation;

            // Animate both position and rotation
            while (time < duration)
            {
                if (transform == null) break; // Safety check

                float t = time / duration;
                t = 1f - Mathf.Pow(1f - t, 3f); // CHANGED: Cubic ease-out (slows at end)

                transform.localPosition = Vector3.Lerp(start, targetPosition, t);
                transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

                time += Time.deltaTime;
                yield return null;
            }

            // Ensure we end at exact target
            if (transform != null)
            {
                transform.localPosition = targetPosition;
                transform.localRotation = targetRotation;
            }
        }
        else
        {
            // Fallback: just wait the duration
            yield return new WaitForSeconds(duration);
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

    private IEnumerator ResetCardsNextFrame()
    {
        // Wait for EventSystem to finish processing
        yield return null;

        // Store reference to this card so we can skip it
        Transform thisCard = transform;

        // Reset all OTHER cards (not this one)
        if (transform.parent != null)
        {
            foreach (Transform child in transform.parent)
            {
                if (child == thisCard) continue; // Skip the card that was just dragged

                // Send pointer exit event
                ExecuteEvents.Execute(child.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);

                // Force button state refresh
                Button button = child.GetComponent<Button>();
                if (button != null && button.targetGraphic != null)
                {
                    bool wasInteractable = button.interactable;
                    button.interactable = false;
                    button.interactable = wasInteractable;
                    button.targetGraphic.color = button.colors.normalColor;
                }

                // Reset position via PowerUpCardDisplay
                PowerUpCardDisplay display = child.GetComponent<PowerUpCardDisplay>();
                if (display != null)
                {
                    display.ResetToFanPosition();
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isDraggable) return;

        isPressed = true;

        // Straighten the card when pressed
        transform.localRotation = Quaternion.identity;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDraggable) return;

        isPressed = false;

        // If we're not dragging (just a click), restore fan rotation
        if (eventData.dragging == false)
        {
            PowerUpCardDisplay display = GetComponent<PowerUpCardDisplay>();
            if (display != null)
            {
                display.ResetToFanPosition();
            }
        }
    }
}