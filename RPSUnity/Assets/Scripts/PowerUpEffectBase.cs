using UnityEngine;

public abstract class PowerUpEffectBase : IPowerUpEffect
{
    protected PowerUp source;
    protected HandController player;
    protected HandController enemy;

    public virtual void Initialize(PowerUp source, HandController player, HandController enemy)
    {
        this.source = source;
        this.player = player;
        this.enemy = enemy;
    }

    public virtual void OnRoundStart() { }

    public virtual void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result) { }

    public virtual void Cleanup() { }

    // NEW: Optional override for modifying damage
    public virtual void ModifyDamage(ref int damage, string signUsed) { }

    // NEW: Optional override for effects that apply at the start of a room
    public virtual void OnRoomStart() { }
}
