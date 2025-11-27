using System.Collections;
using UnityEngine;

public class HouseRuleBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Pattern Detection")]
    [SerializeField] private int requiredConsecutiveSigns = 3; // X times in a row

    [Header("Damage Punishment")]
    [Tooltip("% of player's max HP dealt as damage (e.g., 20 = 20%)")]
    [SerializeField] private float damagePercent = 20f;

    private HandController enemyHand;
    private HandController playerHand;

    // Tracking state
    private string lastPlayerSign = "";
    private int consecutiveCount = 0;

    public void Initialize(HandController enemy, float[] configValues)
    {
        enemyHand = enemy;

        // configValues[0] = requiredConsecutiveSigns
        // configValues[1] = damagePercent
        if (configValues != null && configValues.Length > 0)
        {
            if (configValues.Length > 0)
                requiredConsecutiveSigns = Mathf.RoundToInt(configValues[0]);

            if (configValues.Length > 1)
                damagePercent = configValues[1];
        }

        Debug.Log($"[HouseRuleBehavior] Initialized with {requiredConsecutiveSigns} consecutive signs required, dealing {damagePercent}% damage");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // Store player reference for later use
        if (playerHand == null)
        {
            playerHand = player;
        }

        // Track the player's sign pattern
        if (string.IsNullOrEmpty(lastPlayerSign))
        {
            // First sign of the battle
            lastPlayerSign = playerChoice;
            consecutiveCount = 1;
            Debug.Log($"[HouseRuleBehavior] First sign tracked: {playerChoice}. Count: {consecutiveCount}");
        }
        else if (playerChoice == lastPlayerSign)
        {
            consecutiveCount++;
            Debug.Log($"[HouseRuleBehavior] Player repeated {playerChoice}! Count: {consecutiveCount}/{requiredConsecutiveSigns}");
        }
        else
        {
            // Different sign - reset the counter
            Debug.Log($"[HouseRuleBehavior] Player switched from {lastPlayerSign} to {playerChoice}. Resetting counter.");
            consecutiveCount = 1;
            lastPlayerSign = playerChoice;
        }

        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        // Check if enemy is dead before punishing
        if (enemyHand == null || enemyHand.CurrentHealth <= 0)
        {
            Debug.Log("[HouseRuleBehavior] Enemy is dead - skipping punishment");
            yield break;
        }

        // NEW: Check if player is dead before punishing
        if (player == null || player.CurrentHealth <= 0)
        {
            Debug.Log("[HouseRuleBehavior] Player is dead - skipping punishment");
            yield break;
        }

        // Check if player has triggered the punishment
        if (consecutiveCount >= requiredConsecutiveSigns)
        {
            Debug.Log($"[HouseRuleBehavior] House rule violated! Player used {lastPlayerSign} {consecutiveCount} times in a row.");
            // Play punishment animation
            yield return PlayPunishmentAnimation();
            // Apply the damage punishment
            ApplyDamagePunishment(player);
            // NEW: Wait for the Hit animation to complete
            yield return WaitForHitAnimation(player);
            // Reset counter after punishment
            consecutiveCount = 0;
            lastPlayerSign = "";
            Debug.Log("[HouseRuleBehavior] Counter reset after punishment");
        }
        yield return null;
    }

    // NEW: Helper method to wait for Hit animation
    private IEnumerator WaitForHitAnimation(HandController player)
    {
        if (player == null || player.handAnimator == null)
        {
            yield break;
        }

        bool hitAnimationFinished = false;

        HandController.HitAnimationFinishedHandler callback = (hand) => hitAnimationFinished = true;
        player.HitAnimationFinished += callback;

        // Wait for Hit animation to complete (with timeout for safety)
        float timeout = 2f;
        float elapsed = 0f;

        while (!hitAnimationFinished && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.HitAnimationFinished -= callback;

        Debug.Log("[HouseRuleBehavior] Hit animation completed");
    }

    private IEnumerator PlayPunishmentAnimation()
    {
        // Play animation on the PLAYER, not the enemy
        if (playerHand != null && playerHand.handAnimator != null)
        {
            Animator animator = playerHand.handAnimator;

            if (animator.HasParameter("HouseRulesHammer"))
            {
                Debug.Log("[HouseRuleBehavior] Playing HouseRulesHammer animation on player");
                animator.SetTrigger("HouseRulesHammer");

                // Wait for animation event callback
                bool animationFinished = false;

                // Create the callback with the correct delegate signature
                HandController.HouseRulesAnimationFinishedHandler callback = (hand) => animationFinished = true;

                playerHand.HouseRulesAnimationFinished += callback;

                yield return new WaitUntil(() => animationFinished);

                playerHand.HouseRulesAnimationFinished -= callback;
            }
            else
            {
                Debug.Log("[HouseRuleBehavior] No HouseRulesHammer animation parameter found - using placeholder delay");
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ApplyDamagePunishment(HandController player)
    {
        if (player == null)
        {
            Debug.LogWarning("[HouseRuleBehavior] Cannot apply punishment - player is null!");
            return;
        }

        // Calculate damage as percentage of player's max HP
        int damage = Mathf.RoundToInt(player.maxHealth * (damagePercent / 100f));
        damage = Mathf.Max(1, damage); // At least 1 damage

        Debug.Log($"[HouseRuleBehavior] Dealing {damage} damage to player ({damagePercent}% of {player.maxHealth} max HP)");

        player.TakeDamage(damage, enemyHand);
    }

    private void OnDestroy()
    {
        // Clean up if needed
        Debug.Log("[HouseRuleBehavior] Destroyed");
    }
}