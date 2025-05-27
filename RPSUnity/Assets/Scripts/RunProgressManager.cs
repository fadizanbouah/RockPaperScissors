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
        favor = 0;
        acquiredPowerUps.Clear();

        foreach (var effect in activeEffects)
        {
            if (effect != null)
                GameObject.Destroy(effect.gameObject);
        }

        activeEffects.Clear();
        persistentPowerUps.Clear();

        Debug.Log("[RunProgressManager] Run reset: Favor, power-ups, and effects cleared.");
    }

    public void ApplyPowerUpEffect(PowerUpData data)
    {
        if (data == null) return;

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
}
