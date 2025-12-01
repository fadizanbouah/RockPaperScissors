// RPSUnity/Assets/Scripts/Enemies/Harden/HardenBehavior.cs
using System.Collections;
using UnityEngine;

public class HardenBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Harden Configuration")]
    [Tooltip("HP threshold % to activate (e.g., 50 = activates at 50% HP or below)")]
    [SerializeField] private float hpThresholdPercent = 50f;

    [Tooltip("% of damage reduction when active (e.g., 30 = 30% damage reduction)")]
    [SerializeField] private float damageReductionPercent = 30f;

    private HandController enemyHand;
    private bool isHardened = false;
    private float hpThreshold; // Calculated actual HP value
    private HardenEffect activeEffect; // Reference to the power-up effect we create

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        // configValues[0] = hpThresholdPercent
        // configValues[1] = damageReductionPercent
        if (configValues != null && configValues.Length > 0)
        {
            if (configValues.Length > 0)
                hpThresholdPercent = configValues[0];

            if (configValues.Length > 1)
                damageReductionPercent = configValues[1];
        }

        // Calculate the actual HP threshold
        hpThreshold = enemyHand.maxHealth * (hpThresholdPercent / 100f);

        Debug.Log($"[HardenBehavior] Initialized - Activates at {hpThresholdPercent}% HP ({hpThreshold} HP), {damageReductionPercent}% damage reduction");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // Check if we should activate Harden based on current HP
        CheckHardenActivation();
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        // Check again after damage in case it just crossed the threshold
        CheckHardenActivation();
        yield return null;
    }

    private void CheckHardenActivation()
    {
        if (enemyHand == null || enemyHand.CurrentHealth <= 0) return;

        // Check if we should activate Harden
        bool shouldBeHardened = enemyHand.CurrentHealth <= hpThreshold;

        // If state changed, update it
        if (shouldBeHardened && !isHardened)
        {
            ActivateHarden();
        }
    }

    private void ActivateHarden()
    {
        isHardened = true;
        Debug.Log($"[HardenBehavior] HARDEN ACTIVATED! Enemy now has {damageReductionPercent}% damage reduction");

        // Create a HardenEffect that will handle the damage reduction
        GameObject effectObj = new GameObject("HardenEffect");
        activeEffect = effectObj.AddComponent<HardenEffect>();
        activeEffect.Initialize(damageReductionPercent, enemyHand);

        // Register with PowerUpEffectManager so it gets called during damage modification
        if (PowerUpEffectManager.Instance != null)
        {
            PowerUpEffectManager.Instance.RegisterEffect(activeEffect);
            Debug.Log($"[HardenBehavior] Registered HardenEffect with PowerUpEffectManager");
        }
    }

    private void OnDestroy()
    {
        // Clean up the effect if it exists
        if (activeEffect != null)
        {
            if (PowerUpEffectManager.Instance != null)
            {
                PowerUpEffectManager.Instance.RemoveEffect(activeEffect);
            }
            Destroy(activeEffect.gameObject);
        }

        Debug.Log("[HardenBehavior] Destroyed");
    }

    // Getters for UI/debugging
    public bool IsHardened()
    {
        return isHardened;
    }

    public float GetCurrentThreshold()
    {
        return hpThreshold;
    }

    public float GetThresholdPercent()
    {
        return hpThresholdPercent;
    }
}