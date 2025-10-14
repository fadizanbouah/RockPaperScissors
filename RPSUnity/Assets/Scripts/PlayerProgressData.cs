using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgressData
{
    public int coins = 0;

    public int maxHealthLevel = 0;
    public int baseDamageLevel = 0;
    public int damageReductionLevel = 0;

    // New: Passive PowerUp Bonuses (persist until run reset)
    public int bonusBaseDamage = 0;
    public int bonusRockDamage = 0;
    public int bonusPaperDamage = 0;
    public int bonusScissorsDamage = 0;
    public int bonusMaxHealth = 0;  // NEW: Add this line

    // Track which one-time effects have been applied this run (not serialized)
    [System.NonSerialized]
    private HashSet<string> appliedOneTimeEffects = new HashSet<string>();

    private const string SaveKey = "PlayerProgress";

    private static PlayerProgressData _instance;

    public static PlayerProgressData Instance
    {
        get
        {
            if (_instance == null)
            {
                Load();
            }
            return _instance;
        }
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(_instance);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            _instance = JsonUtility.FromJson<PlayerProgressData>(json);
        }
        else
        {
            _instance = new PlayerProgressData();
        }

        // Initialize the HashSet after loading (it's not serialized)
        if (_instance.appliedOneTimeEffects == null)
        {
            _instance.appliedOneTimeEffects = new HashSet<string>();
        }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        _instance = new PlayerProgressData();
        Save();
    }

    // Call this at the start of a new run (not full reset)
    public void ResetPassiveBonuses()
    {
        bonusBaseDamage = 0;
        bonusRockDamage = 0;
        bonusPaperDamage = 0;
        bonusScissorsDamage = 0;
        bonusMaxHealth = 0;  // NEW: Add this line

        // Clear one-time effects for new run
        if (appliedOneTimeEffects == null)
            appliedOneTimeEffects = new HashSet<string>();
        else
            appliedOneTimeEffects.Clear();

        Debug.Log("[PlayerProgressData] Reset passive bonuses and one-time effects for new run");
    }

    // Check if a one-time effect has been applied this run
    public bool HasAppliedOneTimeEffect(string powerUpName)
    {
        if (appliedOneTimeEffects == null)
            appliedOneTimeEffects = new HashSet<string>();

        bool hasApplied = appliedOneTimeEffects.Contains(powerUpName);
        Debug.Log($"[PlayerProgressData] Checking if {powerUpName} has been applied: {hasApplied}");
        return hasApplied;
    }

    // Mark a one-time effect as applied
    public void MarkOneTimeEffectApplied(string powerUpName)
    {
        if (appliedOneTimeEffects == null)
            appliedOneTimeEffects = new HashSet<string>();

        appliedOneTimeEffects.Add(powerUpName);
        Debug.Log($"[PlayerProgressData] Marked {powerUpName} as applied this run");
    }
}