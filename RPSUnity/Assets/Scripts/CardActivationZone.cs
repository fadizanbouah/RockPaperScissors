using UnityEngine;

public class CardActivationZone : MonoBehaviour
{
    [SerializeField] public Transform activationAnimationTarget;
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

    // Call this when a card is dropped into this zone
    public void BeginPowerUpActivation(GameObject cardObject)
    {
        Debug.Log("[CardActivationZone] Card dropped. Beginning power-up activation.");
        RockPaperScissorsGame.Instance.EnterPowerUpActivationState(() => RockPaperScissorsGame.Instance.OnPowerUpActivationComplete());
    }
}
