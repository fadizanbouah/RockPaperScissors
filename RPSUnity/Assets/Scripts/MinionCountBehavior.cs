using System.Collections;
using UnityEngine;

/// <summary>
/// Basic minion behavior that configures how many minions appear with the enemy.
/// This is a foundation behavior - specific minion mechanics should be in separate behaviors.
/// </summary>
public class MinionCountBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Minion Configuration")]
    [Tooltip("Number of minions to spawn (1-3)")]
    [SerializeField] private int minionCount = 1;

    private MinionController minionController;
    private HandController enemyHand;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;
        minionController = enemy.GetComponent<MinionController>();

        if (minionController == null)
        {
            Debug.LogError($"[MinionCountBehavior] No MinionController found on {enemy.name}! This behavior requires MinionController component.");
            return;
        }

        // Get minion count from config (configValues[0])
        if (configValues != null && configValues.Length > 0)
        {
            minionCount = Mathf.Clamp(Mathf.RoundToInt(configValues[0]), 1, 3);
        }

        Debug.Log($"[MinionCountBehavior] Initialized with {minionCount} minions");

        // Show the configured number of minions
        minionController.ShowMinions(minionCount);
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        yield return null;
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        yield break;
    }

    private void OnDestroy()
    {
        Debug.Log("[MinionCountBehavior] Destroyed");
    }
}