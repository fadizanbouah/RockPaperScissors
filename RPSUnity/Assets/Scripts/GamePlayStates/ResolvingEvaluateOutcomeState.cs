using UnityEngine;
using System.Collections;

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
        RockPaperScissorsGame.Instance.StartCoroutine(WaitForSignsThenEvaluate());
    }

    public void Exit()
    {
        Debug.Log("[ResolvingEvaluateOutcomeState] Exited.");
    }

    public void Update() { }

    private IEnumerator WaitForSignsThenEvaluate()
    {
        yield return new WaitUntil(() =>
            RockPaperScissorsGame.Instance.BothSignAnimationsDone()
        );

        RoundResult result = RockPaperScissorsGame.Instance.DetermineOutcome(playerChoice, enemyChoice);

        // Pass the result and choices to the next state
        GameplayStateMachine.Instance.ChangeState(new ResolvingTakeDamageState(result, playerChoice, enemyChoice));
    }
}
