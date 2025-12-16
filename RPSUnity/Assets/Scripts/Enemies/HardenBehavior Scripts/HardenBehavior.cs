using System.Collections;
using UnityEngine;

public class HardenBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Harden Configuration")]
    [Tooltip("HP threshold % to activate (e.g., 50 = activates at 50% HP or below)")]
    [SerializeField] private float hpThresholdPercent = 50f;

    [Tooltip("% of damage reduction when active (e.g., 30 = 30% damage reduction)")]
    [SerializeField] private float damageReductionPercent = 30f;

    [Header("Visual Effect")]
    [Tooltip("VFX prefab that plays when Harden activates")]
    [SerializeField] private GameObject hardenVFXPrefab;

    private HandController enemyHand;
    private bool isHardened = false;
    private float hpThreshold;
    private HardenEffect activeEffect;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            if (configValues.Length > 0)
                hpThresholdPercent = configValues[0];

            if (configValues.Length > 1)
                damageReductionPercent = configValues[1];
        }

        hpThreshold = enemyHand.maxHealth * (hpThresholdPercent / 100f);

        Debug.Log($"[HardenBehavior] Initialized - Activates at {hpThresholdPercent}% HP ({hpThreshold} HP), {damageReductionPercent}% damage reduction");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return StartCoroutine(CheckHardenActivation());
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        yield return StartCoroutine(CheckHardenActivation());
    }

    private IEnumerator CheckHardenActivation()
    {
        if (enemyHand == null || enemyHand.CurrentHealth <= 0)
        {
            yield break;
        }

        bool shouldBeHardened = enemyHand.CurrentHealth <= hpThreshold;

        if (shouldBeHardened && !isHardened)
        {
            yield return StartCoroutine(ActivateHarden());
        }
    }

    private IEnumerator ActivateHarden()
    {
        isHardened = true;
        Debug.Log($"[HardenBehavior] HARDEN ACTIVATED! Enemy now has {damageReductionPercent}% damage reduction");

        // Play the harden visual effect
        yield return StartCoroutine(PlayHardenVFX());

        // Create damage reduction effect
        CreateDamageReductionEffect();
    }

    private IEnumerator PlayHardenVFX()
    {
        if (hardenVFXPrefab != null && enemyHand != null)
        {
            Debug.Log("[HardenBehavior] Spawning Harden VFX prefab");

            // Spawn the effect at enemy's position
            GameObject vfx = Instantiate(hardenVFXPrefab, enemyHand.transform.position, Quaternion.identity);

            // Parent it to the enemy so it moves with them
            vfx.transform.SetParent(enemyHand.transform);

            // Get the animator from the VFX prefab
            Animator vfxAnimator = vfx.GetComponent<Animator>();

            if (vfxAnimator != null)
            {
                // Wait one frame for animator to initialize
                yield return null;

                // Wait for the animation to finish
                AnimatorStateInfo stateInfo = vfxAnimator.GetCurrentAnimatorStateInfo(0);
                float timeout = 3f;
                float elapsed = 0f;

                while (stateInfo.normalizedTime < 1.0f && elapsed < timeout)
                {
                    yield return null;
                    stateInfo = vfxAnimator.GetCurrentAnimatorStateInfo(0);
                    elapsed += Time.deltaTime;
                }

                if (elapsed >= timeout)
                {
                    Debug.LogWarning("[HardenBehavior] VFX animation timed out!");
                }
            }
            else
            {
                // No animator found, just wait a bit
                Debug.LogWarning("[HardenBehavior] VFX prefab has no Animator - using fallback delay");
                yield return new WaitForSeconds(1f);
            }

            // Clean up the VFX
            Destroy(vfx);
        }
        else
        {
            // No VFX prefab assigned - just use a simple delay
            Debug.Log("[HardenBehavior] No VFX prefab assigned - using delay");
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CreateDamageReductionEffect()
    {
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

    public bool IsHardened() => isHardened;
    public float GetCurrentThreshold() => hpThreshold;
    public float GetThresholdPercent() => hpThresholdPercent;
}