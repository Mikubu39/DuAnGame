using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Apple.ReplayKit; // Thêm thư viện này để dùng TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject panelMainMenu;
    public GameObject panelGameplay;
    public GameObject panelWin;
    public GameObject panelLose;
    public GameObject panelPause;

    [Header("UI Texts")]
    public TextMeshProUGUI txtMenuPlayButton; // Chữ trên nút Play màu xanh
    public TextMeshProUGUI txtGameplayLevel;  // Chữ hiển thị level lúc đang chơi
    public TextMeshProUGUI txtWinningPrize;       

    [Header("Level Settings")]
    public List<GameObject> levelPrefabs;
    public Transform levelContainer;

    private GameObject currentLevelObject;
    private int currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Tối ưu hóa cho di động: Chạy ở 60 FPS để cảm giác mượt mà
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Tắt vSync để targetFrameRate có hiệu lực
    }

    private void Start()
    {
        UpdateLevelText();
        ShowUI(panelMainMenu);
    }
    public void StartGame()
    {
        ShowUI(panelGameplay);
        LoadLevel(currentLevelIndex);
    }

    public void Pause()
    {
        panelPause.SetActive(true);
        Time.timeScale = 0f;
        UseItem.isDestroyingScrew = false;
        UpdateVisual.Instance.UpdateUnscrewImg(false);
    }

    public void Resume()
    {
        panelPause.SetActive(false);
        Time.timeScale = 1f;
    }
    
    public void ReplayLevel()
    {
        ShowUI(panelGameplay);
        LoadLevel(currentLevelIndex);
    }
    public void WinGame()
    {
        ShowUI(panelWin);
        VibrationManager.Instance.Vibrate();
        txtWinningPrize.text = BoardController.Instance.prize.ToString();
        if (currentLevelIndex +1 > PlayerData.Instance.TotalLevelsCompleted)
        {
            PlayerData.Instance.AddCoins(BoardController.Instance.prize);
            UpdateVisual.Instance.UpdateCoins();
            PlayerData.Instance.IncrementTotalCompleted();
        }
        if (PlayerData.Instance.UnlockedLevel < currentLevelIndex + 2 && currentLevelIndex + 1 < levelPrefabs.Count) PlayerData.Instance.UnlockLevel(currentLevelIndex + 2 ) ;
        LevelLock.Instance.UpdateLevelButton();
    }

    public void TimerEnded()
    {
        Time.timeScale = 0f;
        VibrationManager.Instance.Vibrate();
        ShowUI(panelLose);
    }
    public void NextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levelPrefabs.Count)
        {
            Debug.Log("Hoàn thành tất cả các Level!");
            currentLevelIndex = 0;
            UpdateLevelText();
            ShowUI(panelMainMenu);
            if (currentLevelObject != null) Destroy(currentLevelObject);
            return;
        }

        UpdateLevelText();
        StartGame();
    }

    public void SelectLevel(int level)
    {
        if (level < 0 || level > PlayerData.Instance.UnlockedLevel) return;
        currentLevelIndex = level;
        UpdateLevelText();
        StartGame();
    }

    // --- CÁC HÀM MỚI CHO MENU ---

    public void BackToMenu()
    {
        // Dùng cho nút Pause lúc đang chơi
        if (currentLevelObject != null) Destroy(currentLevelObject);
        ShowUI(panelMainMenu);
    }

    private void UpdateLevelText()
    {
        // Tự động đổi chữ thành LEVEL 1, LEVEL 2...
        string levelString = "LEVEL " + (currentLevelIndex + 1).ToString();
        if (txtMenuPlayButton != null) txtMenuPlayButton.text = levelString;
        if (txtGameplayLevel != null) txtGameplayLevel.text = levelString;
    }

    private void LoadLevel(int index)
    {
        if (currentLevelObject != null) Destroy(currentLevelObject);
        if (levelPrefabs.Count > index && levelPrefabs[index] != null)
        {
            currentLevelObject = Instantiate(levelPrefabs[index], levelContainer);
            VibrationManager.Instance.Vibrate();
            TimeLimit();
            Resume();
        }
    }

    private void ShowUI(GameObject panelToShow)
    {
        panelMainMenu.SetActive(false);
        panelGameplay.SetActive(false);
        panelWin.SetActive(false);
        panelPause.SetActive(false);
        panelLose.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }

    void TimeLimit()
    {
        var levelInform = levelPrefabs[currentLevelIndex].GetComponent<LevelInformation>();
        if (levelInform.timeLimit > 0)
        {
            CountdownTimer.isRunning = true;
            CountdownTimer.timeRemaining = levelInform.timeLimit;
        }
        else
        {
            CountdownTimer.timeRemaining = 300;
        }
    }
}