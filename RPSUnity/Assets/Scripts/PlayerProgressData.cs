using UnityEngine;
using System;

[Serializable]
public class PlayerProgressData
{
    public int coins = 0;

    public int maxHealthLevel = 0;
    public int rockDamageLevel = 0;
    public int paperDamageLevel = 0;
    public int scissorsDamageLevel = 0;
    public int damageReductionLevel = 0;

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
}
