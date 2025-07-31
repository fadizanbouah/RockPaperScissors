using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PassivePowerUpTracker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject iconPrefab; // Prefab with Image component
    [SerializeField] private Transform iconContainer; // HorizontalLayoutGroup container

    [Header("Icon Settings")]
    [SerializeField] private float iconWidth = 50f; // Width of each icon
    [SerializeField] private float iconHeight = 50f; // Height of each icon
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
                rect.sizeDelta = new Vector2(iconWidth, iconHeight);
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
        // Simple tooltip implementation
        iconGO.name = $"PassiveIcon_{powerUpData.powerUpName}";

        // Add tooltip component
        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipTitle = powerUpData.powerUpName;

        // Get the appropriate description based on level
        if (powerUpData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int level = RunProgressManager.Instance.GetPowerUpLevel(powerUpData);
            tooltip.tooltipDescription = powerUpData.GetDescriptionForLevel(level);

            // Add level suffix to title if upgradeable
            tooltip.tooltipTitle += powerUpData.GetLevelSuffix(level);
        }
        else
        {
            tooltip.tooltipDescription = powerUpData.description;
        }
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
    public string tooltipTitle;
    public string tooltipDescription;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Show tooltip using the tooltip UI
        TooltipUI.Show(tooltipTitle, tooltipDescription, transform.position);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Hide tooltip
    }
}