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

    public Sprite defaultHandSprite;  // Rock sprite (default hand)
    public Sprite paperHandSprite;
    public Sprite scissorsHandSprite;

    // Reference to the health bar
    public HealthBar healthBar;
    private string playerChoice;

    public delegate void OnDeathHandler(HandController hand);
    public event OnDeathHandler OnDeath;

    void Start()
    {
        handSpriteRenderer.sprite = defaultHandSprite;
        UpdateHealthBar();
    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
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
        // Allow player to see the current hand state before transitioning to the death animation
        yield return new WaitForSeconds(1.0f);

        Die();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");

        // Play the die animation if the animator is assigned
        if (handAnimator != null && handAnimator.HasParameter("Die"))
        {
            handAnimator.SetTrigger("Die");
        }
        else
        {
            Debug.LogWarning("Die trigger not found or animator is not assigned on " + gameObject.name);
        }

        // Start coroutine to destroy the object after animation
        StartCoroutine(DestroyAfterDelay(1.0f));

        // Notify any listeners that this hand has died
        OnDeath?.Invoke(this);
    }

    // Coroutine to destroy the hand after the animation
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        // Reset hand to default first
        ResetHandToDefault();

        // Trigger the shaking animation
        handAnimator.SetTrigger("Shake");

        playerChoice = choice;

        // Store the choice so the correct animation plays after shake
        Invoke(nameof(ChangeHandState), 1.0f);
    }

    private void ChangeHandState()
    {
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
            handSpriteRenderer.sprite = defaultHandSprite;  // Default rock hand
        }
    }

    private void ResetHandToDefault()
    {
        handSpriteRenderer.sprite = defaultHandSprite;  // Reset sprite to default rock hand
        handAnimator.Rebind();  // Reset animator to default state
    }

    public void SelectRock()
    {
        StartShaking("Rock");
    }

    public void SelectPaper()
    {
        StartShaking("Paper");
    }

    public void SelectScissors()
    {
        StartShaking("Scissors");
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
            {
                return true;
            }
        }
        return false;
    }
}
