using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Hệ thống xử lý Click chuột tập trung. 
/// Sử dụng Raycast để bắn xuyên qua gỗ và chỉ tìm Ốc hoặc Lỗ.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Cấu hình")]
    [Tooltip("Layer chứa Ốc và Lỗ (Ví dụ Layer 8)")]
    public LayerMask interactableLayer;

    private Camera _cam;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _cam = Camera.main;
    }

    private void Update()
    {
        // Kiểm tra Click chuột trái hoặc Tap trên điện thoại
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        // 1. Nếu đang click đè lên UI (nút Pause, v.v.) thì bỏ qua
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 2. Bắn tia Raycast từ vị trí chuột
        Vector2 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        
        // TÌM KIẾM XUYÊN THẤU: Lấy tất cả vật thể tại điểm click, không bỏ sót ai
        Collider2D[] allHits = Physics2D.OverlapPointAll(mousePos);
        Debug.Log($"[InputManager] Click tại {mousePos}. Tìm thấy {allHits.Length} vật thể.");

        // A. Kiểm tra xem có trúng con Ốc không (Ưu tiên ốc trước)
        foreach (var hitObj in allHits)
        {
            Screw screw = hitObj.GetComponent<Screw>();
            if (screw == null) screw = hitObj.GetComponentInParent<Screw>();

            if (screw != null)
            {
                Debug.Log($"[InputManager] Click trúng ỐC: {screw.name}");
                screw.SelectScrew(); // Gọi hàm xử lý chọn ốc
                return;
            }
        }

        // B. Tìm cái lỗ (ScrewHole) có Sorting Order cao nhất (Ưu tiên Lỗ lớp trên)
        ScrewHole bestHole = null;
        int maxOrder = -999;

        foreach (var hitObj in allHits)
        {
            ScrewHole h = hitObj.GetComponent<ScrewHole>();
            if (h == null) h = hitObj.GetComponentInParent<ScrewHole>();

            if (h != null)
            {
                SpriteRenderer sr = h.GetComponent<SpriteRenderer>();
                if (sr == null) sr = h.GetComponentInChildren<SpriteRenderer>();
                int order = (sr != null) ? sr.sortingOrder : 0;

                if (order > maxOrder)
                {
                    maxOrder = order;
                    bestHole = h;
                }
            }
        }

        // 2. Nếu tìm thấy Lỗ, mới xét xem nó có bị chặn hay không
        if (bestHole != null)
        {
            // Tìm Sorting Order thực sự (Nếu lỗ tàng hình thì mượn của thanh gỗ chủ quản)
            int holeOrder = 0;
            SpriteRenderer hSr = bestHole.GetComponent<SpriteRenderer>();
            if (hSr == null) hSr = bestHole.GetComponentInChildren<SpriteRenderer>();
            
            if (hSr != null) holeOrder = hSr.sortingOrder;
            else if (bestHole.ownerPlank != null)
            {
                SpriteRenderer pSr = bestHole.ownerPlank.GetComponentInChildren<SpriteRenderer>();
                if (pSr != null) holeOrder = pSr.sortingOrder;
            }

            // PHÂN LOẠI vật thể để kiểm tra Tầm nhìn
            List<WoodPlank> hitPlanks = new List<WoodPlank>();
            List<ScrewHole> hitHoles = new List<ScrewHole>();

            foreach (var hitObj in allHits)
            {
                WoodPlank p = hitObj.GetComponentInParent<WoodPlank>();
                if (p != null && !hitPlanks.Contains(p)) hitPlanks.Add(p);

                ScrewHole h = hitObj.GetComponent<ScrewHole>();
                if (h == null) h = hitObj.GetComponentInParent<ScrewHole>();
                if (h != null && !hitHoles.Contains(h)) hitHoles.Add(h);
            }

            // Kiểm tra xem Lỗ có bị Gỗ "Thịt đặc" nằm trước đè lên không
            bool isBlocked = false;
            string blockReason = "";

            foreach (var plank in hitPlanks)
            {
                // Bỏ qua thanh gỗ đã rơi / đang rơi
                if (plank.IsFreed) continue;

                // Lấy Sorting Order chính xác (ưu tiên SortingGroup nếu có)
                int pOrder = 0;
                UnityEngine.Rendering.SortingGroup sg = plank.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sg != null) pOrder = sg.sortingOrder;
                else
                {
                    SpriteRenderer plankSr = plank.GetComponentInChildren<SpriteRenderer>();
                    pOrder = (plankSr != null) ? plankSr.sortingOrder : 0;
                }

                // Chỉ chặn nếu Gỗ nằm TRƯỚC (Layer cao hơn) lỗ
                if (pOrder <= holeOrder) continue;

                // KIỂM TRA 1: Thanh gỗ này có ScrewHole nào tại vị trí click không?
                bool hasHoleAtPoint = false;
                foreach (var h in hitHoles)
                {
                    if (h.ownerPlank == plank || h.transform.IsChildOf(plank.transform))
                    {
                        hasHoleAtPoint = true;
                        break;
                    }
                }

                // KIỂM TRA 2: Thanh gỗ này có HoleMask (SpriteMask) tại vị trí click không?
                // HoleMask là visual "đục lỗ" trên gỗ - nếu click trúng vùng mask thì gỗ coi như trong suốt
                if (!hasHoleAtPoint)
                {
                    SpriteMask[] masks = plank.GetComponentsInChildren<SpriteMask>();
                    foreach (var mask in masks)
                    {
                        if (mask.sprite == null) continue;
                        
                        // Kiểm tra điểm click có nằm trong bounds của HoleMask không
                        Bounds maskBounds = mask.bounds;
                        if (maskBounds.Contains(new Vector3(mousePos.x, mousePos.y, maskBounds.center.z)))
                        {
                            hasHoleAtPoint = true;
                            Debug.Log($"[InputManager] Điểm click nằm trong HoleMask '{mask.name}' của {plank.name} → cho phép xuyên qua!");
                            break;
                        }
                    }
                }

                if (!hasHoleAtPoint)
                {
                    isBlocked = true;
                    blockReason = plank.name;
                    break;
                }
            }

            if (!isBlocked)
            {
                Debug.Log($"[InputManager] Đã chọn LỖ: {bestHole.name} (Lớp: {holeOrder})");
                bestHole.OnClick(); // Cắm ốc thành công!
            }
            else
            {
                Debug.LogWarning($"[InputManager] Click bị CHẶN bởi {blockReason} (Nằm ở lớp trên lỗ {bestHole.name})");
            }
        }
    }
}
