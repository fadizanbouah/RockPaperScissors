using UnityEngine;

public class SelectingState : IGameplaySubstate
{
    private readonly string playerChoice;

    public SelectingState(string playerChoice)
    {
        this.playerChoice = playerChoice;
    }

    public void Enter()
    {
        Debug.Log("[SelectingState] Entered.");

        RockPaperScissorsGame.Instance.DisablePlayerInput();
        Debug.Log("[SelectingState] Player input disabled.");

        // Store the player's choice
        RockPaperScissorsGame.Instance.SetPlayerSign(playerChoice);
        Debug.Log($"[SelectingState] Player sign set to: {playerChoice}");

        // Random enemy choice
        string enemyChoice = RockPaperScissorsGame.Instance.GetRandomSign();
        RockPaperScissorsGame.Instance.SetEnemySign(enemyChoice);
        Debug.Log($"[SelectingState] Enemy sign set to: {enemyChoice}");

        // Start shaking animation on both hands
        RockPaperScissorsGame.Instance.GetPlayer().StartShaking(playerChoice);
        Debug.Log("[SelectingState] Player shaking started.");
        RockPaperScissorsGame.Instance.GetEnemy().StartShaking(enemyChoice);
        Debug.Log("[SelectingState] Enemy shaking started.");

        // Transition to ResolvingEvaluateOutcome state
        GameplayStateMachine.Instance.ChangeState(
            new ResolvingEvaluateOutcomeState(playerChoice, enemyChoice)
        );
    }

    public void Exit()
    {
        Debug.Log("[SelectingState] Exited.");
    }

    public void Update() { }
}
