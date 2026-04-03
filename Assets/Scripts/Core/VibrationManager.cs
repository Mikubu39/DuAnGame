using UnityEngine;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Vibrate()
    {
        if (PlayerData.Instance?.VibrationEnabled != true) return;
        Handheld.Vibrate();
        Debug.Log("[Vibration] Buzz!");
    }
}
