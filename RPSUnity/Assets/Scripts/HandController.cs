using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandController : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseMaxHealth = 30;
    public int baseDamage = 10;

    [Header("Runtime Stats")]
    public int health;
    public int maxHealth;

    public int CurrentHealth => health;

    public Animation playerHitAnimation;
    public Animator handAnimator;
    public SpriteRenderer handSpriteRenderer;

    public Sprite defaultHandSprite;
    public Sprite paperHandSprite;
    public Sprite scissorsHandSprite;

    public HealthBar healthBar;
    private string playerChoice;
    private bool isDying = false;
    private int temporaryBonusDamage = 0; // One-time damage boost from power-ups

    public bool isPlayer = false;

    [Header("Combat Text")]
    public GameObject combatTextPrefab;

    [Header("Rewards")]
    public int coinReward = 5;
    public int favorReward = 3;

    [Header("Prediction System")]
    [SerializeField] private bool usesPredictionSystem = false;
    [SerializeField] private bool hardMode = false;
    [SerializeField] private int sequenceLength = 3;
    [SerializeField] private List<string> predeterminedSequence = new List<string>();
    private int currentSequenceIndex = 0;
    private List<string> currentSequence = new List<string>();
    private string[] choices = { "Rock", "Paper", "Scissors" };

    public delegate void OnDeathHandler(HandController hand);
    public event OnDeathHandler OnDeath;

    public delegate void OnDeathAnimationFinishedHandler(HandController hand);
    public event OnDeathAnimationFinishedHandler OnDeathAnimationFinished;

    public delegate void SignAnimationFinishedHandler(HandController hand);
    public event SignAnimationFinishedHandler SignAnimationFinished;

    public delegate void PlayerDiedHandler();
    public static event PlayerDiedHandler OnPlayerDied;

    public delegate void HitAnimationFinishedHandler(HandController hand);
    public event HitAnimationFinishedHandler HitAnimationFinished;

    private void Awake()
    {
        // Initialize prediction sequence if this is an enemy
        if (!isPlayer && usesPredictionSystem)
        {
            GenerateNewSequence();
        }
    }

    void Start()
    {
        ApplyUpgrades();
        handSpriteRenderer.sprite = defaultHandSprite;
        UpdateHealthBar();
        // Remove the sequence generation from here
    }

    private void ApplyUpgrades()
    {
        if (isPlayer)
        {
            Debug.Log($"=== DAMAGE CALCULATION DEBUG ===");
            Debug.Log($"[START] baseDamage: {baseDamage}");
            Debug.Log($"[START] baseMaxHealth: {baseMaxHealth}");

            Debug.Log($"UpgradeManager.Instance exists: {UpgradeManager.Instance != null}");
            if (UpgradeManager.Instance != null)
            {
                Debug.Log($"UpgradeManager.GetBaseDamageBonus(): {UpgradeManager.Instance.GetBaseDamageBonus()}");
                Debug.Log($"UpgradeManager.GetMaxHealthBonus(): {UpgradeManager.Instance.GetMaxHealthBonus()}");
            }

            Debug.Log($"PlayerProgressData.baseDamageLevel: {PlayerProgressData.Instance.baseDamageLevel}");
            Debug.Log($"PlayerProgressData.maxHealthLevel: {PlayerProgressData.Instance.maxHealthLevel}");
            Debug.Log($"PlayerProgressData.bonusBaseDamage: {PlayerProgressData.Instance.bonusBaseDamage}");
            Debug.Log($"PlayerProgressData.bonusRockDamage: {PlayerProgressData.Instance.bonusRockDamage}");
            Debug.Log($"PlayerProgressData.bonusPaperDamage: {PlayerProgressData.Instance.bonusPaperDamage}");
            Debug.Log($"PlayerProgressData.bonusScissorsDamage: {PlayerProgressData.Instance.bonusScissorsDamage}");

            // Try to use the new upgrade system first
            if (UpgradeManager.Instance != null)
            {
                maxHealth = baseMaxHealth + UpgradeManager.Instance.GetMaxHealthBonus();
                baseDamage = baseDamage + UpgradeManager.Instance.GetBaseDamageBonus();
                Debug.Log($"[USING UPGRADE MANAGER] Applied UpgradeManager bonuses");
            }
            else
            {
                // Fallback to old hardcoded system
                maxHealth = baseMaxHealth + (PlayerProgressData.Instance.maxHealthLevel * 5);
                baseDamage += PlayerProgressData.Instance.baseDamageLevel * 2;
                Debug.Log($"[USING FALLBACK] Applied PlayerProgressData bonuses (baseDamageLevel * 2 = {PlayerProgressData.Instance.baseDamageLevel * 2})");
            }

            Debug.Log($"[FINAL] baseDamage: {baseDamage}");
            Debug.Log($"[FINAL] maxHealth: {maxHealth}");
            Debug.Log($"=== END DAMAGE DEBUG ===");
        }
        else
        {
            maxHealth = baseMaxHealth;
            Debug.Log($"[ENEMY] maxHealth set to baseMaxHealth: {maxHealth}");
        }

        health = maxHealth;
    }

    public void ApplyTemporaryDamageBoost(int amount)
    {
        temporaryBonusDamage += amount;
    }

    public int GetEffectiveDamage(string signUsed)
    {
        Debug.Log($"=== GET EFFECTIVE DAMAGE DEBUG ===");
        Debug.Log($"[START] baseDamage: {baseDamage}, signUsed: {signUsed}, isPlayer: {isPlayer}");

        int baseFinalDamage = baseDamage;

        if (isPlayer)
        {
            Debug.Log($"[BEFORE BONUSES] baseFinalDamage: {baseFinalDamage}");

            // Add persistent passive flat boosts
            int bonusBaseDamage = PlayerProgressData.Instance.bonusBaseDamage;
            baseFinalDamage += bonusBaseDamage;
            Debug.Log($"[BONUS BASE] Added bonusBaseDamage: {bonusBaseDamage}, new total: {baseFinalDamage}");

            int signSpecificBonus = 0;
            if (signUsed == "Rock")
            {
                signSpecificBonus = PlayerProgressData.Instance.bonusRockDamage;
                baseFinalDamage += signSpecificBonus;
                Debug.Log($"[BONUS ROCK] Added bonusRockDamage: {signSpecificBonus}, new total: {baseFinalDamage}");
            }
            else if (signUsed == "Paper")
            {
                signSpecificBonus = PlayerProgressData.Instance.bonusPaperDamage;
                baseFinalDamage += signSpecificBonus;
                Debug.Log($"[BONUS PAPER] Added bonusPaperDamage: {signSpecificBonus}, new total: {baseFinalDamage}");
            }
            else if (signUsed == "Scissors")
            {
                signSpecificBonus = PlayerProgressData.Instance.bonusScissorsDamage;
                baseFinalDamage += signSpecificBonus;
                Debug.Log($"[BONUS SCISSORS] Added bonusScissorsDamage: {signSpecificBonus}, new total: {baseFinalDamage}");
            }

            // NEW: Add flat damage bonuses from active power-ups (like The Gambler)
            int flatPowerUpBonus = 0;
            var effects = PowerUpEffectManager.Instance?.GetActiveEffects();
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    int bonus = effect.GetFlatDamageBonus(signUsed);
                    if (bonus > 0)
                    {
                        flatPowerUpBonus += bonus;
                        Debug.Log($"[FLAT POWERUP] {effect.GetType().Name} added {bonus} flat damage");
                    }
                }
            }
            baseFinalDamage += flatPowerUpBonus;
            Debug.Log($"[TOTAL FLAT BONUSES] Added {flatPowerUpBonus} from power-ups, new total: {baseFinalDamage}");

            // Start with 1.0f (100%) multiplier
            float multiplier = 1f;
            Debug.Log($"[MULTIPLIER START] multiplier: {multiplier}");

            // Let power-ups modify the multiplier (percentage-based effects only)
            ActivePowerUpHandler.GetModifiedMultiplier(ref multiplier, signUsed);
            Debug.Log($"[MULTIPLIER AFTER] multiplier: {multiplier}");

            // Calculate modified damage from base * multiplier
            int finalDamage = Mathf.RoundToInt(baseFinalDamage * multiplier);
            Debug.Log($"[AFTER MULTIPLIER] baseFinalDamage: {baseFinalDamage} * multiplier: {multiplier} = finalDamage: {finalDamage}");

            // Apply and clear one-time temporary bonus
            finalDamage += temporaryBonusDamage;
            if (temporaryBonusDamage > 0)
            {
                Debug.Log($"[HandController] Temporary bonus damage applied: +{temporaryBonusDamage}");
            }
            temporaryBonusDamage = 0;

            Debug.Log($"[FINAL RESULT] finalDamage: {finalDamage}");
            Debug.Log($"=== END GET EFFECTIVE DAMAGE DEBUG ===");

            return finalDamage;
        }

        Debug.Log($"[ENEMY DAMAGE] returning baseFinalDamage: {baseFinalDamage}");
        return baseFinalDamage;
    }

    public void TakeDamage(int damage, HandController source = null)
    {
        if (isDying) return;

        // If this is the player taking damage, check for damage reduction effects
        if (isPlayer && PowerUpEffectManager.Instance != null)
        {
            var effects = PowerUpEffectManager.Instance.GetActiveEffects();
            foreach (var effect in effects)
            {
                effect.ModifyIncomingDamage(ref damage, source);
            }
        }

        health -= damage;
        if (health < 0) health = 0;
        UpdateHealthBar();

        if (playerHitAnimation != null && damage > 0)
            playerHitAnimation.Play();

        if (combatTextPrefab != null && damage > 0)
            SpawnFloatingDamageText(damage);

        if (health <= 0)
        {
            StartCoroutine(HandleDeathWithDelay());
        }
    }

    private void SpawnFloatingDamageText(int amount)
    {
        GameObject instance = Instantiate(combatTextPrefab, transform.position, Quaternion.identity);
        var textComponent = instance.GetComponentInChildren<TMPro.TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = "-" + amount.ToString();
        }
    }

    private IEnumerator HandleDeathWithDelay()
    {
        yield return new WaitForSeconds(1.0f);
        Die();
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;

        Debug.Log($"{gameObject.name} has been defeated!");

        if (OnDeath != null)
        {
            OnDeath.Invoke(this);
            OnDeath = null;   // Important! Prevent double rewards
        }

        if (handAnimator != null && handAnimator.HasParameter("Die"))
        {
            handAnimator.SetTrigger("Die");
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            Debug.LogWarning($"Die trigger not found or animator is not assigned on {gameObject.name}");
            Destroy(gameObject);
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        yield return null;

        if (handAnimator != null)
        {
            AnimatorStateInfo stateInfo = handAnimator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName("Die") && stateInfo.normalizedTime < 1.0f)
            {
                yield return null;
                stateInfo = handAnimator.GetCurrentAnimatorStateInfo(0);
            }
        }

        Debug.Log($"{gameObject.name} death animation finished.");
        OnDeathAnimationFinished?.Invoke(this);

        if (isPlayer)
        {
            Debug.Log("Player death detected. Triggering OnPlayerDied.");
            OnPlayerDied?.Invoke();
        }

        Destroy(gameObject);
    }

    public void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(health, maxHealth);
        }
    }

    public void StartShaking(string choice)
    {
        if (isDying) return;
        ResetHandToDefault();
        handAnimator.SetTrigger("Shake");
        playerChoice = choice;
        Invoke(nameof(ChangeHandState), 1.0f);
    }

    private void ChangeHandState()
    {
        if (isDying) return;

        if (playerChoice == "Paper")
        {
            handAnimator.SetTrigger("ChoosePaper");
        }
        else if (playerChoice == "Scissors")
        {
            handAnimator.SetTrigger("ChooseScissors");
        }
        else
        {
            handSpriteRenderer.sprite = defaultHandSprite;
        }
    }

    private void ResetHandToDefault()
    {
        handSpriteRenderer.sprite = defaultHandSprite;
        handAnimator.Rebind();
    }

    public void SelectRock() => StartShaking("Rock");
    public void SelectPaper() => StartShaking("Paper");
    public void SelectScissors() => StartShaking("Scissors");

    public void OnSignAnimationFinished()
    {
        if (isDying) return;
        Debug.Log($"{gameObject.name} sign animation finished!");
        SignAnimationFinished?.Invoke(this);
    }

    public void TriggerDeathAnimationFinished()
    {
        Debug.Log($"{gameObject.name} death animation event triggered.");
        OnDeathAnimationFinished?.Invoke(this);
    }

    public void OnHitAnimationFinished()
    {
        if (isDying) return;
        Debug.Log($"{gameObject.name} hit animation finished!");
        HitAnimationFinished?.Invoke(this);
    }

    // Prediction System Methods
    private void GenerateNewSequence()
    {
        if (hardMode || !usesPredictionSystem)
        {
            // Hard mode doesn't use sequences
            return;
        }

        currentSequence.Clear();
        currentSequenceIndex = 0;

        // Generate sequence with variety rules
        bool validSequence = false;
        int attempts = 0;
        const int maxAttempts = 100; // Prevent infinite loops

        while (!validSequence && attempts < maxAttempts)
        {
            attempts++;
            currentSequence.Clear();

            // Generate random sequence
            for (int i = 0; i < sequenceLength; i++)
            {
                currentSequence.Add(choices[Random.Range(0, choices.Length)]);
            }

            // Check if sequence meets variety requirements
            validSequence = IsSequenceValid(currentSequence);
        }

        // Copy for display (this will be shuffled in the UI)
        predeterminedSequence = new List<string>(currentSequence);

        Debug.Log($"[HandController] Generated new sequence for {gameObject.name}: {string.Join(", ", currentSequence)}. Index reset to 0.");
    }

    private bool IsSequenceValid(List<string> sequence)
    {
        if (sequence.Count < 2) return true;

        // Count occurrences of each sign
        Dictionary<string, int> signCounts = new Dictionary<string, int>();
        foreach (string sign in sequence)
        {
            if (!signCounts.ContainsKey(sign))
                signCounts[sign] = 0;
            signCounts[sign]++;
        }

        // Find the maximum count of any single sign
        int maxCount = 0;
        foreach (var count in signCounts.Values)
        {
            if (count > maxCount)
                maxCount = count;
        }

        // Apply rules based on sequence length
        switch (sequence.Count)
        {
            case 2:
                return maxCount < 2; // No duplicates allowed
            case 3:
                return maxCount < 3; // No triple identical signs
            case 4:
                return maxCount < 4; // No quad identical signs
            case 5:
                return maxCount < 4; // No quad or quint identical signs
            default:
                return true;
        }
    }

    public string GetNextPredeterminedChoice()
    {
        if (!usesPredictionSystem || isPlayer)
        {
            // Player or non-prediction enemies use random
            return choices[Random.Range(0, choices.Length)];
        }

        if (hardMode)
        {
            // Hard mode is truly random
            return choices[Random.Range(0, choices.Length)];
        }

        // Use predetermined sequence
        if (currentSequence.Count == 0 || currentSequenceIndex >= currentSequence.Count)
        {
            Debug.Log($"[HandController] Sequence exhausted for {gameObject.name}. Generating new sequence...");
            GenerateNewSequence();
        }

        string choice = currentSequence[currentSequenceIndex];
        currentSequenceIndex++;

        Debug.Log($"[HandController] {gameObject.name} playing sign {currentSequenceIndex}/{currentSequence.Count}: {choice}");

        return choice;
    }

    public List<string> GetCurrentPredictionSequence()
    {
        if (!usesPredictionSystem || hardMode || isPlayer)
        {
            return null;
        }
        return new List<string>(predeterminedSequence);
    }

    public int GetCurrentSequenceIndex()
    {
        return currentSequenceIndex;
    }

    public bool UsesPredictionSystem()
    {
        return usesPredictionSystem && !hardMode && !isPlayer;
    }

    public void ForceNewSequenceIfNeeded()
    {
        if (!usesPredictionSystem || isPlayer || hardMode)
            return;

        // If we've used all signs, generate a new sequence
        if (currentSequenceIndex >= currentSequence.Count && currentSequence.Count > 0)
        {
            Debug.Log($"[HandController] Forcing new sequence for {gameObject.name} (used {currentSequenceIndex}/{currentSequence.Count})");
            GenerateNewSequence();
        }
    }
}

// Extension method to check if a parameter exists in the Animator
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}