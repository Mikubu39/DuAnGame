using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public static float timeRemaining = 0f;
    public TextMeshProUGUI timerText;

    public static bool isRunning = true;

    void Update()
    {
        if (isRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                isRunning = false;
                UpdateTimerDisplay(0);
                GameManager.Instance.TimerEnded();
            }
        }
    }

    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        if (time <= 10)
        {
            timerText.color = Color.red;
        }
        else        {
            timerText.color = Color.white;
        }
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}