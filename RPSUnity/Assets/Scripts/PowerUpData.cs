using UnityEngine;
using System.Collections.Generic;


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

    [Header("Upgrade Settings")]
    [Tooltip("Can this power-up be upgraded to higher levels?")]
    public bool isUpgradeable = false;

    [Header("Prerequisite Settings")]
    [Tooltip("Power-ups required before this can appear")]
    public List<PowerUpData> prerequisitePowerUps = new List<PowerUpData>();

    [Header("Exclusion Settings")]
    [Tooltip("Power-ups that should be blocked after acquiring this one")]
    public List<PowerUpData> blocksThesePowerUps = new List<PowerUpData>();

    [System.Serializable]
    public class UpgradeLevel
    {
        public string levelSuffix = "I";  // I, II, III, etc.
        public float value = 5f;          // The value for this level
        [TextArea(2, 4)]
        public string descriptionOverride = ""; // Optional: Override description for this level
    }

    [Tooltip("Define values for each upgrade level (Level 0 is the first entry)")]
    public List<UpgradeLevel> upgradeLevels = new List<UpgradeLevel>();

    [Header("Description Template")]
    [Tooltip("Use {0} for the value placeholder. Example: 'Increase damage by {0}'")]
    [TextArea(3, 5)]
    public string descriptionTemplate = "";

    // Helper methods
    public float GetValueForLevel(int level)
    {
        if (upgradeLevels == null || upgradeLevels.Count == 0)
            return value; // Fallback to base value

        level = Mathf.Clamp(level, 0, upgradeLevels.Count - 1);
        return upgradeLevels[level].value;
    }

    public string GetLevelSuffix(int level)
    {
        if (!isUpgradeable || upgradeLevels == null || upgradeLevels.Count == 0)
            return "";

        level = Mathf.Clamp(level, 0, upgradeLevels.Count - 1);
        return " " + upgradeLevels[level].levelSuffix;
    }

    public string GetDescriptionForLevel(int level)
    {
        if (!string.IsNullOrEmpty(descriptionTemplate))
        {
            return string.Format(descriptionTemplate, GetValueForLevel(level));
        }

        // Check for level-specific override
        if (upgradeLevels != null && level < upgradeLevels.Count &&
            !string.IsNullOrEmpty(upgradeLevels[level].descriptionOverride))
        {
            return upgradeLevels[level].descriptionOverride;
        }

        return description; // Fallback to base description
    }

    public bool IsMaxLevel(int currentLevel)
    {
        return currentLevel >= upgradeLevels.Count - 1;
    }

    public bool HasMetPrerequisites()
    {
        if (prerequisitePowerUps == null || prerequisitePowerUps.Count == 0)
            return true; // No prerequisites

        // Check if player has ALL required power-ups
        foreach (var prereq in prerequisitePowerUps)
        {
            if (prereq != null && !RunProgressManager.Instance.HasPowerUp(prereq))
            {
                return false; // Missing a prerequisite
            }
        }

        return true; // Has all prerequisites
    }
}
