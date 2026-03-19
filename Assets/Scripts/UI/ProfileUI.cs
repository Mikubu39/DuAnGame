using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ProfileUI : MonoBehaviour
{
    public static ProfileUI Instance { get; private set; }

    [Header("Profile Info")]
    public TextMeshProUGUI playerNameText;
    public Image currentAvatarImage;
    public Image currentFrameImage;
    public TextMeshProUGUI levelBadgeText;
    public TextMeshProUGUI joinDateText;
    public Image countryFlag;

    [Header("Avatar Grid")]
    public Transform avatarGridParent;
    public GameObject avatarButtonPrefab;

    [Header("Frame Grid")]
    public Transform frameGridParent;
    public GameObject frameButtonPrefab;

    [Header("Tab Buttons")]
    public Button avatarTabButton;
    public Button frameTabButton;
    public GameObject avatarGrid;
    public GameObject frameGrid;

    [Header("Name Edit")]
    public Button editNameButton;
    public GameObject nameEditPanel;
    public TMP_InputField nameInput;
    public Button confirmNameButton;
    public Button cancelNameButton;

    [Header("Buttons")]
    public Button saveButton;
    public Button closeButton;

    [Header("Avatar Sprites")]
    public List<Sprite> avatarSprites = new List<Sprite>();
    [Header("Frame Sprites")]
    public List<Sprite> frameSprites = new List<Sprite>();

    private int _selectedAvatar;
    private int _selectedFrame;
    private bool _showingAvatars = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => RefreshProfile();

    private void Start()
    {
        saveButton?.onClick.AddListener(OnSave);
        closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
        editNameButton?.onClick.AddListener(() => nameEditPanel?.SetActive(true));
        confirmNameButton?.onClick.AddListener(OnConfirmName);
        cancelNameButton?.onClick.AddListener(() => nameEditPanel?.SetActive(false));
        avatarTabButton?.onClick.AddListener(() => ShowTab(true));
        frameTabButton?.onClick.AddListener(() => ShowTab(false));

        ShowTab(true);
        BuildAvatarGrid();
        BuildFrameGrid();
    }

    private void RefreshProfile()
    {
        if (PlayerData.Instance == null) return;
        _selectedAvatar = PlayerData.Instance.AvatarIndex;
        _selectedFrame = PlayerData.Instance.FrameIndex;

        if (playerNameText != null) playerNameText.text = PlayerData.Instance.PlayerName;
        if (levelBadgeText != null) levelBadgeText.text = $"Level {PlayerData.Instance.UnlockedLevel}";
        if (joinDateText != null) joinDateText.text = System.DateTime.Now.ToString("d/M/yyyy");

        UpdateCurrentAvatar();
        UpdateCurrentFrame();
    }

    private void BuildAvatarGrid()
    {
        if (avatarGridParent == null || avatarButtonPrefab == null) return;
        foreach (Transform child in avatarGridParent) Destroy(child.gameObject);

        for (int i = 0; i < avatarSprites.Count; i++)
        {
            int index = i;
            var btn = Instantiate(avatarButtonPrefab, avatarGridParent).GetComponent<Button>();
            var img = btn.GetComponent<Image>();
            if (img != null && index < avatarSprites.Count) img.sprite = avatarSprites[index];

            // Lock indicator: avatars > 0 are locked for new players
            bool locked = index > 0 && (PlayerData.Instance?.UnlockedLevel ?? 1) < (index * 5);
            btn.interactable = !locked;

            btn.onClick.AddListener(() => {
                _selectedAvatar = index;
                UpdateCurrentAvatar();
                AudioManager.Instance?.PlaySound("button_click");
            });
        }
    }

    private void BuildFrameGrid()
    {
        if (frameGridParent == null || frameButtonPrefab == null) return;
        foreach (Transform child in frameGridParent) Destroy(child.gameObject);

        for (int i = 0; i < frameSprites.Count; i++)
        {
            int index = i;
            var btn = Instantiate(frameButtonPrefab, frameGridParent).GetComponent<Button>();
            var img = btn.GetComponent<Image>();
            if (img != null && index < frameSprites.Count) img.sprite = frameSprites[index];
            btn.onClick.AddListener(() => {
                _selectedFrame = index;
                UpdateCurrentFrame();
                AudioManager.Instance?.PlaySound("button_click");
            });
        }
    }

    private void ShowTab(bool showAvatars)
    {
        _showingAvatars = showAvatars;
        if (avatarGrid != null) avatarGrid.SetActive(showAvatars);
        if (frameGrid != null) frameGrid.SetActive(!showAvatars);
    }

    private void UpdateCurrentAvatar()
    {
        if (currentAvatarImage != null && _selectedAvatar < avatarSprites.Count)
            currentAvatarImage.sprite = avatarSprites[_selectedAvatar];
    }

    private void UpdateCurrentFrame()
    {
        if (currentFrameImage != null && _selectedFrame < frameSprites.Count)
            currentFrameImage.sprite = frameSprites[_selectedFrame];
    }

    private void OnConfirmName()
    {
        string newName = nameInput?.text?.Trim();
        if (!string.IsNullOrEmpty(newName) && newName.Length <= 20)
        {
            PlayerData.Instance?.SetPlayerName(newName);
            if (playerNameText != null) playerNameText.text = newName;
        }
        nameEditPanel?.SetActive(false);
    }

    private void OnSave()
    {
        PlayerData.Instance?.SetAvatar(_selectedAvatar);
        PlayerData.Instance?.SetFrame(_selectedFrame);
        AudioManager.Instance?.PlaySound("button_click");
        gameObject.SetActive(false);
    }
}
