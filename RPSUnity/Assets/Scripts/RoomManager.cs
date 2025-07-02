using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] private List<RoomPool> roomPools; // List of pools in sequence
    [SerializeField] private SpriteRenderer roomBackground;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField] private RockPaperScissorsGame rockPaperScissorsGame; // Reference to game logic
    [SerializeField] private GameObject roomClearedTextObject;
    [SerializeField] private GameObject powerUpPanelObject;

    public event System.Action OnEnemySpawned;

    private int currentPoolIndex = 0; // Track current pool in sequence
    private RoomData currentRoom;
    private int currentEnemyIndex = 0;
    private HandController currentEnemy;

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
        if (roomPools.Count == 0)
        {
            Debug.LogError("No RoomPools assigned! Cannot start the game.");
            return;
        }
    }

    public void StartRoomSequence()
    {
        Debug.Log("[RoomManager] Starting new room sequence. Resetting run progress...");
        RunProgressManager.Instance.ResetRun();

        currentPoolIndex = 0;
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
            currentEnemy.OnDeath += HandleEnemyDefeat;
            currentEnemy.OnDeathAnimationFinished += HandleDeathAnimationFinished;
            Debug.Log($"Spawned enemy: {currentRoom.enemyPrefabs[currentEnemyIndex].name}");

            GameStateManager.Instance.UpdateEnemy(currentEnemy);
            rockPaperScissorsGame?.UpdateEnemyReference(currentEnemy);


            // Set up prediction UI for the new enemy
            PredictionUI predictionUI = FindObjectOfType<PredictionUI>();
            if (predictionUI != null)
            {
                predictionUI.SetupPrediction(currentEnemy);
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

            // Removed calls to RemoveRoomScopedEffects because those power-ups are deleted

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
        if (roomPools.Count == 0)
        {
            Debug.LogError("No RoomPools assigned! Cannot load next room.");
            return;
        }

        RoomPool currentPool = roomPools[currentPoolIndex];

        if (currentPool == null || currentPool.rooms.Count == 0)
        {
            Debug.LogError($"RoomPool at index {currentPoolIndex} is empty or missing!");
            return;
        }

        currentRoom = currentPool.rooms[Random.Range(0, currentPool.rooms.Count)];
        Debug.Log($"Next Room Selected: {currentRoom.roomName}");

        LoadRoom(currentRoom);
        currentPoolIndex = (currentPoolIndex + 1) % roomPools.Count;
    }

    public HandController GetCurrentEnemy()
    {
        return currentEnemy;
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

            // Populate random power-up cards
            PowerUpCardSpawner spawner = powerUpPanelObject.GetComponent<PowerUpCardSpawner>();
            if (spawner != null)
            {
                spawner.PopulatePowerUpPanel();
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
    }
}
