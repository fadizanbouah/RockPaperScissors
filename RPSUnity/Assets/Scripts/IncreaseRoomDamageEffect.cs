using UnityEngine;

public class IncreaseRoomDamageEffect : PowerUpEffectBase
{
    private float bonusPercentage;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        bonusPercentage = data.value / 100f; // e.g. 50 becomes 0.50 for +50%
        Debug.Log($"[IncreaseRoomDamageEffect] Initialized: +{data.value}% damage multiplier for the current room.");
    }

    public override void ModifyDamageMultiplier(ref float multiplier, string signUsed)
    {
        multiplier += bonusPercentage;
        Debug.Log($"[IncreaseRoomDamageEffect] Added {bonusPercentage * 100}% to multiplier. Current total multiplier: {multiplier}");
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
