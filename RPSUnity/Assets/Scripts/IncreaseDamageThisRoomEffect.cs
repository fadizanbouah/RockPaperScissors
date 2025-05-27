using UnityEngine;

public class IncreaseDamageThisRoomEffect : PowerUpEffectBase
{
    public override void ModifyDamage(ref int damage, string signUsed)
    {
        damage += Mathf.RoundToInt(sourceData.value);
        Debug.Log($"[Effect] +{sourceData.value} room-wide damage applied from: {sourceData.powerUpName}");
    }

    // No cleanup needed — RoomManager clears room-wide power-ups at room transition
}
