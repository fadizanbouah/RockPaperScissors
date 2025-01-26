using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RoomManager : MonoBehaviour
{
    public List<RoomPool> roomPools; // List of room pools
    public Transform roomParent; // Parent object to instantiate rooms under
    public GameObject roomSelectionCanvas; // Canvas to display room choices
    public UnityEngine.UI.Button[] roomButtons; // Buttons for room choices
    public TextMeshProUGUI[] roomButtonTexts; // Text components for the buttons

    private int currentPoolIndex = 0;
    private GameObject currentRoomInstance;

    void Start()
    {
        if (roomPools == null || roomPools.Count == 0)
        {
            Debug.LogError("No room pools assigned in the RoomManager.");
            return;
        }

        if (roomSelectionCanvas == null)
        {
            Debug.LogError("RoomSelectionCanvas is not assigned in the RoomManager.");
            return;
        }

        if (roomParent == null)
        {
            Debug.LogError("RoomParent is not assigned in the RoomManager.");
            return;
        }

        if (roomButtons == null || roomButtons.Length == 0)
        {
            Debug.LogError("RoomButtons are not assigned in the RoomManager.");
            return;
        }

        if (roomButtonTexts == null || roomButtonTexts.Length == 0)
        {
            Debug.LogError("RoomButtonTexts are not assigned in the RoomManager.");
            return;
        }

        LoadNextRoomPool();
    }

    private void LoadNextRoomPool()
    {
        if (currentPoolIndex >= roomPools.Count)
        {
            Debug.Log("Game Over - No more room pools available.");
            return;
        }

        roomSelectionCanvas.SetActive(true);
        RoomPool currentPool = roomPools[currentPoolIndex];

        if (currentPool == null || currentPool.rooms == null || currentPool.rooms.Count == 0)
        {
            Debug.LogError("RoomPool is empty or not assigned correctly.");
            return;
        }

        List<RoomData> selectedRooms = currentPool.GetRandomRooms(3);

        for (int i = 0; i < roomButtons.Length; i++)
        {
            if (i < selectedRooms.Count && selectedRooms[i] != null)
            {
                int index = i;
                roomButtonTexts[i].text = selectedRooms[i].roomName;
                roomButtons[i].onClick.RemoveAllListeners();
                roomButtons[i].onClick.AddListener(() => SelectRoom(selectedRooms[index]));
                roomButtons[i].gameObject.SetActive(true);
            }
            else
            {
                roomButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectRoom(RoomData selectedRoom)
    {
        roomSelectionCanvas.SetActive(false);

        if (selectedRoom == null || selectedRoom.roomPrefab == null)
        {
            Debug.LogError("Selected room or its prefab is null.");
            return;
        }

        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        currentRoomInstance = Instantiate(selectedRoom.roomPrefab, roomParent);
        currentPoolIndex++;

        if (currentPoolIndex < roomPools.Count)
        {
            LoadNextRoomPool();
        }
        else
        {
            Debug.Log("Game Over - No more room pools available.");
        }
    }
}
