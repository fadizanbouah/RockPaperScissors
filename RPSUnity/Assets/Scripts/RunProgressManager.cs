using System.Collections.Generic;
using UnityEngine;

public class RunProgressManager : MonoBehaviour
{
    public static RunProgressManager Instance { get; private set; }

    [Header("Currency")]
    public int favor = 0;
    public int currentFavor => favor;

    [Header("Active PowerUps")]
    public List<PowerUp> activePowerUps = new List<PowerUp>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void AddFavor(int amount)
    {
        favor += amount;
        Debug.Log($"[RunProgress] Gained {amount} favor. Total: {favor}");
    }

    public void ResetRun()
    {
        favor = 0;
        activePowerUps.Clear();
        Debug.Log("[RunProgress] Run reset: Favor and PowerUps cleared.");
    }
}
