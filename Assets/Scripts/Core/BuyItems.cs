using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyItems : MonoBehaviour
{
    TextMeshPro txt;
    public void BuyUnscrew(int amount = 1)
    {
        if (PlayerData.Instance.SpendCoins(amount == 1 ? 25 : (amount - 1) * 25))
        {
            Inventory.Instance.AddUnscrew(amount);           
            PlayerPrefs.Save();
        }
        else{
            NotificationUI.Instance.ShowNotification("Not enough coins to buy unscrew(s)");
        }
    }
    public void Buy60sTime(int amount = 1)
    {
        if (PlayerData.Instance.SpendCoins(amount == 1? 10: (amount - 1) * 10))
        {
            Inventory.Instance.AddTime60s(amount);           
            PlayerPrefs.Save();
        }
        else{
            NotificationUI.Instance.ShowNotification("Not enough coins to buy +60s.");
        }
    }
}
