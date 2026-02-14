using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages minion prefab spawning and tracking for enemies that have minion support.
/// Spawns minion prefabs at designated spawn points.
/// </summary>
public class MinionController : MonoBehaviour
{
    [Header("Minion Spawn Configuration")]
    [Tooltip("Transform points where minions will be spawned (up to 3)")]
    [SerializeField] private Transform[] spawnPoints = new Transform[3];

    private List<GameObject> spawnedMinions = new List<GameObject>();

    private void Awake()
    {
        Debug.Log($"[MinionController] Initialized with {spawnPoints.Length} spawn points");
    }

    /// <summary>
    /// Spawns the specified number of minion prefabs at spawn points.
    /// </summary>
    /// <param name="minionPrefab">The minion prefab to spawn</param>
    /// <param name="count">Number of minions to spawn (1-3)</param>
    public void SpawnMinions(GameObject minionPrefab, int count)
    {
        if (minionPrefab == null)
        {
            Debug.LogError("[MinionController] Minion prefab is null!");
            return;
        }

        // Clamp count to valid range
        count = Mathf.Clamp(count, 0, spawnPoints.Length);

        Debug.Log($"[MinionController] Spawning {count} minions");

        // Clear any existing minions first
        ClearMinions();

        for (int i = 0; i < count; i++)
        {
            if (spawnPoints[i] != null)
            {
                // Spawn minion at spawn point, parent it to this transform
                GameObject minion = Instantiate(minionPrefab, spawnPoints[i].position, Quaternion.identity, transform);
                spawnedMinions.Add(minion);

                // Optional: Pass reference to parent enemy
                MinionBase minionScript = minion.GetComponent<MinionBase>();
                if (minionScript != null)
                {
                    minionScript.Initialize(GetComponent<HandController>());
                }

                Debug.Log($"[MinionController] Spawned minion at spawn point {i}: {minion.name}");
            }
        }

        Debug.Log($"[MinionController] Total spawned minions: {spawnedMinions.Count}");
    }

    /// <summary>
    /// Gets the list of currently spawned minions.
    /// </summary>
    public List<GameObject> GetActiveMinions()
    {
        // Remove any null entries (destroyed minions)
        spawnedMinions.RemoveAll(m => m == null);
        return new List<GameObject>(spawnedMinions);
    }

    /// <summary>
    /// Gets the count of currently active minions.
    /// </summary>
    public int GetMinionCount()
    {
        spawnedMinions.RemoveAll(m => m == null);
        return spawnedMinions.Count;
    }

    /// <summary>
    /// Removes a specific minion from the active list and destroys it.
    /// Useful for behaviors where minions can be destroyed/sacrificed.
    /// </summary>
    public void RemoveMinion(GameObject minion)
    {
        if (spawnedMinions.Contains(minion))
        {
            spawnedMinions.Remove(minion);
            Destroy(minion);
            Debug.Log($"[MinionController] Removed minion: {minion.name}. Remaining: {spawnedMinions.Count}");
        }
    }

    /// <summary>
    /// Destroys all spawned minions and clears the list.
    /// </summary>
    public void ClearMinions()
    {
        foreach (var minion in spawnedMinions)
        {
            if (minion != null)
            {
                Destroy(minion);
            }
        }

        spawnedMinions.Clear();
        Debug.Log("[MinionController] All minions cleared");
    }

    private void OnDestroy()
    {
        // Clean up minions when enemy is destroyed
        ClearMinions();
    }
}