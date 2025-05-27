using UnityEngine;

public class PassiveIncreasePaperDamageEffect : PowerUpEffectBase
{
    public override void OnRoomStart()
    {
        if (PlayerProgressData.Instance == null || sourceData == null) return;

        int amount = Mathf.RoundToInt(sourceData.value);
        PlayerProgressData.Instance.bonusPaperDamage += amount;

        Debug.Log($"[Passive] Paper damage permanently increased by {amount} ({sourceData.powerUpName})");
    }
}
