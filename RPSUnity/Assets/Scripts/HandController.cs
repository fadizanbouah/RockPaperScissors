using UnityEngine;
using System.Collections;

public class HandController : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseMaxHealth = 30;
    public int baseDamage = 10;

    [Header("Runtime Stats")]
    public int health;
    public int maxHealth;

    public Animation playerHitAnimation;
    public Animator handAnimator;
    public SpriteRenderer handSpriteRenderer;

    public Sprite defaultHandSprite;
    public Sprite paperHandSprite;
    public Sprite scissorsHandSprite;

    public HealthBar healthBar;
    private string playerChoice;
    private bool isDying = false;

    public bool isPlayer = false;

    [Header("Combat Text")]
    public GameObject combatTextPrefab;

    [Header("Rewards")]
    public int coinReward = 5;

    public delegate void OnDeathHandler(HandController hand);
    public event OnDeathHandler OnDeath;

    public delegate void OnDeathAnimationFinishedHandler(HandController hand);
    public event OnDeathAnimationFinishedHandler OnDeathAnimationFinished;

    public delegate void SignAnimationFinishedHandler(HandController hand);
    public event SignAnimationFinishedHandler SignAnimationFinished;

    public delegate void PlayerDiedHandler();
    public static event PlayerDiedHandler OnPlayerDied;

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

    public int GetEffectiveDamage()
    {
        return baseDamage;
    }

    public void TakeDamage(int damage)
    {
        if (isDying) return;

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
        OnDeath?.Invoke(this);

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

    private void UpdateHealthBar()
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
