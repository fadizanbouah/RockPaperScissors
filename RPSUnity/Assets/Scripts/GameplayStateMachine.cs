using UnityEngine;
using System.Collections;

public class GameplayStateMachine : MonoBehaviour
{
    public static GameplayStateMachine Instance { get; private set; }

    private IGameplaySubstate currentSubstate;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        currentSubstate?.Update();
    }

    public void ChangeState(IGameplaySubstate newState)
    {
        if (currentSubstate != null)
        {
            currentSubstate.Exit();
        }

        currentSubstate = newState;

        if (currentSubstate != null)
        {
            currentSubstate.Enter();
        }
    }

    public IGameplaySubstate GetCurrentState()
    {
        return currentSubstate;
    }

    // New utility method for checking current state type
    public bool IsCurrentState<T>() where T : IGameplaySubstate
    {
        return currentSubstate is T;
    }

    public IEnumerator StartResolvingEvaluateOutcome(string playerChoice, string enemyChoice)
    {
        yield return new WaitUntil(() => RockPaperScissorsGame.Instance.BothSignAnimationsDone());
        ChangeState(new ResolvingEvaluateOutcomeState(playerChoice, enemyChoice));
    }
}
