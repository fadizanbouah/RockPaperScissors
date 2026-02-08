using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages minion GameObjects for enemies that have minion support.
/// Handles visibility and tracking of active minions.
/// </summary>
public class MinionController : MonoBehaviour
{
    [Header("Minion Configuration")]
    [Tooltip("Array of minion GameObjects (assign in prefab, up to 3)")]
    [SerializeField] private GameObject[] minionSlots = new GameObject[3];

    private List<GameObject> activeMinions = new List<GameObject>();

    private void Awake()
    {
        // Just log initialization - minions should already be disabled in prefab
        Debug.Log($"[MinionController] Initialized with {minionSlots.Length} minion slots");
    }

    /// <summary>
    /// Shows the specified number of minions (1-3).
    /// Enables minions from the array in order.
    /// </summary>
    public void ShowMinions(int count)
    {
        // Clamp count to valid range
        count = Mathf.Clamp(count, 0, minionSlots.Length);

        Debug.Log($"[MinionController] Showing {count} minions");

        activeMinions.Clear();

        for (int i = 0; i < minionSlots.Length; i++)
        {
            if (minionSlots[i] != null)
            {
                bool shouldBeActive = i < count;
                minionSlots[i].SetActive(shouldBeActive);

                Debug.Log($"[MinionController] Minion {i} ({minionSlots[i].name}) - SetActive({shouldBeActive}), IsActive: {minionSlots[i].activeSelf}");

                if (shouldBeActive)
                {
                    activeMinions.Add(minionSlots[i]);
                    Debug.Log($"[MinionController] Activated minion: {minionSlots[i].name}");
                }
            }
        }

        Debug.Log($"[MinionController] Total active minions: {activeMinions.Count}");
    }

    /// <summary>
    /// Gets the list of currently active (visible) minions.
    /// </summary>
    public List<GameObject> GetActiveMinions()
    {
        return new List<GameObject>(activeMinions); // Return a copy to prevent external modification
    }

    /// <summary>
    /// Gets the count of currently active minions.
    /// </summary>
    public int GetMinionCount()
    {
        return activeMinions.Count;
    }

    /// <summary>
    /// Removes a specific minion from the active list and disables it.
    /// Useful for behaviors where minions can be destroyed/sacrificed.
    /// </summary>
    public void RemoveMinion(GameObject minion)
    {
        if (activeMinions.Contains(minion))
        {
            activeMinions.Remove(minion);
            minion.SetActive(false);
            Debug.Log($"[MinionController] Removed minion: {minion.name}. Remaining: {activeMinions.Count}");
        }
    }

    /// <summary>
    /// Hides all minions.
    /// </summary>
    public void HideAllMinions()
    {
        foreach (var minion in minionSlots)
        {
            if (minion != null)
            {
                minion.SetActive(false);
            }
        }

        activeMinions.Clear();
        Debug.Log("[MinionController] All minions hidden");
    }
}