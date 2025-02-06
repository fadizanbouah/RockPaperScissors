using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Rooms/RoomData")]
public class RoomData : ScriptableObject
{
    public string roomName;
    public Sprite backgroundImage;
    public List<GameObject> enemyPrefabs; // List of enemy prefabs for this room
    public int weight = 1; // Rarity weight (default to 1)

    public GameObject GetNextEnemyPrefab()
    {
        if (enemyPrefabs.Count > 0)
        {
            GameObject nextEnemy = enemyPrefabs[0];
            enemyPrefabs.RemoveAt(0);
            return nextEnemy;
        }

        return null; // No more enemies left in the room
    }
}
