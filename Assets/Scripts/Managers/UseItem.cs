using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class UseItem : MonoBehaviour
{
    public Toggle useUnScrewToggle;

    public Image unScrewImage;
    public TextMeshProUGUI unscrewQuantity;
    public TextMeshProUGUI plus60sQuantity;

    public Color onColor = Color.white;
    public Color offColor = Color.gray;

    public static bool isDestroyingScrew = false;

    private void Start()
    {
        useUnScrewToggle.onValueChanged.AddListener(OnUseUnscrew);
        VisualUpdate();
    }
    private void OnUseUnscrew(bool value)
    {
        if (Inventory.Instance.unscrews <= 0) return;
        isDestroyingScrew = value;
        VisualUpdate();
    }

    private void Update()
    {
        Debug.Log("isDestroying: " + isDestroyingScrew);
    }

    public void Use60s()
    {
        if (Inventory.Instance.UseTime60s())
        {
            CountdownTimer.timeRemaining += 60;
        }
    }
    private void VisualUpdate()
    {
        unscrewQuantity.text = Inventory.Instance.unscrews.ToString();
        plus60sQuantity.text = Inventory.Instance.time60s.ToString();
        unScrewImage.color = useUnScrewToggle.isOn ? onColor : offColor;
    }

}
