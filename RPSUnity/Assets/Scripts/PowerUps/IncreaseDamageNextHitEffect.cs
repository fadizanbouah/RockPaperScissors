using UnityEngine;

public class IncreaseDamageNextHitEffect : PowerUpEffectBase
{
    private bool used = false;
    private float bonusPercentage;
    private bool justStolen = false; // NEW: Track if this was just stolen this round

    public override void Initialize(PowerUpData data, HandController player, HandController enemy)
    {
        base.Initialize(data, player, enemy);
        bonusPercentage = data.value / 100f;
        used = false;
        justStolen = false;
        Debug.Log($"[IncreaseDamageNextHitEffect] Initialized. Owner (player field): {player?.name ?? "null"}, isPlayer: {player?.isPlayer}");
    }

    public override void OnRoomStart()
    {
        // If this is being called during a room start (not mid-round stealing), it's safe to use
        justStolen = false;
    }

    public override void ModifyDamageMultiplier(ref float multiplier, string signUsed)
    {
        Debug.Log($"[IncreaseDamageNextHitEffect] ModifyDamageMultiplier called. Owner: {player?.name ?? "null"}, Used: {used}");

        if (used)
        {
            Debug.Log($"[IncreaseDamageNextHitEffect] Already used, skipping");
            return;
        }

        float oldMultiplier = multiplier;
        multiplier += bonusPercentage;
        Debug.Log($"[IncreaseDamageNextHitEffect] Applied bonus for {player?.name ?? "unknown"}. Multiplier: {oldMultiplier} -> {multiplier}");
    }

    public override void OnRoundEnd(string playerChoice, string enemyChoice, RoundResult result)
    {
        Debug.Log($"[IncreaseDamageNextHitEffect] OnRoundEnd - Owner: {player?.name}, isPlayer: {player?.isPlayer}, Result: {result}, Used: {used}, JustStolen: {justStolen}");

        if (used)
        {
            Debug.Log("[IncreaseDamageNextHitEffect] Already used, skipping OnRoundEnd");
            return;
        }

        // NEW: If this was just stolen this round, skip consumption but clear the flag
        if (justStolen)
        {
            Debug.Log("[IncreaseDamageNextHitEffect] Was just stolen this round - skipping consumption, will be available next round");
            justStolen = false;
            return;
        }

        // Check if the owner of this effect won (meaning they dealt damage)
        bool ownerWon = false;

        if (player != null)
        {
            if (player.isPlayer && result == RoundResult.Win)
            {
                ownerWon = true;
                Debug.Log("[IncreaseDamageNextHitEffect] Player owner won!");
            }
            else if (!player.isPlayer && result == RoundResult.Lose)
            {
                ownerWon = true;
                Debug.Log("[IncreaseDamageNextHitEffect] Enemy owner won (player lost)!");
            }
        }

        if (ownerWon)
        {
            used = true;
            Debug.Log($"[IncreaseDamageNextHitEffect] Marking as USED after {player?.name ?? "unknown"} dealt damage");

            // Remove icon from the appropriate tracker
            if (player != null && player.isPlayer)
            {
                PlayerCombatTracker tracker = Object.FindObjectOfType<PlayerCombatTracker>();
                if (tracker != null)
                {
                    tracker.RemoveActiveEffect(this);
                    Debug.Log("[IncreaseDamageNextHitEffect] Removed from PlayerCombatTracker");
                }
            }
            else if (player != null && !player.isPlayer)
            {
                EnemyCombatTracker tracker = Object.FindObjectOfType<EnemyCombatTracker>();
                if (tracker != null)
                {
                    tracker.RemoveActiveEffect(this);
                    Debug.Log("[IncreaseDamageNextHitEffect] Removed from EnemyCombatTracker");
                }
            }

            // Remove this effect from manager
            PowerUpEffectManager.Instance?.RemoveEffect(this);
            Debug.Log("[IncreaseDamageNextHitEffect] Removed from PowerUpEffectManager");
        }
        else
        {
            Debug.Log($"[IncreaseDamageNextHitEffect] Owner did not deal damage this round - keeping effect active");
        }
    }

    public override bool IsEffectActive()
    {
        return !used;
    }

    // NEW: Method to mark this as just stolen (called by stealing mechanics)
    public void MarkAsJustStolen()
    {
        justStolen = true;
        Debug.Log("[IncreaseDamageNextHitEffect] Marked as just stolen - will not be consumed this round");
    }
}