using System.Collections;
using UnityEngine;
using TMPro; // Add this

public class PouchOfFavorBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Favor Reward Configuration")]
    [Tooltip("Maximum favor that can drop (will randomly give 1 to maxFavor)")]
    [SerializeField] private int maxFavor = 3;

    [Header("Visual Effect")]
    [Tooltip("VFX prefab that plays when favor drops")]
    [SerializeField] private GameObject favorDropVFXPrefab;

    private HandController enemyHand;
    private bool hasDroppedFavor = false;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            maxFavor = Mathf.RoundToInt(configValues[0]);
        }

        Debug.Log($"[PouchOfFavorBehavior] Initialized with max favor drop: {maxFavor}");

        if (enemyHand != null)
        {
            enemyHand.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath(HandController hand)
    {
        if (hasDroppedFavor) return;

        int favorAmount = Random.Range(1, maxFavor + 1);

        Debug.Log($"[PouchOfFavorBehavior] Enemy defeated! Dropping {favorAmount} favor");

        if (RunProgressManager.Instance != null)
        {
            RunProgressManager.Instance.AddFavor(favorAmount);
        }

        hasDroppedFavor = true;

        // Play favor drop animation with the amount
        if (favorDropVFXPrefab != null)
        {
            StartCoroutine(PlayFavorDropAnimationAndNotify(favorAmount, hand));
        }
        else
        {
            // No animation, notify immediately
            hand.TriggerTraitAnimationsFinished();
        }
    }

    private IEnumerator PlayFavorDropAnimation(int favorAmount)
    {
        if (favorDropVFXPrefab != null && enemyHand != null)
        {
            Debug.Log("[PouchOfFavorBehavior] Spawning Favor Drop VFX prefab");

            // Spawn the effect at enemy's position
            GameObject vfx = Instantiate(favorDropVFXPrefab, enemyHand.transform.position, Quaternion.identity);

            // Find the TextMeshPro component in the VFX
            TextMeshProUGUI textComponent = vfx.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent == null)
            {
                // Try finding TextMeshPro (3D version)
                TextMeshPro textComponent3D = vfx.GetComponentInChildren<TextMeshPro>();
                if (textComponent3D != null)
                {
                    textComponent3D.text = $"+{favorAmount}";
                    Debug.Log($"[PouchOfFavorBehavior] Set favor text to: +{favorAmount}");
                }
            }
            else
            {
                textComponent.text = $"+{favorAmount}";
                Debug.Log($"[PouchOfFavorBehavior] Set favor text to: +{favorAmount}");
            }

            // Get the animator from the VFX prefab
            Animator vfxAnimator = vfx.GetComponent<Animator>();

            if (vfxAnimator != null)
            {
                yield return null; // Wait one frame for animator to initialize

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
                    Debug.LogWarning("[PouchOfFavorBehavior] VFX animation timed out!");
                }
            }
            else
            {
                Debug.LogWarning("[PouchOfFavorBehavior] VFX prefab has no Animator - using fallback delay");
                yield return new WaitForSeconds(1.5f);
            }

            // Clean up the VFX
            Destroy(vfx);
        }
    }

    private IEnumerator PlayFavorDropAnimationAndNotify(int favorAmount, HandController hand)
    {
        yield return StartCoroutine(PlayFavorDropAnimation(favorAmount));

        // Notify that this trait's animation is complete
        if (hand != null)
        {
            hand.TriggerTraitAnimationsFinished();
        }
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        yield return null;
    }

    private void OnDestroy()
    {
        if (enemyHand != null)
        {
            enemyHand.OnDeath -= OnEnemyDeath;
        }

        Debug.Log("[PouchOfFavorBehavior] Destroyed");
    }
}