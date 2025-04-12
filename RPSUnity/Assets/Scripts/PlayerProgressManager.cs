using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    public static PlayerProgressManager Instance { get; private set; }

    public PlayerProgressData Progress { get; private set; }

    private const string SaveKey = "PlayerProgressData";

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadProgress();
    }

    private void LoadProgress()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            Progress = JsonUtility.FromJson<PlayerProgressData>(json);
            Debug.Log("Player progress loaded.");
        }
        else
        {
            Progress = new PlayerProgressData();
            SaveProgress(); // Save initial default
            Debug.Log("No save found. Created new default progress.");
        }
    }

    public void SaveProgress()
    {
        string json = JsonUtility.ToJson(Progress);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("Player progress saved.");
    }

    public void ResetProgress()
    {
        Progress = new PlayerProgressData(); // Reset to default values
        SaveProgress();
    }
}
