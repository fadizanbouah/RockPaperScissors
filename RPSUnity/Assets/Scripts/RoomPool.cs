using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRoomPool", menuName = "Rooms/RoomPool")]
public class RoomPool : ScriptableObject
{
    public List<RoomData> rooms; // List of rooms in this pool

    public RoomData GetRandomRoom()
    {
        if (rooms.Count == 0) return null;

        int totalWeight = 0;
        foreach (RoomData room in rooms)
        {
            totalWeight += room.weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int accumulatedWeight = 0;

        foreach (RoomData room in rooms)
        {
            accumulatedWeight += room.weight;
            if (randomValue < accumulatedWeight)
            {
                return room;
            }
        }

        return null;
    }
}
