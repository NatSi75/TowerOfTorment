using UnityEngine;
using UnityEngine.UI;

public static class CardGridLayout
{
    public static void Fit(Transform gridContent, int cardCount)
    {
        if (!gridContent.TryGetComponent(out GridLayoutGroup grid) || gridContent is not RectTransform rect) return;
        if (gridContent.TryGetComponent(out MobileUIScale mobileScale)) mobileScale.Apply();

        float baseHeight = rect.sizeDelta.y;
        ScrollRect scrollRect = rect.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
            baseHeight = viewport.rect.height * viewport.lossyScale.y / rect.lossyScale.y;
        }

        for (int i = 0; i < rect.childCount - cardCount; i++) rect.GetChild(i).gameObject.SetActive(false);

        Bounds? cards = CardBounds(rect, cardCount);
        if (cards == null) return;
        const float topMargin = 15f;
        float overflowTop = cards.Value.max.y + topMargin - rect.rect.yMax;
        if (overflowTop > 0f)
        {
            grid.padding.top += Mathf.CeilToInt(overflowTop);
            cards = CardBounds(rect, cardCount);
            if (cards == null) return;
        }

        const float bottomMargin = 60f;
        float height = Mathf.Max(baseHeight, rect.rect.yMax - cards.Value.min.y + bottomMargin);
        float growth = height - rect.sizeDelta.y;
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        rect.localPosition -= new Vector3(0f, growth * (1f - rect.pivot.y) * rect.localScale.y, 0f);

        if (scrollRect != null)
        {
            CenterHorizontally(rect, scrollRect, cardCount);
            Canvas.ForceUpdateCanvases();
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private static void CenterHorizontally(RectTransform grid, ScrollRect scrollRect, int cardCount)
    {
        Bounds? cards = CardBounds(grid, cardCount);
        if (cards == null) return;

        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
        Vector3 viewCenter = grid.InverseTransformPoint(viewport.TransformPoint(viewport.rect.center));
        float shift = viewCenter.x - cards.Value.center.x;
        grid.localPosition += new Vector3(shift * grid.localScale.x, 0f, 0f);
    }

    private static readonly Vector3[] corners = new Vector3[4];

    private static Bounds? CardBounds(RectTransform grid, int cardCount)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(grid);
        Matrix4x4 toGrid = grid.worldToLocalMatrix;
        Bounds? bounds = null;
        for (int i = Mathf.Max(0, grid.childCount - cardCount); i < grid.childCount; i++)
        {
            foreach (Image image in grid.GetChild(i).GetComponentsInChildren<Image>())
            {
                if (!image.enabled || image.sprite == null || image.color.a <= 0f) continue;
                Rect drawn = DrawnRect(image);
                RectTransform rt = image.rectTransform;
                corners[0] = rt.TransformPoint(drawn.xMin, drawn.yMin, 0f);
                corners[1] = rt.TransformPoint(drawn.xMax, drawn.yMax, 0f);
                for (int c = 0; c < 2; c++)
                {
                    Vector3 point = toGrid.MultiplyPoint3x4(corners[c]);
                    if (bounds == null) bounds = new Bounds(point, Vector3.zero);
                    else { Bounds b = bounds.Value; b.Encapsulate(point); bounds = b; }
                }
            }
        }
        return bounds;
    }

    private static Rect DrawnRect(Image image)
    {
        Rect rect = image.rectTransform.rect;
        if (!image.preserveAspect || image.type != Image.Type.Simple) return rect;

        float spriteAspect = image.sprite.rect.width / image.sprite.rect.height;
        if (rect.width / rect.height > spriteAspect)
        {
            float width = rect.height * spriteAspect;
            rect.x += (rect.width - width) * image.rectTransform.pivot.x;
            rect.width = width;
        }
        else
        {
            float height = rect.width / spriteAspect;
            rect.y += (rect.height - height) * image.rectTransform.pivot.y;
            rect.height = height;
        }
        return rect;
    }
}
