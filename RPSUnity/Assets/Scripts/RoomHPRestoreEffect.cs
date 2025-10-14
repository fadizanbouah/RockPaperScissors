using UnityEngine;

public class RoomHPRestoreEffect : PowerUpEffectBase
{
    [Header("Configuration")]
    [SerializeField] private float restorePercentage = 30f;

    private static string lastHealedRoomName = "";

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        Debug.Log($"[RoomHPRestoreEffect] Initialized");
    }

    public override void OnRoomStart()
    {
        if (RoomManager.Instance == null || RoomManager.Instance.GetCurrentRoom() == null)
        {
            Debug.LogWarning("[RoomHPRestoreEffect] RoomManager or current room is null!");
            return;
        }

        string currentRoomName = RoomManager.Instance.GetCurrentRoom().roomName;

        // Only heal once per unique room
        if (lastHealedRoomName == currentRoomName)
        {
            Debug.Log($"[RoomHPRestoreEffect] Already healed in room '{currentRoomName}', skipping");
            return;
        }

        // Get the current restore percentage based on upgrade level
        float currentRestorePercentage = restorePercentage;

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

        // Get player reference
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
        int previousHealth = activePlayer.CurrentHealth;
        int newHealth = Mathf.Min(activePlayer.CurrentHealth + healAmount, activePlayer.maxHealth);
        int actualHealing = newHealth - previousHealth;

        // Apply the healing
        activePlayer.health = newHealth;
        activePlayer.UpdateHealthBar();

        lastHealedRoomName = currentRoomName;

        Debug.Log($"[RoomHPRestoreEffect] Healed player for {actualHealing} HP (from {previousHealth} to {newHealth}) in room '{currentRoomName}' - {currentRestorePercentage}% of max HP");

        // Show floating text
        if (actualHealing > 0 && activePlayer.combatTextPrefab != null)
        {
            GameObject instance = Instantiate(activePlayer.combatTextPrefab, activePlayer.transform.position, Quaternion.identity);
            var textComponent = instance.GetComponentInChildren<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "+" + actualHealing.ToString();
                textComponent.color = Color.green;
            }
        }
    }
}