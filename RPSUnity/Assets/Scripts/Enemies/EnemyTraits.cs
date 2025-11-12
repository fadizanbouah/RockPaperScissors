using UnityEngine;
using System.Collections.Generic;

public class EnemyTraits : MonoBehaviour
{
    [SerializeField] private List<EnemyTraitData> traits = new List<EnemyTraitData>();

    private List<MonoBehaviour> activeBehaviors = new List<MonoBehaviour>();
    private HandController enemyHand;

    private void Awake()
    {
        enemyHand = GetComponent<HandController>();
        InitializeTraits();
    }

    private void InitializeTraits()
    {
        foreach (EnemyTraitData trait in traits)
        {
            if (trait.behaviorPrefab != null)
            {
                GameObject behaviorObj = Instantiate(trait.behaviorPrefab, transform);
                MonoBehaviour behavior = behaviorObj.GetComponent<MonoBehaviour>();

                if (behavior != null)
                {
                    activeBehaviors.Add(behavior);

                    // Initialize with config values if available
                    if (behavior is IEnemyBehavior enemyBehavior && trait.configValues.Length > 0)
                    {
                        enemyBehavior.Initialize(enemyHand, trait.configValues);
                    }

                    Debug.Log($"[EnemyTraits] Initialized trait: {trait.traitName}");
                }
            }
        }
    }

    public List<MonoBehaviour> GetActiveBehaviors()
    {
        return activeBehaviors;
    }

    public List<EnemyTraitData> GetTraitDataList()
    {
        return traits;
    }

    private void OnDestroy()
    {
        // Clean up behaviors
        foreach (var behavior in activeBehaviors)
        {
            if (behavior != null)
                Destroy(behavior.gameObject);
        }
        activeBehaviors.Clear();
    }
}