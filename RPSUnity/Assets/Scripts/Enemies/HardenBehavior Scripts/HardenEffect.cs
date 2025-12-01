// RPSUnity/Assets/Scripts/Enemies/Harden/HardenEffect.cs
using UnityEngine;

public class HardenEffect : PowerUpEffectBase
{
    private float damageReductionPercent;
    private HandController protectedEnemy;

    public void Initialize(float reductionPercent, HandController enemy)
    {
        this.damageReductionPercent = reductionPercent;
        this.protectedEnemy = enemy;

        // Set the player field to the enemy so the ownership check works correctly
        this.player = enemy;

        Debug.Log($"[HardenEffect] Initialized with {reductionPercent}% damage reduction for {enemy.name}");
    }

    public override void ModifyIncomingDamage(ref int damage, HandController source)
    {
        // Only apply if the protected enemy is taking damage from another source
        if (protectedEnemy == null) return;
        if (source == protectedEnemy) return; // Don't reduce self-damage

        int originalDamage = damage;
        float reduction = damageReductionPercent / 100f;
        damage = Mathf.RoundToInt(damage * (1f - reduction));

        Debug.Log($"[HardenEffect] Reduced incoming damage to {protectedEnemy.name} from {originalDamage} to {damage} ({damageReductionPercent}% reduction)");
    }

    public override void Cleanup()
    {
        Debug.Log("[HardenEffect] Cleanup called");
    }

    public override bool IsEffectActive()
    {
        return protectedEnemy != null && protectedEnemy.CurrentHealth > 0;
    }
}