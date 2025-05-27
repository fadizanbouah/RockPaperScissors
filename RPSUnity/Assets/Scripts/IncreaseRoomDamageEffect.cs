using UnityEngine;

public class IncreaseRoomDamageEffect : PowerUpEffectBase
{
    private int bonusDamage;

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        bonusDamage = Mathf.RoundToInt(data.value);
        Debug.Log($"[IncreaseRoomDamageEffect] Initialized: +{bonusDamage} damage for the current room.");
    }

    public override void ModifyDamage(ref int damage, string signUsed)
    {
        Debug.Log("[IncreaseRoomDamageEffect] ModifyDamage called");
        damage += bonusDamage;
        Debug.Log($"[IncreaseRoomDamageEffect] Applied bonus: +{bonusDamage} (New damage: {damage})");
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
