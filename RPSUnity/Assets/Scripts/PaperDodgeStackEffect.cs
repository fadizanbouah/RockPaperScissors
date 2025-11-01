using UnityEngine;

public class PaperDodgeStackEffect : PowerUpEffectBase
{
    [Header("Dodge Stack Configuration")]
    [SerializeField] private float dodgePerStack = 1f; // 1% dodge per stack
    [SerializeField] private float maxDodge = 15f; // 15% maximum dodge cap

    private static float currentDodge = 0f; // Current accumulated dodge%
    private static int currentStacks = 0; // Number of stacks accumulated
    private static PaperDodgeStackEffect instance; // Singleton instance

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);

        // CRITICAL: Only allow ONE instance to exist
        if (instance != null && instance != this)
        {
            Debug.Log($"[PaperDodgeStackEffect] Another instance already exists! Destroying this duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        Debug.Log($"[PaperDodgeStackEffect] Initialized as singleton instance");

        // Use values from ScriptableObject if provided
        if (sourceData != null)
        {
            if (sourceData.value > 0)
            {
                dodgePerStack = sourceData.value;
            }

            // Check if upgradeable for different tiers
            if (sourceData.isUpgradeable && RunProgressManager.Instance != null)
            {
                int currentLevel = RunProgressManager.Instance.GetPowerUpLevel(sourceData);
                dodgePerStack = sourceData.GetValueForLevel(currentLevel);
                Debug.Log($"[PaperDodgeStackEffect] Using level {currentLevel} value: {dodgePerStack}% per stack");
            }
        }

        Debug.Log($"[PaperDodgeStackEffect] Configuration: {dodgePerStack}% per stack, {maxDodge}% max cap");
    }

    public override void OnRoomStart()
    {
        Debug.Log($"[PaperDodgeStackEffect] Room started. Current Dodge: {currentDodge}% ({currentStacks} stacks)");

        // Verify we're still the singleton instance
        if (instance != this)
        {
            Debug.LogWarning($"[PaperDodgeStackEffect] This is not the singleton instance! Destroying self.");
            Destroy(gameObject);
            return;
        }

        // Apply current stacks if any
        if (currentStacks > 0)
        {
            ApplyDodgeToPlayer();
        }
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        // Extra safety check
        if (instance != this)
        {
            Debug.LogWarning($"[PaperDodgeStackEffect] Duplicate instance detected in OnRoundEnd, ignoring");
            return;
        }

        Debug.Log($"[PaperDodgeStackEffect] OnRoundEnd called - Result: {result}, PlayerChoice: {playerChoice}, EnemyChoice: {enemyChoice}");

        // Only trigger on draw with Paper
        if (result != RoundResult.Draw)
        {
            Debug.Log($"[PaperDodgeStackEffect] Not a draw (result was {result}), skipping");
            return;
        }

        if (playerChoice != "Paper")
        {
            Debug.Log($"[PaperDodgeStackEffect] Player didn't use Paper (used {playerChoice}), skipping");
            return;
        }

        // Check if we're at cap
        if (currentDodge >= maxDodge)
        {
            Debug.Log($"[PaperDodgeStackEffect] Already at max dodge ({maxDodge}%), cannot stack more");
            return;
        }

        // Add a stack
        currentStacks++;
        float previousDodge = currentDodge;
        currentDodge = Mathf.Min(currentDodge + dodgePerStack, maxDodge); // Cap at maxDodge

        // Apply the dodge increase to the player
        ApplyDodgeToPlayer();

        Debug.Log($"[PaperDodgeStackEffect] Paper draw detected! Added stack. Dodge: {previousDodge}% -> {currentDodge}% (Stack #{currentStacks})");

        // If we just hit the cap, log it
        if (currentDodge >= maxDodge && previousDodge < maxDodge)
        {
            Debug.Log($"[PaperDodgeStackEffect] Reached maximum dodge cap of {maxDodge}%!");
        }
    }

    private void Update()
    {
        // Continuously ensure the player has at least our stacked dodge amount
        // This allows DodgeBoostEffect and other temporary bonuses to add on top
        if (currentStacks > 0 && player != null)
        {
            ApplyDodgeToPlayer();
        }
    }

    private void ApplyDodgeToPlayer()
    {
        // Get player reference
        HandController activePlayer = player;
        if (activePlayer == null && PowerUpEffectManager.Instance != null)
        {
            activePlayer = PowerUpEffectManager.Instance.GetPlayer();
        }

        if (activePlayer == null)
        {
            Debug.LogWarning("[PaperDodgeStackEffect] Cannot apply dodge - player is null!");
            return;
        }

        // Simply ensure the dodge is AT LEAST our stacked amount
        // This way DodgeBoostEffect and future upgrades can add on top
        if (activePlayer.dodgeChance < currentDodge)
        {
            activePlayer.dodgeChance = currentDodge;
            Debug.Log($"[PaperDodgeStackEffect] Set minimum dodge to {currentDodge}%");
        }
    }

    public override void Cleanup()
    {
        // Clear singleton reference if this was the active instance
        if (instance == this)
        {
            instance = null;
            Debug.Log($"[PaperDodgeStackEffect] Cleared singleton instance. Keeping {currentStacks} stacks ({currentDodge}% dodge)");
        }
    }

    public static void ResetForNewRun()
    {
        currentDodge = 0f;
        currentStacks = 0;
        instance = null;
        Debug.Log("[PaperDodgeStackEffect] Reset for new run - cleared all stacks and singleton");
    }

    public static float GetCurrentDodge()
    {
        return currentDodge;
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