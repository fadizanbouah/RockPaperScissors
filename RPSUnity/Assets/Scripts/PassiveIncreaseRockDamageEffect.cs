using UnityEngine;

public class PassiveIncreaseRockDamageEffect : PowerUpEffectBase
{
    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null || sourceData == null) return;

        int amount = Mathf.RoundToInt(sourceData.value);
        PlayerProgressData.Instance.bonusRockDamage += amount;

        Debug.Log($"[Passive] Rock damage permanently increased by {amount} ({sourceData.powerUpName})");
    }
}
