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
    private bool droppedInActivationZone = false;
    private PowerUpCardDisplay cardDisplay;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        cardDisplay = GetComponent<PowerUpCardDisplay>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        droppedInActivationZone = false;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        // Reset any hover offset before drag starts
        if (cardDisplay != null)
        {
            cardDisplay.ResetHoverPosition();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        rectTransform.anchoredPosition += eventData.delta / transform.root.GetComponent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        canvasGroup.blocksRaycasts = true;

        if (!droppedInActivationZone)
        {
            StartCoroutine(SmoothReturnToOriginalPosition());
        }
    }

    public void DisableInteraction()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void BeginActivationSequence(Vector3 targetPosition)
    {
        if (!isDraggable) return;

        Debug.Log("[PowerUpCardDrag] Begin activation sequence at " + targetPosition);
        droppedInActivationZone = true;

        // TODO: Add animation and effect application
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
