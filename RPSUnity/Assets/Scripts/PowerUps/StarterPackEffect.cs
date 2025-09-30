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

        // Destroy this effect after applying (it's a one-time passive)
        Destroy(gameObject);
    }

    private void ApplyDamageBonus()
    {
        if (PlayerProgressData.Instance == null) return;

        // Calculate the damage increase based on percentage
        int baseDamageIncrease = Mathf.RoundToInt(10 * (damagePercentage / 100f)); // Assuming base damage is 10

        PlayerProgressData.Instance.bonusBaseDamage += baseDamageIncrease;

        Debug.Log($"[StarterPackEffect] Applied +{baseDamageIncrease} base damage ({damagePercentage}% increase)");

        if (showNotifications)
        {
            ShowNotification($"+{damagePercentage}% Damage!");
        }
    }

    private void ApplyHealthBonus()
    {
        if (player == null) return;

        // Calculate health increase based on percentage of base max health
        int healthIncrease = Mathf.RoundToInt(player.baseMaxHealth * (healthPercentage / 100f));

        // Increase max health
        player.maxHealth += healthIncrease;

        // Also heal the player by the same amount (so they get the benefit immediately)
        player.health = Mathf.Min(player.health + healthIncrease, player.maxHealth);
        player.UpdateHealthBar();

        Debug.Log($"[StarterPackEffect] Applied +{healthIncrease} max health ({healthPercentage}% increase)");

        if (showNotifications)
        {
            ShowNotification($"+{healthPercentage}% Max HP!");
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

        if (showNotifications)
        {
            ShowNotification($"Received: {randomCard.powerUpName}!");
        }

        // Refresh the power-up card display if in gameplay
        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
        {
            spawner.SpawnActivePowerUps();
        }
    }

    private void ShowNotification(string message)
    {
        // This is a placeholder for a notification system
        // You could implement a floating text or UI notification here
        Debug.Log($"[StarterPackEffect Notification] {message}");

        // Example: Spawn floating text at player position
        if (player != null && player.combatTextPrefab != null)
        {
            GameObject textInstance = Instantiate(player.combatTextPrefab, player.transform.position + Vector3.up, Quaternion.identity);
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