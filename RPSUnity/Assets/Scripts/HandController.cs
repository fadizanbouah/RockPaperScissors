using UnityEngine;

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
