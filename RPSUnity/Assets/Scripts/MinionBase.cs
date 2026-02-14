using UnityEngine;

/// <summary>
/// Base class for all minion prefabs.
/// Individual minion types (Hound, Shield, etc.) should inherit from this.
/// </summary>
public class MinionBase : MonoBehaviour
{
    protected HandController parentEnemy;
    protected Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        OnMinionAwake();
    }

    /// <summary>
    /// Called when minion is spawned and connected to parent enemy.
    /// </summary>
    public virtual void Initialize(HandController enemy)
    {
        parentEnemy = enemy;
        Debug.Log($"[MinionBase] {gameObject.name} initialized with parent: {enemy.name}");
        OnMinionInitialized();
    }

    /// <summary>
    /// Override this in child classes for custom awake logic.
    /// </summary>
    protected virtual void OnMinionAwake() { }

    /// <summary>
    /// Override this in child classes for custom initialization logic.
    /// </summary>
    protected virtual void OnMinionInitialized() { }

    /// <summary>
    /// Play an animation on this minion.
    /// </summary>
    public virtual void PlayAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
            Debug.Log($"[MinionBase] Playing animation: {triggerName}");
        }
    }

    /// <summary>
    /// Get the parent enemy reference.
    /// </summary>
    public HandController GetParentEnemy()
    {
        return parentEnemy;
    }
}