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

    // NEW: Store which type of stack effect each icon represents
    private Dictionary<GameObject, System.Type> iconEffectTypes = new Dictionary<GameObject, System.Type>();
    private Dictionary<GameObject, TextMeshProUGUI> iconStackTexts = new Dictionary<GameObject, TextMeshProUGUI>();

    private void Start()
    {
        // Check for GridLayoutGroup first
        GridLayoutGroup gridLayout = iconContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            // GridLayoutGroup is already configured in Inspector
            // No need to set properties programmatically
        }
        else
        {
            // Fallback to HorizontalLayoutGroup if no GridLayoutGroup
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
        }

        RefreshDisplay();
    }

    private void Update()
    {
        UpdateStackEffectTexts();
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

            // NEW: Set up stack effect text if applicable
            SetupStackEffectText(iconGO, powerUpData);

            // Optional: Add tooltip or hover info
            AddTooltip(iconGO, powerUpData);
        }

        activeIcons.Add(iconGO);
    }

    // NEW: Setup text display for stack effects (Rock DR, Paper Dodge, Scissors Crit)
    private void SetupStackEffectText(GameObject iconGO, PowerUpData powerUpData)
    {
        if (powerUpData.effectPrefab == null) return;

        TextMeshProUGUI statText = null;
        System.Type effectType = null;

        // Check if this is a Rock DR Stack power-up
        if (powerUpData.effectPrefab.GetComponent<RockDRStackEffect>() != null)
        {
            effectType = typeof(RockDRStackEffect);
            statText = iconGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (statText != null)
            {
                statText.gameObject.SetActive(true);
                statText.text = $"{RockDRStackEffect.GetCurrentDR():F0}%";
                Debug.Log($"[PassivePowerUpTracker] Enabled DR text for Rock DR Stack");
            }
        }
        // Check if this is a Paper Dodge Stack power-up
        else if (powerUpData.effectPrefab.GetComponent<PaperDodgeStackEffect>() != null)
        {
            effectType = typeof(PaperDodgeStackEffect);
            statText = iconGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (statText != null)
            {
                statText.gameObject.SetActive(true);
                statText.text = $"{PaperDodgeStackEffect.GetCurrentDodge():F0}%";
                Debug.Log($"[PassivePowerUpTracker] Enabled Dodge text for Paper Dodge Stack");
            }
        }
        // Check if this is a Scissors Crit Stack power-up
        else if (powerUpData.effectPrefab.GetComponent<ScissorsCritChanceStackEffect>() != null)
        {
            effectType = typeof(ScissorsCritChanceStackEffect);
            statText = iconGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (statText != null)
            {
                statText.gameObject.SetActive(true);
                statText.text = $"{ScissorsCritChanceStackEffect.GetCurrentCrit():F0}%";
                Debug.Log($"[PassivePowerUpTracker] Enabled Crit text for Scissors Crit Stack");
            }
        }

        // Store the references if we found a stack effect
        if (statText != null && effectType != null)
        {
            iconStackTexts[iconGO] = statText;
            iconEffectTypes[iconGO] = effectType;
        }
        else if (effectType != null)
        {
            Debug.LogWarning("[PassivePowerUpTracker] Stack effect icon missing TextMeshProUGUI child for stat display!");
        }
    }

    // NEW: Update stack effect texts based on their type
    private void UpdateStackEffectTexts()
    {
        foreach (var kvp in iconStackTexts)
        {
            GameObject iconGO = kvp.Key;
            TextMeshProUGUI textComponent = kvp.Value;

            if (iconGO == null || textComponent == null) continue;

            // Get the effect type for this icon
            if (!iconEffectTypes.TryGetValue(iconGO, out System.Type effectType)) continue;

            // Update based on the specific effect type
            if (effectType == typeof(RockDRStackEffect))
            {
                float currentDR = RockDRStackEffect.GetCurrentDR();
                textComponent.text = $"{currentDR:F0}%";
            }
            else if (effectType == typeof(PaperDodgeStackEffect))
            {
                float currentDodge = PaperDodgeStackEffect.GetCurrentDodge();
                textComponent.text = $"{currentDodge:F0}%";
            }
            else if (effectType == typeof(ScissorsCritChanceStackEffect))
            {
                float currentCrit = ScissorsCritChanceStackEffect.GetCurrentCrit();
                textComponent.text = $"{currentCrit:F0}%";
            }
        }
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
        iconStackTexts.Clear(); // Clear the text dictionary
        iconEffectTypes.Clear(); // Clear the type dictionary
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
    private bool isHovering = false;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Show tooltip using the tooltip UI
        Debug.Log($"[Tooltip] OnPointerEnter - {tooltipTitle}");
        isHovering = true;
        TooltipUI.Show(tooltipTitle, tooltipDescription, transform.position);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Hide tooltip
        Debug.Log($"[Tooltip] OnPointerExit - {tooltipTitle}");
        isHovering = false;
        TooltipUI.Hide();
    }

    private void OnDisable()
    {
        // Ensure tooltip is hidden if this object is disabled
        if (isHovering)
        {
            TooltipUI.Hide();
            isHovering = false;
        }
    }

    private void OnDestroy()
    {
        // Ensure tooltip is hidden if this object is destroyed
        if (isHovering)
        {
            TooltipUI.Hide();
        }
    }
}