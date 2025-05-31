using UnityEngine;

public class HitAnimationEventRelay : MonoBehaviour
{
    public HandController handController;

    public void OnHitAnimationFinished()
    {
        if (handController != null)
        {
            handController.OnHitAnimationFinished();
        }
        else
        {
            Debug.LogWarning("HitAnimationEventRelay is missing a reference to HandController.");
        }
    }
}
