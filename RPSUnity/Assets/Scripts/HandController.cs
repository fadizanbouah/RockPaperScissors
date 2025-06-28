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

    void Start()
    {
        ApplyUpgrades();
        handSpriteRenderer.sprite = defaultHandSprite;
        UpdateHealthBar();
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