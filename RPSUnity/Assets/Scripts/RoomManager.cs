using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] private List<RoomPool> roomPools; // List of pools in sequence
    [SerializeField] private SpriteRenderer roomBackground;
    [SerializeField] private Transform enemySpawnPoint;

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

    // This method will now be called from GameStateManager when Gameplay starts
    public void StartRoomSequence()
    {
        SelectNextRoom();
    }

    private void LoadRoom(RoomData room)
    {
        Debug.Log($"Loading Room: {room.roomName}");
        currentRoom = room;
        currentEnemyIndex = 0; // Reset enemy index when entering a new room

        // Set background for the room
        if (roomBackground != null)
        {
            roomBackground.sprite = room.backgroundImage;
        }

        // Spawn the first enemy in the room
        SpawnNextEnemy();
    }

    private void SpawnNextEnemy()
    {
        if (currentRoom == null || currentRoom.enemyPrefabs.Count == 0 || currentEnemyIndex >= currentRoom.enemyPrefabs.Count)
        {
            Debug.Log("No more enemies left in this room. Moving to next room.");
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
            currentEnemy.OnDeath += HandleEnemyDefeat;
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

    // Called when an enemy is defeated
    private void HandleEnemyDefeat(HandController defeatedEnemy)
    {
        Debug.Log($"{defeatedEnemy.gameObject.name} has been defeated! Checking for next enemy...");
        SpawnNextEnemy();
    }

    public void SelectNextRoom()
    {
        if (roomPools.Count == 0)
        {
            Debug.LogError("No RoomPools assigned! Cannot load next room.");
            return;
        }

        // Get the current pool
        RoomPool currentPool = roomPools[currentPoolIndex];

        if (currentPool == null || currentPool.rooms.Count == 0)
        {
            Debug.LogError($"RoomPool at index {currentPoolIndex} is empty or missing!");
            return;
        }

        // Select a random room from the current pool
        currentRoom = currentPool.rooms[Random.Range(0, currentPool.rooms.Count)];
        Debug.Log($"Next Room Selected: {currentRoom.roomName}");

        LoadRoom(currentRoom);

        // Move to the next pool in sequence (loop back if at the end)
        currentPoolIndex = (currentPoolIndex + 1) % roomPools.Count;
    }

    public HandController GetCurrentEnemy()
    {
        return currentEnemy;
    }
}
