using UnityEngine;

[System.Serializable]
public class PowerUp
{
    public string powerUpName;      // Display name
    public string description;      // Short description
    public int favorCost;           // How much favor it costs
    public Sprite icon;             // Image/icon to display
    public PowerUpType type;        // Buff, debuff, passive, etc.
    public float effectValue;       // The magnitude of the effect (e.g., +10%)

    // Example: can add more properties later if needed
}

// Simple enum for powerup categories
public enum PowerUpType
{
    IncreaseDamageNextHit,      // Only affects the next successful attack
    IncreaseDamageThisRoom,     // Lasts the entire room
    IncreaseMaxHealthThisRoom,  // Boosts HP only for this room

    // Future support (existing and new types)
    IncreaseDamage,
    IncreaseHealth,
    DamageReduction,
    SpecialMove,
    Other
}
