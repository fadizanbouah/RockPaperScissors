using System.Collections;
using UnityEngine;

public class PouchOfFavorBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Favor Reward Configuration")]
    [Tooltip("Maximum favor that can drop (will randomly give 1 to maxFavor)")]
    [SerializeField] private int maxFavor = 3;

    private HandController enemyHand;
    private bool hasDroppedFavor = false; // Prevent double-drops

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            maxFavor = Mathf.RoundToInt(configValues[0]);
        }

        Debug.Log($"[PouchOfFavorBehavior] Initialized with max favor drop: {maxFavor}");

        // Subscribe to enemy death to drop favor
        if (enemyHand != null)
        {
            enemyHand.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath(HandController hand)
    {
        if (hasDroppedFavor) return; // Safety check

        // Roll for random favor amount (1 to maxFavor inclusive)
        int favorAmount = Random.Range(1, maxFavor + 1);

        Debug.Log($"[PouchOfFavorBehavior] Enemy defeated! Dropping {favorAmount} favor");

        // Add favor to player
        if (RunProgressManager.Instance != null)
        {
            RunProgressManager.Instance.AddFavor(favorAmount);
        }

        hasDroppedFavor = true;

        // TODO: Play favor drop animation here later
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // No behavior needed before rounds
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        // No behavior needed after damage
        yield return null;
    }

    private void OnDestroy()
    {
        // Clean up subscription
        if (enemyHand != null)
        {
            enemyHand.OnDeath -= OnEnemyDeath;
        }

        Debug.Log("[PouchOfFavorBehavior] Destroyed");
    }
}