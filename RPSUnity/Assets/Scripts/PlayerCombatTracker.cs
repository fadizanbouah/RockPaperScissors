using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

    [Header("Damage Text Settings")]
    [SerializeField] private Color damageTextColor = Color.white;
    [SerializeField] private int damageTextFontSize = 14;
    [SerializeField] private bool showDamageText = true;

    private HandController playerHand;
    private List<GameObject> currentIcons = new List<GameObject>();
    private Dictionary<string, TextMeshProUGUI> damageTexts = new Dictionary<string, TextMeshProUGUI>();
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

        // Update tooltips and damage text with current damage values
        if (isInitialized && playerHand != null)
        {
            UpdateTooltips();
            UpdateDamageTexts();
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

        // Create damage text overlay
        if (showDamageText)
        {
            CreateDamageTextOverlay(iconGO, damageType);
        }

        // Add tooltip
        AddTooltip(iconGO, damageType);

        currentIcons.Add(iconGO);
    }

    private void CreateDamageTextOverlay(GameObject iconGO, string damageType)
    {
        // Create a new GameObject for the text
        GameObject textGO = new GameObject($"DamageText_{damageType}");
        textGO.transform.SetParent(iconGO.transform, false);

        // Add TextMeshProUGUI component
        TextMeshProUGUI damageText = textGO.AddComponent<TextMeshProUGUI>();

        // Configure text properties
        damageText.text = "0";
        damageText.fontSize = damageTextFontSize;
        damageText.color = damageTextColor;
        damageText.alignment = TextAlignmentOptions.Center;
        damageText.fontStyle = FontStyles.Bold;

        // Add outline for better visibility
        damageText.outlineWidth = 0.2f;
        damageText.outlineColor = Color.black;

        // Position the text (bottom-right corner of icon)
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0f);
        textRect.anchorMax = new Vector2(1f, 0.5f);
        textRect.offsetMin = new Vector2(0, 0);
        textRect.offsetMax = new Vector2(0, 0);
        textRect.anchoredPosition = new Vector2(5, -5); // Slight offset from corner

        // Store reference for updates
        damageTexts[damageType] = damageText;
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
                    // Get the total damage including all bonuses
                    int totalDamage = GetTotalDamageForType(damageType);
                    tooltip.tooltipDescription = $"{totalDamage}";
                }
            }
        }
    }

    private void UpdateDamageTexts()
    {
        if (playerHand == null || !showDamageText) return;

        foreach (var kvp in damageTexts)
        {
            string damageType = kvp.Key;
            TextMeshProUGUI text = kvp.Value;

            if (text != null)
            {
                int damage = GetTotalDamageForType(damageType);
                text.text = damage.ToString();
            }
        }
    }

    private int GetTotalDamageForType(string damageType)
    {
        if (playerHand == null) return 0;

        // Get the base damage for this specific sign (WITHOUT active power-up effects)
        int baseForSign = damageType switch
        {
            "Rock" => playerHand.rockDamage,
            "Paper" => playerHand.paperDamage,
            "Scissors" => playerHand.scissorsDamage,
            _ => 10
        };

        // Add only the persistent passive bonuses (from PlayerProgressData)
        int passiveBonus = 0;
        if (playerHand.isPlayer)
        {
            passiveBonus += PlayerProgressData.Instance.bonusBaseDamage;

            if (damageType == "Rock")
                passiveBonus += PlayerProgressData.Instance.bonusRockDamage;
            else if (damageType == "Paper")
                passiveBonus += PlayerProgressData.Instance.bonusPaperDamage;
            else if (damageType == "Scissors")
                passiveBonus += PlayerProgressData.Instance.bonusScissorsDamage;
        }

        int totalBaseDamage = baseForSign + passiveBonus;
        return totalBaseDamage;
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
        damageTexts.Clear();
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
        if (effect == null || effect.SourceData == null) return;
        if (activeEffectIcons.ContainsKey(effect)) return;

        PowerUpData data = effect.SourceData;

        if (data.statusIcon == null)
        {
            Debug.Log($"[PlayerCombatTracker] {data.powerUpName} has no status icon - not adding to tracker");
            return;
        }

        GameObject iconGO = Instantiate(iconPrefab, iconContainer);

        // Configure icon
        Image iconImage = iconGO.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = data.statusIcon;
        }

        RectTransform rect = iconGO.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        // NEW: Set up counter text if effect has duration
        TextMeshProUGUI counterText = iconGO.GetComponentInChildren<TextMeshProUGUI>(true);
        Debug.Log($"[PlayerCombatTracker] Found counter text: {counterText != null}");
        if (counterText != null)
        {
            // Check if effect has rounds remaining
            if (effect is IDurationEffect durationEffect)
            {
                Debug.Log($"[PlayerCombatTracker] Effect has duration: {durationEffect.GetRoundsRemaining()}");
                counterText.gameObject.SetActive(true);
                counterText.text = durationEffect.GetRoundsRemaining().ToString();
            }
            else
            {
                counterText.gameObject.SetActive(false);
            }
        }

        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipTitle = data.powerUpName;
        tooltip.tooltipDescription = data.description;

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
        ClearActiveEffects();
        UpdatePlayerReference();

        // Rebuild icons for active effects
        if (PowerUpEffectManager.Instance != null)
        {
            var activeEffects = PowerUpEffectManager.Instance.GetActiveEffects();
            foreach (var effect in activeEffects)
            {
                if (effect != null && effect.SourceData != null &&
                    !effect.SourceData.isPassive && effect.IsEffectActive())
                {
                    AddActiveEffect(effect);
                }
            }
        }
    }

    public void UpdateEffectCounter(PowerUpEffectBase effect)
    {
        if (!activeEffectIcons.ContainsKey(effect)) return;

        GameObject iconGO = activeEffectIcons[effect];
        TextMeshProUGUI counterText = iconGO.GetComponentInChildren<TextMeshProUGUI>(true);

        if (counterText != null && effect is IDurationEffect durationEffect)
        {
            int remaining = durationEffect.GetRoundsRemaining();
            if (remaining > 0)
            {
                counterText.text = remaining.ToString();
            }
        }
    }
}