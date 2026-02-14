using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns minion prefabs with the enemy.
/// Minion prefab and count are configured via trait data.
/// </summary>
public class MinionCountBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Minion Configuration")]
    [Tooltip("The minion prefab to spawn")]
    [SerializeField] private GameObject minionPrefab;

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

        Debug.Log($"[MinionCountBehavior] Initialized - will spawn {minionCount} minions");

        // Spawn the configured number of minions using the prefab
        minionController.SpawnMinions(minionPrefab, minionCount);
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