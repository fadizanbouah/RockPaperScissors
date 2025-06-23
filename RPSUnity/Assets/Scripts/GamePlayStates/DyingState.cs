using UnityEngine;

public class DyingState : IGameplaySubstate
{
    private HandController dyingHand;

    public DyingState(HandController hand)
    {
        dyingHand = hand;
    }

    public void Enter()
    {
        Debug.Log("[DyingState] Entered. Waiting for death animation to finish...");

        if (dyingHand != null)
        {
            dyingHand.OnDeathAnimationFinished += HandleDeathAnimationFinished;
        }
        else
        {
            Debug.LogError("[DyingState] No dying hand was provided.");
        }
    }

    public void Update() { }

    public void Exit()
    {
        if (dyingHand != null)
        {
            dyingHand.OnDeathAnimationFinished -= HandleDeathAnimationFinished;
        }
    }

    private void HandleDeathAnimationFinished(HandController hand)
    {
        Debug.Log("[DyingState] Death animation finished.");

        if (hand.isPlayer)
        {
            Debug.Log("[DyingState] Player died. Triggering game over...");
            GameStateManager.Instance.StartCoroutine(GameStateManager.Instance.FadeToMainMenu());
        }
        else
        {
            GameplayStateMachine.Instance.ChangeState(new EnemySpawnState());
            //Debug.Log("[DyingState] Enemy died. Proceeding to spawn next enemy.");
            //RoomManager.Instance.OnEnemySpawned += OnEnemySpawned;
            //RoomManager.Instance.SpawnNextEnemy();
        }
    }

    private void OnEnemySpawned()
    {
        RoomManager.Instance.OnEnemySpawned -= OnEnemySpawned;
        GameplayStateMachine.Instance.ChangeState(new IdleState());
    }
}
