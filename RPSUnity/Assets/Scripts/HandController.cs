using UnityEngine;
using System.Collections;

public class HandController : MonoBehaviour
{
    // Health properties
    public int health = 30;
    public int maxHealth = 30;

    // Damage values for Rock, Paper, Scissors
    public int rockDamage = 10;
    public int paperDamage = 7;
    public int scissorsDamage = 5;

    // Hand animation properties
    public Animator handAnimator;
    public SpriteRenderer handSpriteRenderer;

    public Sprite defaultHandSprite;
    public Sprite paperHandSprite;
    public Sprite scissorsHandSprite;

    // Reference to the health bar
    public HealthBar healthBar;
    private string playerChoice;
    private bool isDying = false; // Prevent multiple triggers

    public delegate void OnDeathHandler(HandController hand);
    public event OnDeathHandler OnDeath;

    public delegate void OnDeathAnimationFinishedHandler(HandController hand);
    public event OnDeathAnimationFinishedHandler OnDeathAnimationFinished;

    void Start()
    {
        handSpriteRenderer.sprite = defaultHandSprite;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDying) return; // Prevent damage after death

        health -= damage;
        if (health < 0) health = 0;
        UpdateHealthBar();

        if (health <= 0)
        {
            StartCoroutine(HandleDeathWithDelay());
        }
    }

    private IEnumerator HandleDeathWithDelay()
    {
        yield return new WaitForSeconds(1.0f);
        Die();
    }

    private void Die()
    {
        if (isDying) return; // Prevent multiple calls
        isDying = true;

        Debug.Log($"{gameObject.name} has been defeated!");

        if (handAnimator != null && handAnimator.HasParameter("Die"))
        {
            handAnimator.SetTrigger("Die");
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            Debug.LogWarning($"Die trigger not found or animator is not assigned on {gameObject.name}");
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        yield return null; // Wait for the animation to start

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
        if (isDying) return; // Prevent interactions if dying
        ResetHandToDefault();
        handAnimator.SetTrigger("Shake");
        playerChoice = choice;
        Invoke(nameof(ChangeHandState), 1.0f);
    }

    private void ChangeHandState()
    {
        if (isDying) return; // Prevent animation change if dying

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
}

// Extension method to check if a parameter exists in the Animator
public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
}
