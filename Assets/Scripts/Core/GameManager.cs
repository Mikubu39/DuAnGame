using UnityEngine;
using System.Collections.Generic;
using TMPro; // Thêm thư viện này để dùng TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject panelMainMenu;
    public GameObject panelGameplay;
    public GameObject panelWin;
    public GameObject panelSettings; // Khai báo thêm bảng Settings

    [Header("UI Texts")]
    public TextMeshProUGUI txtMenuPlayButton; // Chữ trên nút Play màu xanh
    public TextMeshProUGUI txtGameplayLevel;  // Chữ hiển thị level lúc đang chơi

    [Header("Level Settings")]
    public List<GameObject> levelPrefabs;
    public Transform levelContainer;

    private GameObject currentLevelObject;
    private int currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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

    public void WinGame()
    {
        ShowUI(panelWin);
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

    // --- CÁC HÀM MỚI CHO MENU ---

    public void OpenSettings()
    {
        panelSettings.SetActive(true); // Chỉ bật đè lên, không tắt MainMenu
    }

    public void CloseSettings()
    {
        panelSettings.SetActive(false);
    }

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
        }
    }

    private void ShowUI(GameObject panelToShow)
    {
        panelMainMenu.SetActive(false);
        panelGameplay.SetActive(false);
        panelWin.SetActive(false);
        panelSettings.SetActive(false); // Ẩn settings nếu đang mở

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}