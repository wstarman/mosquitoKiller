using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private Dictionary<string, AudioClip> clips =
        new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllSounds();
    }

    private void LoadAllSounds()
    {
        AudioClip[] allClips = Resources.LoadAll<AudioClip>("Sounds");

        foreach (AudioClip clip in allClips)
        {
            clips[clip.name] = clip;
        }

        Debug.Log($"Loaded {clips.Count} audio clips.");
    }

    public void PlayMusic(string clipName, bool loop = true)
    {
        if (!clips.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"Audio not found: {clipName}");
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (!clips.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"Audio not found: {clipName}");
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }
}