using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyTrait", menuName = "Enemy/Trait Data")]
public class EnemyTraitData : ScriptableObject
{
    public string traitName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    // Reference to the behavior component type
    public GameObject behaviorPrefab; // The actual behavior component prefab

    // Configuration values
    public float[] configValues; // For things like the 30% chance
}