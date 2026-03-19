using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoardBackground : MonoBehaviour
{
    public Color boardColor  = new Color(0.87f, 0.77f, 0.56f);
    public Color borderColor = new Color(0.60f, 0.46f, 0.28f);

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = 0;
        sr.sprite = Build();
    }

    private Sprite Build()
    {
        int W = 480, H = 680, B = 22, CR = 28;
        var tex = new Texture2D(W, H);
        var px  = new Color[W * H];
        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            bool outer = IR(x,y,0,0,W,H,CR);
            bool inner = IR(x,y,B,B,W-B*2,H-B*2,Mathf.Max(0,CR-B));
            Color c = Color.clear;
            if (outer)
            {
                if (inner)
                {
                    float n  = Mathf.PerlinNoise(x*0.014f, y*0.07f);
                    float n2 = Mathf.PerlinNoise(x*0.007f+70f, y*0.028f+40f);
                    float g  = n*0.6f + n2*0.4f;
                    c = new Color(
                        boardColor.r*(0.90f+g*0.10f),
                        boardColor.g*(0.88f+g*0.12f),
                        boardColor.b*(0.86f+g*0.14f), 1f);
                }
                else
                {
                    float sh = 0.80f + 0.20f*((float)y/H);
                    c = new Color(borderColor.r*sh, borderColor.g*sh, borderColor.b*sh, 1f);
                }
            }
            px[y*W+x] = c;
        }
        tex.SetPixels(px); tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0,0,W,H), Vector2.one*0.5f, 75f);
    }

    private bool IR(int px2,int py,int x,int y,int w,int h,int r)
    {
        if(px2<x||px2>=x+w||py<y||py>=y+h) return false;
        int x1=x+r,x2=x+w-r-1,y1=y+r,y2=y+h-r-1;
        if(px2<x1&&py<y1) return Df(px2,py,x1,y1)<=r;
        if(px2>x2&&py<y1) return Df(px2,py,x2,y1)<=r;
        if(px2<x1&&py>y2) return Df(px2,py,x1,y2)<=r;
        if(px2>x2&&py>y2) return Df(px2,py,x2,y2)<=r;
        return true;
    }
    private float Df(int x1,int y1,int x2,int y2)
    { float a=x1-x2,b=y1-y2; return Mathf.Sqrt(a*a+b*b); }
}
