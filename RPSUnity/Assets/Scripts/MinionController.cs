using System.Collections.Generic;
using UnityEngine;

public class MinionController : MonoBehaviour
{
    [Header("Minion Spawn Configuration")]
    [Tooltip("Transform points where minions will be spawned (up to 3)")]
    [SerializeField] private Transform[] spawnPoints = new Transform[3];

    [Header("Rendering")]
    [Tooltip("Starting Order in Layer value for first minion")]
    [SerializeField] private int baseOrderInLayer = 0;
    [Tooltip("Increment Order in Layer for each subsequent minion")]
    [SerializeField] private int orderInLayerIncrement = 1;

    [Header("Animation")]
    [Tooltip("Random offset range for idle animation start times (0-1, where 1 = full animation loop)")]
    [SerializeField] private float animationOffsetRange = 1f;

    private List<GameObject> spawnedMinions = new List<GameObject>();

    private void Awake()
    {
        Debug.Log($"[MinionController] Initialized with {spawnPoints.Length} spawn points");
    }

    /// <summary>
    /// Spawns the same minion prefab multiple times (original method).
    /// </summary>
    public void SpawnMinions(GameObject minionPrefab, int count)
    {
        if (minionPrefab == null)
        {
            Debug.LogError("[MinionController] Minion prefab is null!");
            return;
        }

        count = Mathf.Clamp(count, 0, spawnPoints.Length);

        Debug.Log($"[MinionController] Spawning {count} minions");

        ClearMinions();

        for (int i = 0; i < count; i++)
        {
            if (spawnPoints[i] != null)
            {
                SpawnMinionAtIndex(minionPrefab, i);
            }
        }

        Debug.Log($"[MinionController] Total spawned minions: {spawnedMinions.Count}");
    }

    /// <summary>
    /// Spawns different minion prefabs at each spawn point (new method).
    /// Pass an array where each element is the prefab for that spawn point.
    /// Null elements are skipped.
    /// </summary>
    public void SpawnMinionsMultiType(GameObject[] minionPrefabs)
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0)
        {
            Debug.LogError("[MinionController] Minion prefabs array is null or empty!");
            return;
        }

        Debug.Log($"[MinionController] Spawning multi-type minions from array of {minionPrefabs.Length}");

        ClearMinions();

        int maxIndex = Mathf.Min(minionPrefabs.Length, spawnPoints.Length);

        for (int i = 0; i < maxIndex; i++)
        {
            // Skip null prefabs (allows for gaps)
            if (minionPrefabs[i] != null && spawnPoints[i] != null)
            {
                SpawnMinionAtIndex(minionPrefabs[i], i);
            }
            else if (minionPrefabs[i] == null)
            {
                Debug.Log($"[MinionController] Skipping spawn point {i} (no prefab assigned)");
            }
        }

        Debug.Log($"[MinionController] Total spawned minions: {spawnedMinions.Count}");
    }

    /// <summary>
    /// Internal helper to spawn a single minion at a specific spawn point index.
    /// </summary>
    private void SpawnMinionAtIndex(GameObject minionPrefab, int index)
    {
        GameObject minion = Instantiate(minionPrefab, spawnPoints[index].position, Quaternion.identity, transform);
        spawnedMinions.Add(minion);

        // Set Order in Layer based on spawn index
        SetMinionSortingOrder(minion, index);

        // Offset animation timing for natural look
        OffsetMinionAnimation(minion);

        MinionBase minionScript = minion.GetComponent<MinionBase>();
        if (minionScript != null)
        {
            minionScript.Initialize(GetComponent<HandController>());
        }

        Debug.Log($"[MinionController] Spawned {minionPrefab.name} at spawn point {index}");
    }

    private void SetMinionSortingOrder(GameObject minion, int spawnIndex)
    {
        int orderInLayer = baseOrderInLayer + (spawnIndex * orderInLayerIncrement);

        SpriteRenderer[] renderers = minion.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = orderInLayer;
        }
    }

    private void OffsetMinionAnimation(GameObject minion)
    {
        Animator animator = minion.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            float randomOffset = Random.Range(0f, animationOffsetRange);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, randomOffset);

            Debug.Log($"[MinionController] Offset minion animation by {randomOffset:F2}");
        }
    }

    public List<GameObject> GetActiveMinions()
    {
        spawnedMinions.RemoveAll(m => m == null);
        return new List<GameObject>(spawnedMinions);
    }

    public int GetMinionCount()
    {
        spawnedMinions.RemoveAll(m => m == null);
        return spawnedMinions.Count;
    }

    public void RemoveMinion(GameObject minion)
    {
        if (spawnedMinions.Contains(minion))
        {
            spawnedMinions.Remove(minion);
            Destroy(minion);
            Debug.Log($"[MinionController] Removed minion: {minion.name}. Remaining: {spawnedMinions.Count}");
        }
    }

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
        ClearMinions();
    }
}