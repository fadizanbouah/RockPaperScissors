using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class EnemyCombatTracker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject iconPrefab; // Same prefab as player tracker
    [SerializeField] private Transform iconContainer; // The CombatIconContainer child

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

    private HandController enemyHand;
    private List<GameObject> currentIcons = new List<GameObject>();
    private List<GameObject> currentTraitIcons = new List<GameObject>();
    private Dictionary<string, TextMeshProUGUI> damageTexts = new Dictionary<string, TextMeshProUGUI>();
    private bool isInitialized = false;

    // NEW: Track active effect icons (like PlayerCombatTracker)
    private Dictionary<PowerUpEffectBase, GameObject> activeEffectIcons = new Dictionary<PowerUpEffectBase, GameObject>();

    private void OnEnable()
    {
        // Reset initialization flag
        isInitialized = false;
    }

    private void Update()
    {
        // Wait until we're in gameplay and enemy exists before initializing
        if (!isInitialized)
        {
            if (GameStateManager.Instance != null &&
                GameStateManager.Instance.currentState == GameStateManager.GameState.Gameplay &&
                RoomManager.Instance != null)
            {
                HandController currentEnemy = RoomManager.Instance.GetCurrentEnemy();
                if (currentEnemy != null)
                {
                    UpdateEnemyReference(currentEnemy);
                    isInitialized = true;
                }
            }
        }

        // Update tooltips and damage text with current damage values
        if (isInitialized && enemyHand != null)
        {
            UpdateTooltips();
            UpdateDamageTexts();
        }

        // NEW: Clean up icons for effects that have been removed
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
        Debug.Log($"[EnemyCombatTracker] Initializing for enemy: {enemyHand?.name ?? "null"}");

        // Auto-find IconContainer if not assigned
        if (iconContainer == null)
        {
            iconContainer = transform.Find("CombatIconContainer");
            if (iconContainer == null)
            {
                Debug.LogError("[EnemyCombatTracker] CombatIconContainer child not found! Creating one...");
                GameObject container = new GameObject("CombatIconContainer");
                container.transform.SetParent(transform);
                iconContainer = container.transform;
            }
        }

        // Set up GridLayoutGroup
        GridLayoutGroup gridLayout = iconContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = iconContainer.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(iconWidth, iconHeight);
            gridLayout.spacing = new Vector2(spacing, spacing);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3; // Rock, Paper, Scissors in a row
        }

        CreateDamageIcons();
        CreateTraitIcons();
    }

    private void CreateDamageIcons()
    {
        Debug.Log("[EnemyCombatTracker] Creating damage icons");

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
        if (iconPrefab == null || iconContainer == null || iconSprite == null)
        {
            Debug.LogWarning($"[EnemyCombatTracker] Missing required components for {damageType} icon");
            return;
        }

        GameObject iconGO = Instantiate(iconPrefab, iconContainer);
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
        iconGO.name = $"EnemyCombatIcon_{damageType}";

        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipTitle = $"{damageType} Damage";
        tooltip.tooltipDescription = "Calculating..."; // Will be updated in UpdateTooltips()
    }

    private void UpdateTooltips()
    {
        if (enemyHand == null) return;

        foreach (GameObject icon in currentIcons)
        {
            SimpleTooltip tooltip = icon.GetComponent<SimpleTooltip>();
            if (tooltip != null)
            {
                string damageType = GetDamageTypeFromTooltip(tooltip.tooltipTitle);
                if (!string.IsNullOrEmpty(damageType))
                {
                    // Get the base damage for this specific sign (no power-ups for enemies currently)
                    int damage = GetDamageForType(damageType);
                    tooltip.tooltipDescription = $"{damage}";
                }
            }
        }
    }

    private void UpdateDamageTexts()
    {
        if (enemyHand == null || !showDamageText) return;

        foreach (var kvp in damageTexts)
        {
            string damageType = kvp.Key;
            TextMeshProUGUI text = kvp.Value;

            if (text != null)
            {
                int damage = GetDamageForType(damageType);
                text.text = damage.ToString();
            }
        }
    }

    private int GetDamageForType(string damageType)
    {
        if (enemyHand == null) return 0;

        return damageType switch
        {
            "Rock" => enemyHand.rockDamage,
            "Paper" => enemyHand.paperDamage,
            "Scissors" => enemyHand.scissorsDamage,
            _ => 10
        };
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

    public void UpdateEnemyReference(HandController enemy)
    {
        if (enemy != null && !enemy.isPlayer)
        {
            enemyHand = enemy;

            if (!isInitialized)
            {
                Initialize();
                isInitialized = true;
            }
            else
            {
                // Clear old trait icons before creating new ones
                ClearTraitIcons();

                // Recreate trait icons for the new enemy
                CreateTraitIcons();

                // Just refresh the tooltips and damage texts for the new enemy
                UpdateTooltips();
                UpdateDamageTexts();
            }

            Debug.Log($"[EnemyCombatTracker] Updated enemy reference to: {enemy.name}");
        }
    }

    // Call this when a new enemy spawns or room changes
    public void OnRoomStart()
    {
        // Wait for the new enemy to be set
        HandController currentEnemy = RoomManager.Instance?.GetCurrentEnemy();
        if (currentEnemy != null)
        {
            UpdateEnemyReference(currentEnemy);
        }
        else
        {
            // Clear icons if no enemy
            ClearIcons();
            ClearTraitIcons(); // NEW: Also clear trait icons
            enemyHand = null;
            isInitialized = false;
        }

        // Clear active effects for new room
        ClearActiveEffects();
    }

    private void CreateTraitIcons()
    {
        Debug.Log("[EnemyCombatTracker] CreateTraitIcons called");

        if (enemyHand == null)
        {
            Debug.LogWarning("[EnemyCombatTracker] enemyHand is null!");
            return;
        }

        // Get enemy traits
        EnemyTraits enemyTraits = enemyHand.GetComponent<EnemyTraits>();
        Debug.Log($"[EnemyCombatTracker] EnemyTraits component: {(enemyTraits != null ? "Found" : "NOT FOUND")}");

        if (enemyTraits == null)
        {
            Debug.Log("[EnemyCombatTracker] Enemy has no traits");
            return;
        }

        // Get the trait list
        List<EnemyTraitData> traits = enemyTraits.GetTraitDataList();
        Debug.Log($"[EnemyCombatTracker] Found {traits.Count} traits");

        foreach (EnemyTraitData trait in traits)
        {
            Debug.Log($"[EnemyCombatTracker] Processing trait: {(trait != null ? trait.traitName : "NULL")}");

            if (trait == null || trait.icon == null)
            {
                Debug.LogWarning($"[EnemyCombatTracker] Trait or icon is null!");
                continue;
            }

            CreateTraitIcon(trait);
        }

        Debug.Log($"[EnemyCombatTracker] Created {currentTraitIcons.Count} trait icons");
    }

    private void CreateTraitIcon(EnemyTraitData traitData)
    {
        if (iconPrefab == null || iconContainer == null) return; // Use iconContainer, not traitIconContainer

        GameObject iconGO = Instantiate(iconPrefab, iconContainer); // Spawn in same container!
        Image iconImage = iconGO.GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.sprite = traitData.icon;
        }

        // Set size
        RectTransform rect = iconGO.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        // Add tooltip
        SimpleTooltip tooltip = iconGO.AddComponent<SimpleTooltip>();
        tooltip.tooltipTitle = traitData.traitName;
        tooltip.tooltipDescription = traitData.description;

        currentTraitIcons.Add(iconGO);
    }

    public void ClearTraitIcons()
    {
        foreach (GameObject icon in currentTraitIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        currentTraitIcons.Clear();
    }

    // NEW: Methods for managing active effect icons (like PlayerCombatTracker)
    public void AddActiveEffect(PowerUpEffectBase effect)
    {
        if (effect == null || effect.SourceData == null) return;
        if (activeEffectIcons.ContainsKey(effect)) return;

        PowerUpData data = effect.SourceData;

        if (data.statusIcon == null)
        {
            Debug.Log($"[EnemyCombatTracker] {data.powerUpName} has no status icon - not adding to tracker");
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

        // Set up counter text if effect has duration
        TextMeshProUGUI counterText = iconGO.GetComponentInChildren<TextMeshProUGUI>(true);
        if (counterText != null)
        {
            // Check if effect has rounds remaining
            if (effect is IDurationEffect durationEffect)
            {
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

        Debug.Log($"[EnemyCombatTracker] Added active effect icon: {data.powerUpName}");
    }

    public void RemoveActiveEffect(PowerUpEffectBase effect)
    {
        if (activeEffectIcons.ContainsKey(effect))
        {
            Destroy(activeEffectIcons[effect]);
            activeEffectIcons.Remove(effect);
            Debug.Log($"[EnemyCombatTracker] Removed active effect icon");
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