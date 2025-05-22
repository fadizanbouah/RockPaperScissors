using UnityEngine;

public class CardActivationZone : MonoBehaviour
{
    [SerializeField] public Transform activationAnimationTarget; // e.g. center of screen
    [SerializeField] private GameObject visualHighlight;

    public Vector3 GetActivationTargetPosition()
    {
        return activationAnimationTarget != null ? activationAnimationTarget.position : transform.position;
    }

    public void ShowVisual()
    {
        if (visualHighlight != null)
            visualHighlight.SetActive(true);
    }

    public void HideVisual()
    {
        if (visualHighlight != null)
            visualHighlight.SetActive(false);
    }
}
