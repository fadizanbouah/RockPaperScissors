using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerCombatTracker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject iconPrefab; // Same prefab as passive tracker
    [SerializeField] private Transform iconContainer; // Where icons will be spawned

    [Header("Icon Settings")]
    [SerializeField] private float iconWidth = 50f;
    [SerializeField] private float iconHeight = 50f;
    [SerializeField] private float spacing = 10f;

    [Header("Damage Type Icons")]
    [SerializeField] private Sprite rockIcon;
    [SerializeField] private Sprite paperIcon;
    [SerializeField] private Sprite scissorsIcon;

    private HandController playerHand;
    private List<GameObject> currentIcons = new List<GameObject>();
    private bool isInitialized = false;
    private Dictionary<PowerUpEffectBase, GameObject> activeEffectIcons = new Dictionary<PowerUpEffectBase, GameObject>();

    private void OnEnable()
    {
        // Reset initialization flag
        isInitialized = false;
    }

    private void Update()
    {
        // Wait until we're in gameplay and player exists before initializing
        if (!isInitialized)
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.currentState == GameStateManager.GameState.Gameplay &&
                GameObject.FindWithTag("Player") != null)
            {
                Initialize();
                isInitialized = true;
            }
        }

        // Update tooltips with current damage values
        if (isInitialized && playerHand != null)
        {
            UpdateTooltips();
        }

        // Clean up icons for effects that have been removed
        if (isInitialized)
        {
            List<PowerUpEffectBase> toRemove = new List<PowerUpEffectBase>();

            foreach (var kvp in activeEffectIcons)
            {
                // If the effect GameObject is null (destroyed), remove its icon
                if (kvp.Key == null)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var effect in toRemove)
            {
                RemoveActiveEffect(effect);
            }
        }
    }

    private void Initialize()
    {

        // Auto-find IconContainer if not assigned
        if (iconContainer == null)
        {
            iconContainer = transform.Find("IconContainer");
            if (iconContainer == null)
            {
                Debug.LogError("[PlayerCombatTracker] IconContainer child not found! Creating one...");
                GameObject container = new GameObject("IconContainer");
                container.transform.SetParent(transform);
                iconContainer = container.transform;
            }
        }

        // Check for GridLayoutGroup first (like PassivePowerUpTracker)
        GridLayoutGroup gridLayout = iconContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            // GridLayoutGroup is already configured in Inspector
            Debug.Log("[PlayerCombatTracker] Using existing GridLayoutGroup");
        }
        else
        {
            // No grid layout, set up horizontal layout
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

        // Find the player
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerHand = playerGO.GetComponent<HandController>();
        }

        CreateDamageIcons();
    }

    private void CreateDamageIcons()
    {

        // Clear any existing icons
        ClearIcons();

        // Create Rock icon
        CreateDamageIcon(rockIcon, "Rock");

        // Create Paper icon
        CreateDamageIcon(paperIcon, "Paper");

        // Create Scissors icon
        CreateDamageIcon(scissorsIcon, "Scissors");

    }

    private void CreateDamageIcon(Sprite iconSprite, string damageType)
    {

        if (iconPrefab == null)
        {
            return;
        }

        if (iconContainer == null)
        {
            return;
        }

        if (iconSprite == null)
        {
            return;
        }

        GameObject iconGO = Instantiate(iconPrefab, iconContainer);

        if (iconGO == null)
        {
            return;
        }


        Image iconImage = iconGO.GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
        }

        // Set size
        RectTransform rect = iconGO.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        // Add tooltip
        AddTooltip(iconGO, damageType);

        currentIcons.Add(iconGO);
    }

    private void AddTooltip(GameObject iconGO, string damageType)
    {
        iconGO.name = $"CombatIcon_{damageType}";

        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipTitle = $"{damageType} Damage";
        tooltip.tooltipDescription = "Calculating..."; // Will be updated in Update()
    }

    private void UpdateTooltips()
    {
        foreach (GameObject icon in currentIcons)
        {
            SimpleTooltip tooltip = icon.GetComponent<SimpleTooltip>();
            if (tooltip != null)
            {
                string damageType = GetDamageTypeFromTooltip(tooltip.tooltipTitle);
                if (!string.IsNullOrEmpty(damageType))
                {
                    int damage = playerHand.GetEffectiveDamage(damageType);
                    int baseDamage = playerHand.baseDamage;
                    int bonus = damage - baseDamage;

                    tooltip.tooltipDescription = $"Total Damage: {damage}\nBase: {baseDamage}";
                    if (bonus > 0)
                    {
                        tooltip.tooltipDescription += $"\nBonuses: +{bonus}";
                    }
                }
            }
        }
    }

    private string GetDamageTypeFromTooltip(string tooltipTitle)
    {
        if (tooltipTitle.Contains("Rock")) return "Rock";
        if (tooltipTitle.Contains("Paper")) return "Paper";
        if (tooltipTitle.Contains("Scissors")) return "Scissors";
        return "";
    }

    private void ClearIcons()
    {
        foreach (GameObject icon in currentIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        currentIcons.Clear();
    }

    // Call this when player reference changes (e.g., new room)
    public void UpdatePlayerReference()
    {
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
        {
            playerHand = playerGO.GetComponent<HandController>();
        }
    }

    public void AddActiveEffect(PowerUpEffectBase effect)
    {

        if (effect == null || effect.SourceData == null)
        {
            return;
        }

        // Don't add if already tracking this effect
        if (activeEffectIcons.ContainsKey(effect)) return;

        PowerUpData data = effect.SourceData;
        Sprite iconToUse = data.statusIcon != null ? data.statusIcon : data.icon;

        if (iconToUse == null) return;

        GameObject iconGO = Instantiate(iconPrefab, iconContainer);

        // Configure the icon (same as before)
        Image iconImage = iconGO.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = iconToUse;
        }

        RectTransform rect = iconGO.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipTitle = data.powerUpName + " (Active)";
        tooltip.tooltipDescription = data.description;

        // Map the effect to its icon
        activeEffectIcons[effect] = iconGO;
    }

    public void RemoveActiveEffect(PowerUpEffectBase effect)
    {
        if (activeEffectIcons.ContainsKey(effect))
        {
            Destroy(activeEffectIcons[effect]);
            activeEffectIcons.Remove(effect);
        }
    }

    public void ClearActiveEffects()
    {
        foreach (var kvp in activeEffectIcons)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }
        activeEffectIcons.Clear();
    }

    public void OnRoomStart()
    {
        ClearActiveEffects(); // This already clears all effect icons
        UpdatePlayerReference(); // Make sure we have the current player
    }
}