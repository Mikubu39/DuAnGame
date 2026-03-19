using UnityEngine;

/// <summary>
/// Generates a simple wooden plank texture at runtime using Texture2D.
/// Attach to your WoodPlank prefab to auto-generate wood grain texture.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlankTexturGenerator : MonoBehaviour
{
    [Header("Texture Settings")]
    public int texWidth = 256;
    public int texHeight = 64;
    public int grainLines = 6;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        GenerateWoodTexture(_sr.color);
    }

    public void GenerateWoodTexture(Color baseColor)
    {
        Texture2D tex = new Texture2D(texWidth, texHeight);
        Color[] pixels = new Color[texWidth * texHeight];

        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.3f);
                Color woodColor = new Color(
                    baseColor.r * (0.85f + noise * 0.15f),
                    baseColor.g * (0.85f + noise * 0.15f),
                    baseColor.b * (0.8f + noise * 0.2f)
                );
                // Add grain lines
                for (int g = 0; g < grainLines; g++)
                {
                    float grainY = (float)g / grainLines * texHeight;
                    float wave = Mathf.Sin(x * 0.02f + g) * 3f;
                    if (Mathf.Abs(y - grainY - wave) < 1f)
                        woodColor *= 0.92f;
                }
                // Round corners
                float cx = (float)x / texWidth - 0.5f;
                float cy = (float)y / texHeight - 0.5f;
                float cornerRadius = 0.08f;
                float cornerDist = Mathf.Max(Mathf.Abs(cx) - (0.5f - cornerRadius), 0f)
                                 + Mathf.Max(Mathf.Abs(cy) - (0.5f - cornerRadius), 0f);
                float alpha = cornerDist < cornerRadius ? 1f : 0f;
                pixels[y * texWidth + x] = new Color(woodColor.r, woodColor.g, woodColor.b, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        _sr.sprite = Sprite.Create(tex,
            new Rect(0, 0, texWidth, texHeight),
            new Vector2(0.5f, 0.5f), 100f);
    }
}
