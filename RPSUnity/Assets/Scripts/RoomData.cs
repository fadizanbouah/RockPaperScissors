using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Game/RoomData")]
public class RoomData : ScriptableObject
{
    public string roomName;
    public GameObject roomPrefab;  // The prefab representing the room
    public int weight = 1;         // Weight for random selection
}
