using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string description;
        public int targetValue;
        public int coinReward;
        public bool completed;
    }

    public List<Achievement> achievements = new List<Achievement>()
    {
        new Achievement { id="first_level",   description="Complete the first level",                       targetValue=1,   coinReward=10 },
        new Achievement { id="100_levels",    description="Complete 100 levels",                            targetValue=100, coinReward=50 },
        new Achievement { id="speed_run",     description="Complete 20 levels in under 1 minute each",     targetValue=20,  coinReward=50 },
        new Achievement { id="no_lose_5",     description="Complete 5 consecutive levels without losing",  targetValue=5,   coinReward=50 },
        new Achievement { id="daily_7",       description="Log in and complete 1 level each day for 7 days",targetValue=7,  coinReward=50 },
    };

    private const string KEY_ACH_PREFIX = "ach_";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAchievements();
    }

    private void LoadAchievements()
    {
        foreach (var ach in achievements)
            ach.completed = PlayerPrefs.GetInt(KEY_ACH_PREFIX + ach.id, 0) == 1;
    }

    public void OnLevelCompleted(int levelNumber)
    {
        PlayerData.Instance?.IncrementTotalCompleted();

        // First level
        CheckAchievement("first_level", PlayerData.Instance?.TotalLevelsCompleted ?? 0);
        // 100 levels
        CheckAchievement("100_levels", PlayerData.Instance?.TotalLevelsCompleted ?? 0);
        // Daily 7
        CheckAchievement("daily_7", PlayerData.Instance?.DailyStreak ?? 0);
    }

    public void OnConsecutiveWin(int count)
    {
        CheckAchievement("no_lose_5", count);
    }

    public void OnLevelLost()
    {
        PlayerData.Instance?.SetConsecutiveWins(0);
    }

    private void CheckAchievement(string id, int currentValue)
    {
        var ach = achievements.Find(a => a.id == id);
        if (ach == null || ach.completed) return;
        if (currentValue >= ach.targetValue)
        {
            ach.completed = true;
            PlayerPrefs.SetInt(KEY_ACH_PREFIX + ach.id, 1);
            PlayerPrefs.Save();
            PlayerData.Instance?.AddCoins(ach.coinReward);
            Debug.Log($"Achievement Unlocked: {ach.description} +{ach.coinReward} coins!");
        }
    }

    public int GetProgress(string id)
    {
        switch (id)
        {
            case "first_level":  return PlayerData.Instance?.TotalLevelsCompleted ?? 0;
            case "100_levels":   return PlayerData.Instance?.TotalLevelsCompleted ?? 0;
            case "no_lose_5":    return PlayerData.Instance?.ConsecutiveWins ?? 0;
            case "daily_7":      return PlayerData.Instance?.DailyStreak ?? 0;
            default:             return 0;
        }
    }
}
