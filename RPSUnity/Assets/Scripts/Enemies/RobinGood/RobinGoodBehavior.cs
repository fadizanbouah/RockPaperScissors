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
        float roll = Random.Range(0f, 100f);
        Debug.Log($"[RobinGoodBehavior] Rolled {roll:F1} (need < {stealChance} to steal)");

        if (roll < stealChance)
        {
            List<PowerUpData> activePowerUps = GetPlayerActivePowerUps(player);

            if (activePowerUps.Count > 0)
            {
                PowerUpData stolenPowerUp = activePowerUps[Random.Range(0, activePowerUps.Count)];
                Debug.Log($"[RobinGoodBehavior] Robin Good stole: {stolenPowerUp.powerUpName}!");
                yield return ApplyStolenPowerUp(stolenPowerUp, player);
            }
            else
            {
                Debug.Log($"[RobinGoodBehavior] No active power-ups to steal!");
            }
        }

        yield return null;
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

            if (PowerUpEffectManager.Instance != null)
            {
                PowerUpEffectManager.Instance.RegisterEffect(effect);
                stolenEffects.Add(effect); // Track this stolen effect
                Debug.Log($"[RobinGoodBehavior] Registered stolen effect with manager");
            }

            // NEW: Add icon to enemy combat tracker
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
            List<GameObject> cardsToDestroy = new List<GameObject>();

            foreach (PowerUpCardDisplay display in allCards)
            {
                if (display != null && display.GetPowerUpData() == powerUpData)
                {
                    PowerUpCardDrag drag = display.GetComponent<PowerUpCardDrag>();
                    if (drag != null && drag.isDraggable)
                    {
                        cardsToDestroy.Add(display.gameObject);
                    }
                }
            }

            FanLayout fanLayout = FindObjectOfType<FanLayout>();
            if (fanLayout != null)
            {
                fanLayout.StopAllCoroutines();
            }

            foreach (GameObject card in cardsToDestroy)
            {
                Destroy(card);
                Debug.Log($"[RobinGoodBehavior] Destroyed card visual for {powerUpData.powerUpName}");
            }

            if (cardsToDestroy.Count > 0 && fanLayout != null)
            {
                StartCoroutine(RefreshFanLayoutNextFrame(fanLayout));
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