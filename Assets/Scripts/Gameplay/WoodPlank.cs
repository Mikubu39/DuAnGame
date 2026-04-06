using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class WoodPlank : MonoBehaviour
{
    [HideInInspector]
    public List<Screw> _activeScrews = new List<Screw>();

    public Color plankColor = new Color(0.70f, 0.42f, 0.13f);

    [Header("Cài đặt tầng gỗ")]
    [Tooltip("Thanh gỗ chỉ va chạm với các thanh gỗ CÙNG số Layer ID")]
    public int layerID = 0;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private bool _isFreed = false;
    public bool IsFreed => _isFreed;

    private Coroutine _fallCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        // MỚI: Tìm SpriteRenderer ở các mục con (Square)
        _sr = GetComponentInChildren<SpriteRenderer>();

        // Tối ưu hóa vật lý chuyên nghiệp: Mượt mà và ổn định
        if (_rb != null)
        {
            _rb.gravityScale = 0f;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            _rb.mass = 1.0f;
            _rb.drag = 0.5f;
            _rb.angularDrag = 0.8f;
        }

        if (_sr != null && _sr.sprite == null) _sr.sprite = MakePlankSprite(plankColor);
    }

    private void Start()
    {
        // KHÔNG CẦN tìm nữa, vì con ốc sẽ tự động báo cho ta khi nó quét thấy ta ở Start()
        foreach (var screw in _activeScrews) EnsurePlankHole(screw);

        // MỚI: Chỉ va chạm với các thanh gỗ CÙNG TẦNG (dựa trên Layer ID)
        Collider2D myCol = GetComponentInChildren<Collider2D>(); // TÌM Ở MỤC CON (SQUARE)
        
        if (myCol == null)
        {
            Debug.LogWarning("Không tìm thấy Collider2D ở thanh gỗ: " + name);
            return;
        }

        WoodPlank[] allPlanks = FindObjectsOfType<WoodPlank>();
        foreach (var other in allPlanks)
        {
            if (other != this)
            {
                // Nếu KHÁC Layer ID thì xuyên qua nhau TUYỆT ĐỐI
                if (other.layerID != this.layerID)
                {
                    Collider2D otherCol = other.GetComponentInChildren<Collider2D>();
                    if (otherCol != null)
                    {
                        Physics2D.IgnoreCollision(myCol, otherCol);
                    }
                }
            }
        }
    }

    private void EnsurePlankHole(Screw screw)
    {
        // Kiểm tra xem đã có lỗ ở vị trí này chưa
        float radius = 0.15f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(screw.transform.position, radius);
        foreach (var h in hits)
        {
            if (h.GetComponent<ScrewHole>() != null && h.transform.IsChildOf(transform)) return;
        }

        // Tạo lỗ mới trực tiếp bằng code (không dùng Prefab vì GameManager không có)
        GameObject hObj = new GameObject($"PlankHole_{screw.name}");
        hObj.transform.SetParent(transform);
        hObj.transform.position = screw.transform.position;
        hObj.transform.localPosition += new Vector3(0, 0, -0.01f);
        hObj.layer = gameObject.layer;

        // Thêm SpriteRenderer và gán hình ảnh lỗ
        SpriteRenderer hSr = hObj.AddComponent<SpriteRenderer>();
        hSr.sprite = ScrewHole.MakeHoleSprite();
        hSr.sortingOrder = _sr != null ? _sr.sortingOrder + 1 : 10;
        hSr.color = new Color(0, 0, 0, 0); // Lỗ ẩn khi có ốc cắm vào

        // Thêm component ScrewHole và thiết lập
        ScrewHole sh = hObj.AddComponent<ScrewHole>();
        sh.holeType = ScrewHole.HoleType.PlankHole;
        sh.ownerPlank = this;
        sh.SetScrew(screw);

        // Thêm va chạm Trigger cho lỗ
        CircleCollider2D col = hObj.AddComponent<CircleCollider2D>();
        col.radius = 0.28f;
        col.isTrigger = true;
    }

    public void OnScrewDetached(Screw screw)
    {
        if (!gameObject.activeInHierarchy) return;

        _activeScrews.Remove(screw);

        // MỚI: Tìm cái lỗ tương ứng trên thanh gỗ này và báo nó đã trống
        foreach (var hole in GetComponentsInChildren<ScrewHole>())
        {
            if (Vector2.Distance(hole.transform.position, screw.transform.position) < 0.1f)
            {
                hole.SetScrew(null);
            }
        }

        if (_activeScrews.Count <= 0)
        {
            if (_fallCoroutine == null) _fallCoroutine = StartCoroutine(FallOff());
        }
        else if (_activeScrews.Count == 1)
        {
            // Nếu chỉ còn 1 ốc -> Cho phép đung đưa mượt mà (HingeJoint lo liệu)
            _rb.constraints = RigidbodyConstraints2D.None;
            _rb.gravityScale = 1.0f;
            _rb.WakeUp();
        }
    }

    public void OnScrewAttached(Screw screw)
    {
        if (!_activeScrews.Contains(screw))
            _activeScrews.Add(screw);

        // MỚI: Tìm cái lỗ tương ứng trên thanh gỗ này và báo nó đã có ốc (để tắt Collider của lỗ)
        foreach (var hole in GetComponentsInChildren<ScrewHole>())
        {
            if (Vector2.Distance(hole.transform.position, screw.transform.position) < 0.1f)
            {
                hole.SetScrew(screw);
            }
        }

        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
            _isFreed = false;
        }

        if (_activeScrews.Count >= 2)
        {
            // Nếu có từ 2 ốc trở lên thì khóa cứng hoàn toàn
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
        else if (_activeScrews.Count == 1)
        {
            // Nếu chỉ có 1 ốc thì để nó đung đưa mượt mà qua HingeJoint
            _rb.gravityScale = 1.0f;
            _rb.constraints = RigidbodyConstraints2D.None;
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