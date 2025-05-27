using UnityEngine;

public abstract class PowerUpEffectBase : MonoBehaviour, IPowerUpEffect
{
    protected PowerUpData sourceData;
    protected HandController player;
    protected HandController enemy;

    public virtual void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        this.sourceData = data;
        this.player = player;
        this.enemy = enemy;
    }

    public virtual void OnRoundStart() { }

    public virtual void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result) { }

    public virtual void Cleanup() { }

    public virtual void ModifyDamage(ref int damage, string signUsed) { }

    public virtual void OnRoomStart() { }
}
