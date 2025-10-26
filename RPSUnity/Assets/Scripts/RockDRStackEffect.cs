using UnityEngine;

public class RockDRStackEffect : PowerUpEffectBase
{
    [Header("Damage Reduction Configuration")]
    [SerializeField] private float drPerStack = 1f; // 1% DR per stack
    [SerializeField] private float maxDR = 15f; // 15% maximum DR cap

    private static float currentDR = 0f; // Current accumulated DR%
    private static int currentStacks = 0; // Number of stacks accumulated
    private static RockDRStackEffect instance; // Singleton instance

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // CRITICAL: Only allow ONE instance to exist
        if (instance != null && instance != this)
        {
            Debug.Log($"[RockDRStackEffect] Another instance already exists! Destroying this duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        Debug.Log($"[RockDRStackEffect] Initialized as singleton instance");

        // Use values from ScriptableObject if provided
        if (sourceData != null)
        {
            if (sourceData.value > 0)
            {
                drPerStack = sourceData.value;
            }

            // Check if upgradeable for different tiers
            if (sourceData.isUpgradeable && RunProgressManager.Instance != null)
            {
                int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
                drPerStack = sourceData.GetValueForLevel(currentLevel);
                Debug.Log($"[RockDRStackEffect] Using level {currentLevel} value: {drPerStack}% per stack");
            }
        }

        Debug.Log($"[RockDRStackEffect] Configuration: {drPerStack}% per stack, {maxDR}% max cap");
    }

    public override void OnRoomStart()
    {
        Debug.Log($"[RockDRStackEffect] Room started. Current DR: {currentDR}% ({currentStacks} stacks)");

        // Verify we're still the singleton instance
        if (instance != this)
        {
            Debug.LogWarning($"[RockDRStackEffect] This is not the singleton instance! Destroying self.");
            Destroy(gameObject);
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        // Extra safety check
        if (instance != this)
        {
            Debug.LogWarning($"[RockDRStackEffect] Duplicate instance detected in OnRoundEnd, ignoring");
            return;
        }

        Debug.Log($"[RockDRStackEffect] OnRoundEnd called - Result: {result}, PlayerChoice: {playerChoice}, EnemyChoice: {enemyChoice}");

        // Only trigger on draw with Rock
        if (result != RoundResult.Draw)
        {
            Debug.Log($"[RockDRStackEffect] Not a draw (result was {result}), skipping");
            return;
        }

        if (playerChoice != "Rock")
        {
            Debug.Log($"[RockDRStackEffect] Player didn't use Rock (used {playerChoice}), skipping");
            return;
        }

        // Check if we're at cap
        if (currentDR >= maxDR)
        {
            Debug.Log($"[RockDRStackEffect] Already at max DR ({maxDR}%), cannot stack more");
            return;
        }

        // Add a stack
        currentStacks++;
        float previousDR = currentDR;
        currentDR = Mathf.Min(currentDR + drPerStack, maxDR); // Cap at maxDR

        Debug.Log($"[RockDRStackEffect] Rock draw detected! Added stack. DR: {previousDR}% -> {currentDR}% (Stack #{currentStacks})");

        // If we just hit the cap, log it
        if (currentDR >= maxDR && previousDR < maxDR)
        {
            Debug.Log($"[RockDRStackEffect] Reached maximum DR cap of {maxDR}%!");
        }
    }

    public override void ModifyIncomingDamage(ref int damage, HandController source)
    {
        // Extra safety check
        if (instance != this)
        {
            Debug.LogWarning($"[RockDRStackEffect] Duplicate instance detected in ModifyIncomingDamage, ignoring");
            return;
        }

        Debug.Log($"[RockDRStackEffect] ModifyIncomingDamage called - Original damage: {damage}, Source: {source?.name ?? "null"}, CurrentDR: {currentDR}%");

        // Only reduce damage from enemies
        if (source == player)
        {
            Debug.Log($"[RockDRStackEffect] Source is player, skipping");
            return;
        }

        if (currentDR <= 0)
        {
            Debug.Log($"[RockDRStackEffect] No DR accumulated yet, skipping");
            return;
        }

        float reduction = currentDR / 100f;
        int originalDamage = damage;
        damage = Mathf.RoundToInt(damage * (1f - reduction));

        Debug.Log($"[RockDRStackEffect] Reduced incoming damage from {originalDamage} to {damage} ({currentDR}% reduction)");
    }

    public override void Cleanup()
    {
        // Clear singleton reference if this was the active instance
        if (instance == this)
        {
            instance = null;
            Debug.Log($"[RockDRStackEffect] Cleared singleton instance. Keeping {currentStacks} stacks ({currentDR}% DR)");
        }
    }

    public static void ResetForNewRun()
    {
        currentDR = 0f;
        currentStacks = 0;
        instance = null; // Clear singleton reference
        Debug.Log("[RockDRStackEffect] Reset for new run - cleared all stacks and singleton");
    }

    public static float GetCurrentDR()
    {
        return currentDR;
    }

    public static int GetCurrentStacks()
    {
        return currentStacks;
    }

    public override bool IsEffectActive()
    {
        return true; // Always active once acquired
    }
}