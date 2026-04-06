using UnityEngine;

/// <summary>
/// [ĐÃ VÔ HIỆU HÓA] Script cũ dùng OnMouseDown() để xử lý click lỗ ốc.
/// Đã bị thay thế bởi InputManager.HandleInput() có đầy đủ kiểm tra blocking.
/// OnMouseDown bypass hoàn toàn hệ thống chặn tầm nhìn → ốc gắn xuyên qua gỗ.
/// 
/// GIỮ LẠI FILE NÀY để Unity không mất reference trong prefab.
/// Nếu muốn dọn dẹp hoàn toàn: xóa component ScrewHoleClick khỏi ScrewHolePrefab.
/// </summary>
[RequireComponent(typeof(ScrewHole))]
[RequireComponent(typeof(CircleCollider2D))]
public class ScrewHoleClick : MonoBehaviour
{
    // Không làm gì cả — InputManager đã xử lý click tập trung
    // OnMouseDown đã bị xóa để tránh bypass blocking check
}