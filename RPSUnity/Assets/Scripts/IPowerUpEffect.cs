using UnityEngine;

public interface IPowerUpEffect
{
    void Initialize(PowerUpData source, HandController player, HandController enemy);
    void OnRoundStart();
    void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result);
    void ModifyDamage(ref int damage, string signUsed);
    void OnRoomStart();
    void Cleanup();
}
