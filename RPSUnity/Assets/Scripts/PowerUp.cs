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
}

// Power-up type definitions
public enum PowerUpType
{
    IncreaseDamageNextHit,         // Affects next successful hit only
    IncreaseDamageThisRoom,        // Temporary effect until room ends
    IncreaseMaxHealthThisRoom,     // Temporary max HP boost for one room

    // Passive (persistent until run ends)
    PassiveIncreaseRockDamage,     // Adds to Rock attack damage
    PassiveIncreasePaperDamage,    // Adds to Paper attack damage
    PassiveIncreaseScissorsDamage, // Adds to Scissors attack damage
    PassiveIncreaseDamage,         // Adds to all sign types
}
