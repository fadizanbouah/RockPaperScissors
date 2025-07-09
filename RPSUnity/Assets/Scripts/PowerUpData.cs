using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUps/PowerUp")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    public string description;
    public Sprite icon;
    public int favorCost;
    public bool isPassive = false;

    [Tooltip("Assign a prefab with the desired PowerUpEffectBase script attached (for active effects only)")]
    public GameObject effectPrefab;

    public float value;  // Used by many effects, e.g. +5% damage or +10 health

    [Header("Spawn Settings")]
    [Tooltip("If true, this power-up can only be acquired once per run")]
    public bool isUnique = false;
}
