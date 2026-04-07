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

        // MỚI: Ốc luôn là Kinematic để không bị gỗ đẩy hay tự nó làm xáo trộn vật lý
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;
    }

    private void Start()
    {
        _restPos = transform.position;

        // AUTO-DETECT: Tìm tất cả thanh gỗ và lỗ nằm ở vị trí này
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.2f);
        foreach (var hit in hits)
        {
            // Tìm WoodPlank (kiểm tra cả mục cha nếu collider ở mục con)
            WoodPlank plank = hit.GetComponentInParent<WoodPlank>();
            if (plank != null && !pinningPlanks.Contains(plank))
            {
                pinningPlanks.Add(plank);
                plank.OnScrewAttached(this);
            }

            // Tự động tìm lỗ nếu chưa được gán
            if (currentHole == null)
            {
                ScrewHole hole = hit.GetComponentInParent<ScrewHole>();
                if (hole != null) currentHole = hole;
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

    public void SelectScrew()
    {
        if (isMoving) return;

        // TRƯỜNG HỢP: Dùng vật phẩm phá ốc
        if (UseItem.isDestroyingScrew)
        {
            if (Inventory.Instance.UseUnscrew())
            {
                SelfDestruct();
                UseItem.isDestroyingScrew = false;
                UpdateVisual.Instance.UpdateUnscrewImg(false);
                return;
            }
        }

        if (!isSelected) PickUp();
        else PutDown_Cancel();
    }

    public void SelfDestruct()
    {
        if (isMoving) return;
        
        // 1. Giải phóng lỗ ốc
        if (currentHole != null) currentHole.SetScrew(null);

        // 2. Phá bỏ các liên kết vật lý và báo cho các tấm ván
        DestroyJoint();
        foreach (WoodPlank plank in pinningPlanks)
        {
            if (plank != null)
            {
                plank.GetComponent<Rigidbody2D>().isKinematic = false;
                plank.OnScrewDetached(this);
            }
        }

        // 3. Nếu đang bị cầm thì bỏ ra
        if (_held == this) _held = null;

        // 4. Hiệu ứng âm thanh
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound("Screw");

        // 5. Biến mất
        Destroy(gameObject);
    }

    public void PickUp()
    {
        if (_held != null && _held != this) _held.PutDown_Cancel();

        isSelected = true;
        _held = this;
        
        // GIẢI PHÓNG LỖ: Khi nhấc ốc lên, báo cho lỗ biết nó đang trống để nó hiện ra (màu nâu)
        if (currentHole != null) currentHole.SetScrew(null);

        _sr.color = new Color(1f, 0.92f, 0.25f);
        _sr.sortingOrder = 10;

        // TẮT collider tạm thời — LiftAnim sẽ bật lại khi ốc đã lên vị trí bay
        // Tắt để không đẩy gỗ khi di chuyển, nhưng sẽ bật lại ở vị trí mới 
        // để vẫn chặn được gỗ khác (solid physics)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // MỚI: Chỉ bỏ qua va chạm với những thanh gỗ mà nó đang ghim
        // Việc này giúp ốc không đẩy gỗ (khi nhấc lên) nhưng vẫn đỡ được gỗ khác chồng lên nó
        foreach (WoodPlank plank in pinningPlanks)
        {
            Collider2D pCol = plank.GetComponentInChildren<Collider2D>();
            if (pCol != null && col != null) Physics2D.IgnoreCollision(col, pCol, true);
        }

        DestroyJoint();
        
        // Đóng băng tất cả các ván đang bị ốc này ghim
        foreach (WoodPlank plank in pinningPlanks)
        {
            if (plank != null)
            {
                Rigidbody2D rb = plank.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }
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

        // Collider sẽ được bật lại bởi ReturnAnimAndReconnect sau khi hạ cánh
        // KHÔNG bật ở đây vì ốc còn đang bay về → sẽ đẩy gỗ

        AudioManager.Instance.PlaySound("Screw");
        StopAllCoroutines();
        StartCoroutine(ReturnAnimAndReconnect());
    }

    private IEnumerator LiftAnim()
    {
        Collider2D col = GetComponent<Collider2D>();

        // Bay lên — collider TẮT (không đẩy gỗ)
        Vector3 start = transform.position;
        Vector3 end = _restPos + Vector3.up * liftHeight;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.12f;
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // Đã lên tới vị trí → BẬT LẠI collider (solid) để chặn gỗ khác
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
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

        // TẮT collider trong khi bay về (không đẩy gỗ)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Vector3 start = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(start, _restPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        transform.position = _restPos;

        // Báo cho các thanh gỗ biết ốc đã gắn lại
        foreach (var plank in pinningPlanks)
        {
            if (plank != null)
            {
                Collider2D pCol = plank.GetComponentInChildren<Collider2D>();
                if (pCol != null && col != null) Physics2D.IgnoreCollision(col, pCol, false);
                plank.OnScrewAttached(this);
            }
        }

        // Tạo Joint TRƯỚC khi bật collider
        if (pinningPlanks.Count > 0) CreateJoint();

        // BẬT LẠI collider (solid) sau khi đã tạo Joint
        yield return new WaitForFixedUpdate();
        if (col != null)
        {
            col.isTrigger = false;
            col.enabled = true;
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

        // TẮT collider trong suốt quá trình bay (không đẩy gỗ)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // BÀI TOÁN GỖ ĐỨNG IM: Lưu lại danh sách cũ nhưng CHƯA tháo gỗ vội
        ScrewHole oldHole = currentHole;
        List<WoodPlank> oldPlanks = new List<WoodPlank>(pinningPlanks);
        pinningPlanks.Clear();

        Vector3 sp = transform.position;
        Vector3 ep = target.transform.position;
        Vector3 mp = (sp + ep) * 0.5f + Vector3.up * 0.5f;
        float elapsed = 0f;

        // GIAI ĐOẠN 1: Bay sang lỗ mới (Trong lúc này Gỗ ở lỗ cũ vẫn FREEZE tuyệt đối)
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

        // GIAI ĐOẠN 2: Đã hạ cánh — tháo gỗ cũ
        if (oldHole != null) oldHole.SetScrew(null);
        foreach (var p in oldPlanks)
        {
            if (p != null)
            {
                Collider2D pCol = p.GetComponentInChildren<Collider2D>();
                if (pCol != null && col != null) Physics2D.IgnoreCollision(col, pCol, false);
                p.GetComponent<Rigidbody2D>().isKinematic = false;
                p.OnScrewDetached(this);
            }
        }

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

        // GIAI ĐOẠN 3: Gắn vào các ván mới (OnScrewAttached tự xử lý physics đúng)
        foreach (var p in pinningPlanks)
        {
            if (p != null)
            {
                p.OnScrewAttached(this);
                // KHÔNG gọi isKinematic = false — OnScrewAttached đã xử lý đúng rồi
            }
        }

        // Tạo Joint TRƯỚC khi khôi phục collider (Joint tắt collision giữa connected bodies)
        CreateJoint();

        // BẬT LẠI collider (solid) sau khi đã tạo Joint
        yield return new WaitForFixedUpdate();
        if (col != null)
        {
            col.isTrigger = false;
            col.enabled = true;
        }

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