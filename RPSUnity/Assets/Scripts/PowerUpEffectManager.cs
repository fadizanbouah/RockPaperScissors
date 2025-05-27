using System.Collections.Generic;
using UnityEngine;

public class PowerUpEffectManager : MonoBehaviour
{
    public static PowerUpEffectManager Instance { get; private set; }

    private List<PowerUpEffectBase> activeEffects = new List<PowerUpEffectBase>();

    private HandController player;
    private HandController enemy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(HandController player, HandController enemy)
    {
        this.player = player;
        this.enemy = enemy;

        Debug.Log($"[PowerUpEffectManager] Initialized with Player: {player?.name}, Enemy: {(enemy != null ? enemy.name : "null (no enemy)")}");

        CleanupAllEffects();

        // IMPORTANT: Do NOT instantiate active effects here.
        // Active effects are only triggered by dragging cards into the CardActivationZone.
    }

    public void OnRoundStart()
    {
        foreach (var effect in activeEffects)
        {
            effect.OnRoundStart();
        }
    }

    public void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        foreach (var effect in activeEffects)
        {
            effect.OnRoundEnd(playerChoice, enemyChoice, result);
        }
    }

    public void CleanupAllEffects()
    {
        foreach (var effect in activeEffects)
        {
            effect.Cleanup();

            if (effect is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }
        }

        activeEffects.Clear();
    }

    public void RemoveEffect(PowerUpEffectBase effect)
    {
        if (effect != null && activeEffects.Contains(effect))
        {
            activeEffects.Remove(effect);

            if (effect is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }

            Debug.Log($"[PowerUpEffectManager] Removed and destroyed effect: {effect.GetType().Name}");
        }
    }

    public List<PowerUpEffectBase> GetActiveEffects()
    {
        return new List<PowerUpEffectBase>(activeEffects); // Return a copy for safety
    }

    public void RemoveRoomScopedEffects()
    {
        var toRemove = new List<PowerUpEffectBase>();

        foreach (var effect in activeEffects)
        {
            if (effect is IncreaseDamageThisRoomEffect || effect is IncreaseMaxHealthThisRoomEffect)
            {
                toRemove.Add(effect);
            }
        }

        foreach (var effect in toRemove)
        {
            RemoveEffect(effect);
        }
    }

    public void RegisterEffect(PowerUpEffectBase effect)
    {
        if (effect == null)
        {
            Debug.LogWarning("[PowerUpEffectManager] Tried to register a null effect.");
            return;
        }

        activeEffects.Add(effect);
        Debug.Log($"[PowerUpEffectManager] Registered effect at runtime: {effect.GetType().Name}");
    }
}
