using UnityEngine;
using System.Collections.Generic;

public class StarterPackEffect : PowerUpEffectBase
{
    [Header("Starter Pack Configuration")]
    [SerializeField] private bool grantDamageBonus = true;
    [SerializeField] private float damagePercentage = 10f; // 10% damage increase

    [SerializeField] private bool grantHealthBonus = true;
    [SerializeField] private float healthPercentage = 20f; // 20% max health increase

    [SerializeField] private bool grantRandomCard = true;
    [SerializeField] private PowerUpData[] possibleCards; // Pool of cards to choose from

    [Header("Visual Feedback")]
    [SerializeField] private bool showNotifications = true;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[StarterPackEffect] Initialized Starter Pack for {player?.name ?? "null"}");
    }

    public override void OnRoomStart()
    {
        Debug.Log("[StarterPackEffect] Applying Starter Pack bonuses...");
        Debug.Log($"[StarterPackEffect] Player reference: {player?.name ?? "NULL"}");

        // Apply damage bonus if configured
        if (grantDamageBonus)
        {
            ApplyDamageBonus();
        }

        // Apply health bonus if configured
        if (grantHealthBonus)
        {
            ApplyHealthBonus();
        }

        // Grant random card if configured
        if (grantRandomCard && possibleCards != null && possibleCards.Length > 0)
        {
            GrantRandomCard();
        }
    }

    private void ApplyDamageBonus()
    {
        if (PlayerProgressData.Instance == null)
        {
            Debug.LogWarning("[StarterPackEffect] PlayerProgressData is null!");
            return;
        }

        // CRITICAL FIX: Get player reference from PowerUpEffectManager if we don't have it
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
            Debug.Log($"[StarterPackEffect] Retrieved player from PowerUpEffectManager: {activePlayer?.name ?? "NULL"}");
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackEffect] Cannot apply damage bonus - player is null!");
            return;
        }

        // Calculate damage increase for each sign based on their actual values
        int rockDamageIncrease = Mathf.RoundToInt(activePlayer.rockDamage * (damagePercentage / 100f));
        int paperDamageIncrease = Mathf.RoundToInt(activePlayer.paperDamage * (damagePercentage / 100f));
        int scissorsDamageIncrease = Mathf.RoundToInt(activePlayer.scissorsDamage * (damagePercentage / 100f));

        // Apply the increases to the appropriate bonus fields
        PlayerProgressData.Instance.bonusRockDamage += rockDamageIncrease;
        PlayerProgressData.Instance.bonusPaperDamage += paperDamageIncrease;
        PlayerProgressData.Instance.bonusScissorsDamage += scissorsDamageIncrease;

        Debug.Log($"[StarterPackEffect] Applied damage bonuses ({damagePercentage}% increase):");
        Debug.Log($"  Rock: +{rockDamageIncrease} (base was {activePlayer.rockDamage})");
        Debug.Log($"  Paper: +{paperDamageIncrease} (base was {activePlayer.paperDamage})");
        Debug.Log($"  Scissors: +{scissorsDamageIncrease} (base was {activePlayer.scissorsDamage})");

        if (showNotifications)
        {
            ShowNotification($"+{damagePercentage}% All Damage!", activePlayer);
        }
    }

    private void ApplyHealthBonus()
    {
        // CRITICAL FIX: Get player reference from PowerUpEffectManager if we don't have it
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
            Debug.Log($"[StarterPackEffect] Retrieved player from PowerUpEffectManager for health: {activePlayer?.name ?? "NULL"}");
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[StarterPackEffect] Cannot apply health bonus - player is null!");
            return;
        }

        // Calculate health increase based on percentage of base max health
        int healthIncrease = Mathf.RoundToInt(activePlayer.baseMaxHealth * (healthPercentage / 100f));

        // Increase max health
        activePlayer.maxHealth += healthIncrease;

        // Also heal the player by the same amount (so they get the benefit immediately)
        activePlayer.health = Mathf.Min(activePlayer.health + healthIncrease, activePlayer.maxHealth);
        activePlayer.UpdateHealthBar();

        Debug.Log($"[StarterPackEffect] Applied +{healthIncrease} max health ({healthPercentage}% of {activePlayer.baseMaxHealth})");

        if (showNotifications)
        {
            ShowNotification($"+{healthPercentage}% Max HP!", activePlayer);
        }
    }

    private void GrantRandomCard()
    {
        if (RunProgressManager.Instance == null) return;

        // Filter out cards the player already has if they're unique
        List<PowerUpData> availableCards = new List<PowerUpData>();
        foreach (var card in possibleCards)
        {
            if (card != null && (!card.isUnique || !RunProgressManager.Instance.HasPowerUp(card)))
            {
                availableCards.Add(card);
            }
        }

        if (availableCards.Count == 0)
        {
            Debug.LogWarning("[StarterPackEffect] No available cards to grant!");
            return;
        }

        // Pick a random card
        PowerUpData randomCard = availableCards[Random.Range(0, availableCards.Count)];

        // Add it to the player's acquired power-ups
        RunProgressManager.Instance.AddAcquiredPowerUp(randomCard);

        Debug.Log($"[StarterPackEffect] Granted random card: {randomCard.powerUpName}");

        // Get player for notification
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (showNotifications)
        {
            ShowNotification($"Received: {randomCard.powerUpName}!", activePlayer);
        }

        // Refresh the power-up card display if in gameplay
        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
        {
            spawner.SpawnActivePowerUps();
        }
    }

    private void ShowNotification(string message, HandController targetPlayer = null)
    {
        Debug.Log($"[StarterPackEffect Notification] {message}");

        // Use provided player or try to get it
        if (targetPlayer == null)
        {
            targetPlayer = player;
        }
        if (targetPlayer == null && PowerUpEffectManager.Instance != null)
        {
            targetPlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        // Spawn floating text at player position
        if (targetPlayer != null && targetPlayer.combatTextPrefab != null)
        {
            GameObject textInstance = Instantiate(targetPlayer.combatTextPrefab, targetPlayer.transform.position + Vector3.up, Quaternion.identity);
            var textComponent = textInstance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = Color.yellow;
                textComponent.fontSize = 24;
            }
        }
    }
}