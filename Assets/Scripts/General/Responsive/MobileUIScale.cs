using UnityEngine;
using UnityEngine.UI;

// Enlarges a UI element on small (touch) screens so the cards inside stay readable.
// The element grows around the centre of its visible content, so it stays where it was.
[DisallowMultipleComponent]
public class MobileUIScale : MonoBehaviour
{
    [SerializeField] private float scale = 1.4f;
    [Tooltip("For a grid of cards: keep the grid's on-screen size, so it shows fewer but larger columns")]
    [SerializeField] private bool keepOnScreenSize;

    private bool applied;

    private void Start()
    {
        Apply();
    }

    // a grid may still be empty at Start (its cards are spawned when the panel opens)
    private void LateUpdate()
    {
        if (!applied) Apply();
    }

    public void Apply()
    {
        if (applied) return;
        if (!MobileLayout.IsSmallScreen)
        {
            applied = true;
            return;
        }

        RectTransform rectTransform = (RectTransform)transform;
        if (keepOnScreenSize && rectTransform.childCount == 0) return;
        applied = true;

        if (keepOnScreenSize)
        {
            // keep the content's top-left corner where it was, the cards themselves are offset from their grid cells
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            Bounds? before = ContentBounds(rectTransform);
            rectTransform.sizeDelta /= scale;
            ScaleBy(rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            Bounds? after = ContentBounds(rectTransform);
            if (before.HasValue && after.HasValue)
            {
                rectTransform.localPosition += new Vector3(
                    before.Value.min.x - after.Value.min.x,
                    before.Value.max.y - after.Value.max.y,
                    0f);
            }
            return;
        }

        Vector3 center = rectTransform.localPosition;
        if (rectTransform.parent != null)
        {
            Canvas.ForceUpdateCanvases();
            center = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, rectTransform).center;
        }
        ScaleBy(rectTransform);
        Vector3 position = rectTransform.localPosition;
        rectTransform.localPosition = new Vector3(
            center.x + (position.x - center.x) * scale,
            center.y + (position.y - center.y) * scale,
            position.z);
    }

    // Bounds of the children in the parent's space, or null when there are none.
    private static Bounds? ContentBounds(RectTransform rectTransform)
    {
        if (rectTransform.parent == null || rectTransform.childCount == 0) return null;
        Canvas.ForceUpdateCanvases();
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform.parent, rectTransform);
        return bounds.size == Vector3.zero ? null : bounds;
    }

    private void ScaleBy(RectTransform rectTransform)
    {
        Vector3 localScale = rectTransform.localScale;
        rectTransform.localScale = new Vector3(localScale.x * scale, localScale.y * scale, localScale.z);
    }
}
