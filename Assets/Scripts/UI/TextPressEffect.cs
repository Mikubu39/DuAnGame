using UnityEngine;

public class TextPressEffect : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, -5f, 0);

    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }

    public void PressDown()
    {
        transform.localPosition = originalPos + offset;
    }

    public void Release()
    {
        transform.localPosition = originalPos;
    }
}