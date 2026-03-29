using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyItems : MonoBehaviour
{
    TextMeshPro txt;
    public void BuyUnscrew(int amount = 1)
    {
        if (PlayerData.Instance.SpendCoins(10))
        {
            Inventory.Instance.AddUnscrew();           
            PlayerPrefs.Save();
        }
        else{
            NotificationUI.Instance.ShowNotification("Not enough coins to buy unscrew(s)");
        }
    }
    public void Buy30sTime(int amount = 1)
    {
        if (PlayerData.Instance.SpendCoins(25))
        {
            Inventory.Instance.AddTime60s();           
            PlayerPrefs.Save();
        }
        else{
            NotificationUI.Instance.ShowNotification("Not enough coins to buy +60s.");
        }
    }
}
