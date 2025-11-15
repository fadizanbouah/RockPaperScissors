using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobinGoodBehavior : MonoBehaviour, IEnemyBehavior
{
    private float stealChance = 30f;
    private HandController enemyHand;
    private List<PowerUpEffectBase> stolenEffects = new List<PowerUpEffectBase>(); // Track stolen effects

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            stealChance = configValues[0];
        }

        Debug.Log($"[RobinGoodBehavior] Initialized with {stealChance}% steal chance");

        // Subscribe to enemy death to clean up stolen effects
        if (enemyHand != null)
        {
            enemyHand.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath(HandController hand)
    {
        Debug.Log($"[RobinGoodBehavior] Enemy died - cleaning up {stolenEffects.Count} stolen effects");

        // Remove all stolen effects from PowerUpEffectManager
        foreach (var effect in stolenEffects)
        {
            if (effect != null && PowerUpEffectManager.Instance != null)
            {
                PowerUpEffectManager.Instance.RemoveEffect(effect);
                Debug.Log($"[RobinGoodBehavior] Removed stolen effect: {effect.GetType().Name}");
            }
        }

        stolenEffects.Clear();
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // No longer doing anything before round resolves - stealing moved to after damage
        Debug.Log("[RobinGoodBehavior] OnBeforeRoundResolves - no action (stealing happens after damage now)");
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        Debug.Log($"[RobinGoodBehavior] OnAfterDamageResolved called - Result: {result}");

        // Roll for steal
        float roll = Random.Range(0f, 100f);
        Debug.Log($"[RobinGoodBehavior] Rolled {roll:F1} (need < {stealChance} to steal)");

        if (roll < stealChance)
        {
            List<PowerUpData> activePowerUps = GetPlayerActivePowerUps(player);

            if (activePowerUps.Count > 0)
            {
                PowerUpData stolenPowerUp = activePowerUps[Random.Range(0, activePowerUps.Count)];
                Debug.Log($"[RobinGoodBehavior] Robin Good is stealing: {stolenPowerUp.powerUpName}!");

                // Placeholder for steal animation
                yield return PlayStealAnimation();

                // Apply the stolen power-up
                yield return ApplyStolenPowerUp(stolenPowerUp, player);
            }
            else
            {
                Debug.Log($"[RobinGoodBehavior] No active power-ups to steal!");
            }
        }

        yield return null;
    }

    private IEnumerator PlayStealAnimation()
    {
        // Check if enemy has a Steal animation
        if (enemyHand != null && enemyHand.handAnimator != null)
        {
            Animator animator = enemyHand.handAnimator;

            if (animator.HasParameter("Steal"))
            {
                Debug.Log("[RobinGoodBehavior] Playing Steal animation");
                animator.SetTrigger("Steal");

                // Wait for animation to complete
                // You can adjust this timing or add a proper animation event callback later
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                // No animation exists yet, just a small delay for visual feedback
                Debug.Log("[RobinGoodBehavior] No Steal animation parameter found - using placeholder delay");
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    private List<PowerUpData> GetPlayerActivePowerUps(HandController player)
    {
        List<PowerUpData> activePowerUps = new List<PowerUpData>();

        if (RunProgressManager.Instance != null)
        {
            foreach (PowerUpData powerUp in RunProgressManager.Instance.acquiredPowerUps)
            {
                if (!powerUp.isPassive)
                {
                    activePowerUps.Add(powerUp);
                }
            }
        }

        return activePowerUps;
    }

    private IEnumerator ApplyStolenPowerUp(PowerUpData powerUpData, HandController player)
    {
        if (powerUpData.effectPrefab == null)
        {
            Debug.LogWarning($"[RobinGoodBehavior] No effect prefab for {powerUpData.powerUpName}");
            yield break;
        }

        GameObject effectObj = Instantiate(powerUpData.effectPrefab);
        PowerUpEffectBase effect = effectObj.GetComponent<PowerUpEffectBase>();

        if (effect != null)
        {
            effect.Initialize(powerUpData, enemyHand, player);
            effect.OnRoomStart();

            if (effect is IncreaseDamageNextHitEffect nextHitEffect)
            {
                nextHitEffect.MarkAsJustStolen();
            }

            if (PowerUpEffectManager.Instance != null)
            {
                PowerUpEffectManager.Instance.RegisterEffect(effect);
                stolenEffects.Add(effect); // Track this stolen effect
                Debug.Log($"[RobinGoodBehavior] Registered stolen effect with manager");
            }

            // Add icon to enemy combat tracker
            EnemyCombatTracker tracker = FindObjectOfType<EnemyCombatTracker>();
            if (tracker != null && !powerUpData.isPassive && powerUpData.statusIcon != null)
            {
                tracker.AddActiveEffect(effect);
                Debug.Log($"[RobinGoodBehavior] Added {powerUpData.powerUpName} icon to enemy combat tracker");
            }

            if (RunProgressManager.Instance != null)
            {
                RunProgressManager.Instance.RemoveAcquiredPowerUp(powerUpData);
                Debug.Log($"[RobinGoodBehavior] Removed {powerUpData.powerUpName} from player inventory");
            }

            RemoveCardFromGameplay(powerUpData);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            Debug.LogWarning($"[RobinGoodBehavior] No PowerUpEffectBase on {powerUpData.powerUpName}");
            Destroy(effectObj);
        }
    }


    private void RemoveCardFromGameplay(PowerUpData powerUpData)
    {
        PowerUpCardSpawnerGameplay spawner = FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
        {
            PowerUpCardDisplay[] allCards = FindObjectsOfType<PowerUpCardDisplay>();
            GameObject cardToDestroy = null; // Changed from List to single GameObject

            // Find the FIRST matching card only
            foreach (PowerUpCardDisplay display in allCards)
            {
                if (display != null && display.GetPowerUpData() == powerUpData)
                {
                    PowerUpCardDrag drag = display.GetComponent<PowerUpCardDrag>();
                    if (drag != null && drag.isDraggable)
                    {
                        cardToDestroy = display.gameObject;
                        Debug.Log($"[RobinGoodBehavior] Found card to destroy: {powerUpData.powerUpName}");
                        break; // Stop after finding the first one!
                    }
                }
            }

            if (cardToDestroy != null)
            {
                FanLayout fanLayout = FindObjectOfType<FanLayout>();
                if (fanLayout != null)
                {
                    fanLayout.StopAllCoroutines();
                }

                Destroy(cardToDestroy);
                Debug.Log($"[RobinGoodBehavior] Destroyed card visual for {powerUpData.powerUpName}");

                if (fanLayout != null)
                {
                    StartCoroutine(RefreshFanLayoutNextFrame(fanLayout));
                }
            }
        }
        else
        {
            Debug.LogWarning("[RobinGoodBehavior] PowerUpCardSpawnerGameplay not found!");
        }
    }

    private IEnumerator RefreshFanLayoutNextFrame(FanLayout fanLayout)
    {
        yield return null;
        if (fanLayout != null)
        {
            fanLayout.RefreshLayout();
        }
    }

    private void OnDestroy()
    {
        // Clean up subscription
        if (enemyHand != null)
        {
            enemyHand.OnDeath -= OnEnemyDeath;
        }
    }
}