using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class UseItem : MonoBehaviour
{
    public Toggle useUnScrewToggle;


    public static bool isDestroyingScrew = false;

    private void Start()
    {
        useUnScrewToggle.onValueChanged.AddListener(OnUseUnscrew);
    }
    private void OnUseUnscrew(bool value)
    {
        if (Inventory.Instance.unscrews <= 0) return;
        isDestroyingScrew = value;
    }

    public void Use60s()
    {
        if (Inventory.Instance.UseTime60s())
        {
            CountdownTimer.timeRemaining += 60;
            UpdateVisual.Instance.UpdateItemQuantity();
        }
    }
}
