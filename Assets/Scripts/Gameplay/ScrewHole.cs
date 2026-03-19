using UnityEngine;

public class ScrewHole : MonoBehaviour
{
    public enum HoleType { BoardHole, PlankHole }

    [Header("Loại lỗ")]
    public HoleType holeType = HoleType.BoardHole;

    [Header("Trạng thái")]
    [Tooltip("Tick = lỗ trống (không có ốc)")]
    public bool isEmpty = false;

    [HideInInspector] public Screw currentScrew;
    [HideInInspector] public WoodPlank ownerPlank;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && _sr.sprite == null)
            _sr.sprite = MakeHoleSprite();
        RefreshVisual();
    }

    private void Start()
    {
        if (holeType == HoleType.PlankHole)
            ownerPlank = GetComponentInParent<WoodPlank>();
    }

    public void SetScrew(Screw screw)
    {
        currentScrew = screw;
        isEmpty = (screw == null);
        RefreshVisual();

        // THÊM MỚI: Tắt nhận click của lỗ nếu đã có ốc cắm vào (để nhường click cho ốc)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = isEmpty;
    }

    private void RefreshVisual()
    {
        if (_sr == null) return;
        if (isEmpty) _sr.color = new Color(0.15f, 0.08f, 0.03f, 0.85f);
        else _sr.color = new Color(0f, 0f, 0f, 0f);
    }

    private static Sprite _cachedSprite;
    public static Sprite MakeHoleSprite()
    {
        if (_cachedSprite != null) return _cachedSprite;
        int sz = 48;
        var tex = new Texture2D(sz, sz);
        var px = new Color[sz * sz];
        float cx = sz / 2f, r = sz / 2f - 1f, inner = r * 0.70f;
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                float dx = x - cx, dy = y - cx;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                Color c = Color.clear;
                if (d <= r)
                {
                    c = new Color(0.10f, 0.06f, 0.02f, 1f);
                    if (d <= inner) c = new Color(0.04f, 0.02f, 0.00f, 1f);
                }
                px[y * sz + x] = c;
            }
        tex.SetPixels(px); tex.Apply();
        _cachedSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), Vector2.one * 0.5f, 100f);
        return _cachedSprite;
    }
}