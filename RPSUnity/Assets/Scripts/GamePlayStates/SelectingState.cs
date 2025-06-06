using UnityEngine;
using System.Collections;

public class SelectingState : IGameplaySubstate
{
    private readonly string playerChoice;
    private readonly string enemyChoice;
    private HandController player;
    private HandController enemy;

    public SelectingState(string playerChoice)
    {
        this.playerChoice = playerChoice;
        this.enemyChoice = GetRandomEnemyChoice();
        this.player = RockPaperScissorsGame.Instance.GetPlayer();
        this.enemy = RockPaperScissorsGame.Instance.GetEnemy();
    }

    public void Enter()
    {
        Debug.Log("[SelectingState] Entered.");

        RockPaperScissorsGame.Instance.DisableButtons();
        PowerUpCardSpawnerGameplay spawner = Object.FindObjectOfType<PowerUpCardSpawnerGameplay>();
        if (spawner != null)
            spawner.SetAllCardsInteractable(false);

        RockPaperScissorsGame.Instance.ResetSignAnimationFlags();

        Debug.Log($"[SelectingState] Player chose: {playerChoice}");
        player.StartShaking(playerChoice);

        Debug.Log($"[SelectingState] Enemy chose: {enemyChoice}");
        enemy.StartShaking(enemyChoice);

        // Wait for both animations before proceeding
        RockPaperScissorsGame.Instance.StartCoroutine(WaitForAnimationsThenResolve());
    }

    private IEnumerator WaitForAnimationsThenResolve()
    {
        yield return new WaitUntil(() => RockPaperScissorsGame.Instance.BothSignAnimationsDone());
        Debug.Log("[SelectingState] Both sign animations done. Transitioning to outcome resolution...");
        GameplayStateMachine.Instance.ChangeState(new ResolvingEvaluateOutcomeState(playerChoice, enemyChoice));
    }

    public void Exit()
    {
        Debug.Log("[SelectingState] Exited.");
    }

    public void Update() { }

    private string GetRandomEnemyChoice()
    {
        string[] options = { "Rock", "Paper", "Scissors" };
        return options[Random.Range(0, options.Length)];
    }
}
