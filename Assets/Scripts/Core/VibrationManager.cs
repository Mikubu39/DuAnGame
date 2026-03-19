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
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS haptic via native plugin
        iOSHaptics.ImpactLight();
#endif
        // In editor, just log
        Debug.Log("[Vibration] Buzz!");
    }
}
