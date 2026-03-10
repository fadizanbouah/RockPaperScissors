using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindTricksBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Mind Tricks Configuration")]
    [SerializeField] private float triggerChance = 30f; // % chance to trigger
    [SerializeField] private float damageModifierPercent = 30f; // % damage bonus/penalty

    [Header("Visual")]
    [SerializeField] private GameObject thoughtBubblePrefab;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 2f, 0); // Position above enemy

    private HandController thisEnemy;
    private GameObject currentBubble;
    private string bluffSign; // The sign shown in the bubble
    private bool mindTricksActive = false;

    public void Initialize(HandController enemy, float[] configValues)
    {
        thisEnemy = enemy;

        // configValues[0] = triggerChance
        // configValues[1] = damageModifierPercent
        if (configValues != null)
        {
            if (configValues.Length > 0)
                triggerChance = configValues[0];

            if (configValues.Length > 1)
                damageModifierPercent = configValues[1];
        }

        Debug.Log($"[MindTricksBehavior] Initialized with {triggerChance}% trigger chance, {damageModifierPercent}% damage modifier");
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        // Clean up any existing bubble from previous round
        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }

        mindTricksActive = false;

        // Roll to see if Mind Tricks triggers
        float roll = Random.Range(0f, 100f);

        if (roll < triggerChance)
        {
            Debug.Log($"[MindTricksBehavior] Mind Tricks triggered! (rolled {roll} < {triggerChance})");

            // Get available signs from prediction
            List<string> availableSigns = GetAvailableSigns();

            // Check fail conditions
            if (availableSigns.Count <= 1)
            {
                Debug.Log("[MindTricksBehavior] Only 1 or fewer signs available - skipping");
                yield break;
            }

            if (AreAllSignsSame(availableSigns))
            {
                Debug.Log("[MindTricksBehavior] All signs are the same - skipping");
                yield break;
            }

            // Pick random sign to show in bubble
            bluffSign = availableSigns[Random.Range(0, availableSigns.Count)];
            Debug.Log($"[MindTricksBehavior] Showing bluff sign: {bluffSign}");

            // Show thought bubble
            yield return ShowThoughtBubble(bluffSign);

            mindTricksActive = true;
        }
        else
        {
            Debug.Log($"[MindTricksBehavior] Mind Tricks did not trigger (rolled {roll} >= {triggerChance})");
        }

        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        if (!mindTricksActive)
        {
            yield break;
        }

        // Determine if player countered or fell for the bluff
        string counterSign = GetCounterSign(bluffSign);
        string losesSign = GetLosesSign(bluffSign);

        if (playerChoice == counterSign)
        {
            // Player countered the bluff - BONUS damage to enemy
            Debug.Log($"[MindTricksBehavior] Player countered the bluff! Bonus damage to enemy.");

            int bonusDamage = CalculateBonusDamage(playerChoice);
            if (bonusDamage > 0)
            {
                thisEnemy.TakeDamage(bonusDamage, player);
                Debug.Log($"[MindTricksBehavior] Dealt {bonusDamage} bonus damage to enemy");
            }
        }
        else if (playerChoice == losesSign)
        {
            // Player fell for the bluff - EXTRA damage to player
            Debug.Log($"[MindTricksBehavior] Player fell for the bluff! Extra damage to player.");

            int penaltyDamage = CalculatePenaltyDamage(enemyChoice);
            if (penaltyDamage > 0)
            {
                player.TakeDamage(penaltyDamage, thisEnemy);
                Debug.Log($"[MindTricksBehavior] Dealt {penaltyDamage} penalty damage to player");
            }
        }
        else
        {
            // Tie - no modifier
            Debug.Log($"[MindTricksBehavior] Player tied with bluff - no modifier");
        }

        // Clean up bubble
        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }

        mindTricksActive = false;

        yield return null;
    }

    private IEnumerator ShowThoughtBubble(string sign)
    {
        if (thoughtBubblePrefab == null)
        {
            Debug.LogWarning("[MindTricksBehavior] No thought bubble prefab assigned!");
            yield break;
        }

        // Spawn bubble above enemy
        currentBubble = Instantiate(thoughtBubblePrefab, thisEnemy.transform.position + bubbleOffset, Quaternion.identity);

        // Parent to enemy so it moves with them
        currentBubble.transform.SetParent(thisEnemy.transform);

        // Get the sprite for this sign from the enemy's hand
        Sprite signSprite = GetSpriteForSign(sign);

        // Find and set the hand sprite directly (recursive search for nested structure)
        SpriteRenderer handSpriteRenderer = null;
        Transform[] allChildren = currentBubble.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "HandSignSprite")
            {
                handSpriteRenderer = child.GetComponent<SpriteRenderer>();
                break;
            }
        }

        if (handSpriteRenderer != null && signSprite != null)
        {
            handSpriteRenderer.sprite = signSprite;
            Debug.Log($"[MindTricksBehavior] Set thought bubble sprite to {signSprite.name}");
        }
        else
        {
            if (handSpriteRenderer == null)
                Debug.LogWarning("[MindTricksBehavior] Could not find HandSignSprite child!");
            if (signSprite == null)
                Debug.LogWarning("[MindTricksBehavior] Sign sprite is null!");
        }

        // Wait a moment for bubble to appear
        yield return new WaitForSeconds(0.3f);
    }

    private int CalculateBonusDamage(string playerChoice)
    {
        // Calculate bonus damage based on what the player used
        int baseDamage = 0;

        switch (playerChoice)
        {
            case "Rock":
                baseDamage = thisEnemy.rockDamage;
                break;
            case "Paper":
                baseDamage = thisEnemy.paperDamage;
                break;
            case "Scissors":
                baseDamage = thisEnemy.scissorsDamage;
                break;
        }

        int bonus = Mathf.RoundToInt(baseDamage * (damageModifierPercent / 100f));
        return bonus;
    }

    private int CalculatePenaltyDamage(string enemyChoice)
    {
        // Calculate penalty damage based on what the enemy used
        int baseDamage = 0;

        switch (enemyChoice)
        {
            case "Rock":
                baseDamage = thisEnemy.rockDamage;
                break;
            case "Paper":
                baseDamage = thisEnemy.paperDamage;
                break;
            case "Scissors":
                baseDamage = thisEnemy.scissorsDamage;
                break;
        }

        int penalty = Mathf.RoundToInt(baseDamage * (damageModifierPercent / 100f));
        return penalty;
    }

    private Sprite GetSpriteForSign(string sign)
    {
        // Use HandController's existing method (same as prediction UI uses)
        string handName = sign + "Hand"; // e.g., "RockHand"
        Sprite sprite = thisEnemy.GetSpriteFromHandObject(handName);

        if (sprite != null)
        {
            Debug.Log($"[MindTricksBehavior] Found sprite for {sign}: {sprite.name}");
        }
        else
        {
            Debug.LogWarning($"[MindTricksBehavior] Could not find sprite for {sign}");
        }

        return sprite;
    }

    private List<string> GetAvailableSigns()
    {
        if (thisEnemy == null)
        {
            return new List<string>();
        }

        return thisEnemy.GetRemainingSequence();
    }

    private bool AreAllSignsSame(List<string> signs)
    {
        if (signs.Count == 0) return true;

        string firstSign = signs[0];
        foreach (string sign in signs)
        {
            if (sign != firstSign)
            {
                return false;
            }
        }
        return true;
    }

    private string GetCounterSign(string sign)
    {
        switch (sign)
        {
            case "Rock": return "Paper";
            case "Paper": return "Scissors";
            case "Scissors": return "Rock";
            default: return "";
        }
    }

    private string GetLosesSign(string sign)
    {
        switch (sign)
        {
            case "Rock": return "Scissors";
            case "Paper": return "Rock";
            case "Scissors": return "Paper";
            default: return "";
        }
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        // Clean up bubble if enemy dies
        if (currentBubble != null)
        {
            Destroy(currentBubble);
            currentBubble = null;
        }

        yield break;
    }

    private void OnDestroy()
    {
        if (currentBubble != null)
        {
            Destroy(currentBubble);
        }
        Debug.Log("[MindTricksBehavior] Destroyed");
    }
}