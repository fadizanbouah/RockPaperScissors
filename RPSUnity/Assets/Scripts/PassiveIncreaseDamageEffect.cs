using UnityEngine;

public class PassiveIncreaseDamageEffect : PowerUpEffectBase
{
    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null || sourceData == null) return;

        int amount = Mathf.RoundToInt(sourceData.value);
        PlayerProgressData.Instance.bonusBaseDamage += amount;

        Debug.Log($"[Passive] Base damage permanently increased by {amount} ({sourceData.powerUpName})");
    }
}
