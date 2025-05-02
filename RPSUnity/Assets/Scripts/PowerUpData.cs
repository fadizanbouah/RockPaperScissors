using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUps/PowerUp")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    public string description;
    public Sprite icon;
    public int favorCost;

    public PowerUpType powerUpType;
    public float value;  // e.g., 5 for +5% damage, 10 for +10 max health, etc.
}
