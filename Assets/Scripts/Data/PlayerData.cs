using UnityEngine;
using System;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    // Keys
    private const string KEY_COINS = "coins";
    private const string KEY_UNLOCKED_LEVEL = "unlockedLevel";
    private const string KEY_PLAYER_NAME = "playerName";
    private const string KEY_AVATAR_INDEX = "avatarIndex";
    private const string KEY_FRAME_INDEX = "frameIndex";
    private const string KEY_SOUND = "sound";
    private const string KEY_MUSIC = "music";
    private const string KEY_VIBRATION = "vibration";
    private const string KEY_TOTAL_COMPLETED = "totalCompleted";

    // Properties
    public int Coins { get; private set; }
    public int UnlockedLevel { get; private set; }
    public string PlayerName { get; private set; }
    public int AvatarIndex { get; private set; }
    public int FrameIndex { get; private set; }
    public bool SoundEnabled { get; private set; }
    public bool MusicEnabled { get; private set; }
    public bool VibrationEnabled { get; private set; }
    public int DailyStreak { get; private set; }
    public int TotalLevelsCompleted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Update()
    {
        UnlockedLevel = PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, 1);
        TotalLevelsCompleted = PlayerPrefs.GetInt(KEY_TOTAL_COMPLETED, 0);
    }
    public void Load()
    {
        Coins = PlayerPrefs.GetInt(KEY_COINS, 0);
        UnlockedLevel = PlayerPrefs.GetInt(KEY_UNLOCKED_LEVEL, 1);
        PlayerName = PlayerPrefs.GetString(KEY_PLAYER_NAME, "Player_001");
        AvatarIndex = PlayerPrefs.GetInt(KEY_AVATAR_INDEX, 0);
        FrameIndex = PlayerPrefs.GetInt(KEY_FRAME_INDEX, 0);
        SoundEnabled = PlayerPrefs.GetInt(KEY_SOUND, 1) == 1;
        MusicEnabled = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        VibrationEnabled = PlayerPrefs.GetInt(KEY_VIBRATION, 1) == 1;
        TotalLevelsCompleted = PlayerPrefs.GetInt(KEY_TOTAL_COMPLETED, 0);
    }

    private void Save() => PlayerPrefs.Save();

    public void AddCoins(int amount) { Coins += amount; PlayerPrefs.SetInt(KEY_COINS, Coins); Save(); }
    public bool SpendCoins(int amount) { if (Coins < amount) return false; Coins -= amount; PlayerPrefs.SetInt(KEY_COINS, Coins); Save(); return true; }
    public void UnlockLevel(int level) { if (level > UnlockedLevel) { UnlockedLevel = level; PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL, UnlockedLevel); Save(); } }
    public void SetPlayerName(string name) { PlayerName = name; PlayerPrefs.SetString(KEY_PLAYER_NAME, name); Save(); }
    public void SetAvatar(int index) { AvatarIndex = index; PlayerPrefs.SetInt(KEY_AVATAR_INDEX, index); Save(); }
    public void SetFrame(int index) { FrameIndex = index; PlayerPrefs.SetInt(KEY_FRAME_INDEX, index); Save(); }
    public void SetSound(bool on) { SoundEnabled = on; PlayerPrefs.SetInt(KEY_SOUND, on ? 1 : 0); Save(); }
    public void SetMusic(bool on) { MusicEnabled = on; PlayerPrefs.SetInt(KEY_MUSIC, on ? 1 : 0); Save(); }
    public void SetVibration(bool on) { VibrationEnabled = on; PlayerPrefs.SetInt(KEY_VIBRATION, on ? 1 : 0); Save(); }
    public void IncrementTotalCompleted() { TotalLevelsCompleted++; PlayerPrefs.SetInt(KEY_TOTAL_COMPLETED, TotalLevelsCompleted); Save(); }
    public void ResetUnlockedLevel() {PlayerPrefs.SetInt(KEY_UNLOCKED_LEVEL, 1); PlayerPrefs.SetInt(KEY_TOTAL_COMPLETED, 0); Save(); }
}
