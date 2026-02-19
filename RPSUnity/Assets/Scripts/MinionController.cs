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
                GameObject minion = Instantiate(minionPrefab, spawnPoints[i].position, Quaternion.identity, transform);
                spawnedMinions.Add(minion);

                // Set Order in Layer based on spawn index
                SetMinionSortingOrder(minion, i);

                // Offset animation timing for natural look
                OffsetMinionAnimation(minion);

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

    private void SetMinionSortingOrder(GameObject minion, int spawnIndex)
    {
        // Calculate order based on spawn index
        int orderInLayer = baseOrderInLayer + (spawnIndex * orderInLayerIncrement);

        // Find all SpriteRenderers in the minion (including children)
        SpriteRenderer[] renderers = minion.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingOrder = orderInLayer;
            Debug.Log($"[MinionController] Set {renderer.gameObject.name} Order in Layer to {orderInLayer}");
        }
    }

    private void OffsetMinionAnimation(GameObject minion)
    {
        // Find animator in children (MinionContainer)
        Animator animator = minion.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            // Generate random offset within range (0 to animationOffsetRange)
            float randomOffset = Random.Range(0f, animationOffsetRange);

            // Play the current state at the random offset
            // This starts the idle animation at a different point in the loop
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, randomOffset);

            Debug.Log($"[MinionController] Offset minion animation by {randomOffset:F2}");
        }
        else
        {
            Debug.LogWarning($"[MinionController] No Animator found on {minion.name} for animation offset");
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