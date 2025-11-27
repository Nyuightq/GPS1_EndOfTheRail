using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remarks: Go region Usage Examples for guide.
// Remark: for future use, change to service locator (auto intantiate across different scene)

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    [HideInInspector]
    public AudioSource source;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private Sound[] soundEffects;

    [Header("Music Tracks")]
    [SerializeField] private Sound[] musicTracks;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> musicDictionary = new Dictionary<string, Sound>();
    private string currentMusicTrack = "";
    [SerializeField] private string startMusic;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Instance.PlayMusic(startMusic);
    }

    private void InitializeAudio()
    {
        // Create AudioSources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }

        // Initialize sound effects dictionary
        foreach (Sound s in soundEffects)
        {
            if (!soundDictionary.ContainsKey(s.name))
            {
                soundDictionary.Add(s.name, s);
            }
        }

        // Initialize music tracks dictionary
        foreach (Sound m in musicTracks)
        {
            if (!musicDictionary.ContainsKey(m.name))
            {
                musicDictionary.Add(m.name, m);
            }
        }
    }

    #region Sound Effects
    // Play a sound effect by name
    public void PlaySFX(string soundName)
    {
        Debug.Log(soundName + " " + sfxVolume * masterVolume + " " + masterVolume);
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundName}' not found!");
        }
    }

    // Play a sound effect with custom volume
    public void PlaySFX(string soundName, float volumeMultiplier)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sfxSource.pitch = sound.pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume * volumeMultiplier * sfxVolume * masterVolume);
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundName}' not found!");
        }
    }


    // Play a sound effect with custom pitch
    public void PlaySFXWithPitch(string soundName, float pitch)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundName}' not found!");
        }
    }

    // Play a random sound effect from multiple options (useful for variations)
    public void PlayRandomSFX(params string[] soundNames)
    {
        if (soundNames.Length == 0) return;
        string randomSound = soundNames[Random.Range(0, soundNames.Length)];
        PlaySFX(randomSound);
    }
    #endregion

    #region Music
    // Play a music track by name
    public void PlayMusic(string musicName)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            if (currentMusicTrack == musicName && musicSource.isPlaying)
                return;

            musicSource.clip = music.clip;
            musicSource.volume = music.volume * musicVolume * masterVolume;
            musicSource.pitch = music.pitch;
            musicSource.loop = music.loop;
            musicSource.Play();
            currentMusicTrack = musicName;
        }
        else
        {
            Debug.LogWarning($"Music track '{musicName}' not found!");
        }
    }


    // Play music with fade in effect
    public void PlayMusicWithFade(string musicName, float fadeDuration = 1f)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            StartCoroutine(FadeMusic(musicName, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"Music track '{musicName}' not found!");
        }
    }


    // Stop the currently playing music
    public void StopMusic()
    {
        musicSource.Stop();
        currentMusicTrack = "";
    }


    // Stop music with fade out effect
    public void StopMusicWithFade(float fadeDuration = 1f)
    {
        StartCoroutine(FadeOutMusic(fadeDuration));
    }


    // Pause the currently playing music
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    // Resume the paused music
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    private IEnumerator FadeMusic(string musicName, float duration)
    {
        float startVolume = musicSource.volume;

        // Fade out current music
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / (duration / 2);
            yield return null;
        }

        // Play new music
        PlayMusic(musicName);

        // Fade in new music
        musicSource.volume = 0f;
        float targetVolume = musicDictionary[musicName].volume * musicVolume * masterVolume;
        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += targetVolume * Time.deltaTime / (duration / 2);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }
        StopMusic();
    }
    #endregion

    #region Volume Controls
    // Set master volume (affects all audio)
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    // Set music volume
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    // Set SFX volume
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateMusicVolume()
    {
        if (musicSource.isPlaying && !string.IsNullOrEmpty(currentMusicTrack))
        {
            if (musicDictionary.TryGetValue(currentMusicTrack, out Sound music))
            {
                musicSource.volume = music.volume * musicVolume * masterVolume;
            }
        }
    }

    public void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
            Debug.Log("[SoundManager] Instance destroyed for replay.");
        }
    }


    // Get current master volume
    public float GetMasterVolume() => masterVolume;

    // <summary>
    // Get current music volume
    public float GetMusicVolume() => musicVolume;

    // Get current SFX volume
    public float GetSFXVolume() => sfxVolume;
    #endregion

    #region Utility
    // Check if a sound effect exists
    public bool HasSound(string soundName)
    {
        return soundDictionary.ContainsKey(soundName);
    }

    // Check if a music track exists
    public bool HasMusic(string musicName)
    {
        return musicDictionary.ContainsKey(musicName);
    }

    // Get the currently playing music track name
    public string GetCurrentMusicTrack() => currentMusicTrack;

    // Check if music is currently playing
    public bool IsMusicPlaying() => musicSource.isPlaying;
    #endregion
}

#region Usage Examples
// Play sound effects from any script
// SoundManager.Instance.PlaySFX("Jump");
// SoundManager.Instance.PlaySFX("Coin", 0.5f); // With custom volume

// Play music
// SoundManager.Instance.PlayMusic("LevelTheme");
// SoundManager.Instance.PlayMusicWithFade("BossTheme", 2f);

// Control volume
// SoundManager.Instance.SetMasterVolume(0.8f);
// SoundManager.Instance.SetMusicVolume(0.5f);
// SoundManager.Instance.SetSFXVolume(0.7f);

// Music controls
// SoundManager.Instance.PauseMusic();
// SoundManager.Instance.ResumeMusic();
// SoundManager.Instance.StopMusic();

// Play random variation
//SoundManager.Instance.PlayRandomSFX("Hit1", "Hit2", "Hit3");
#endregion