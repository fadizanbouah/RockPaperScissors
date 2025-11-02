using UnityEngine;

public class ScissorsCritChanceStackEffect : PowerUpEffectBase
{
    [Header("Crit Chance Stack Configuration")]
    [SerializeField] private float critPerStack = 1f; // 1% crit chance per stack
    [SerializeField] private float maxCrit = 15f; // 15% maximum crit chance cap

    private static float currentCrit = 0f; // Current accumulated crit%
    private static int currentStacks = 0; // Number of stacks accumulated
    private static ScissorsCritChanceStackEffect instance; // Singleton instance

    // NEW: Store the base crit so we can add to it properly
    private static float storedBaseCrit = 0f;
    private static bool hasStoredBase = false;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // CRITICAL: Only allow ONE instance to exist
        if (instance != null && instance != this)
        {
            Debug.Log($"[ScissorsCritChanceStackEffect] Another instance already exists! Destroying this duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        Debug.Log($"[ScissorsCritChanceStackEffect] Initialized as singleton instance");

        // Use values from ScriptableObject if provided
        if (sourceData != null)
        {
            if (sourceData.value > 0)
            {
                critPerStack = sourceData.value;
            }

            // Check if upgradeable for different tiers
            if (sourceData.isUpgradeable && RunProgressManager.Instance != null)
            {
                int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
                critPerStack = sourceData.GetValueForLevel(currentLevel);
                Debug.Log($"[ScissorsCritChanceStackEffect] Using level {currentLevel} value: {critPerStack}% per stack");
            }
        }

        Debug.Log($"[ScissorsCritChanceStackEffect] Configuration: {critPerStack}% per stack, {maxCrit}% max cap");
    }

    public override void OnRoomStart()
    {
        Debug.Log($"[ScissorsCritChanceStackEffect] Room started. Current Crit: {currentCrit}% ({currentStacks} stacks)");

        // Verify we're still the singleton instance
        if (instance != this)
        {
            Debug.LogWarning($"[ScissorsCritChanceStackEffect] This is not the singleton instance! Destroying self.");
            Destroy(gameObject);
            return;
        }

        // NEW: Store base crit on first application
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer != null && !hasStoredBase)
        {
            storedBaseCrit = activePlayer.critChance;
            hasStoredBase = true;
            Debug.Log($"[ScissorsCritChanceStackEffect] Stored base crit: {storedBaseCrit}%");
        }

        // Apply current stacks if any
        if (currentStacks > 0)
        {
            ApplyCritToPlayer();
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        // Extra safety check
        if (instance != this)
        {
            Debug.LogWarning($"[ScissorsCritChanceStackEffect] Duplicate instance detected in OnRoundEnd, ignoring");
            return;
        }

        Debug.Log($"[ScissorsCritChanceStackEffect] OnRoundEnd called - Result: {result}, PlayerChoice: {playerChoice}, EnemyChoice: {enemyChoice}");

        // Only trigger on draw with Scissors
        if (result != RoundResult.Draw)
        {
            Debug.Log($"[ScissorsCritChanceStackEffect] Not a draw (result was {result}), skipping");
            return;
        }

        if (playerChoice != "Scissors")
        {
            Debug.Log($"[ScissorsCritChanceStackEffect] Player didn't use Scissors (used {playerChoice}), skipping");
            return;
        }

        // Check if we're at cap
        if (currentCrit >= maxCrit)
        {
            Debug.Log($"[ScissorsCritChanceStackEffect] Already at max crit ({maxCrit}%), cannot stack more");
            return;
        }

        // Add a stack
        currentStacks++;
        float previousCrit = currentCrit;
        currentCrit = Mathf.Min(currentCrit + critPerStack, maxCrit); // Cap at maxCrit

        // Apply the crit increase to the player
        ApplyCritToPlayer();

        Debug.Log($"[ScissorsCritChanceStackEffect] Scissors draw detected! Added stack. Crit: {previousCrit}% -> {currentCrit}% (Stack #{currentStacks})");

        // If we just hit the cap, log it
        if (currentCrit >= maxCrit && previousCrit < maxCrit)
        {
            Debug.Log($"[ScissorsCritChanceStackEffect] Reached maximum crit cap of {maxCrit}%!");
        }
    }

    private void Update()
    {
        // Continuously apply the crit bonus to maintain it
        if (currentStacks > 0 && player != null)
        {
            ApplyCritToPlayer();
        }
    }

    private void ApplyCritToPlayer()
    {
        // Get player reference
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[ScissorsCritChanceStackEffect] Cannot apply crit - player is null!");
            return;
        }

        // NEW: Always set to base + stacks, capped at 100%
        float totalCrit = Mathf.Min(storedBaseCrit + currentCrit, 100f);
        activePlayer.critChance = totalCrit;

        Debug.Log($"[ScissorsCritChanceStackEffect] Applied crit: base({storedBaseCrit}%) + stacks({currentCrit}%) = {totalCrit}%");
    }

    public override void Cleanup()
    {
        // Clear singleton reference if this was the active instance
        if (instance == this)
        {
            instance = null;
            Debug.Log($"[ScissorsCritChanceStackEffect] Cleared singleton instance. Keeping {currentStacks} stacks ({currentCrit}% crit)");
        }
    }

    public static void ResetForNewRun()
    {
        currentCrit = 0f;
        currentStacks = 0;
        instance = null;
        storedBaseCrit = 0f;
        hasStoredBase = false;
        Debug.Log("[ScissorsCritChanceStackEffect] Reset for new run - cleared all stacks and singleton");
    }

    public static float GetCurrentCrit()
    {
        return currentCrit;
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