using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    private RoomData currentRoom;

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

        // Set background
        if (roomBackground != null)
        {
            roomBackground.sprite = room.backgroundImage;
        }

        // Spawn enemy
        if (room.enemyPrefab != null)
        {
            GameObject enemyInstance = Instantiate(room.enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
            currentEnemy = enemyInstance.GetComponent<HandController>();

            if (currentEnemy != null)
            {
                currentEnemy.OnDeath += HandleRoomCompletion; // Subscribe to enemy defeat event
            }
            else
            {
                Debug.LogError("Spawned enemy does not have a HandController script!");
            }
        }
        else
        {
            Debug.LogError("Room does not have an assigned enemyPrefab!");
        }
    }

    // Triggered when an enemy dies
    private void HandleRoomCompletion(HandController defeatedEnemy)
    {
        Debug.Log("Room Cleared! Selecting Next Room...");
        SelectNextRoom();
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
