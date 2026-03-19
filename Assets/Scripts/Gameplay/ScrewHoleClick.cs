using UnityEngine;

[RequireComponent(typeof(ScrewHole))]
[RequireComponent(typeof(CircleCollider2D))]
public class ScrewHoleClick : MonoBehaviour
{
    private ScrewHole _hole;

    private void Awake() => _hole = GetComponent<ScrewHole>();

    private void OnMouseDown()
    {
        // CHỈ cho phép đặt ốc xuống nếu lỗ trống và người chơi đang giữ ốc
        if (_hole.isEmpty && Screw.HasHeld)
        {
            Screw.TryPlace(_hole);
        }
    }
}