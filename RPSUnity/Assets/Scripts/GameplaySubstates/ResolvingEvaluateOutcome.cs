using UnityEngine;

public class ResolvingEvaluateOutcomeState : IGameplaySubstate
{
    private readonly string playerChoice;
    private readonly string enemyChoice;

    public ResolvingEvaluateOutcomeState(string playerChoice, string enemyChoice)
    {
        this.playerChoice = playerChoice;
        this.enemyChoice = enemyChoice;
    }

    public void Enter()
    {
        Debug.Log("[ResolvingEvaluateOutcomeState] Entered.");

        RockPaperScissorsGame.Instance.StartCoroutine(
            RockPaperScissorsGame.Instance.ResolveRound(playerChoice, enemyChoice)
        );
    }

    public void Exit()
    {
        Debug.Log("[ResolvingEvaluateOutcomeState] Exited.");
    }

    public void Update() { }
}
