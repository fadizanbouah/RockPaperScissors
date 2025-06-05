using UnityEngine;

public class IncreaseRoomDamageEffect : PowerUpEffectBase
{
    private float bonusPercentage;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        bonusPercentage = data.value / 100f; // e.g. 10 becomes 0.10 (10% boost)
        Debug.Log($"[IncreaseRoomDamageEffect] Initialized: +{data.value}% damage for the current room.");
    }

    public override void ModifyDamage(ref int damage, string signUsed)
    {
        Debug.Log("[IncreaseRoomDamageEffect] ModifyDamage called");
        int bonus = Mathf.RoundToInt(damage * bonusPercentage);
        damage += bonus;
        Debug.Log($"[IncreaseRoomDamageEffect] Applied {bonusPercentage * 100}% bonus: +{bonus} (New damage: {damage})");
    }

    public override void OnRoomStart()
    {
        Debug.Log("[IncreaseRoomDamageEffect] OnRoomStart called — removing effect.");
        PowerUpEffectManager.Instance?.RemoveEffect(this);
    }

    public override void Cleanup()
    {
        Debug.Log("[IncreaseRoomDamageEffect] Cleanup called — effect is being destroyed.");
    }
}
