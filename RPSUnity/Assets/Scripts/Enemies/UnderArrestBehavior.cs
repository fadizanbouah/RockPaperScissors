using System.Collections;
using UnityEngine;

public class UnderArrestBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Under Arrest Configuration")]
    [SerializeField] private int triggerEveryXRounds = 3; // Lock triggers every X rounds
    [SerializeField] private int lockDuration = 1; // Lock lasts for Y rounds

    [Header("Visual")]
    [SerializeField] private GameObject lockVFXPrefab; // Assign VFX prefab here

    private HandController thisEnemy;
    private GameObject currentLockVFX; // Track the spawned VFX
    private int roundCounter = 0; // Counts up to triggerEveryXRounds
    private int lockRoundsRemaining = 0; // Counts down when lock is active
    private string lockedSign = ""; // Which sign is currently locked
    private bool isLockActive = false;

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
    }

    public IEnumerator OnIdleStateEntered()
    {
        // Apply lock if it's pending
        if (isLockActive && !string.IsNullOrEmpty(lockedSign))
        {
            Debug.Log($"[UnderArrestBehavior] Applying lock to {lockedSign}");

            // Disable the locked button
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
                        currentLockVFX = Instantiate(lockVFXPrefab, buttonTransform.position, Quaternion.identity);
                        currentLockVFX.transform.SetParent(buttonTransform);
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

            // Pick random sign to lock
            string[] signs = { "Rock", "Paper", "Scissors" };
            lockedSign = signs[Random.Range(0, signs.Length)];

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

    private void OnDestroy()
    {
        // Cleanup: destroy VFX and unlock sign if still locked
        if (currentLockVFX != null)
        {
            Destroy(currentLockVFX);
            currentLockVFX = null;
        }

        if (isLockActive && !string.IsNullOrEmpty(lockedSign))
        {
            RockPaperScissorsGame gameManager = FindObjectOfType<RockPaperScissorsGame>();
            // Only unlock if game manager still exists (not destroyed during Unity shutdown)
            if (gameManager != null && gameManager.gameObject != null)
            {
                gameManager.UnlockPlayerSign(lockedSign);
            }
        }

        Debug.Log("[UnderArrestBehavior] Destroyed");
    }
}