using UnityEngine;

public class PassiveIncreaseScissorsDamageEffect : PowerUpEffectBase
{
    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null || sourceData == null) return;

        int amount = Mathf.RoundToInt(sourceData.value);
        PlayerProgressData.Instance.bonusScissorsDamage += amount;

        Debug.Log($"[Passive] Scissors damage permanently increased by {amount} ({sourceData.powerUpName})");
    }
}
