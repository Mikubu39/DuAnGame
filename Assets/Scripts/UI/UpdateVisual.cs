using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class UpdateVisual : MonoBehaviour
{
    public static UpdateVisual Instance { get; private set; }
    public TextMeshProUGUI unscrewQuantity;
    public TextMeshProUGUI plus60sQuantity;
    public TextMeshProUGUI txtCoins;

    public Image unScrewImage;

    public Toggle useUnScrewToggle;

    public Color onColor = Color.red;
    public Color offColor = Color.white;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    void Start()
    {
        UpdateItemQuantity();
        UpdateCoins();
        useUnScrewToggle.onValueChanged.AddListener(IsUsingUS);
    }

    public void UpdateItemQuantity()
    {
        unscrewQuantity.text = Inventory.Instance.unscrews.ToString();
        plus60sQuantity.text = Inventory.Instance.time60s.ToString();
    }

    public void UpdateCoins()
    {
        txtCoins.text = PlayerData.Instance.Coins.ToString();
    }

    public void UpdateUnscrewImg(bool value)
    {
        useUnScrewToggle.isOn = value;
        unScrewImage.color = value ? onColor : offColor;
    }

    private void IsUsingUS(bool value)
    {
        if (Inventory.Instance.unscrews <= 0) return;
        UpdateUnscrewImg(value);
    }
}
