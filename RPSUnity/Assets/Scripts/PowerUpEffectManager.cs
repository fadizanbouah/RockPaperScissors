using System.Collections.Generic;
using UnityEngine;

public class PowerUpEffectManager : MonoBehaviour
{
    private List<IPowerUpEffect> activeEffects = new List<IPowerUpEffect>();

    private HandController player;
    private HandController enemy;

    public void Initialize(HandController player, HandController enemy)
    {
        this.player = player;
        this.enemy = enemy;

        activeEffects.Clear();

        foreach (PowerUp powerUp in RunProgressManager.Instance.activePowerUps)
        {
            IPowerUpEffect effect = CreateEffectInstance(powerUp);
            if (effect != null)
            {
                effect.Initialize(powerUp, player, enemy);
                activeEffects.Add(effect);
                Debug.Log($"[EffectManager] Initialized effect: {powerUp.powerUpName}");
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
        }

        activeEffects.Clear();
    }

    private IPowerUpEffect CreateEffectInstance(PowerUp powerUp)
    {
        switch (powerUp.type)
        {
            case PowerUpType.IncreaseDamageNextHit:
                return new IncreaseDamageNextHitEffect();

            case PowerUpType.IncreaseDamageThisRoom:
                return new IncreaseDamageThisRoomEffect();

            case PowerUpType.IncreaseMaxHealthThisRoom:
                return new IncreaseMaxHealthThisRoomEffect();

            // Passive ones can be ignored here or handled separately
            default:
                return null;
        }
    }
}
