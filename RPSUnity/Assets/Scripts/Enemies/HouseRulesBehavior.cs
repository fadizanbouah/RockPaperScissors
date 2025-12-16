using System.Collections;
using UnityEngine;

public class HouseRuleBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Pattern Detection")]
    [SerializeField] private int requiredConsecutiveSigns = 3; // X times in a row

    [Header("Damage Punishment")]
    [Tooltip("% of player's max HP dealt as damage (e.g., 20 = 20%)")]
    [SerializeField] private float damagePercent = 20f;

    [Header("Visual Effect")]
    [Tooltip("VFX prefab that plays when House Rules triggers")]
    [SerializeField] private GameObject houseRulesVFXPrefab;

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

        // Check if player is dead before punishing
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

            // Wait for the Hit animation to complete
            yield return WaitForHitAnimation(player);

            // Reset counter after punishment
            consecutiveCount = 0;
            lastPlayerSign = "";
            Debug.Log("[HouseRuleBehavior] Counter reset after punishment");
        }

        yield return null;
    }

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
        if (houseRulesVFXPrefab != null && playerHand != null)
        {
            Debug.Log("[HouseRuleBehavior] Spawning House Rules VFX prefab");

            // Spawn the effect at player's position
            GameObject vfx = Instantiate(houseRulesVFXPrefab, playerHand.transform.position, Quaternion.identity);

            // Parent it to the player so it moves with them
            vfx.transform.SetParent(playerHand.transform);

            // Get the animator from the VFX prefab
            Animator vfxAnimator = vfx.GetComponent<Animator>();

            if (vfxAnimator != null)
            {
                // Wait one frame for animator to initialize
                yield return null;

                // Wait for the animation to finish
                AnimatorStateInfo stateInfo = vfxAnimator.GetCurrentAnimatorStateInfo(0);
                float timeout = 3f;
                float elapsed = 0f;

                while (stateInfo.normalizedTime < 1.0f && elapsed < timeout)
                {
                    yield return null;
                    stateInfo = vfxAnimator.GetCurrentAnimatorStateInfo(0);
                    elapsed += Time.deltaTime;
                }

                if (elapsed >= timeout)
                {
                    Debug.LogWarning("[HouseRuleBehavior] VFX animation timed out!");
                }
            }
            else
            {
                // No animator found, just wait a bit
                Debug.LogWarning("[HouseRuleBehavior] VFX prefab has no Animator - using fallback delay");
                yield return new WaitForSeconds(1f);
            }

            // Clean up the VFX
            Destroy(vfx);
        }
        else
        {
            // No VFX prefab assigned - just use a simple delay
            Debug.Log("[HouseRuleBehavior] No VFX prefab assigned - using delay");
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