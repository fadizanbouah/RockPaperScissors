using UnityEngine;

public class ResolvingTakeDamageState : IGameplaySubstate
{
    private readonly RoundResult result;
    private readonly string playerChoice;
    private readonly string enemyChoice;

    public ResolvingTakeDamageState(RoundResult result, string playerChoice, string enemyChoice)
    {
        this.result = result;
        this.playerChoice = playerChoice;
        this.enemyChoice = enemyChoice;
    }

    public void Enter()
    {
        Debug.Log("[ResolvingTakeDamageState] Entered.");

        RockPaperScissorsGame.Instance.StartCoroutine(
            RockPaperScissorsGame.Instance.HandleTakeDamage(result, playerChoice, enemyChoice, OnDamageResolved)
        );
    }

    public void Exit()
    {
        Debug.Log("[ResolvingTakeDamageState] Exited.");
    }

    public void Update() { }

    private void OnDamageResolved()
    {
        var player = RockPaperScissorsGame.Instance.GetPlayer();
        var enemy = RockPaperScissorsGame.Instance.GetEnemy();

        if (player.IsDead())
        {
            GameplayStateMachine.Instance.ChangeState(new DyingState(true)); // Player died
        }
        else if (enemy.IsDead())
        {
            GameplayStateMachine.Instance.ChangeState(new DyingState(false)); // Enemy died
        }
        else
        {
            GameplayStateMachine.Instance.ChangeState(new IdleState());
        }
    }
}
