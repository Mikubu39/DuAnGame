using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class WoodPlank : MonoBehaviour
{
    [HideInInspector]
    public List<Screw> _activeScrews = new List<Screw>();

    public Color plankColor = new Color(0.70f, 0.42f, 0.13f);

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private bool _isFreed = false;
    public bool IsFreed => _isFreed;

    private Coroutine _fallCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (_sr != null && _sr.sprite == null) _sr.sprite = MakePlankSprite(plankColor);
    }

    private void Start()
    {
        // ĐÃ SỬA LỖI Ở ĐÂY: Chuyển sang dùng s.pinningPlanks.Contains(this)
        Screw[] allScrewsInScene = FindObjectsOfType<Screw>();
        foreach (Screw s in allScrewsInScene)
        {
            if (s.pinningPlanks.Contains(this) && !_activeScrews.Contains(s))
            {
                _activeScrews.Add(s);
            }
        }

        foreach (var screw in _activeScrews) EnsurePlankHole(screw);
    }

    private void EnsurePlankHole(Screw screw)
    {
        foreach (var hole in GetComponentsInChildren<ScrewHole>())
        {
            if (hole.holeType == ScrewHole.HoleType.PlankHole)
            {
                if (Vector2.Distance(hole.transform.position, screw.transform.position) < 0.15f) return;
            }
        }

        var hObj = new GameObject($"PlankHole_{screw.name}");
        hObj.transform.SetParent(transform);
        hObj.transform.position = screw.transform.position;
        hObj.layer = gameObject.layer;

        var sr = hObj.AddComponent<SpriteRenderer>();
        sr.sprite = ScrewHole.MakeHoleSprite();
        sr.sortingOrder = _sr != null ? _sr.sortingOrder + 1 : 3;
        sr.color = new Color(0f, 0f, 0f, 0f);

        var hole2 = hObj.AddComponent<ScrewHole>();
        hole2.holeType = ScrewHole.HoleType.PlankHole;
        hole2.isEmpty = false;
        hole2.ownerPlank = this;


        var col = hObj.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.isTrigger = true; // Thêm dòng này để tránh ván va chạm vật lý với lỗ trống

        hObj.AddComponent<ScrewHoleClick>();
        hole2.SetScrew(screw);
    }

    public void OnScrewDetached(Screw screw)
    {
        if (!gameObject.activeInHierarchy) return;

        _activeScrews.Remove(screw);

        if (_activeScrews.Count <= 0)
        {
            if (_fallCoroutine == null) _fallCoroutine = StartCoroutine(FallOff());
        }
        else
        {
            _rb.constraints = RigidbodyConstraints2D.None;
            _rb.gravityScale = 1.2f;
            _rb.WakeUp();
        }
    }

    public void OnScrewAttached(Screw screw)
    {
        if (!_activeScrews.Contains(screw))
            _activeScrews.Add(screw);

        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
            _isFreed = false;
        }

        if (_activeScrews.Count >= 1)
        {
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }

    private IEnumerator FallOff()
    {
        _isFreed = true;
        _rb.constraints = RigidbodyConstraints2D.None;
        _rb.gravityScale = 2.5f;

        yield return new WaitForSeconds(2.5f);

        if (gameObject != null) gameObject.SetActive(false);

        // THÊM DÒNG NÀY: Báo cho trọng tài biết là "Tôi đã rớt rồi nhé, kiểm tra xem thắng chưa!"
        if (BoardController.Instance != null) BoardController.Instance.CheckWinCondition();
    }

    public static Sprite MakePlankSprite(Color col)
    {
        int W = 256, H = 64, cr = 14;
        var tex = new Texture2D(W, H);
        var px = new Color[W * H];
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                if (!InRR(x, y, W, H, cr)) { px[y * W + x] = Color.clear; continue; }
                float n = Mathf.PerlinNoise(x * 0.042f, y * 0.45f);
                float n2 = Mathf.PerlinNoise(x * 0.018f + 60f, y * 0.18f + 30f);
                float g = n * 0.65f + n2 * 0.35f;
                float ed = Mathf.Clamp01(Mathf.Min(x, y, W - 1 - x, H - 1 - y) / (float)cr);
                px[y * W + x] = new Color(
                    col.r * (0.84f + g * 0.16f) * (0.87f + ed * 0.13f),
                    col.g * (0.82f + g * 0.18f) * (0.87f + ed * 0.13f),
                    col.b * (0.80f + g * 0.20f) * (0.87f + ed * 0.13f), 1f);
            }
        tex.SetPixels(px); tex.Apply(); tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, W, H), Vector2.one * 0.5f, 100f);
    }

    private static bool InRR(int px2, int py, int W, int H, int r)
    {
        if (px2 < 0 || px2 >= W || py < 0 || py >= H) return false;
        int x1 = r, x2 = W - r - 1, y1 = r, y2 = H - r - 1;
        if (px2 < x1 && py < y1) return Df(px2, py, x1, y1) <= r;
        if (px2 > x2 && py < y1) return Df(px2, py, x2, y1) <= r;
        if (px2 < x1 && py > y2) return Df(px2, py, x1, y2) <= r;
        if (px2 > x2 && py > y2) return Df(px2, py, x2, y2) <= r;
        return true;
    }
    private static float Df(int x1, int y1, int x2, int y2)
    { float a = x1 - x2, b = y1 - y2; return Mathf.Sqrt(a * a + b * b); }
}