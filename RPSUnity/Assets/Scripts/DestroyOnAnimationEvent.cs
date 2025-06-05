using UnityEngine;

public class DestroyOnAnimationEvent : MonoBehaviour
{
    public void DestroySelf()
    {
        Debug.Log("[PowerUpCard] Activation animation ended — destroying self.");
        Destroy(gameObject);

        if (RockPaperScissorsGame.Instance != null &&
            RockPaperScissorsGame.Instance.IsInPowerUpActivationSubstate())
        {
            Debug.Log("[PowerUp] Activation animation complete.");
            RockPaperScissorsGame.Instance.OnPowerUpActivationComplete();
        }
    }
}
