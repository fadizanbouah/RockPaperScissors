using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    private RoomData currentRoom;
    private int currentEnemyIndex = 0; // Track enemy index

    [SerializeField] private RoomPool roomPool; // Assign this in the Inspector
    [SerializeField] private SpriteRenderer roomBackground;
    [SerializeField] private Transform enemySpawnPoint;

    private HandController currentEnemy; // Store reference to the active enemy

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

    private void LoadRoom(RoomData room)
    {
        Debug.Log($"Loading Room: {room.roomName}");
        currentRoom = room;
        currentEnemyIndex = 0; // Reset enemy index when entering a new room

        // Set background
        if (roomBackground != null)
        {
            roomBackground.sprite = room.backgroundImage;
        }

        // Spawn the first enemy
        SpawnNextEnemy();
    }

    private void SpawnNextEnemy()
    {
        if (currentRoom == null || currentRoom.enemyPrefabs.Count == 0 || currentEnemyIndex >= currentRoom.enemyPrefabs.Count)
        {
            Debug.Log("No more enemies left in this room. Room cleared.");
            SelectNextRoom();
            return;
        }

        // Destroy previous enemy if it exists
        if (currentEnemy != null)
        {
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        // Instantiate the new enemy
        GameObject enemyInstance = Instantiate(currentRoom.enemyPrefabs[currentEnemyIndex], enemySpawnPoint.position, enemySpawnPoint.rotation);
        currentEnemy = enemyInstance.GetComponent<HandController>();

        if (currentEnemy != null)
        {
            currentEnemy.OnDeath += HandleEnemyDefeat; // Subscribe to enemy defeat event
            Debug.Log($"Spawned enemy: {currentRoom.enemyPrefabs[currentEnemyIndex].name}");

            // Notify GameStateManager that a new enemy is spawned
            GameStateManager.Instance.UpdateEnemy(currentEnemy);
        }
        else
        {
            Debug.LogError("Spawned enemy does not have a HandController script!");
        }

        currentEnemyIndex++; // Move to next enemy in the list
    }

    // Triggered when an enemy dies
    private void HandleEnemyDefeat(HandController defeatedEnemy)
    {
        Debug.Log($"{defeatedEnemy.gameObject.name} has been defeated! Checking for next enemy...");
        SpawnNextEnemy(); // Spawn the next enemy in the list
    }

    public void SelectNextRoom()
    {
        if (roomPool == null || roomPool.rooms.Count == 0)
        {
            Debug.LogError("No RoomPool assigned or empty! Cannot load next room.");
            return;
        }

        // Select a random room from the pool and load it
        currentRoom = roomPool.rooms[Random.Range(0, roomPool.rooms.Count)];
        Debug.Log($"Next Room Selected: {currentRoom.roomName}");
        LoadRoom(currentRoom);
    }

    // Method to return the currently active enemy
    public HandController GetCurrentEnemy()
    {
        return currentEnemy;
    }
}
