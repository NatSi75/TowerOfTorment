using UnityEngine;

// Scales a background sprite up (never down) so it always covers the whole camera view,
// e.g. on ultra-wide phones or tall browser windows.
[DefaultExecutionOrder(-900)]
[RequireComponent(typeof(SpriteRenderer))]
public class ResponsiveBackground : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private Vector2 baseSize;
    private Vector3 baseCenterOffset;
    private float lastOrthographicSize;
    private float lastAspect;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseSize = spriteRenderer.bounds.size;
        baseCenterOffset = spriteRenderer.bounds.center - transform.position;
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic || baseSize.x <= 0f || baseSize.y <= 0f) return;
        if (Mathf.Approximately(cam.orthographicSize, lastOrthographicSize) && Mathf.Approximately(cam.aspect, lastAspect)) return;

        lastOrthographicSize = cam.orthographicSize;
        lastAspect = cam.aspect;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 offset = transform.position + baseCenterOffset - cam.transform.position;
        float scaleX = 2f * (halfWidth + Mathf.Abs(offset.x)) / baseSize.x;
        float scaleY = 2f * (halfHeight + Mathf.Abs(offset.y)) / baseSize.y;
        float factor = Mathf.Max(1f, scaleX, scaleY);
        transform.localScale = new Vector3(baseScale.x * factor, baseScale.y * factor, baseScale.z);
    }
}
