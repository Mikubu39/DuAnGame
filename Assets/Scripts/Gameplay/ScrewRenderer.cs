using UnityEngine;

/// <summary>
/// Generates a screw sprite at runtime.
/// Attach to your Screw prefab for auto-generated visuals.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ScrewRenderer : MonoBehaviour
{
    [Header("Screw Appearance")]
    public Color screwHeadColor = new Color(0.7f, 0.7f, 0.3f); // golden
    public Color slotColor = new Color(0.3f, 0.3f, 0.1f);
    public int texSize = 64;

    private void Awake()
    {
        GetComponent<SpriteRenderer>().sprite = CreateScrewSprite();
    }

    private Sprite CreateScrewSprite()
    {
        Texture2D tex = new Texture2D(texSize, texSize);
        Color[] pixels = new Color[texSize * texSize];
        float center = texSize * 0.5f;
        float radius = texSize * 0.45f;
        float innerRadius = texSize * 0.35f;
        float slotWidth = texSize * 0.08f;

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                Color c = Color.clear;
                if (dist <= radius)
                {
                    // Outer ring (darker)
                    c = screwHeadColor * 0.7f;
                    if (dist <= innerRadius)
                    {
                        // Inner circle
                        c = screwHeadColor;
                        // Add shading gradient
                        c = Color.Lerp(c, Color.white, (1f - dist / innerRadius) * 0.3f);
                    }
                    // Slot cross (+)
                    if (Mathf.Abs(dx) < slotWidth || Mathf.Abs(dy) < slotWidth)
                    {
                        if (dist <= innerRadius * 0.9f)
                            c = slotColor;
                    }
                    c.a = 1f;
                }
                pixels[y * texSize + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), 100f);
    }
}
