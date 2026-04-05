using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelLock : MonoBehaviour
{
    public static LevelLock Instance { get; private set; }

    [System.Serializable]
    public class LevelData
    {
        public int levelIndex;
        public Button levelButton;
    }

    [Header("Level")]
    public List<LevelData> levels = new List<LevelData>();
    Dictionary<int, LevelData> levelButtonMap = new Dictionary<int, LevelData>();
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var level in levels)
        {
            if (!levelButtonMap.ContainsKey(level.levelIndex))
            {
                levelButtonMap.Add(level.levelIndex, level);
            }
        }
    }

    void Start()
    {
        foreach (var level in levels)
        {
            Transform child = level.levelButton.transform.GetChild(0);
            TextMeshProUGUI text = child.GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = level.levelIndex.ToString();
            }
            if(level.levelIndex > PlayerPrefs.GetInt("unlockedLevel", 1))
            {
                level.levelButton.interactable = false;
            }
        }
    }
}
