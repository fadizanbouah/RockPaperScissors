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
    }

    private void Initialize()
    {
        Debug.Log("[PlayerCombatTracker] Initializing...");

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
            Debug.Log("[PlayerCombatTracker] Found player: " + playerGO.name);
        }

        CreateDamageIcons();
    }

    private void CreateDamageIcons()
    {
        Debug.Log("[PlayerCombatTracker] CreateDamageIcons called");

        // Clear any existing icons
        ClearIcons();

        // Create Rock icon
        CreateDamageIcon(rockIcon, "Rock");

        // Create Paper icon
        CreateDamageIcon(paperIcon, "Paper");

        // Create Scissors icon
        CreateDamageIcon(scissorsIcon, "Scissors");

        Debug.Log($"[PlayerCombatTracker] Total icons created: {currentIcons.Count}");
    }

    private void CreateDamageIcon(Sprite iconSprite, string damageType)
    {
        Debug.Log($"[PlayerCombatTracker] Attempting to create {damageType} icon");

        if (iconPrefab == null)
        {
            Debug.LogError("[PlayerCombatTracker] iconPrefab is null!");
            return;
        }

        if (iconContainer == null)
        {
            Debug.LogError("[PlayerCombatTracker] iconContainer is null!");
            return;
        }

        if (iconSprite == null)
        {
            Debug.LogError($"[PlayerCombatTracker] {damageType} sprite is null!");
            return;
        }

        Debug.Log($"[PlayerCombatTracker] All checks passed, instantiating {damageType} icon");

        GameObject iconGO = Instantiate(iconPrefab, iconContainer);

        if (iconGO == null)
        {
            Debug.LogError($"[PlayerCombatTracker] Failed to instantiate icon for {damageType}!");
            return;
        }

        Debug.Log($"[PlayerCombatTracker] Successfully instantiated {damageType} icon: {iconGO.name}");

        Image iconImage = iconGO.GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            Debug.Log($"[PlayerCombatTracker] Set sprite for {damageType}");
        }
        else
        {
            Debug.LogError($"[PlayerCombatTracker] No Image component found on instantiated icon!");
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
        Debug.Log($"[PlayerCombatTracker] Added {damageType} to currentIcons list. Count: {currentIcons.Count}");
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
}