using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private HandController handController;

    private void Awake()
    {
        // Automatically find the HandController in the parent hierarchy
        handController = GetComponentInParent<HandController>();

        if (handController == null)
        {
            Debug.LogError("HandController not found in parent! AnimationEventRelay will not function.");
        }
    }

    // Called by animation events at the end of sign animations
    public void TriggerSignAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnSignAnimationFinished();
        }
    }

    // Called by animation events at the end of the Die animation
    public void TriggerDeathAnimationFinished()
    {
        if (handController != null)
        {
            handController.TriggerDeathAnimationFinished();
        }
    }

    // Called by animation events at the end of the Dodge animation
    public void TriggerDodgeAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnDodgeAnimationFinished();
        }
    }

    public void TriggerHitAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnHitAnimationFinished();
        }
    }

    // Called by animation events at the end of the CheatDeath animation
    public void TriggerCheatDeathAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnCheatDeathAnimationFinished();
        }
    }

    public void TriggerCheatDeathHeal()
    {
        Debug.Log("[AnimationEventRelay] TriggerCheatDeathHeal called!");

        if (handController != null)
        {
            handController.OnCheatDeathHeal();
        }
    }

    public void TriggerStealAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnStealAnimationFinished();
        }
    }

    public void TriggerHouseRulesAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnHouseRulesAnimationFinished();
        }
    }

    public void TriggerHoundAttackHit()
    {
        Debug.Log("[AnimationEventRelay] TriggerHoundAttackHit called!");

        if (handController != null)
        {
            handController.OnHoundAttackHit();
        }
    }

    public void TriggerHoundAttackFinished()
    {
        Debug.Log("[AnimationEventRelay] TriggerHoundAttackFinished called!");

        if (handController != null)
        {
            handController.OnHoundAttackFinished();
        }
    }

    // Called mid-animation when attack hits
    public void TriggerMurderousAttackHit()
    {
        Debug.Log("[AnimationEventRelay] TriggerMurderousAttackHit called!");

        if (handController != null)
        {
            handController.OnMurderousAttackHit();
        }
    }

    // Called at end of animation
    public void TriggerMurderousAttackFinished()
    {
        Debug.Log("[AnimationEventRelay] TriggerMurderousAttackFinished called!");

        if (handController != null)
        {
            handController.OnMurderousAttackFinished();
        }
    }

    public void TriggerThrowCuffsFinished()
    {
        Debug.Log("[AnimationEventRelay] ThrowCuffs animation finished!");
        if (handController != null)
        {
            handController.OnThrowCuffsAnimationFinished();
        }
    }
}
