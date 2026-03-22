using UnityEngine;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    public Toggle musicToggle;
    public Toggle soundToggle;

    
    public Image musicImage;
    public Image soundImage;

    public Color onColor = Color.white;
    public Color offColor = Color.gray;

    private void Start()
    {
        musicToggle.isOn = PlayerData.Instance.MusicEnabled;
        soundToggle.isOn = PlayerData.Instance.SoundEnabled;

        musicToggle.onValueChanged.AddListener(OnMusicChanged);
        soundToggle.onValueChanged.AddListener(OnSoundChanged);
        UpdateVisual();
    }

    private void OnMusicChanged(bool value)
    {
        PlayerData.Instance.SetMusic(value);
        AudioManager.Instance.ApplySettings();
        UpdateVisual();
    }

    private void OnSoundChanged(bool value)
    {
        PlayerData.Instance.SetSound(value);
        AudioManager.Instance.ApplySettings();
        UpdateVisual();
    }

    void UpdateVisual()
    {
        musicImage.color = musicToggle.isOn ? onColor : offColor;
        soundImage.color = soundToggle.isOn ? onColor : offColor;
    }
}