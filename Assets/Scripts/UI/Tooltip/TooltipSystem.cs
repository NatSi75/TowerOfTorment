using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One tooltip for the whole game. It is built from code on first use, survives scene loads,
// follows the pointer and flips to stay inside the screen.
public class TooltipSystem : MonoBehaviour
{
    private const float Padding = 18f;
    private const float TitleGap = 8f;
    private const float PointerOffset = 28f;

    private static TooltipSystem instance;

    private TooltipStyle style;
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform panel;
    private TMP_Text titleText;
    private TMP_Text descriptionText;
    private TooltipTrigger currentSource;
    private string shownTitle;
    private string shownDescription;

    public static void Show(TooltipTrigger source)
    {
        if (source == null) return;
        source.GetText(out string title, out string description);
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description)) return;
        GetInstance().Open(source, title, description);
    }

    public static void Hide(TooltipTrigger source)
    {
        if (instance != null && instance.currentSource == source)
        {
            instance.Close();
        }
    }

    private static TooltipSystem GetInstance()
    {
        if (instance == null)
        {
            GameObject tooltipObject = new("TooltipSystem");
            DontDestroyOnLoad(tooltipObject);
            instance = tooltipObject.AddComponent<TooltipSystem>();
            instance.Build();
        }
        return instance;
    }

    private void Build()
    {
        style = Resources.Load<TooltipStyle>("TooltipStyle");
        if (style == null) style = ScriptableObject.CreateInstance<TooltipStyle>();

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvasRect = (RectTransform)transform;

        panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(Outline)).GetComponent<RectTransform>();
        panel.SetParent(transform, false);
        panel.anchorMin = panel.anchorMax = Vector2.zero;
        panel.pivot = new Vector2(0f, 1f);

        Image background = panel.GetComponent<Image>();
        background.sprite = style.Background;
        background.type = style.Background != null && style.Background.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        background.color = style.BackgroundColor;
        background.raycastTarget = false;

        Outline border = panel.GetComponent<Outline>();
        border.effectColor = style.BorderColor;
        border.effectDistance = new Vector2(2f, -2f);

        titleText = CreateText("Title", style.TitleSize, style.TitleColor, style.TitleMaterial);
        descriptionText = CreateText("Description", style.DescriptionSize, style.DescriptionColor, null);

        panel.gameObject.SetActive(false);
    }

    private TMP_Text CreateText(string objectName, float size, Color color, Material material)
    {
        TextMeshProUGUI text = new GameObject(objectName, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        text.rectTransform.SetParent(panel, false);
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0f, 1f);
        text.rectTransform.pivot = new Vector2(0f, 1f);
        if (style.Font != null) text.font = style.Font;
        if (material != null) text.fontSharedMaterial = material;
        text.fontSize = size;
        text.color = color;
        text.richText = true;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.TopLeft;
        return text;
    }

    private void Open(TooltipTrigger source, string title, string description)
    {
        currentSource = source;
        SetText(title, description);
        panel.gameObject.SetActive(true);
        UpdatePosition();
    }

    private void Close()
    {
        currentSource = null;
        if (panel != null) panel.gameObject.SetActive(false);
    }

    private void SetText(string title, string description)
    {
        shownTitle = title;
        shownDescription = description;
        bool hasTitle = !string.IsNullOrWhiteSpace(title);
        bool hasDescription = !string.IsNullOrWhiteSpace(description);
        titleText.gameObject.SetActive(hasTitle);
        descriptionText.gameObject.SetActive(hasDescription);
        titleText.text = title;
        descriptionText.text = description;

        float maxTextWidth = style.MaxWidth - Padding * 2f;
        float width = Mathf.Min(maxTextWidth, Mathf.Max(
            hasTitle ? titleText.GetPreferredValues(title, maxTextWidth, 0f).x : 0f,
            hasDescription ? descriptionText.GetPreferredValues(description, maxTextWidth, 0f).x : 0f));
        float titleHeight = hasTitle ? titleText.GetPreferredValues(title, width, 0f).y : 0f;
        float descriptionHeight = hasDescription ? descriptionText.GetPreferredValues(description, width, 0f).y : 0f;
        float gap = hasTitle && hasDescription ? TitleGap : 0f;

        titleText.rectTransform.anchoredPosition = new Vector2(Padding, -Padding);
        titleText.rectTransform.sizeDelta = new Vector2(width, titleHeight);
        descriptionText.rectTransform.anchoredPosition = new Vector2(Padding, -Padding - titleHeight - gap);
        descriptionText.rectTransform.sizeDelta = new Vector2(width, descriptionHeight);
        panel.sizeDelta = new Vector2(width + Padding * 2f, titleHeight + gap + descriptionHeight + Padding * 2f);
    }

    private void LateUpdate()
    {
        if (currentSource == null)
        {
            if (panel.gameObject.activeSelf) Close();
            return;
        }
        if (!currentSource.isActiveAndEnabled)
        {
            Close();
            return;
        }

        currentSource.GetText(out string title, out string description);
        if (title != shownTitle || description != shownDescription)
        {
            SetText(title, description);
        }
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float scale = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 pointer = (Vector2)Input.mousePosition / scale;
        Vector2 size = panel.sizeDelta;

        // Default: right of and below the pointer; flip when it would leave the screen.
        float x = pointer.x + PointerOffset;
        float y = pointer.y - PointerOffset;
        if (x + size.x > canvasSize.x) x = pointer.x - PointerOffset - size.x;
        if (y - size.y < 0f) y = pointer.y + PointerOffset + size.y;

        x = Mathf.Clamp(x, 0f, Mathf.Max(0f, canvasSize.x - size.x));
        y = Mathf.Clamp(y, Mathf.Min(size.y, canvasSize.y), canvasSize.y);
        panel.anchoredPosition = new Vector2(x, y);
    }
}
