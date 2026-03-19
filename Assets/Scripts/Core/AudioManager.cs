using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Sounds")]
    public List<SoundEntry> sounds = new List<SoundEntry>();
    public AudioClip backgroundMusic;

    private Dictionary<string, SoundEntry> _soundMap = new Dictionary<string, SoundEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
            if (!_soundMap.ContainsKey(s.name)) _soundMap[s.name] = s;
    }

    private void Start()
    {
        ApplySettings();
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            if (PlayerData.Instance?.MusicEnabled == true)
                musicSource.Play();
        }
    }

    public void PlaySound(string soundName)
    {
        if (PlayerData.Instance?.SoundEnabled == false) return;
        if (_soundMap.TryGetValue(soundName, out SoundEntry entry) && entry.clip != null)
            sfxSource.PlayOneShot(entry.clip, entry.volume);
    }

    public void ApplySettings()
    {
        if (PlayerData.Instance == null) return;
        if (sfxSource != null) sfxSource.mute = !PlayerData.Instance.SoundEnabled;
        if (musicSource != null)
        {
            musicSource.mute = !PlayerData.Instance.MusicEnabled;
            if (PlayerData.Instance.MusicEnabled && !musicSource.isPlaying && musicSource.clip != null)
                musicSource.Play();
        }
    }
}
