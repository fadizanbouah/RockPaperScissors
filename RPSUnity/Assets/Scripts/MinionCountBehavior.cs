using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns minion prefabs with the enemy.
/// Supports spawning different minion types at each spawn point.
/// </summary>
public class MinionCountBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Minion Configuration")]
    [Tooltip("Minion prefabs to spawn (one per spawn point, up to 3)")]
    [SerializeField] private GameObject[] minionPrefabs = new GameObject[3];

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

        Debug.Log($"[MinionCountBehavior] Initialized with {minionPrefabs.Length} minion slots");

        // Spawn the configured minions
        minionController.SpawnMinionsMultiType(minionPrefabs);
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