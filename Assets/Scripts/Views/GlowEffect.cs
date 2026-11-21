using DG.Tweening;
using UnityEngine;

// Soft glow drawn behind a card or a character, used as feedback when cards are dragged and played.
public static class GlowEffect
{
    public static readonly Color Ready = new(1f, 1f, 1f, 0.85f);
    public static readonly Color NotEnoughMana = new(1f, 0.25f, 0.2f, 0.85f);
    public static readonly Color Target = new(1f, 1f, 1f, 0.75f);

    private static Sprite sprite;
    private static Sprite ellipse;

    // A glow behind all sprites of 'root' (it follows root), hidden (alpha 0) until faded in.
    public static SpriteRenderer CreateBehind(Transform root, float padding, int sortingOrder, bool round = false)
    {
        Bounds? bounds = null;
        foreach (SpriteRenderer sr in root.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.sprite == null || !sr.enabled || sr.sprite == Sprite() || sr.sprite == Ellipse()) continue;
            if (bounds == null) bounds = sr.bounds;
            else { Bounds b = bounds.Value; b.Encapsulate(sr.bounds); bounds = b; }
        }
        if (bounds == null) return null;

        GameObject go = new("Glow");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(bounds.Value.center.x, bounds.Value.center.y, root.position.z);
        go.transform.rotation = Quaternion.identity;
        Vector3 parentScale = root.lossyScale;
        Vector3 size = bounds.Value.size * padding;
        go.transform.localScale = new Vector3(size.x / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)), size.y / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)), 1f);

        SpriteRenderer glow = go.AddComponent<SpriteRenderer>();
        glow.sprite = round ? Ellipse() : Sprite();
        glow.color = new Color(1f, 1f, 1f, 0f);
        glow.sortingOrder = sortingOrder;
        return glow;
    }

    // A short flash behind a character: the target of the card that was just played.
    public static void Flash(CombatantView target)
    {
        if (target == null || target.spriteRenderer == null) return;
        SpriteRenderer body = target.spriteRenderer;
        SpriteRenderer glow = CreateBehind(body.transform, 1.1f, body.sortingOrder - 1, true);
        if (glow == null) return;
        glow.sortingLayerID = body.sortingLayerID;
        Color color = Target;
        glow.color = new Color(color.r, color.g, color.b, 0f);
        Sequence flash = DOTween.Sequence().SetLink(glow.gameObject);
        flash.Append(glow.DOFade(color.a, 0.12f));
        flash.Append(glow.DOFade(0f, 0.45f));
        flash.OnComplete(() => Object.Destroy(glow.gameObject));
        body.transform.DOKill(true);
        body.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 6, 0.5f);
    }

    // Soft rounded rectangle, drawn once in code.
    public static Sprite Sprite()
    {
        if (sprite != null) return sprite;
        const int size = 64;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - size / 2f) - size * 0.3f);
                float dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - size / 2f) - size * 0.3f);
                float edge = Mathf.Sqrt(dx * dx + dy * dy) / (size * 0.2f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - edge)));
            }
        }
        texture.Apply();
        sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = "Glow";
        return sprite;
    }

    // Soft oval, for glows behind characters.
    public static Sprite Ellipse()
    {
        if (ellipse != null) return ellipse;
        const int size = 64;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - size / 2f) / (size / 2f);
                float dy = (y + 0.5f - size / 2f) / (size / 2f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) / 0.5f)));
            }
        }
        texture.Apply();
        ellipse = UnityEngine.Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        ellipse.name = "Glow Ellipse";
        return ellipse;
    }
}
