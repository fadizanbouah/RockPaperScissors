using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NarratorEntry
{
    public string entryName;
    [Range(0f, 1f)] public float chance = 1f;
    public bool playOncePerSession = false;
    public AudioClip[] clips;
}

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource narratorSource;
    [Range(0f, 1f)]
    [SerializeField] private float narratorVolume = 1f;

    [Header("Narrator Entries")]
    [SerializeField] private NarratorEntry[] entries;

    private HashSet<string> _sessionPlayedEntries = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void TryPlay(string entryName)
    {
        NarratorEntry entry = System.Array.Find(entries, e => e.entryName == entryName);
        if (entry == null)
        {
            Debug.LogWarning($"[NarratorManager] Entry '{entryName}' not found.");
            return;
        }

        if (entry.playOncePerSession && _sessionPlayedEntries.Contains(entryName))
            return;

        if (narratorSource.isPlaying)
            return;

        if (Random.value > entry.chance)
            return;

        AudioClip clip = PickClip(entry);
        if (clip == null)
        {
            Debug.LogWarning($"[NarratorManager] Entry '{entryName}' has no valid clips.");
            return;
        }

        if (entry.playOncePerSession)
            _sessionPlayedEntries.Add(entryName);

        narratorSource.PlayOneShot(clip, narratorVolume);
    }

    private AudioClip PickClip(NarratorEntry entry)
    {
        if (entry.clips == null || entry.clips.Length == 0)
            return null;

        // Collect non-null clips and pick a random one
        int startIndex = Random.Range(0, entry.clips.Length);
        for (int i = 0; i < entry.clips.Length; i++)
        {
            AudioClip candidate = entry.clips[(startIndex + i) % entry.clips.Length];
            if (candidate != null) return candidate;
        }
        return null;
    }
}
