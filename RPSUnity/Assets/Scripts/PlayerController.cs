using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int health = 30;
    public int maxHealth = 30;

    public int rockDamage = 10;
    public int paperDamage = 7;
    public int scissorsDamage = 5;

    // Method to take damage
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;
    }
}
