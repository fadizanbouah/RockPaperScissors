using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PassivePowerUpTracker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject iconPrefab; // Prefab with Image component
    [SerializeField] private Transform iconContainer; // HorizontalLayoutGroup container

    [Header("Icon Settings")]
    [SerializeField] private float iconSize = 50f; // Size of each icon
    [SerializeField] private float spacing = 10f; // Space between icons
    [SerializeField] private bool autoHide = true; // Hide when no passives

    private List<GameObject> activeIcons = new List<GameObject>();

    private void Start()
    {
        // Set up the layout group if needed
        HorizontalLayoutGroup layoutGroup = iconContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = iconContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        layoutGroup.spacing = spacing;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        // Clear existing icons
        ClearIcons();

        // Get passive power-ups from RunProgressManager
        if (RunProgressManager.Instance == null) return;

        List<PowerUpData> passivePowerUps = RunProgressManager.Instance.persistentPowerUps;

        // Hide container if no passives and autoHide is true
        if (autoHide && passivePowerUps.Count == 0)
        {
            iconContainer.gameObject.SetActive(false);
            return;
        }

        iconContainer.gameObject.SetActive(true);

        // Create icon for each passive power-up
        foreach (PowerUpData powerUp in passivePowerUps)
        {
            CreateIcon(powerUp);
        }
    }

    private void CreateIcon(PowerUpData powerUpData)
    {
        if (iconPrefab == null || iconContainer == null) return;

        GameObject iconGO = Instantiate(iconPrefab, iconContainer);
        Image iconImage = iconGO.GetComponent<Image>();

        if (iconImage != null)
        {
            // Use passiveTrackerIcon if available, otherwise fall back to regular icon
            Sprite iconSprite = GetIconForPowerUp(powerUpData);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
            }

            // Set size
            RectTransform rect = iconGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            // Optional: Add tooltip or hover info
            AddTooltip(iconGO, powerUpData);
        }

        activeIcons.Add(iconGO);
    }

    private Sprite GetIconForPowerUp(PowerUpData data)
    {
        // First check if there's a specific tracker icon
        if (data.passiveTrackerIcon != null)
            return data.passiveTrackerIcon;

        // Fall back to regular icon
        return data.icon;
    }

    private void AddTooltip(GameObject iconGO, PowerUpData powerUpData)
    {
        // Simple tooltip implementation - you can expand this
        // For now, just store the power-up name in the GameObject name
        iconGO.name = $"PassiveIcon_{powerUpData.powerUpName}";

        // Optional: Add a simple hover tooltip component
        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipText = $"{powerUpData.powerUpName}\n{powerUpData.description}";
    }

    private void ClearIcons()
    {
        foreach (GameObject icon in activeIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        activeIcons.Clear();
    }

    // Call this when entering a new room
    public void OnRoomStart()
    {
        RefreshDisplay();
    }
}

// Simple tooltip component
public class SimpleTooltip : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string tooltipText;
    private static GameObject tooltipPanel; // Shared tooltip panel

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Show tooltip - implement your tooltip UI here
        Debug.Log($"Tooltip: {tooltipText}");
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Hide tooltip
    }
}