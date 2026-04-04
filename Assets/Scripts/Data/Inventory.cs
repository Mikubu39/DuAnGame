using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    private const string KEY_UNSCREW = "unscrews";
    private const string KEY_TIME60S = "60s";

    public int unscrews { get; private set; }
    public int time60s { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();

        // TEST SUPPORT: Nếu chưa có item nào thì tặng luôn 10 cái để test!
        if (unscrews <= 0) AddUnscrew(10);
        if (time60s <= 0) AddTime60s(10);
    }

    public void Load()
    {
        unscrews = PlayerPrefs.GetInt(KEY_UNSCREW, 0);
        time60s = PlayerPrefs.GetInt(KEY_TIME60S, 0);
    }

    public void AddUnscrew(int amount = 1)
    {
        unscrews += amount;
        PlayerPrefs.SetInt(KEY_UNSCREW, unscrews);
        PlayerPrefs.Save();
    }

    public void AddTime60s(int amount = 1)
    {
        time60s += amount;
        PlayerPrefs.SetInt(KEY_TIME60S, time60s);
        PlayerPrefs.Save();
    }

    public bool UseUnscrew()
    {
        if (unscrews <= 0) return false;
        unscrews--;
        PlayerPrefs.SetInt(KEY_UNSCREW, unscrews);
        PlayerPrefs.Save();

        if (UpdateVisual.Instance != null) UpdateVisual.Instance.UpdateItemQuantity();
        return true;
    }

    public bool UseTime60s()
    {
        if (time60s <= 0) return false;
        time60s--;
        PlayerPrefs.SetInt(KEY_TIME60S, time60s);
        PlayerPrefs.Save();
        return true;
    }
}
