using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Areas")]
    [SerializeField] private List<AreaData> areas; // NEW: List of areas instead of flat pool list

    [SerializeField] private SpriteRenderer roomBackground;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private RockPaperScissorsGame rockPaperScissorsGame;
    [SerializeField] private GameObject roomClearedTextObject;
    [SerializeField] private GameObject powerUpPanelObject;

    public event System.Action OnEnemySpawned;

    // NEW: Track current area and pool within area
    private int currentAreaIndex = 0;
    private int currentPoolIndexInArea = 0;

    private RoomData currentRoom;
    private int currentEnemyIndex = 0;
    private HandController currentEnemy;
    private int poolDepthForCurrentRoom = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (areas.Count == 0)
        {
            Debug.LogError("No Areas assigned! Cannot start the game.");
            return;
        }
    }

    public void StartRoomSequence()
    {
        Debug.Log("[RoomManager] Starting new room sequence. Resetting run progress...");
        RunProgressManager.Instance.ResetRun();

        // Clear enemy combat tracker for new run
        EnemyCombatTracker enemyTracker = FindObjectOfType<EnemyCombatTracker>();
        if (enemyTracker != null)
        {
            enemyTracker.ClearTraitIcons();
            enemyTracker.ClearActiveEffects();
            Debug.Log("[RoomManager] Cleared EnemyCombatTracker for new run");
        }

        // NEW: Reset area and pool tracking
        currentAreaIndex = 0;
        currentPoolIndexInArea = 0;

        SelectNextRoom();
    }

    private void LoadRoom(RoomData room)
    {
        Debug.Log($"Loading Room: {room.roomName}");
        currentRoom = room;
        currentEnemyIndex = 0;

        if (roomBackground != null)
        {
            roomBackground.sprite = room.backgroundImage;
        }

        ApplyPersistentPowerUps();
        PowerUpEffectManager.Instance?.TriggerRoomStart();

        // Clear combat tracker effects for new room
        PlayerCombatTracker tracker = FindObjectOfType<PlayerCombatTracker>();
        if (tracker != null)
        {
            tracker.OnRoomStart();
        }

        FadeInAfterRoomLoad();
        SpawnNextEnemy();
        FindObjectOfType<PowerUpCardSpawnerGameplay>()?.SpawnActivePowerUps();
    }

    private void FadeInAfterRoomLoad()
    {
        ScreenFader fader = FindObjectOfType<ScreenFader>();
        if (fader != null)
        {
            StartCoroutine(fader.FadeInRoutine());
        }
        else
        {
            Debug.LogWarning("ScreenFader not found in scene!");
        }
    }

    private void SpawnNextEnemy()
    {
        if (currentRoom == null || currentRoom.enemyPrefabs.Count == 0 || currentEnemyIndex >= currentRoom.enemyPrefabs.Count)
        {
            Debug.Log("No more enemies left in this room. Starting fade-out transition...");
            GameStateManager.Instance.BeginRoomTransition();
            return;
        }

        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        GameObject enemyInstance = Instantiate(currentRoom.enemyPrefabs[currentEnemyIndex], enemySpawnPoint.position, enemySpawnPoint.rotation);
        currentEnemy = enemyInstance.GetComponent<HandController>();

        if (currentEnemy != null)
        {
            // CRITICAL: Scale stats IMMEDIATELY after instantiation, BEFORE Start() runs
            AreaData currentArea = GetCurrentArea();
            currentEnemy.PreInitialize(currentArea, poolDepthForCurrentRoom);

            currentEnemy.OnDeath += HandleEnemyDefeat;
            currentEnemy.OnDeathAnimationFinished += HandleDeathAnimationFinished;
            Debug.Log($"Spawned enemy: {currentRoom.enemyPrefabs[currentEnemyIndex].name}");

            GameStateManager.Instance.UpdateEnemy(currentEnemy);
            rockPaperScissorsGame?.UpdateEnemyReference(currentEnemy);

            // Update the enemy combat tracker
            EnemyCombatTracker enemyTracker = FindObjectOfType<EnemyCombatTracker>();
            if (enemyTracker != null)
            {
                enemyTracker.UpdateEnemyReference(currentEnemy);
            }

            // Update the sign shuffle UI
            SignShuffleUI shuffleUI = FindObjectOfType<SignShuffleUI>();
            if (shuffleUI != null)
            {
                shuffleUI.UpdateEnemyReference(currentEnemy);
            }

            // Notify that the enemy has been fully spawned
            OnEnemySpawned?.Invoke();
        }
        else
        {
            Debug.LogError("Spawned enemy does not have a HandController script!");
        }

        currentEnemyIndex++;
    }

    private void HandleEnemyDefeat(HandController defeatedEnemy)
    {
        Debug.Log($"{defeatedEnemy.gameObject.name} has been defeated! Waiting for Die animation to finish...");
    }

    private void HandleDeathAnimationFinished(HandController defeatedEnemy)
    {
        if (defeatedEnemy == null) return;

        defeatedEnemy.OnDeathAnimationFinished -= HandleDeathAnimationFinished;
        defeatedEnemy.OnDeath -= HandleEnemyDefeat;

        Debug.Log($"{defeatedEnemy.gameObject.name} death animation finished. Checking for next enemy...");

        Destroy(defeatedEnemy.gameObject);
        currentEnemy = null;

        if (currentEnemyIndex >= currentRoom.enemyPrefabs.Count)
        {
            Debug.Log("[RoomManager] All enemies defeated in this room.");

            if (rockPaperScissorsGame != null && rockPaperScissorsGame.roomClearedTextObject != null)
            {
                rockPaperScissorsGame.roomClearedTextObject.SetActive(true);
                Debug.Log("[RoomManager] Activated RoomClearedTextObject.");
            }
            else
            {
                Debug.LogWarning("[RoomManager] rockPaperScissorsGame or roomClearedTextObject is not assigned!");
            }
        }
        else
        {
            StartCoroutine(DelayedSpawnNextEnemy(0.5f));
        }
    }

    private IEnumerator DelayedSpawnNextEnemy(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnNextEnemy();
    }

    public void SelectNextRoom()
    {
        // NEW: Check if we have any areas
        if (areas.Count == 0)
        {
            Debug.LogError("No Areas assigned! Cannot load next room.");
            return;
        }

        // NEW: Get current area
        AreaData currentArea = areas[currentAreaIndex];

        if (currentArea == null || currentArea.roomPools.Count == 0)
        {
            Debug.LogError($"Area at index {currentAreaIndex} has no room pools!");
            return;
        }

        // NEW: Get current pool from current area
        RoomPool currentPool = currentArea.roomPools[currentPoolIndexInArea];

        if (currentPool == null || currentPool.rooms.Count == 0)
        {
            Debug.LogError($"RoomPool at index {currentPoolIndexInArea} in area '{currentArea.areaName}' is empty!");
            return;
        }

        // Store the pool depth BEFORE advancing (this is what enemies in this room will use)
        poolDepthForCurrentRoom = currentPoolIndexInArea;

        // Select random room from pool
        currentRoom = currentPool.GetRandomRoom();
        Debug.Log($"[RoomManager] Area: {currentArea.areaName} | Pool: {currentPoolIndexInArea + 1}/{currentArea.roomPools.Count} | Room: {currentRoom.roomName}");

        LoadRoom(currentRoom);

        // Advance to next pool
        AdvanceToNextPool();
    }

    // Handle pool/area progression
    private void AdvanceToNextPool()
    {
        AreaData currentArea = areas[currentAreaIndex];

        currentPoolIndexInArea++;

        // Check if we've finished all pools in this area
        if (currentPoolIndexInArea >= currentArea.roomPools.Count)
        {
            Debug.Log($"[RoomManager] Completed area: {currentArea.areaName}");

            // Move to next area
            currentAreaIndex++;
            currentPoolIndexInArea = 0;

            // Check if we've finished all areas
            if (currentAreaIndex >= areas.Count)
            {
                Debug.Log("[RoomManager] All areas completed! Looping back to first area.");
                currentAreaIndex = 0; // Loop back (or you could trigger victory screen)
            }
            else
            {
                Debug.Log($"[RoomManager] Advancing to new area: {areas[currentAreaIndex].areaName}");
                // TODO: Trigger area transition cinematic here
            }
        }
    }

    public HandController GetCurrentEnemy()
    {
        return currentEnemy;
    }

    // NEW: Expose current area data
    public AreaData GetCurrentArea()
    {
        if (currentAreaIndex >= 0 && currentAreaIndex < areas.Count)
        {
            return areas[currentAreaIndex];
        }
        return null;
    }

    // NEW: Get pool depth within current area
    public int GetPoolDepthInCurrentArea()
    {
        return currentPoolIndexInArea;
    }

    public void OnRoomClearedAnimationFinished()
    {
        Debug.Log("[RoomManager] Room cleared animation finished. Hiding text.");
        if (roomClearedTextObject != null)
        {
            Debug.Log("[RoomManager] Setting roomClearedTextObject to inactive: " + roomClearedTextObject.name);
            roomClearedTextObject.SetActive(false);
        }

        if (powerUpPanelObject != null)
        {
            Debug.Log("[RoomManager] Activating powerUpPanelObject: " + powerUpPanelObject.name);
            powerUpPanelObject.SetActive(true);

            PowerUpCardSpawner spawner = powerUpPanelObject.GetComponent<PowerUpCardSpawner>();
            if (spawner != null)
            {
                spawner.PopulatePowerUpPanel();
                spawner.PopulateActiveTab();
                Debug.Log("[RoomManager] Populated both passive and active power-up tabs");
            }
            else
            {
                Debug.LogWarning("[RoomManager] PowerUpPanel does not have a PowerUpCardSpawner component!");
            }
        }
        else
        {
            Debug.LogWarning("[RoomManager] powerUpPanelObject is not assigned!");
        }
    }

    public void OnPowerUpContinueButtonClicked()
    {
        Debug.Log("[RoomManager] Continue button clicked. Hiding PowerUpPanel and starting room transition.");

        if (powerUpPanelObject != null)
        {
            powerUpPanelObject.SetActive(false);
        }

        GameStateManager.Instance.BeginRoomTransition();
    }

    private void ApplyPersistentPowerUps()
    {
        PassivePowerUpHandler.ApplyAllPersistentPowerUps();

        PassivePowerUpTracker tracker = FindObjectOfType<PassivePowerUpTracker>();
        if (tracker != null)
        {
            tracker.RefreshDisplay();
        }
    }

    public RoomData GetCurrentRoom()
    {
        return currentRoom;
    }
}