using UnityEngine;
using System;

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
    }
}
