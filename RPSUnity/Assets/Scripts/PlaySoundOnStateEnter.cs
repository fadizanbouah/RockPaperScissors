using UnityEngine;

public class PlaySoundOnStateEnter : StateMachineBehaviour
{
    [Tooltip("Name of the sound from AudioManager's Sound Library")]
    [SerializeField] private string soundName;

    [Tooltip("Only play once per state entry (prevents retriggering on loops)")]
    [SerializeField] private bool playOnce = true;

    private bool hasPlayed = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnce && hasPlayed) return;

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
        {
            AudioManager.Instance.PlaySoundByName(soundName);
            Debug.Log($"[PlaySoundOnStateEnter] Playing sound: {soundName} on state enter");
            hasPlayed = true;
        }
        else if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("[PlaySoundOnStateEnter] Sound name is empty!");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reset for next time the state is entered
        hasPlayed = false;
    }
}