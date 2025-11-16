using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandController : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseMaxHealth = 30;
    // Keep baseDamage for backward compatibility, but hide it
    [HideInInspector]
    public int baseDamage = 10;

    [Header("Sign-Specific Damage")]
    public int rockDamage = 10;
    public int paperDamage = 10;
    public int scissorsDamage = 10;

    [HideInInspector] public int baseRockDamage;
    [HideInInspector] public int basePaperDamage;
    [HideInInspector] public int baseScissorsDamage;

    [Header("Runtime Stats")]
    public int health;
    public int maxHealth;

    [Header("Dodge Stats")]
    [Range(0f, 100f)]
    public float dodgeChance = 0f; // Percentage chance to dodge (0-100)
    public bool canDodge = true; // Can be disabled for certain enemies or conditions

    [Header("Critical Hit Stats")]
    [Range(0f, 100f)]
    public float critChance = 10f; // Percentage chance to crit (0-100)
    public float critDamageMultiplier = 2.0f; // 2x damage on crit (200%)

    public int CurrentHealth => health;
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
    public GameObject combatTextCritPrefab;

    [Header("Rewards")]
    public int coinReward = 5;
    public int favorReward = 3;

    [Header("Prediction System")]
    [SerializeField] private bool usesPredictionSystem = false;
    [SerializeField] private bool hardMode = false;
    [SerializeField] private int sequenceLength = 3;
    [SerializeField] private List<string> predeterminedSequence = new List<string>();

    [Header("Sign Shuffle System")]
    [SerializeField] private bool useSignShuffle = false;
    [SerializeField] private bool shuffleAfter1 = false;
    [SerializeField] private bool shuffleAfter2 = false;
    [SerializeField] private bool shuffleAfter3 = false;
    [SerializeField] private bool shuffleAfter4 = false;
    [SerializeField] private bool shuffleAfter5 = false;

    private int currentSequenceIndex = 0;
    private List<string> currentSequence = new List<string>();
    private string[] choices = { "Rock", "Paper", "Scissors" };

    private int roundsUntilShuffle = 0;
    private int roundsSinceLastShuffle = 0;
    private List<int> possibleShufflePoints = new List<int>();
    private bool shufflePendingAfterRound = false;

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

    public delegate void DodgeHandler(HandController hand);
    public event DodgeHandler OnDodge;

    public delegate void CheatDeathAnimationFinishedHandler(HandController hand);
    public event CheatDeathAnimationFinishedHandler CheatDeathAnimationFinished;

    public delegate void StealAnimationFinishedHandler(HandController hand);
    public event StealAnimationFinishedHandler StealAnimationFinished;

    public delegate bool CheatDeathCheckHandler(HandController hand);
    public event CheatDeathCheckHandler OnCheatDeathCheck;

    private void Awake()
    {
        // Store base values before any modifications
        baseRockDamage = rockDamage;
        basePaperDamage = paperDamage;
        baseScissorsDamage = scissorsDamage;

        // Initialize prediction sequence if this is an enemy
        if (!isPlayer && usesPredictionSystem)
        {
            GenerateNewSequence();
            InitializeSignShuffle();
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
            Debug.Log($"[START] rockDamage: {rockDamage}, paperDamage: {paperDamage}, scissorsDamage: {scissorsDamage}");
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

            // Apply upgrades to health
            if (UpgradeManager.Instance != null)
            {
                maxHealth = baseMaxHealth + UpgradeManager.Instance.GetMaxHealthBonus();
                // Apply base damage upgrade to all sign types
                int damageUpgrade = UpgradeManager.Instance.GetBaseDamageBonus();
                rockDamage += damageUpgrade;
                paperDamage += damageUpgrade;
                scissorsDamage += damageUpgrade;
                Debug.Log($"[USING UPGRADE MANAGER] Applied UpgradeManager bonuses");
            }
            else
            {
                // Fallback to old hardcoded system
                maxHealth = baseMaxHealth + (PlayerProgressData.Instance.maxHealthLevel * 5);
                int damageUpgrade = PlayerProgressData.Instance.baseDamageLevel * 2;
                rockDamage += damageUpgrade;
                paperDamage += damageUpgrade;
                scissorsDamage += damageUpgrade;
                Debug.Log($"[USING FALLBACK] Applied PlayerProgressData bonuses (baseDamageLevel * 2 = {damageUpgrade})");
            }

            Debug.Log($"[FINAL] rockDamage: {rockDamage}, paperDamage: {paperDamage}, scissorsDamage: {scissorsDamage}");
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

    public int GetEffectiveDamage(string signUsed, out bool isCriticalHit)
    {
        isCriticalHit = false;

        // Get base damage for the specific sign
        int baseDamageForSign = signUsed switch
        {
            "Rock" => rockDamage,
            "Paper" => paperDamage,
            "Scissors" => scissorsDamage,
            _ => rockDamage
        };

        int baseFinalDamage = baseDamageForSign;
        int finalDamage = baseFinalDamage;

        if (isPlayer)
        {
            // Add persistent passive flat boosts (player only)
            int bonusBaseDamage = PlayerProgressData.Instance.bonusBaseDamage;
            baseFinalDamage += bonusBaseDamage;

            int signSpecificBonus = 0;
            if (signUsed == "Rock")
            {
                signSpecificBonus = PlayerProgressData.Instance.bonusRockDamage;
                baseFinalDamage += signSpecificBonus;
            }
            else if (signUsed == "Paper")
            {
                signSpecificBonus = PlayerProgressData.Instance.bonusPaperDamage;
                baseFinalDamage += signSpecificBonus;
            }
            else if (signUsed == "Scissors")
            {
                signSpecificBonus = PlayerProgressData.Instance.bonusScissorsDamage;
                baseFinalDamage += signSpecificBonus;
            }

            // Add flat damage bonuses from active power-ups
            int flatPowerUpBonus = 0;
            var effects = PowerUpEffectManager.Instance?.GetActiveEffects();
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    // Only apply if this effect belongs to THIS hand controller
                    if (IsEffectOwnedByThis(effect))
                    {
                        int bonus = effect.GetFlatDamageBonus(signUsed);
                        if (bonus > 0)
                        {
                            flatPowerUpBonus += bonus;
                        }
                    }
                }
            }
            baseFinalDamage += flatPowerUpBonus;
        }

        // Apply multipliers for everyone (player or enemy)
        float multiplier = 1f;
        var allEffects = PowerUpEffectManager.Instance?.GetActiveEffects();
        if (allEffects != null)
        {
            foreach (var effect in allEffects)
            {
                // Only apply effects owned by THIS hand controller
                if (IsEffectOwnedByThis(effect))
                {
                    effect.ModifyDamageMultiplier(ref multiplier, signUsed);
                }
            }
        }

        // Calculate modified damage from base * multiplier
        finalDamage = Mathf.RoundToInt(baseFinalDamage * multiplier);

        // Apply temporary bonus (player only)
        if (isPlayer)
        {
            finalDamage += temporaryBonusDamage;
            temporaryBonusDamage = 0;
        }

        // Check for critical hit (works for both player and enemy)
        float critRoll = Random.Range(0f, 100f);
        if (critRoll < critChance)
        {
            isCriticalHit = true;
            finalDamage = Mathf.RoundToInt(finalDamage * critDamageMultiplier);
            Debug.Log($"[{gameObject.name} CRITICAL HIT!] {critRoll:F1} < {critChance:F1}% - Damage: {finalDamage}");
        }

        return finalDamage;
    }

    private bool IsEffectOwnedByThis(PowerUpEffectBase effect)
    {
        if (effect == null) return false;

        // Use reflection to get the protected "player" field
        var playerField = typeof(PowerUpEffectBase).GetField("player",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (playerField != null)
        {
            HandController effectOwner = playerField.GetValue(effect) as HandController;
            return effectOwner == this;
        }

        return false;
    }

    public void TakeDamage(int damage, HandController source = null, bool isCriticalHit = false)
    {
        if (isDying) return;

        // Check for dodge BEFORE applying any damage
        if (damage > 0 && canDodge && dodgeChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll < dodgeChance)
            {
                Debug.Log($"{gameObject.name} dodged the attack! (Roll: {roll:F1} < Dodge: {dodgeChance:F1}%)");
                OnDodge?.Invoke(this);
                TriggerDodgeAnimation();
                return;
            }
        }

        // MODIFIED: Check for damage reduction effects that belong to THIS hand controller
        if (PowerUpEffectManager.Instance != null)
        {
            var effects = PowerUpEffectManager.Instance.GetActiveEffects();
            foreach (var effect in effects)
            {
                // Only apply damage reduction if this effect belongs to THIS hand controller
                if (IsEffectOwnedByThis(effect))
                {
                    effect.ModifyIncomingDamage(ref damage, source);
                }
            }
        }

        health -= damage;

        // Check for Cheat Death (player only)
        if (health <= 0 && isPlayer)
        {
            Debug.Log($"[TakeDamage] Health <= 0 detected. Health before clamp: {health}");

            bool cheatDeathTriggered = TryTriggerCheatDeath();

            Debug.Log($"[TakeDamage] CheatDeath triggered: {cheatDeathTriggered}");

            if (cheatDeathTriggered)
            {
                Debug.Log("[TakeDamage] CheatDeath saved player! Skipping Hit animation and returning early.");
                UpdateHealthBar();

                if (combatTextPrefab != null && damage > 0)
                    SpawnFloatingDamageText(damage, isCriticalHit);

                return;
            }

            Debug.Log("[TakeDamage] CheatDeath did not trigger, continuing with normal death flow.");
        }

        // Clamp health
        if (health < 0) health = 0;
        UpdateHealthBar();

        // Play Hit animation
        if (handAnimator != null && handAnimator.HasParameter("Hit") && damage > 0)
        {
            Debug.Log($"[TakeDamage] Playing Hit animation. isDying: {isDying}, damage: {damage}");
            handAnimator.SetTrigger("Hit");
            handAnimator.Update(0f);
        }

        // Show damage text
        if (combatTextPrefab != null && damage > 0)
            SpawnFloatingDamageText(damage, isCriticalHit);

        // Handle death
        if (health <= 0)
        {
            StartCoroutine(HandleDeathWithDelay());
        }
    }

    //Check if Cheat Death can trigger
    private bool TryTriggerCheatDeath()
    {
        // Fire a special event that Cheat Death can listen to
        if (OnCheatDeathCheck != null)
        {
            // IMPORTANT: Invoke each subscriber individually and stop on first true
            foreach (CheatDeathCheckHandler handler in OnCheatDeathCheck.GetInvocationList())
            {
                bool result = handler.Invoke(this);
                if (result)
                {
                    // First handler that returns true means CheatDeath triggered
                    Debug.Log("[TryTriggerCheatDeath] CheatDeath triggered by a handler!");
                    return true;
                }
            }
        }

        Debug.Log("[TryTriggerCheatDeath] No CheatDeath handler triggered");
        return false; // No Cheat Death active or none triggered
    }

    private void SpawnFloatingDamageText(int amount, bool isCriticalHit = false)
    {
        GameObject prefabToUse = isCriticalHit ? combatTextCritPrefab : combatTextPrefab;

        if (prefabToUse == null)
        {
            Debug.LogWarning($"[SpawnFloatingDamageText] Missing prefab - Crit: {isCriticalHit}");
            return;
        }

        GameObject instance = Instantiate(prefabToUse, transform.position, Quaternion.identity);
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

    public void OnCheatDeathAnimationFinished()
    {
        Debug.Log($"{gameObject.name} cheat death animation finished!");
        CheatDeathAnimationFinished?.Invoke(this);

        // IMPORTANT: Also trigger HitAnimationFinished so the game flow continues
        // This makes CheatDeath animation work the same as Hit animation for game flow
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
            return choices[Random.Range(0, choices.Length)];
        }

        if (hardMode)
        {
            return choices[Random.Range(0, choices.Length)];
        }

        // Check if we need to shuffle AFTER this round (not immediately)
        if (UsesSignShuffle())
        {
            roundsSinceLastShuffle++;

            // Mark for shuffle after round completes (not now!)
            if (roundsSinceLastShuffle >= roundsUntilShuffle)
            {
                Debug.Log($"[SignShuffle] Shuffle pending after this round (round {roundsSinceLastShuffle}/{roundsUntilShuffle})");
                shufflePendingAfterRound = true;
            }
        }

        // Continue with normal sequence logic
        if (currentSequence.Count == 0 || currentSequenceIndex >= currentSequence.Count)
        {
            // Only generate new sequence if NOT using sign shuffle
            // (sign shuffle handles its own sequence generation)
            if (!UsesSignShuffle())
            {
                Debug.Log($"[HandController] Sequence exhausted for {gameObject.name}. Generating new sequence...");
                GenerateNewSequence();
            }
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

    private void InitializeSignShuffle()
    {
        if (!useSignShuffle || isPlayer) return;

        // Build list of possible shuffle points
        possibleShufflePoints.Clear();
        if (shuffleAfter1) possibleShufflePoints.Add(1);
        if (shuffleAfter2) possibleShufflePoints.Add(2);
        if (shuffleAfter3) possibleShufflePoints.Add(3);
        if (shuffleAfter4) possibleShufflePoints.Add(4);
        if (shuffleAfter5) possibleShufflePoints.Add(5);

        // If no options selected, default to playing all signs
        if (possibleShufflePoints.Count == 0)
        {
            useSignShuffle = false;
            Debug.LogWarning($"[SignShuffle] No shuffle points selected for {gameObject.name}, disabling shuffle");
            return;
        }

        // Pick initial shuffle point
        SelectNextShufflePoint();
        roundsSinceLastShuffle = 0;

        Debug.Log($"[SignShuffle] Initialized for {gameObject.name} with shuffle points: {string.Join(", ", possibleShufflePoints)}");
    }

    private void SelectNextShufflePoint()
    {
        if (possibleShufflePoints.Count == 0) return;

        int randomIndex = Random.Range(0, possibleShufflePoints.Count);
        roundsUntilShuffle = possibleShufflePoints[randomIndex];

        Debug.Log($"[SignShuffle] Next shuffle in {roundsUntilShuffle} rounds");
    }

    public bool UsesSignShuffle()
    {
        return useSignShuffle && !isPlayer && possibleShufflePoints.Count > 0;
    }

    public int GetRoundsUntilShuffle()
    {
        if (!UsesSignShuffle()) return -1;
        return roundsUntilShuffle - roundsSinceLastShuffle;
    }

    public void OnRoundComplete()
    {
        if (!UsesSignShuffle()) return;

        // Don't shuffle if enemy is dying or dead
        if (isDying || health <= 0)
        {
            Debug.Log($"[SignShuffle] Skipping shuffle - enemy is dying/dead");
            return;
        }

        // Check if shuffle was pending
        if (shufflePendingAfterRound)
        {
            Debug.Log($"[SignShuffle] Executing pending shuffle after round complete");

            // Generate new sequence
            GenerateNewSequence();
            SelectNextShufflePoint();
            roundsSinceLastShuffle = 0;
            currentSequenceIndex = 0; // Reset to start of new sequence
            shufflePendingAfterRound = false;

            // Notify UI to refresh
            PredictionUI predictionUI = FindObjectOfType<PredictionUI>();
            if (predictionUI != null)
            {
                predictionUI.SetupPrediction(this);
            }

            Debug.Log($"[SignShuffle] Shuffle complete. New sequence ready for next round.");
        }
    }

    private IEnumerator SimulateHitAnimationComplete()
    {
        // Small delay to make dodge feel natural
        yield return new WaitForSeconds(0.2f);

        // Trigger the hit animation finished event even though we dodged
        OnHitAnimationFinished();
    }

    public void TriggerDodgeAnimation()
    {
        if (handAnimator != null && handAnimator.HasParameter("Dodge"))
        {
            handAnimator.SetTrigger("Dodge");
            Debug.Log($"{gameObject.name} playing dodge animation");
        }
    }

    public void OnDodgeAnimationFinished()
    {
        Debug.Log($"{gameObject.name} dodge animation finished!");
        // Trigger the hit animation finished event to continue game flow
        HitAnimationFinished?.Invoke(this);
    }

    // Force generation of a new sequence (public version)
    public void ForceNewSequence()
    {
        if (!usesPredictionSystem || isPlayer || hardMode)
        {
            Debug.LogWarning($"[HandController] Cannot force new sequence - conditions not met");
            return;
        }

        Debug.Log($"[HandController] Forcing new sequence for {gameObject.name}");
        GenerateNewSequence();
    }

    // Reset sign shuffle system
    public void ResetSignShuffle()
    {
        if (!UsesSignShuffle())
        {
            Debug.LogWarning($"[HandController] Cannot reset sign shuffle - not using shuffle system");
            return;
        }

        Debug.Log($"[HandController] Resetting sign shuffle for {gameObject.name}");

        // Pick a new random shuffle point
        SelectNextShufflePoint();

        // Reset the counter
        roundsSinceLastShuffle = 0;
        shufflePendingAfterRound = false;

        Debug.Log($"[HandController] Sign shuffle reset - next shuffle in {roundsUntilShuffle} rounds");
    }

    public void OnStealAnimationFinished()
    {
        Debug.Log($"{gameObject.name} steal animation finished!");
        StealAnimationFinished?.Invoke(this);
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