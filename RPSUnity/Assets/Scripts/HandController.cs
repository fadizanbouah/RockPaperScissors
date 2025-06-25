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
            maxHealth = baseMaxHealth + (PlayerProgressData.Instance.maxHealthLevel * 5);
            baseDamage += PlayerProgressData.Instance.baseDamageLevel * 2;
        }
        else
        {
            maxHealth = baseMaxHealth;
        }

        health = maxHealth;
    }

    public void ApplyTemporaryDamageBoost(int amount)
    {
        temporaryBonusDamage += amount;
    }

    public int GetEffectiveDamage(string signUsed)
    {
        int baseFinalDamage = baseDamage;

        if (isPlayer)
        {
            // Add persistent passive flat boosts
            baseFinalDamage += PlayerProgressData.Instance.bonusBaseDamage;

            if (signUsed == "Rock")
                baseFinalDamage += PlayerProgressData.Instance.bonusRockDamage;
            else if (signUsed == "Paper")
                baseFinalDamage += PlayerProgressData.Instance.bonusPaperDamage;
            else if (signUsed == "Scissors")
                baseFinalDamage += PlayerProgressData.Instance.bonusScissorsDamage;

            // Start with 1.0f (100%) multiplier
            float multiplier = 1f;

            // Let power-ups modify the multiplier
            ActivePowerUpHandler.GetModifiedMultiplier(ref multiplier, signUsed);

            // Calculate modified damage from base * multiplier
            int finalDamage = Mathf.RoundToInt(baseFinalDamage * multiplier);

            // Apply and clear one-time temporary bonus
            finalDamage += temporaryBonusDamage;
            if (temporaryBonusDamage > 0)
                Debug.Log($"[HandController] Temporary bonus damage applied: +{temporaryBonusDamage}");
            temporaryBonusDamage = 0;

            return finalDamage;
        }

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
