using UnityEngine;

public class IncreaseDamageThisRoomEffect : PowerUpEffectBase
{
    public override void ModifyDamage(ref int damage, string signUsed)
    {
        damage += Mathf.RoundToInt(source.effectValue);
        Debug.Log($"[Effect] +{source.effectValue} room-wide damage applied from: {source.powerUpName}");
    }

    // No cleanup needed — RoomManager clears room-wide power-ups at room transition
}
