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
}
