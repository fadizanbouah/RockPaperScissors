using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobinGoodBehavior : MonoBehaviour, IEnemyBehavior
{
    private float stealChance = 30f; // Default 30%
    private HandController enemyHand;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            stealChance = configValues[0];
        }

        Debug.Log($"[RobinGoodBehavior] Initialized with {stealChance}% steal chance");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // Roll the dice
        float roll = Random.Range(0f, 100f);

        Debug.Log($"[RobinGoodBehavior] Rolled {roll:F1} (need < {stealChance} to steal)");

        if (roll < stealChance)
        {
            // Get all active power-ups from player's hand
            List<PowerUpData> activePowerUps = GetPlayerActivePowerUps(player);

            if (activePowerUps.Count > 0)
            {
                // Pick a random active power-up
                PowerUpData stolenPowerUp = activePowerUps[Random.Range(0, activePowerUps.Count)];

                Debug.Log($"[RobinGoodBehavior] Robin Good stole: {stolenPowerUp.powerUpName}!");

                // Apply the stolen power-up to the enemy
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

        // Instantiate the effect
        GameObject effectObj = Instantiate(powerUpData.effectPrefab);
        PowerUpEffectBase effect = effectObj.GetComponent<PowerUpEffectBase>();

        if (effect != null)
        {
            // IMPORTANT: Initialize with SWAPPED player/enemy references
            // This makes the effect benefit the enemy instead of the player
            effect.Initialize(powerUpData, enemyHand, player);

            // Apply the effect immediately
            effect.OnRoomStart();

            // Register with PowerUpEffectManager if needed
            if (PowerUpEffectManager.Instance != null)
            {
                PowerUpEffectManager.Instance.RegisterEffect(effect);
                Debug.Log($"[RobinGoodBehavior] Registered stolen effect with manager");
            }

            // Remove the card from player's inventory
            if (RunProgressManager.Instance != null)
            {
                RunProgressManager.Instance.RemoveAcquiredPowerUp(powerUpData);
                Debug.Log($"[RobinGoodBehavior] Removed {powerUpData.powerUpName} from player inventory");
            }

            // Remove the card visually from gameplay
            RemoveCardFromGameplay(powerUpData);

            yield return new WaitForSeconds(0.5f); // Brief pause to show the steal
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
            // Find and destroy the card in the gameplay area
            Transform cardContainer = spawner.transform.Find("CardContainer");
            if (cardContainer != null)
            {
                foreach (Transform child in cardContainer)
                {
                    PowerUpCardDisplay display = child.GetComponent<PowerUpCardDisplay>();
                    if (display != null && display.GetPowerUpData() == powerUpData)
                    {
                        Destroy(child.gameObject);
                        Debug.Log($"[RobinGoodBehavior] Destroyed card visual for {powerUpData.powerUpName}");

                        // Refresh fan layout
                        FanLayout fanLayout = cardContainer.GetComponent<FanLayout>();
                        if (fanLayout != null)
                        {
                            fanLayout.RefreshLayout();
                        }
                        break;
                    }
                }
            }
        }
    }
}