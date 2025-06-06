using UnityEngine;

public class IdleState : IGameplaySubstate
{
    public void Enter()
    {
        Debug.Log("[IdleState] Entered.");
        // Enable UI buttons or input here if needed
        RockPaperScissorsGame.Instance.AllowPlayerInput();
    }

    public void Exit()
    {
        Debug.Log("[IdleState] Exited.");
        // Disable input if needed
    }

    public void Update()
    {
        // Idle logic can go here if needed
    }
}
