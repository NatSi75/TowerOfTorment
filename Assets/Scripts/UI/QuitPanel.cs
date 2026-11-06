using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuitPanel : MonoBehaviour
{
    [Header("Look (same as the other menus)")]
    [SerializeField] private Sprite windowSprite;
    [SerializeField] private Sprite headerSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private TMP_FontAsset font;

    [Header("Text")]
    [SerializeField] private string title = "Exit";
    [SerializeField] private string question = "Are you sure you want to quit?";
    [SerializeField] private string thanksTitle = "Thanks for playing!";
    [SerializeField] private string thanksMessage = "Your settings are saved.\nYou can close this tab now.";

    private static QuitPanel openPanel;
    public static bool IsOpen => openPanel != null;

    private GameObject confirmView;
    private GameObject thanksView;

    public static void Open()
    {
        if (openPanel != null) return;
        QuitPanel prefab = Resources.Load<QuitPanel>("QuitPanel");
        if (prefab == null)
        {
            Debug.LogWarning("QuitPanel prefab not found in Resources.");
            return;
        }
        openPanel = Instantiate(prefab);
    }

    private void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 29500;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        BuildConfirmView();
        BuildThanksView();
        thanksView.SetActive(false);
    }

    private void BuildConfirmView()
    {
        confirmView = Panel("Confirm", transform, new Color(0f, 0f, 0f, 0.65f));

        Image window = NewImage("Window", confirmView.transform, windowSprite, Color.white);
        Place(window.rectTransform, new Vector2(760f, 400f), Vector2.zero);

        Image header = NewImage("Header", window.transform, headerSprite, Color.white);
        Place(header.rectTransform, new Vector2(380f, 90f), new Vector2(0f, 190f));
        Place(NewText("Title", header.transform, title, 44f).rectTransform, new Vector2(340f, 80f), Vector2.zero);

        Place(NewText("Question", window.transform, question, 40f).rectTransform, new Vector2(660f, 120f), new Vector2(0f, 30f));

        NewButton("Yes", window.transform, new Vector2(-150f, -110f), Quit);
        NewButton("No", window.transform, new Vector2(150f, -110f), Close);
    }

    private void BuildThanksView()
    {
        thanksView = Panel("Thanks", transform, Color.black);
        Place(NewText("Title", thanksView.transform, thanksTitle, 72f).rectTransform, new Vector2(1400f, 120f), new Vector2(0f, 110f));
        TMP_Text message = NewText("Message", thanksView.transform, thanksMessage, 36f);
        message.color = new Color(1f, 1f, 1f, 0.8f);
        Place(message.rectTransform, new Vector2(1400f, 140f), new Vector2(0f, -10f));
        Button back = NewButton("Back to the game", thanksView.transform, new Vector2(0f, -150f), Close);
        ((RectTransform)back.transform).sizeDelta = new Vector2(380f, 90f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    private void Quit()
    {
        PlayerPrefs.Save();
#if UNITY_STANDALONE && !UNITY_EDITOR
        Application.Quit();
#else
        confirmView.SetActive(false);
        thanksView.SetActive(true);
        ExitFullscreen();
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void WebPageExitFullscreen();
    private static void ExitFullscreen() => WebPageExitFullscreen();
#else
    private static void ExitFullscreen()
    {
        if (Screen.fullScreen) Screen.fullScreen = false;
    }
#endif

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openPanel == this) openPanel = null;
    }

    // ---------------------------------------------------------------- building helpers

    private static GameObject Panel(string name, Transform parent, Color color)
    {
        Image image = NewImage(name, parent, null, color);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return image.gameObject;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private TMP_Text NewText(string name, Transform parent, string text, float size)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    private Button NewButton(string text, Transform parent, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        Image image = NewImage(text + " Button", parent, buttonSprite, Color.white);
        Place(image.rectTransform, new Vector2(230f, 85f), position);
        Button button = image.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        TMP_Text label = NewText("Text", image.transform, text, 36f);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        return button;
    }

    private static void Place(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }
}
