using System.Collections.Generic;
using UnityEngine;

public class RunProgressManager : MonoBehaviour
{
    public static RunProgressManager Instance { get; private set; }

    [Header("Currency")]
    public int favor = 0;
    public int currentFavor => favor;

    [Header("Active PowerUp Effects (instantiated prefabs)")]
    public List<PowerUpEffectBase> activeEffects = new List<PowerUpEffectBase>();

    [Header("Acquired PowerUps (Visual Cards in Gameplay)")]
    public List<PowerUpData> acquiredPowerUps = new List<PowerUpData>();

    [Header("Persistent Passive PowerUps")]
    public List<PowerUpData> persistentPowerUps = new List<PowerUpData>();

    [Header("Acquired Unique PowerUps (Won't spawn again)")]
    public List<PowerUpData> acquiredUniquePowerUps = new List<PowerUpData>();

    [Header("PowerUp Upgrade Tracking")]
    private Dictionary<PowerUpData, int> powerUpLevels = new Dictionary<PowerUpData, int>();

    [Header("Blocked PowerUps")]
    private List<PowerUpData> blockedPowerUps = new List<PowerUpData>();

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
        Debug.Log($"[RunProgressManager] Gained {amount} favor. Total: {favor}");
    }

    public void AddAcquiredPowerUp(PowerUpData powerUpData)
    {
        acquiredPowerUps.Add(powerUpData);
        Debug.Log($"[RunProgressManager] Acquired power-up: {powerUpData.powerUpName}");
    }

    public void RemoveAcquiredPowerUp(PowerUpData data)
    {
        if (acquiredPowerUps.Contains(data))
        {
            acquiredPowerUps.Remove(data);
            Debug.Log($"[RunProgressManager] Removed used power-up: {data.powerUpName}");
        }
    }

    public void ResetRun()
    {
        Debug.Log("[RunProgressManager] ===== RESET RUN CALLED =====");
        Debug.Log($"[RunProgressManager] BEFORE RESET - persistentPowerUps count: {persistentPowerUps.Count}");

        favor = 0;
        acquiredPowerUps.Clear();
        persistentPowerUps.Clear();
        acquiredUniquePowerUps.Clear();
        powerUpLevels.Clear();
        blockedPowerUps.Clear();

        foreach (var effect in activeEffects)
        {
            if (effect != null)
                GameObject.Destroy(effect.gameObject);
        }

        activeEffects.Clear();

        // NEW: Clean up ALL effects registered with PowerUpEffectManager
        if (PowerUpEffectManager.Instance != null)
        {
            PowerUpEffectManager.Instance.CleanupAllEffects();
            Debug.Log("[RunProgressManager] Cleaned up all PowerUpEffectManager effects");
        }

        // UPDATED: Properly reset GamblerUI
        GamblerUI[] allGamblerUIs = Resources.FindObjectsOfTypeAll<GamblerUI>();
        foreach (var gamblerUI in allGamblerUIs)
        {
            if (gamblerUI.gameObject.scene.IsValid())
            {
                gamblerUI.FullReset();  // Call FullReset instead of just SetActive(false)
                Debug.Log($"[RunProgressManager] Fully reset GamblerUI: {gamblerUI.name}");
            }
        }

        // Clear prediction UI on run reset
        PredictionUI predictionUI = FindObjectOfType<PredictionUI>();
        if (predictionUI != null)
        {
            predictionUI.ClearPrediction();
            Debug.Log("[RunProgressManager] Cleared prediction UI");
        }

        Debug.Log("[RunProgressManager] Run reset: Favor, power-ups, and effects cleared.");
        Debug.Log("[RunProgressManager] ===== RESET RUN COMPLETE =====");
    }

    public void ApplyPowerUpEffect(PowerUpData data)
    {
        if (data == null) return;

        // Track unique power-ups
        if (data.isUnique && !acquiredUniquePowerUps.Contains(data))
        {
            acquiredUniquePowerUps.Add(data);
            Debug.Log($"[RunProgressManager] Marked unique power-up as acquired: {data.powerUpName}");
        }

        // Track blocked power-ups
        if (data.blocksThesePowerUps != null && data.blocksThesePowerUps.Count > 0)
        {
            foreach (var blockedPowerUp in data.blocksThesePowerUps)
            {
                if (blockedPowerUp != null && !blockedPowerUps.Contains(blockedPowerUp))
                {
                    blockedPowerUps.Add(blockedPowerUp);
                    Debug.Log($"[RunProgressManager] Blocked power-up: {blockedPowerUp.powerUpName}");
                }
            }
        }

        // Check upgrade status BEFORE adding to any lists
        bool isFirstTimeAcquiring = !HasPowerUp(data);

        // Track power-up level
        if (data.isUpgradeable)
        {
            if (!isFirstTimeAcquiring)
            {
                // Upgrading existing power-up
                int currentLevel = GetPowerUpLevel(data);
                UpgradePowerUp(data, currentLevel + 1);
                Debug.Log($"[RunProgressManager] Upgrading {data.powerUpName} from level {currentLevel} to {currentLevel + 1}");
                return; // Don't add it again, just upgrade
            }
            else
            {
                // First time getting this power-up
                powerUpLevels[data] = 0;
                Debug.Log($"[RunProgressManager] First time acquiring {data.powerUpName} at level 0");
            }
        }

        // Now proceed with adding to lists and applying effects
        if (data.isPassive)
        {
            persistentPowerUps.Add(data);
            Debug.Log($"[RunProgressManager] Stored persistent power-up: {data.powerUpName}");
        }
        else
        {
            if (data.effectPrefab == null)
            {
                Debug.LogWarning($"[RunProgressManager] No prefab assigned for active power-up: {data.powerUpName}");
                return;
            }
            GameObject instance = Instantiate(data.effectPrefab);
            PowerUpEffectBase effect = instance.GetComponent<PowerUpEffectBase>();
            if (effect == null)
            {
                Debug.LogError($"[RunProgressManager] Prefab for {data.powerUpName} is missing a PowerUpEffectBase script!");
                Destroy(instance);
                return;
            }
            HandController player = PowerUpEffectManager.Instance != null ? PowerUpEffectManager.Instance.GetPlayer() : null;
            HandController enemy = PowerUpEffectManager.Instance != null ? PowerUpEffectManager.Instance.GetEnemy() : null;
            if (player == null)
            {
                Debug.LogWarning("[RunProgressManager] Could not find player HandController when initializing power-up!");
            }
            effect.Initialize(data, player, enemy);
            effect.OnRoomStart();
            activeEffects.Add(effect);
            Debug.Log($"[DEBUG] About to register effect: {effect.GetType().Name}");
            PowerUpEffectManager.Instance?.RegisterEffect(effect); // Register with central manager
            Debug.Log($"[RunProgressManager] Instantiated and registered effect: {data.powerUpName}");
        }
    }

    public int GetPowerUpLevel(PowerUpData data)
    {
        if (data == null || !powerUpLevels.ContainsKey(data))
            return 0;
        return powerUpLevels[data];
    }

    public bool HasPowerUp(PowerUpData data)
    {
        if (data == null) return false;

        // For passive power-ups, check if it's in persistentPowerUps
        if (data.isPassive)
        {
            bool hasPowerUp = persistentPowerUps.Contains(data);
            Debug.Log($"[HasPowerUp] Checking passive {data.powerUpName}: {hasPowerUp}");
            return hasPowerUp;
        }

        // For active power-ups, check acquiredPowerUps
        return acquiredPowerUps.Contains(data);
    }

    public void UpgradePowerUp(PowerUpData data, int newLevel)
    {
        if (data == null) return;

        powerUpLevels[data] = newLevel;
        Debug.Log($"[RunProgressManager] Upgraded {data.powerUpName} to level {newLevel}");

        // If it's a passive power-up, we need to recalculate bonuses
        if (data.isPassive)
        {
            // Trigger reapplication of all passive effects
            PassivePowerUpHandler.ApplyAllPersistentPowerUps();
        }
    }

    public bool IsPowerUpBlocked(PowerUpData data)
    {
        return blockedPowerUps.Contains(data);
    }
}
