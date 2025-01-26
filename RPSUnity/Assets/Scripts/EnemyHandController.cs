using UnityEngine;
using System.Collections;

public class EnemyHandController : MonoBehaviour
{
    private Animator handAnimator;
    public SpriteRenderer handSpriteRenderer;
    public Sprite paperHandSprite;
    public Sprite scissorsHandSprite;

    private bool isShaking = false;
    private string[] choices = { "Rock", "Paper", "Scissors" };
    public int health = 10; // Enemy starts with a configurable health value
    public int maxHealth;   // Maximum health should match the initial health value

    public int rockDamage = 8;
    public int paperDamage = 6;
    public int scissorsDamage = 4;

    public HealthBar healthBar; // Reference to enemy's health bar
    void Start()
    {
        handAnimator = GetComponent<Animator>();
        maxHealth = health; // Set maxHealth dynamically based on initial health
        healthBar.SetHealth(health, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;

        Debug.Log("Enemy current health: " + health + "/" + maxHealth);

        healthBar.SetHealth(health, maxHealth);

        if (health <= 0)
        {
            Debug.Log("Enemy defeated!");
            // Additional logic for enemy defeat can go here
        }
    }
    public string MakeChoice()
    {
        return choices[Random.Range(0, choices.Length)];
    }
    public void StartShaking()
    {
        if (isShaking) return; // Prevent duplicate shakes
        isShaking = true;

        Debug.Log("Enemy hand is shaking...");
        handAnimator.SetTrigger("Shake");  // Ensure shaking animation starts

        // Prevent multiple calls
        StopAllCoroutines();
        StartCoroutine(WaitForShakingAnimation());
    }

    private IEnumerator WaitForShakingAnimation()
    {
        yield return new WaitForSeconds(1.0f);  // Ensure animation completes before selection
        isShaking = false;
        Debug.Log("Shaking animation completed. Waiting for game logic to assign choice...");
    }

    public void SetHandChoice(string choice)
    {
        // Reset existing triggers to avoid conflicts
        handAnimator.ResetTrigger("ChoosePaper");
        handAnimator.ResetTrigger("ChooseScissors");

        Debug.Log("Setting enemy hand choice: " + choice);

        switch (choice)
        {
            case "Rock":
                Debug.Log("Enemy hand remains in default Rock position.");
                break;  // Rock is default, no change needed
            case "Paper":
                Debug.Log("Enemy hand set to Paper.");
                handAnimator.SetTrigger("ChoosePaper");
                handSpriteRenderer.sprite = paperHandSprite;
                break;
            case "Scissors":
                Debug.Log("Enemy hand set to Scissors.");
                handAnimator.SetTrigger("ChooseScissors");
                handSpriteRenderer.sprite = scissorsHandSprite;
                break;
            default:
                Debug.LogError("Invalid enemy hand choice: " + choice);
                break;
        }
    }
}
