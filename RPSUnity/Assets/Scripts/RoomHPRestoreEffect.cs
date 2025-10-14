using UnityEngine;

public class RoomHPRestoreEffect : PowerUpEffectBase
{
    [Header("Configuration")]
    [SerializeField] private float restorePercentage = 30f; // Default fallback

    private bool hasHealedThisRoom = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[RoomHPRestoreEffect] Initialized");
    }

    public override void OnRoomStart()
    {
        // Only heal once per room
        if (hasHealedThisRoom)
        {
            Debug.Log("[RoomHPRestoreEffect] Already healed this room, skipping");
            return;
        }

        // Get the current restore percentage based on upgrade level
        float currentRestorePercentage = restorePercentage; // Default

        if (sourceData != null && sourceData.isUpgradeable && RunProgressManager.Instance != null)
        {
            int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
            currentRestorePercentage = sourceData.GetValueForLevel(currentLevel);
            Debug.Log($"[RoomHPRestoreEffect] Using level {currentLevel} value: {currentRestorePercentage}%");
        }
        else if (sourceData != null && sourceData.value > 0)
        {
            currentRestorePercentage = sourceData.value;
        }

        // Get player reference from PowerUpEffectManager if we don't have it
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[RoomHPRestoreEffect] Cannot restore HP - player is null!");
            return;
        }

        // Calculate heal amount
        int healAmount = Mathf.RoundToInt(activePlayer.maxHealth * (currentRestorePercentage / 100f));

        // Calculate actual healing (can't exceed max health)
        int previousHealth = activePlayer.CurrentHealth;
        int newHealth = Mathf.Min(activePlayer.CurrentHealth + healAmount, activePlayer.maxHealth);
        int actualHealing = newHealth - previousHealth;

        // Apply the healing
        activePlayer.health = newHealth;
        activePlayer.UpdateHealthBar();

        hasHealedThisRoom = true; // Mark as healed

        Debug.Log($"[RoomHPRestoreEffect] Healed player for {actualHealing} HP (from {previousHealth} to {newHealth}) - {currentRestorePercentage}% of max HP");

        // Optional: Show floating text
        if (actualHealing > 0 && activePlayer.combatTextPrefab != null)
        {
            GameObject instance = Instantiate(activePlayer.combatTextPrefab, activePlayer.transform.position, Quaternion.identity);
            var textComponent = instance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "+" + actualHealing.ToString();
                textComponent.color = Color.green; // Green for healing
            }
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        // Reset the flag at round end so it can heal again next room
        hasHealedThisRoom = false;
    }
}