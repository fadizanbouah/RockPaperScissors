using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Rooms/RoomData")]
public class RoomData : ScriptableObject
{
    public string roomName;
    public Sprite backgroundImage;
    public List<HandController> enemies; // List of enemies in the room
    public int weight = 1; // Rarity weight (default to 1)
    public GameObject enemyPrefab; // The enemy assigned to this room

    public HandController GetNextEnemy()
    {
        if (enemies.Count > 0)
        {
            HandController nextEnemy = enemies[0];
            enemies.RemoveAt(0);
            return nextEnemy;
        }

        return null; // No more enemies left in the room
    }
}
