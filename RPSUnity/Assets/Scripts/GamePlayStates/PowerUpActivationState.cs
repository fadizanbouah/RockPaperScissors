using UnityEngine;

public class PowerUpActivationState : IGameplaySubstate
{
    private GameObject cardGO;
    private System.Action onCompleteCallback;

    private bool animationFinished = false;

    public PowerUpActivationState(GameObject cardGO)
    {
        this.cardGO = cardGO;
    }

    public void Enter()
    {
        Debug.Log("[PowerUpActivationState] Entered.");

        // Disable input
        RockPaperScissorsGame.Instance.DisableButtons();

        PowerUpCardSpawnerGameplay spawner = Object.FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);

        // Listen for animation complete signal
        animationFinished = false;
        onCompleteCallback = () => animationFinished = true;

        RockPaperScissorsGame.Instance.RegisterPowerUpAnimationCallback(onCompleteCallback);

        RockPaperScissorsGame.Instance.StartCoroutine(WaitForAnimationThenApplyEffect());
    }

    public void Exit()
    {
        Debug.Log("[PowerUpActivationState] Exited.");
        onCompleteCallback = null;
    }

    public void Update() { }

    private System.Collections.IEnumerator WaitForAnimationThenApplyEffect()
    {
        Debug.Log("[PowerUpActivationState] Waiting for power-up animation to complete...");
        yield return new WaitUntil(() => animationFinished);

        Debug.Log("[PowerUpActivationState] Animation done. Applying effect...");

        if (cardGO != null)
        {
            PowerUpCardDisplay cardDisplay = cardGO.GetComponent<PowerUpCardDisplay>();
            PowerUpData data = cardDisplay?.GetPowerUpData();

            if (data != null)
            {
                RunProgressManager.Instance.ApplyPowerUpEffect(data);
                RunProgressManager.Instance.RemoveAcquiredPowerUp(data);
                Debug.Log($"[PowerUpActivationState] Applied effect: {data.powerUpName}");
            }
            else
            {
                Debug.LogWarning("[PowerUpActivationState] No PowerUpData found!");
            }
        }

        // Transition back to idle
        GameplayStateMachine.Instance.ChangeState(new IdleState());
    }
}
