using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using System;

public class RewardedAdsManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
{
    [SerializeField] private Button showAdButton;
    [SerializeField] private string androidGameId = "6082913";
    [SerializeField] private bool testMode = true;

    private string _adUnitId;
    private string _gameId;

    public event Action OnRewardGranted;

    private bool _isAdLoaded = false;

    void Awake()
    {
        _gameId = androidGameId;
        _adUnitId = "Rewarded_Android";
    }
    void Start()
    {
        Advertisement.debugMode = true;

        if (string.IsNullOrEmpty(_gameId))
        {
            Debug.LogError("Game ID is missing! Set it in the Inspector.");
            return;
        }

        // Initialize Unity Ads
        Advertisement.Initialize(_gameId, testMode, this);

        if (showAdButton != null)
        {
            showAdButton.onClick.AddListener(ShowRewardedAd);
            showAdButton.interactable = false;
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads Initialized Successfully!");
        LoadRewardedAd();           // Safe to load ad now
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error} - {message}");
    }

    public void LoadRewardedAd()
    {
        _isAdLoaded = false;
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowRewardedAd()
    {
        if (_isAdLoaded)
        {
            Debug.Log("Showing rewarded ad...");
            Advertisement.Show(_adUnitId, this);
        }
        else
        {
            Debug.LogWarning("Ad not loaded yet. Trying to load again...");
            LoadRewardedAd();
        }
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == _adUnitId)
        {
            Debug.Log("Rewarded ad loaded successfully!");
            _isAdLoaded = true;

            if (showAdButton != null)
                showAdButton.interactable = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Failed to load ad {adUnitId}: {error} - {message}");
        _isAdLoaded = false;
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Ad show failed: {error} - {message}");
        _isAdLoaded = false;
        LoadRewardedAd();
    }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Ad watched completely → Granting reward");
            GrantReward();
        }
        else
        {
            Debug.Log("Ad skipped or failed");
        }

        _isAdLoaded = false;
        LoadRewardedAd();
    }

    private void GrantReward()
    {
        PlayerData.Instance.AddCoins(100);
        UpdateVisual.Instance.UpdateCoins();
        OnRewardGranted?.Invoke();
        Debug.Log("Player received reward!");
    }

    private void OnDestroy()
    {
        if (showAdButton != null)
            showAdButton.onClick.RemoveListener(ShowRewardedAd);
    }
}