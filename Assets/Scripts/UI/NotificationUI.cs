using System.Collections;
using TMPro;
using UnityEngine;


public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }
    public GameObject panel;
    public TextMeshProUGUI messageText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        panel.SetActive(false);
    }
    public void ShowNotification(string message)
    {
        messageText.text = message;
        panel.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideAfterSeconds(3f));
    }

    IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        panel.SetActive(false);
    }
}
