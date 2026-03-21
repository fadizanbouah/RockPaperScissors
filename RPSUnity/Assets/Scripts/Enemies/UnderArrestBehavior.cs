using System.Collections;
using UnityEngine;

public class UnderArrestBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Under Arrest Configuration")]
    [SerializeField] private int triggerEveryXRounds = 3; // Lock triggers every X rounds
    [SerializeField] private int lockDuration = 1; // Lock lasts for Y rounds

    [Header("Visual")]
    [SerializeField] private GameObject lockVFXPrefab; // Animation prefab for lock
    [SerializeField] private string throwAnimationTrigger = "ThrowCuffs"; // Animator trigger name

    private HandController thisEnemy;
    private int roundCounter = 0; // Counts up to triggerEveryXRounds
    private int lockRoundsRemaining = 0; // Counts down when lock is active
    private string lockedSign = ""; // Which sign is currently locked
    private bool isLockActive = false;
    private GameObject currentLockVFX;

    // Animation event tracking
    private bool throwAnimationFinished = false;

    public void Initialize(HandController enemy, float[] configValues)
    {
        thisEnemy = enemy;

        // configValues[0] = triggerEveryXRounds
        // configValues[1] = lockDuration
        if (configValues != null)
        {
            if (configValues.Length > 0)
                triggerEveryXRounds = Mathf.RoundToInt(configValues[0]);

            if (configValues.Length > 1)
                lockDuration = Mathf.RoundToInt(configValues[1]);
        }

        Debug.Log($"[UnderArrestBehavior] Initialized - Lock every {triggerEveryXRounds} rounds for {lockDuration} rounds");

        // Register for animation events through HandController
        if (thisEnemy != null)
        {
            thisEnemy.OnThrowCuffsFinished += OnThrowAnimationFinished;
            Debug.Log("[UnderArrestBehavior] Registered for ThrowCuffs animation events");
        }
    }

    public IEnumerator OnIdleStateEntered()
    {
        // Apply lock if it's pending
        if (isLockActive && !string.IsNullOrEmpty(lockedSign))
        {
            Debug.Log($"[UnderArrestBehavior] Applying lock to {lockedSign}");

            // STEP 1: Play enemy throw animation
            if (thisEnemy != null && thisEnemy.handAnimator != null && !string.IsNullOrEmpty(throwAnimationTrigger))
            {
                Debug.Log($"[UnderArrestBehavior] Playing throw animation: {throwAnimationTrigger}");
                throwAnimationFinished = false;
                thisEnemy.handAnimator.SetTrigger(throwAnimationTrigger);

                // Wait for animation to finish with timeout
                float timeout = 3f;
                float elapsed = 0f;

                while (!throwAnimationFinished && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (elapsed >= timeout)
                {
                    Debug.LogWarning("[UnderArrestBehavior] Throw animation timed out!");
                }
                else
                {
                    Debug.Log("[UnderArrestBehavior] Throw animation completed");
                }
            }

            // STEP 2: Disable the locked button and spawn VFX
            RockPaperScissorsGame gameManager = FindObjectOfType<RockPaperScissorsGame>();
            if (gameManager != null)
            {
                gameManager.LockPlayerSign(lockedSign);

                // Spawn lock VFX on the button
                if (lockVFXPrefab != null)
                {
                    Transform buttonTransform = gameManager.GetButtonTransform(lockedSign);
                    if (buttonTransform != null)
                    {
                        currentLockVFX = Instantiate(lockVFXPrefab, buttonTransform);

                        RectTransform vfxRect = currentLockVFX.GetComponent<RectTransform>();
                        if (vfxRect != null)
                        {
                            vfxRect.anchoredPosition = Vector2.zero;
                            vfxRect.localScale = Vector3.one;
                        }
                        else
                        {
                            currentLockVFX.transform.localScale = Vector3.one;
                        }

                        Debug.Log($"[UnderArrestBehavior] Spawned lock VFX on {lockedSign} button");
                    }
                }
            }
        }

        yield return null;
    }

    public IEnumerator OnBeforeRoundResolves(HandController player, string playerChoice, string enemyChoice)
    {
        yield return null;
    }

    public IEnumerator OnAfterDamageResolved(HandController player, string playerChoice, string enemyChoice, RoundResult result)
    {
        // Increment round counter
        roundCounter++;
        Debug.Log($"[UnderArrestBehavior] Round {roundCounter}/{triggerEveryXRounds} completed");

        // Check if it's time to trigger a lock
        if (roundCounter >= triggerEveryXRounds && !isLockActive)
        {
            Debug.Log("[UnderArrestBehavior] Lock threshold reached! Preparing lock for next round");

            // Lock the player's last-used sign instead of random
            lockedSign = playerChoice;

            isLockActive = true;
            lockRoundsRemaining = lockDuration;
            roundCounter = 0; // Reset counter

            Debug.Log($"[UnderArrestBehavior] {lockedSign} will be locked for {lockDuration} rounds starting next round");
        }
        // Decrement lock duration if active
        else if (isLockActive)
        {
            lockRoundsRemaining--;
            Debug.Log($"[UnderArrestBehavior] Lock duration: {lockRoundsRemaining} rounds remaining");

            if (lockRoundsRemaining <= 0)
            {
                Debug.Log($"[UnderArrestBehavior] Lock expired! Unlocking {lockedSign}");

                // Destroy lock VFX
                if (currentLockVFX != null)
                {
                    Destroy(currentLockVFX);
                    currentLockVFX = null;
                }

                // Unlock the sign
                RockPaperScissorsGame gameManager = FindObjectOfType<RockPaperScissorsGame>();
                if (gameManager != null)
                {
                    gameManager.UnlockPlayerSign(lockedSign);
                }

                isLockActive = false;
                lockedSign = "";
            }
        }

        yield return null;
    }

    public IEnumerator OnPostDeath(HandController enemy)
    {
        // Unlock sign and destroy VFX if enemy dies while lock is active
        if (isLockActive && !string.IsNullOrEmpty(lockedSign))
        {
            if (currentLockVFX != null)
            {
                Destroy(currentLockVFX);
                currentLockVFX = null;
            }

            RockPaperScissorsGame gameManager = FindObjectOfType<RockPaperScissorsGame>();
            if (gameManager != null)
            {
                gameManager.UnlockPlayerSign(lockedSign);
            }
        }

        yield break;
    }

    private void OnThrowAnimationFinished()
    {
        throwAnimationFinished = true;
        Debug.Log("[UnderArrestBehavior] Throw animation event received");
    }

    private void OnDestroy()
    {
        // Unregister from animation events
        if (thisEnemy != null)
        {
            thisEnemy.OnThrowCuffsFinished -= OnThrowAnimationFinished;
        }

        // Cleanup: destroy VFX and unlock sign if still locked
        if (currentLockVFX != null)
        {
            Destroy(currentLockVFX);
            currentLockVFX = null;
        }

        if (isLockActive && !string.IsNullOrEmpty(lockedSign))
        {
            RockPaperScissorsGame gameManager = FindObjectOfType<RockPaperScissorsGame>();
            // Only unlock if game manager still exists
            if (gameManager != null && gameManager.gameObject != null)
            {
                gameManager.UnlockPlayerSign(lockedSign);
            }
        }

        Debug.Log("[UnderArrestBehavior] Destroyed");
    }
}