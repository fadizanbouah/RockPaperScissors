using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy attacks the player for a percentage of their average damage when reaching HP thresholds.
/// Triggers at 75%, 50%, and 25% HP.
/// </summary>
public class MurderousIntentBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Configuration")]
    [SerializeField] private float damagePercent = 100f;

    private HandController thisEnemy;

    private int attackDamage;
    private HandController targetPlayer;
    private bool shouldDealDamage;

    // Track which thresholds have been triggered
    private bool triggered75 = false;
    private bool triggered50 = false;
    private bool triggered25 = false;

    // HP thresholds
    private const float THRESHOLD_75 = 0.75f;
    private const float THRESHOLD_50 = 0.50f;
    private const float THRESHOLD_25 = 0.25f;

    public void Initialize(HandController enemy, float[] configValues)
    {
        thisEnemy = enemy;

        if (configValues != null && configValues.Length > 0)
        {
            damagePercent = configValues[0];
        }

        // Register with AnimationEventRelay so it can call us back
        AnimationEventRelay relay = thisEnemy.GetComponentInChildren<AnimationEventRelay>();
        if (relay != null)
        {
            relay.RegisterMurderousIntent(this);
            Debug.Log("[MurderousIntent] Registered with AnimationEventRelay");
        }
        else
        {
            Debug.LogError("[MurderousIntent] AnimationEventRelay not found!");
        }

        Debug.Log($"[MurderousIntent] Initialized with {damagePercent}% damage on thresholds");
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        float hpPercent = (float)thisEnemy.health / (float)thisEnemy.maxHealth;

        Debug.Log($"[MurderousIntent] Enemy HP: {thisEnemy.health}/{thisEnemy.maxHealth} ({hpPercent * 100}%)");

        if (!triggered75 && hpPercent <= THRESHOLD_75)
        {
            triggered75 = true;
            Debug.Log("[MurderousIntent] 75% threshold triggered!");
            yield return TriggerAttack(player);
        }
        else if (!triggered50 && hpPercent <= THRESHOLD_50)
        {
            triggered50 = true;
            Debug.Log("[MurderousIntent] 50% threshold triggered!");
            yield return TriggerAttack(player);
        }
        else if (!triggered25 && hpPercent <= THRESHOLD_25)
        {
            triggered25 = true;
            Debug.Log("[MurderousIntent] 25% threshold triggered!");
            yield return TriggerAttack(player);
        }

        yield return null;
    }

    private IEnumerator TriggerAttack(HandController player)
    {
        if (player == null)
        {
            Debug.LogError("[MurderousIntent] Player is null, cannot attack!");
            yield break;
        }

        // Calculate damage
        float averageDamage = (thisEnemy.rockDamage + thisEnemy.paperDamage + thisEnemy.scissorsDamage) / 3f;
        attackDamage = Mathf.RoundToInt(averageDamage * (damagePercent / 100f));

        Debug.Log($"[MurderousIntent] Attacking for {attackDamage} damage");

        // Cache for AnimationEvent callback
        targetPlayer = player;
        shouldDealDamage = true;

        // Trigger animation
        Animator enemyAnimator = thisEnemy.GetComponentInChildren<Animator>();
        if (enemyAnimator != null && enemyAnimator.HasParameter("MurderousAttack"))
        {
            Debug.Log("[MurderousIntent] Playing MurderousAttack animation");
            enemyAnimator.SetTrigger("MurderousAttack");

            // Wait for animation to finish via event
            bool animationFinished = false;
            HandController.MurderousAttackFinishedHandler callback = (hand) => animationFinished = true;
            thisEnemy.MurderousAttackFinished += callback;

            yield return new WaitUntil(() => animationFinished);

            thisEnemy.MurderousAttackFinished -= callback;
            Debug.Log("[MurderousIntent] Animation finished");
        }
        else
        {
            Debug.LogWarning("[MurderousIntent] No MurderousAttack parameter - using fallback");
            yield return new WaitForSeconds(1.0f);
        }

        yield return null;
    }

    // Called by AnimationEvent at the moment of impact
    public void OnMurderousAttack()
    {
        Debug.Log($"[MurderousIntent] OnMurderousAttack called! shouldDealDamage: {shouldDealDamage}, targetPlayer: {(targetPlayer != null ? targetPlayer.name : "null")}");

        if (!shouldDealDamage || targetPlayer == null)
        {
            Debug.LogWarning("[MurderousIntent] OnMurderousAttack called but no valid target!");
            return;
        }

        Debug.Log($"[MurderousIntent] Dealing {attackDamage} damage to player");
        targetPlayer.TakeDamage(attackDamage, thisEnemy);

        // Clear cache after use
        targetPlayer = null;
        shouldDealDamage = false;

        Debug.Log("[MurderousIntent] Damage dealt, cache cleared");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        yield break;
    }

    private void OnDestroy()
    {
        Debug.Log("[MurderousIntent] Destroyed");
    }
}