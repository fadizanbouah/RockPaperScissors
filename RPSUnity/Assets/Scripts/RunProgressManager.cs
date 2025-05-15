using System.Collections.Generic;
using UnityEngine;

public class RunProgressManager : MonoBehaviour
{
    public static RunProgressManager Instance { get; private set; }

    [Header("Currency")]
    public int favor = 0;
    public int currentFavor => favor;

    [Header("Active PowerUps (Runtime Effects)")]
    public List<PowerUp> activePowerUps = new List<PowerUp>();

    [Header("Acquired PowerUps (Visual Cards in Gameplay)")]
    public List<PowerUpData> acquiredPowerUps = new List<PowerUpData>();

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

    public void AddAcquiredPowerUp(PowerUpData powerUpData)
    {
        acquiredPowerUps.Add(powerUpData);
        Debug.Log($"[RunProgress] Acquired power-up: {powerUpData.powerUpName}");
    }

    public void ResetRun()
    {
        favor = 0;
        activePowerUps.Clear();
        acquiredPowerUps.Clear();
        Debug.Log("[RunProgress] Run reset: Favor and PowerUps cleared.");
    }
}
