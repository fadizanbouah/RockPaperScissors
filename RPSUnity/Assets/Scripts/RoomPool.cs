using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRoomPool", menuName = "Room System/Room Pool")]
public class RoomPool : ScriptableObject
{
    [System.Serializable]
    public class RoomEntry
    {
        public RoomData room;
        public int weight;  // Determines the likelihood of this room appearing
    }

    public List<RoomEntry> rooms;  // List of room entries with weight

    public List<RoomData> GetRandomRooms(int count)
    {
        List<RoomData> selectedRooms = new List<RoomData>();

        // Create weighted room selection
        List<RoomEntry> weightedList = new List<RoomEntry>();
        foreach (var entry in rooms)
        {
            for (int i = 0; i < entry.weight; i++)
            {
                weightedList.Add(entry);
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (weightedList.Count == 0) break;

            int randomIndex = Random.Range(0, weightedList.Count);
            selectedRooms.Add(weightedList[randomIndex].room);
            weightedList.RemoveAt(randomIndex);
        }

        return selectedRooms;
    }
}
