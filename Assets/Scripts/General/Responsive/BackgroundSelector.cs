using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ActBackground
{
    public Sprite act1;
    public Sprite act2;

    public Sprite Get(int act)
    {
        return act == 2 && act2 != null ? act2 : act1;
    }
}

// Picks the scene background for the current Act (and, in Battle, the enemy type) and makes it cover
// the whole screen without stretching. Works on a world SpriteRenderer or on a UI Image.
// Runs after ResponsiveCamera (camera size is final) and before ResponsiveBackground (which keeps it covered).
[DefaultExecutionOrder(-950)]
public class BackgroundSelector : MonoBehaviour
{
    [Tooltip("Background used by this scene")]
    [SerializeField] private ActBackground background = new();

    [Header("Battle only")]
    [Tooltip("Pick the background by enemy type (Minor / Elite / Boss) instead of the one above")]
    [SerializeField] private bool byEnemyType;
    [SerializeField] private ActBackground minorEnemy = new();
    [SerializeField] private ActBackground eliteEnemy = new();
    [SerializeField] private ActBackground boss = new();
    [Tooltip("Moves the background up (world units, SpriteRenderer only) so its floor lines up with the characters' feet. The background is enlarged to keep the screen covered.")]
    [SerializeField] private float verticalOffset;

    private void Awake()
    {
        Sprite sprite = PickSprite();
        if (sprite == null) return;

        if (TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            ApplyToSpriteRenderer(spriteRenderer, sprite, verticalOffset);
        }
        else if (TryGetComponent(out Image image))
        {
            ApplyToImage(image, sprite);
        }
    }

    private Sprite PickSprite()
    {
        GameDataManager data = GameDataManager.Instance;
        int act = data != null ? data.currentAct : 1;
        if (!byEnemyType) return background.Get(act);

        int enemyType = data != null ? data.indexEnemy : 0;
        return enemyType switch
        {
            1 => eliteEnemy.Get(act),
            2 => boss.Get(act),
            _ => minorEnemy.Get(act),
        };
    }

    private static void ApplyToSpriteRenderer(SpriteRenderer spriteRenderer, Sprite sprite, float verticalOffset)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        Transform t = spriteRenderer.transform;
        t.position = new Vector3(cam.transform.position.x, cam.transform.position.y + verticalOffset, t.position.z);

        // cover the camera view; the parent's scale is taken into account
        Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
        float viewWidth = cam.orthographicSize * 2f * cam.aspect;
        // with an offset the sprite must reach further on one side, so cover the larger height
        float viewHeight = (cam.orthographicSize + Mathf.Abs(verticalOffset)) * 2f;
        float cover = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);
        Vector3 parentScale = t.parent != null ? t.parent.lossyScale : Vector3.one;
        t.localScale = new Vector3(
            cover / NonZero(parentScale.x),
            cover / NonZero(parentScale.y),
            t.localScale.z);
    }

    private static void ApplyToImage(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.preserveAspect = false;
        image.type = Image.Type.Simple;

        AspectRatioFitter fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter == null) fitter = image.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    private static float NonZero(float value)
    {
        return Mathf.Approximately(value, 0f) ? 1f : value;
    }
}
