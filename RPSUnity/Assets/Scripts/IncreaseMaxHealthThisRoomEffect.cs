using UnityEngine;

public class IncreaseMaxHealthThisRoomEffect : PowerUpEffectBase
{
    private bool applied = false;

    public override void OnRoomStart()
    {
        if (!player.isPlayer || applied)
            return;

        int bonus = Mathf.RoundToInt(source.effectValue);
        player.maxHealth += bonus;
        player.health += bonus;
        player.UpdateHealthBar();

        applied = true;

        Debug.Log($"[Effect] Max HP increased by {bonus} for this room ({source.powerUpName})");
    }

    // No need to remove manually — RoomManager will wipe this at the end of the room
}
