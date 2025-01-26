using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    private string[] choices = { "Rock", "Paper", "Scissors" };
    public int health = 10; // Enemy starts with a configurable health value
    public int maxHealth;   // Maximum health should match the initial health value

    public int rockDamage = 8;
    public int paperDamage = 6;
    public int scissorsDamage = 4;

    public HealthBar healthBar; // Reference to enemy's health bar

    void Start()
    {
        maxHealth = health; // Set maxHealth dynamically based on initial health
        healthBar.SetHealth(health, maxHealth);

        // Ensure UI updates to reflect correct health values immediately
        FindObjectOfType<RockPaperScissorsGame>().UpdateHealthUI();
    }

    public string MakeChoice()
    {
        return choices[Random.Range(0, choices.Length)];
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
}
