using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private Vector2 padding = new Vector2(20f, 20f);
    [SerializeField] private Vector2 offset = new Vector2(10f, 10f);

    private RectTransform rectTransform;
    private Canvas canvas;

    private static TooltipUI instance;

    private void Awake()
    {
        // Simple singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Start hidden
        gameObject.SetActive(false);
    }

    public static void Show(string title, string description, Vector3 worldPosition)
    {
        if (instance == null) return;

        instance.ShowTooltip(title, description, worldPosition);
    }

    public static void Hide()
    {
        if (instance == null) return;

        instance.gameObject.SetActive(false);
    }

    private void ShowTooltip(string title, string description, Vector3 worldPosition)
    {
        // Set text
        if (titleText != null)
            titleText.text = title;
        if (descriptionText != null)
            descriptionText.text = description;

        gameObject.SetActive(true);

        // Make the tooltip non-interactable so it doesn't block raycasts
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Force layout rebuild to get correct size
        Canvas.ForceUpdateCanvases();

        // Position tooltip below the icon
        PositionTooltipBelowIcon(worldPosition);
    }

    private void PositionTooltipBelowIcon(Vector3 iconWorldPosition)
    {
        // Convert icon world position to screen position
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, iconWorldPosition);

        // Convert screen position to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // Apply offset
        // X offset: positive = right, negative = left
        // Y offset: positive = up, negative = down
        localPoint.x += offset.x;
        localPoint.y -= offset.y; // Negative to go below the icon

        // Set position
        rectTransform.localPosition = localPoint;
        LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRect);

        // Keep tooltip on screen
        KeepOnScreen();
    }

    private void KeepOnScreen()
    {
        // Get canvas bounds
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.rect.size;

        // Get tooltip bounds
        Vector2 tooltipSize = backgroundRect.rect.size;
        Vector2 position = rectTransform.localPosition;

        // Clamp position to keep tooltip on screen
        float minX = -canvasSize.x / 2 + tooltipSize.x / 2 + padding.x;
        float maxX = canvasSize.x / 2 - tooltipSize.x / 2 - padding.x;
        float minY = -canvasSize.y / 2 + tooltipSize.y / 2 + padding.y;
        float maxY = canvasSize.y / 2 - tooltipSize.y / 2 - padding.y;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        rectTransform.localPosition = position;
    }
}