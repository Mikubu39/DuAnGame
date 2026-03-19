using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns coin icons that fly from the game board to the coin counter in the HUD.
/// Call CoinFlyAnimation.Instance.PlayCoinFly(count, startWorldPos) to trigger.
/// </summary>
public class CoinFlyAnimation : MonoBehaviour
{
    public static CoinFlyAnimation Instance { get; private set; }

    [Header("References")]
    public Canvas uiCanvas;
    public RectTransform coinTarget; // The coin icon in HUD
    public GameObject coinIconPrefab; // Simple Image prefab

    [Header("Settings")]
    public int coinsToSpawn = 8;
    public float flyDuration = 0.8f;
    public float spawnDelay = 0.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlayCoinFly(int coinCount, Vector3 worldStartPos)
    {
        if (coinIconPrefab == null || uiCanvas == null) return;
        int count = Mathf.Min(coinCount, coinsToSpawn);
        StartCoroutine(SpawnCoins(count, worldStartPos));
    }

    private IEnumerator SpawnCoins(int count, Vector3 worldStartPos)
    {
        Camera cam = Camera.main;
        Vector2 screenPos = cam.WorldToScreenPoint(worldStartPos);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 50f;
            SpawnCoin(screenPos + offset);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnCoin(Vector2 screenStartPos)
    {
        if (uiCanvas == null) return;
        var coin = Instantiate(coinIconPrefab, uiCanvas.transform);
        var rect = coin.GetComponent<RectTransform>();

        // Convert screen pos to canvas local pos
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiCanvas.GetComponent<RectTransform>(),
            screenStartPos,
            uiCanvas.worldCamera,
            out Vector2 localPos
        );
        rect.anchoredPosition = localPos;

        StartCoroutine(FlyToTarget(rect));
    }

    private IEnumerator FlyToTarget(RectTransform coinRect)
    {
        if (coinTarget == null) { Destroy(coinRect.gameObject); yield break; }

        Vector2 startPos = coinRect.anchoredPosition;
        Vector2 targetPos = coinTarget.anchoredPosition;
        Vector2 midPoint = Vector2.Lerp(startPos, targetPos, 0.5f) + Vector2.up * 100f;

        float elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            // Bezier curve
            Vector2 p = Mathf.Pow(1 - t, 2) * startPos +
                        2 * (1 - t) * t * midPoint +
                        t * t * targetPos;
            coinRect.anchoredPosition = p;

            // Scale pulse
            float scale = 1f - t * 0.3f;
            coinRect.localScale = Vector3.one * scale;
            yield return null;
        }

        AudioManager.Instance?.PlaySound("coin_earn");
        Destroy(coinRect.gameObject);
    }
}
