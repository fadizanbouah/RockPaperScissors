using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Settings")]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Header("SFX Settings")]
    [SerializeField] private float sfxVolume = 1f;

    private Coroutine crossfadeCoroutine;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio sources
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        // Initialize SFX source
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void PlayAreaMusic(AudioClip newMusic)
    {
        if (newMusic == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null music clip!");
            return;
        }

        // If same music is already playing, do nothing
        if (musicSource.clip == newMusic && musicSource.isPlaying)
        {
            Debug.Log($"[AudioManager] {newMusic.name} is already playing");
            return;
        }

        Debug.Log($"[AudioManager] Changing music to: {newMusic.name}");

        // Stop any ongoing crossfade
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }

        // Start crossfade
        crossfadeCoroutine = StartCoroutine(CrossfadeMusic(newMusic));
    }

    public void StopMusic()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }

        crossfadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume); // multiply by sfxVolume
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume) // NEW METHOD
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        // Fade out current music
        while (elapsed < crossfadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (crossfadeDuration / 2f));
            yield return null;
        }

        // Switch to new clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < crossfadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / (crossfadeDuration / 2f));
            yield return null;
        }

        musicSource.volume = musicVolume;
        crossfadeCoroutine = null;
    }

    private IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / crossfadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
        crossfadeCoroutine = null;
    }
}