using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArea", menuName = "Rooms/AreaData")]
public class AreaData : ScriptableObject
{
    [Header("Area Info")]
    public string areaName = "Area 1";

    [Header("Room Pools")]
    [Tooltip("All room pools that belong to this area (played in sequence)")]
    public List<RoomPool> roomPools = new List<RoomPool>();

    [Header("Difficulty Scaling")]
    [Tooltip("Percentage increase per pool depth within this area (e.g., 5 = 5% per pool)")]
    public float scalingPercentPerPool = 5f;

    [Header("Optional: Future Features")]
    [Tooltip("Background music for this area")]
    public AudioClip areaMusic;

    [Tooltip("Cinematic to play when entering this area")]
    public GameObject areaCinematicPrefab;
}