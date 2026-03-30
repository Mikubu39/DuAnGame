using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Screw : MonoBehaviour
{
    [Header("Gán trong Inspector")]
    public ScrewHole currentHole;
    public List<WoodPlank> pinningPlanks = new List<WoodPlank>();

    private List<HingeJoint2D> _joints = new List<HingeJoint2D>();

    [Header("Settings")]
    public float liftHeight = 0.8f;
    public float moveDuration = 0.25f;

    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool isMoving = false;

    private static Screw _held;
    private SpriteRenderer _sr;
    private Vector3 _restPos;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr.sprite == null) _sr.sprite = MakeScrewSprite();
    }

    private void Start()
    {
        _restPos = transform.position;

        // AUTO-DETECT: Tự động dùng sóng âm quét xem có tấm ván nào nằm dưới đít con ốc không
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        foreach (var hit in hits)
        {
            WoodPlank plank = hit.GetComponent<WoodPlank>();
            // Đề phòng collider nằm ở vật thể con
            if (plank == null) plank = hit.GetComponentInParent<WoodPlank>();

            // Nếu phát hiện ván và ván chưa có trong danh sách -> Tự động thêm vào!
            if (plank != null && !pinningPlanks.Contains(plank))
            {
                pinningPlanks.Add(plank);
            }
        }

        if (currentHole != null) currentHole.SetScrew(this);

        // Tạo chốt khóa vật lý
        if (pinningPlanks.Count > 0) CreateJoint();
    }

    private void CreateJoint()
    {
        if (pinningPlanks.Count == 0) return;

        foreach (WoodPlank plank in pinningPlanks)
        {
            if (plank == null) continue;
            HingeJoint2D joint = plank.gameObject.AddComponent<HingeJoint2D>();
            joint.connectedBody = GetComponent<Rigidbody2D>();
            joint.anchor = plank.transform.InverseTransformPoint(transform.position);
            joint.connectedAnchor = Vector2.zero;
            joint.enableCollision = false;

            _joints.Add(joint);
        }
    }

    private void DestroyJoint()
    {
        foreach (var joint in _joints)
        {
            if (joint != null) Destroy(joint);
        }
        _joints.Clear();
    }

    private void OnMouseDown()
    {
        if (isMoving) return;

        if (!isSelected) PickUp();
        else PutDown_Cancel();
    }

    public void PickUp()
    {
        if (_held != null && _held != this) _held.PutDown_Cancel();

        isSelected = true;
        _held = this;
        _sr.color = new Color(1f, 0.92f, 0.25f);
        _sr.sortingOrder = 10;

        DestroyJoint();
        if (currentHole != null) currentHole.SetScrew(null);

        // Đánh thức tất cả các ván đang bị ốc này ghim để chúng rơi xuống
        foreach (WoodPlank plank in pinningPlanks)
        {
            if (plank != null)
            {
                plank.GetComponent<Rigidbody2D>().isKinematic = true;
                plank.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                plank.GetComponent<Rigidbody2D>().angularVelocity = 0f;
            }
        }
        AudioManager.Instance.PlaySound("Screw");
        StopAllCoroutines();
        StartCoroutine(LiftAnim());
    }

    public void PutDown_Cancel()
    {
        if (!isSelected) return;
        isSelected = false;
        if (_held == this) _held = null;
        _sr.color = Color.white;
        _sr.sortingOrder = 5;

        if (currentHole != null) currentHole.SetScrew(this);
        AudioManager.Instance.PlaySound("Screw");
        StopAllCoroutines();
        StartCoroutine(ReturnAnimAndReconnect());
    }

    private IEnumerator LiftAnim()
    {
        Vector3 start = transform.position;
        Vector3 end = _restPos + Vector3.up * liftHeight;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.12f;
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        while (isSelected)
        {
            transform.position = end + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.05f;
            yield return null;
        }
    }

    private IEnumerator ReturnAnimAndReconnect()
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(start, _restPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        transform.position = _restPos;

        if (pinningPlanks.Count > 0)
        {
            CreateJoint();
            foreach (var plank in pinningPlanks)
            {
                if (plank != null) plank.GetComponent<Rigidbody2D>().isKinematic = false;
            }
        }
        isMoving = false;
    }

    public void PlaceIntoHole(ScrewHole target)
    {
        if (isMoving || !isSelected) return;
        AudioManager.Instance.PlaySound("Screw");
        StartCoroutine(PlaceAnim(target));
    }

    private IEnumerator PlaceAnim(ScrewHole target)
    {
        isMoving = true;
        isSelected = false;
        if (_held == this) _held = null;
        _sr.color = Color.white;

        ScrewHole oldHole = currentHole;

        // Lưu danh sách ván cũ để tháo ốc
        List<WoodPlank> oldPlanks = new List<WoodPlank>(pinningPlanks);
        pinningPlanks.Clear();

        if (oldHole != null) oldHole.SetScrew(null);

        // Báo cho các ván cũ biết ốc đã rút ra
        foreach (var p in oldPlanks)
        {
            if (p != null)
            {
                p.GetComponent<Rigidbody2D>().isKinematic = false;
                p.OnScrewDetached(this);
            }
        }

        Vector3 sp = transform.position;
        Vector3 ep = target.transform.position;
        Vector3 mp = (sp + ep) * 0.5f + Vector3.up * 0.5f;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float u = 1f - t;
            transform.position = u * u * sp + 2 * u * t * mp + t * t * ep;
            transform.Rotate(0, 0, -500f * Time.deltaTime);
            yield return null;
        }
        transform.position = ep;
        transform.rotation = Quaternion.identity;
        _sr.sortingOrder = 5;

        currentHole = target;
        _restPos = ep;
        target.SetScrew(this);

        // Tự động quét tìm các ván gỗ nằm dưới cái lỗ mới
        Collider2D[] hits = Physics2D.OverlapCircleAll(ep, 0.2f);
        foreach (var hit in hits)
        {
            WoodPlank plank = hit.GetComponentInParent<WoodPlank>();
            if (plank != null && !pinningPlanks.Contains(plank))
            {
                pinningPlanks.Add(plank);
            }
        }

        // Khóa các ván mới và tạo kết nối
        foreach (var p in pinningPlanks)
        {
            if (p != null)
            {
                p.OnScrewAttached(this);
                p.GetComponent<Rigidbody2D>().isKinematic = false;
            }
        }

        CreateJoint();
        isMoving = false;
    }

    public static void TryPlace(ScrewHole target)
    {
        if (_held == null) return;
        if (!target.isEmpty) return;
        _held.PlaceIntoHole(target);
    }

    public static bool HasHeld => _held != null;
    public static void ClearHeld() { if (_held != null) _held.PutDown_Cancel(); }

    public static Sprite MakeScrewSprite()
    {
        int sz = 64; float cx = sz / 2f;
        var tex = new Texture2D(sz, sz);
        var px = new Color[sz * sz];
        float outerR = sz / 2f - 1f, innerR = sz * 0.30f, slot = sz * 0.07f;
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                float dx = x - cx, dy = y - cx, d = Mathf.Sqrt(dx * dx + dy * dy);
                Color c = Color.clear;
                if (d <= outerR)
                {
                    c = new Color(0.48f, 0.42f, 0.10f);
                    if (d <= innerR)
                    {
                        float hi = 1f - d / innerR;
                        c = new Color(0.80f + hi * 0.15f, 0.70f + hi * 0.15f, 0.20f + hi * 0.08f);
                        if (Mathf.Abs(dx) < slot || Mathf.Abs(dy) < slot)
                            c = new Color(0.15f, 0.10f, 0.02f);
                    }
                    c.a = 1f;
                }
                px[y * sz + x] = c;
            }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, sz, sz), Vector2.one * 0.5f, 100f);
    }
}