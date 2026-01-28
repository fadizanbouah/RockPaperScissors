using System.Collections;
using UnityEngine;

public class PouchOfCoinsBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Coin Reward Configuration")]
    [Tooltip("Maximum coins that can drop (will randomly give 1 to maxCoins)")]
    [SerializeField] private int maxCoins = 3;

    private HandController enemyHand;
    private bool hasDroppedCoins = false; // Prevent double-drops

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            maxCoins = Mathf.RoundToInt(configValues[0]);
        }

        Debug.Log($"[PouchOfCoinsBehavior] Initialized with max coin drop: {maxCoins}");

        // Subscribe to enemy death to drop coins
        if (enemyHand != null)
        {
            enemyHand.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath(HandController hand)
    {
        if (hasDroppedCoins) return; // Safety check

        // Roll for random coin amount (1 to maxCoins inclusive)
        int coinAmount = Random.Range(1, maxCoins + 1);

        Debug.Log($"[PouchOfCoinsBehavior] Enemy defeated! Dropping {coinAmount} coins");

        // Add coins to player's persistent currency
        if (PlayerProgressData.Instance != null)
        {
            PlayerProgressData.Instance.coins += coinAmount;
            PlayerProgressData.Save();
            Debug.Log($"[PouchOfCoinsBehavior] Player now has {PlayerProgressData.Instance.coins} total coins");
        }

        hasDroppedCoins = true;

        // TODO: Play coin drop animation here later
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

        Debug.Log("[PouchOfCoinsBehavior] Destroyed");
    }
}