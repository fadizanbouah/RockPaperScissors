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

        Debug.Log($"[PowerUpEffectManager] Re-linked references: Player = {player?.name}, Enemy = {enemy?.name}");
        Debug.Log($"[PowerUpEffectManager] Found {activeEffects.Count} active effects to update");

        // Update all existing effects with the new player/enemy references
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                Debug.Log($"[PowerUpEffectManager] Updating references for {effect.GetType().Name}");
                effect.UpdateReferences(player, enemy);
            }
            else
            {
                Debug.LogWarning("[PowerUpEffectManager] Found null effect in activeEffects!");
            }
        }
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
        // Since those effects are deleted, this method can be empty or removed entirely
        // Or keep it empty to avoid errors if called elsewhere
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

    public HandController GetPlayer()
    {
        return player;
    }

    public HandController GetEnemy()
    {
        return enemy;
    }

    public void TriggerRoomStart()
    {
        // Create a copy of the list before iterating to avoid modification errors
        var effectsCopy = new List<PowerUpEffectBase>(activeEffects);

        // Track any null effects we find
        List<PowerUpEffectBase> nullEffects = new List<PowerUpEffectBase>();

        foreach (var effect in effectsCopy)
        {
            if (effect != null)
            {
                effect.OnRoomStart();
            }
            else
            {
                nullEffects.Add(effect);
                Debug.LogWarning("[PowerUpEffectManager] Found null effect in activeEffects list");
            }
        }

        // Clean up any null effects
        foreach (var nullEffect in nullEffects)
        {
            activeEffects.Remove(nullEffect);
        }
    }
}
