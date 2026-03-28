using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private const string KEY_UNSCREW = "unscrews";
    private const string KEY_TIME30S = "30s";

    public int unscrews { get; private set; }
    public int time30s { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Load()
    {
        unscrews = PlayerPrefs.GetInt(KEY_UNSCREW, 0);
        time30s = PlayerPrefs.GetInt(KEY_TIME30S, 0);
    }

    public void AddUnscrew(int amount = 1)
    {
        unscrews += amount;
        PlayerPrefs.SetInt(KEY_UNSCREW, unscrews);
        PlayerPrefs.Save();
    }

    public void AddTime30s(int amount = 1)
    {
        time30s += amount;
        PlayerPrefs.SetInt(KEY_TIME30S, time30s);
        PlayerPrefs.Save();
    }

    public bool UseUnscrew()
    {
        if (unscrews <= 0) return false;
        unscrews--;
        PlayerPrefs.SetInt(KEY_UNSCREW, unscrews);
        PlayerPrefs.Save();
        return true;
    }

    public bool UseTime30s()
    {
        if (time30s <= 0) return false;
        time30s--;
        PlayerPrefs.SetInt(KEY_TIME30S, time30s);
        PlayerPrefs.Save();
        return true;
    }
}
