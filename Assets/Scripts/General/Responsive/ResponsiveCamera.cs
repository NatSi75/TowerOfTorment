using UnityEngine;

// Keeps the area the scene was designed for (designOrthographicSize at designResolution's aspect)
// always fully visible. Wider screens see more on the sides, narrower screens see more on top/bottom.
[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(Camera))]
public class ResponsiveCamera : MonoBehaviour
{
    [SerializeField] private float designOrthographicSize = 5f;
    [SerializeField] private Vector2 designResolution = new(1920f, 1080f);

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    private void LateUpdate()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            Apply();
        }
    }

    private void Apply()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        if (!cam.orthographic || lastScreenWidth <= 0 || lastScreenHeight <= 0) return;

        float designAspect = designResolution.x / designResolution.y;
        float aspect = lastScreenWidth * cam.rect.width / (lastScreenHeight * cam.rect.height);
        cam.orthographicSize = aspect < designAspect
            ? designOrthographicSize * designAspect / aspect
            : designOrthographicSize;
    }

    public void SetDesignOrthographicSize(float size)
    {
        designOrthographicSize = size;
    }

    private void Reset()
    {
        designOrthographicSize = GetComponent<Camera>().orthographicSize;
    }
}
