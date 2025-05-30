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

    public void TriggerSignAnimationFinished()
    {
        handController?.OnSignAnimationFinished();
    }

    public void TriggerDeathAnimationFinished()
    {
        Debug.Log("TriggerDeathAnimationFinished called via animation event.");
        handController?.TriggerDeathAnimationFinished();
    }

    // Now this calls the proper method
    public void TriggerHitAnimationFinished()
    {
        Debug.Log("TriggerHitAnimationFinished called via animation event.");
        handController?.TriggerHitAnimationFinished();
    }
}
