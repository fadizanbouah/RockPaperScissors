using UnityEngine;

public class RoomClearedTextRelay : MonoBehaviour
{
    private void Awake()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("[RoomClearedTextRelay] RoomManager.Instance not found!");
        }
    }

    // This method will be called by the Animation Event
    public void TriggerRoomClearedAnimationFinished()
    {
        Debug.Log("[RoomClearedTextRelay] TriggerRoomClearedAnimationFinished called, forwarding to RoomManager.");
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomClearedAnimationFinished();
        }
    }
}
