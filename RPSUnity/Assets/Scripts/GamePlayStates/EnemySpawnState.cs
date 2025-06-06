using UnityEngine;

public class EnemySpawnState : IGameplaySubstate
{
    public void Enter()
    {
        Debug.Log("[EnemySpawnState] Entered.");

        // Subscribe to the RoomManager event to know when the enemy is ready
        RoomManager.Instance.OnEnemySpawned += HandleEnemySpawned;
    }

    public void Exit()
    {
        Debug.Log("[EnemySpawnState] Exited.");

        // Clean up the event subscription
        RoomManager.Instance.OnEnemySpawned -= HandleEnemySpawned;
    }

    public void Update()
    {
        // No per-frame behavior needed
    }

    private void HandleEnemySpawned()
    {
        Debug.Log("[EnemySpawnState] Enemy spawned. Switching to IdleState.");

        // Change state back to Idle so the player can act
        GameplayStateMachine.Instance.ChangeState(new IdleState());
    }
}
