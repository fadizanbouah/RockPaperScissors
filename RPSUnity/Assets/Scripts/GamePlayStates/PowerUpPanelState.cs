using UnityEngine;

public class PowerUpPanelState : IGameplaySubstate
{
    private bool continueClicked = false;

    public void Enter()
    {
        Debug.Log("[PowerUpPanelState] Entered.");

        RoomManager.Instance.OnRoomClearedAnimationFinished();
        RoomManager.Instance.ShowPowerUpPanel();

        RoomManager.Instance.OnPowerUpContinueClicked += HandleContinueClicked;
    }

    public void Exit()
    {
        Debug.Log("[PowerUpPanelState] Exited.");

        RoomManager.Instance.OnPowerUpContinueClicked -= HandleContinueClicked;
    }

    public void Update()
    {
        // Nothing to update frame-by-frame
    }

    private void HandleContinueClicked()
    {
        Debug.Log("[PowerUpPanelState] Continue button clicked.");
        continueClicked = true;
        GameplayStateMachine.Instance.ChangeState(new TransitionState());
    }
}