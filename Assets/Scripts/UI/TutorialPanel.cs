using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Step-by-step in-game tutorial. Each step has a short text and can point at a spot on the screen
// (a pulsing ring around it). Shown once per topic (battle, map).
public class TutorialPanel : MonoBehaviour
{
    [Header("Look (same as the other menus)")]
    [SerializeField] private Sprite windowSprite;
    [SerializeField] private Sprite buttonSprite;
    [SerializeField] private TMP_FontAsset font;

    public class Step
    {
        public string Title;
        public string Text;
        public Func<Vector2?> Target; // screen position (pixels) to point at, or null for none
    }

    private static TutorialPanel openPanel;
    public static bool IsOpen => openPanel != null;

    private RectTransform canvasRect;
    private RectTransform box;
    private RectTransform ring;
    private TMP_Text titleText;
    private TMP_Text bodyText;
    private TMP_Text counterText;
    private TMP_Text nextLabel;
    private List<Step> steps;
    private int index;

    // ---------------------------------------------------------------- opening

    public static bool HasSeen(string topic) => PlayerPrefs.GetInt("Tutorial_" + topic, 0) == 1;

    public static void ShowOnce(string topic, List<Step> steps)
    {
        if (HasSeen(topic)) return;
        PlayerPrefs.SetInt("Tutorial_" + topic, 1);
        PlayerPrefs.Save();
        Show(steps);
    }

    public static void Show(List<Step> steps)
    {
        if (openPanel != null || steps == null || steps.Count == 0) return;
        TutorialPanel prefab = Resources.Load<TutorialPanel>("TutorialPanel");
        if (prefab == null)
        {
            Debug.LogWarning("TutorialPanel prefab not found in Resources.");
            return;
        }
        openPanel = Instantiate(prefab);
        openPanel.steps = steps;
        openPanel.ShowStep(0);
    }

    // ---------------------------------------------------------------- the two tutorials

    public static List<Step> BattleSteps() => new()
    {
        new Step { Title = "Welcome to the Tower", Text = "Defeat every enemy to clear the room. Your HP carries over between fights, so play carefully." },
        new Step { Title = "Your Hand", Text = "Drag a card up and release it to play it. A card that hits one enemy is dragged onto that enemy. The card glows white when you can let go.", Target = HandTarget },
        new Step { Title = "Mana", Text = "Each card costs Mana, shown in its top-left corner. You get 3 Mana every turn. A card glows red when you don't have enough.", Target = () => UITarget(FindAnyObjectByType<ManaUI>()) },
        new Step { Title = "Enemy Intent", Text = "The icon above an enemy shows what it will do on its turn: attack, defend or cast an effect. Hover over it for details.", Target = IntentTarget },
        new Step { Title = "Torment", Text = "Torment rises every turn. When it is full, you can no longer see what enemies are planning. Rest at a campfire to lower it.", Target = TormentTarget },
        new Step { Title = "End Turn", Text = "When you are done, press End Turn. The enemies act, then you draw a new hand.", Target = () => UITarget(FindAnyObjectByType<EndTurnButtonUI>()) },
        new Step { Title = "Tip", Text = "Hold Shift or right-click on a card to zoom in (on phones: touch and hold)." },
    };

    public static List<Step> MapSteps() => new()
    {
        new Step { Title = "The Tower Map", Text = "Climb from the bottom of the tower to the Boss at the top. Each icon is a room: fights, campfires, merchants, treasure and more. Hover over a room to see what it is." },
        new Step { Title = "Choose Your Path", Text = "Pulsing rooms are the ones you can enter next. The arrow shows where you are, and darkened paths can no longer be reached.", Target = AttainableNodeTarget },
    };

    private static Vector2? WorldTarget(Vector3 world)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;
        return cam.WorldToScreenPoint(world);
    }

    private static Vector2? UITarget(Component ui)
    {
        if (ui == null || ui.transform is not RectTransform rect) return null;
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.rootCanvas.worldCamera : null;
        return RectTransformUtility.WorldToScreenPoint(cam, rect.TransformPoint(rect.rect.center));
    }

    private static Vector2? HandTarget()
    {
        HandView hand = FindAnyObjectByType<HandView>();
        Vector3? center = hand != null ? hand.CardsCenter() : null;
        return center.HasValue ? WorldTarget(center.Value) : null;
    }

    private static Vector2? IntentTarget()
    {
        EnemyView enemy = EnemySystem.Instance != null ? EnemySystem.Instance.Enemies.FirstOrDefault(e => e != null) : null;
        return enemy != null ? WorldTarget(enemy.IntentPosition) : null;
    }

    private static Vector2? TormentTarget()
    {
        HeroView hero = HeroSystem.Instance != null ? HeroSystem.Instance.HeroView : null;
        return hero != null ? WorldTarget(hero.TormentTextPosition) : null;
    }

    private static Vector2? AttainableNodeTarget()
    {
        Map.MapNode node = FindObjectsByType<Map.MapNode>(FindObjectsSortMode.None)
            .Where(n => n.State == Map.NodeStates.Attainable)
            .OrderBy(n => n.transform.position.y)
            .FirstOrDefault();
        return node != null ? WorldTarget(node.transform.position) : null;
    }

    // ---------------------------------------------------------------- building the UI

    private void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 29400;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        canvasRect = (RectTransform)transform;

        Image dim = NewImage("Dim", transform, null, new Color(0f, 0f, 0f, 0.65f));
        Stretch(dim.rectTransform);

        Image ringImage = NewImage("Pointer", transform, RingSprite(), new Color(1f, 1f, 1f, 0.95f));
        ringImage.raycastTarget = false;
        ring = ringImage.rectTransform;
        ring.sizeDelta = new Vector2(190f, 190f);
        ring.DOScale(1.15f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(ring.gameObject).SetUpdate(true);

        Image window = NewImage("Window", transform, windowSprite, Color.white);
        box = window.rectTransform;
        box.sizeDelta = new Vector2(900f, 330f);

        titleText = NewText("Title", box, 46f);
        Place(titleText.rectTransform, new Vector2(820f, 64f), new Vector2(0f, 110f));
        bodyText = NewText("Text", box, 32f);
        Place(bodyText.rectTransform, new Vector2(800f, 150f), new Vector2(0f, 10f));
        counterText = NewText("Counter", box, 26f);
        counterText.color = new Color(1f, 1f, 1f, 0.7f);
        Place(counterText.rectTransform, new Vector2(160f, 50f), new Vector2(-340f, -112f));

        NewButton("Skip", box, new Vector2(115f, -110f), new Vector2(170f, 66f), Close);
        nextLabel = NewButton("Next", box, new Vector2(300f, -110f), new Vector2(170f, 66f), Next);
    }

    private void Update()
    {
        if (steps == null) { Close(); return; }
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) Next();
        PlacePointer(); // the target may move (cards being dealt, map scrolling)
    }

    private void ShowStep(int stepIndex)
    {
        index = stepIndex;
        Step step = steps[index];
        titleText.text = step.Title;
        bodyText.text = step.Text;
        counterText.text = (index + 1) + " / " + steps.Count;
        nextLabel.text = index == steps.Count - 1 ? "Got it" : "Next";
        PlacePointer();
    }

    private void PlacePointer()
    {
        Vector2? screen = steps[index].Target?.Invoke();
        ring.gameObject.SetActive(screen.HasValue);
        if (!screen.HasValue)
        {
            box.anchoredPosition = Vector2.zero;
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen.Value, null, out Vector2 local);
        ring.anchoredPosition = local;
        // keep the text box on the other half of the screen from what it points at
        box.anchoredPosition = new Vector2(0f, local.y < 0f ? 170f : -170f);
    }

    private void Next()
    {
        AudioManager.PlaySfx(Sfx.ButtonClick);
        if (index + 1 < steps.Count) ShowStep(index + 1);
        else Close();
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openPanel == this) openPanel = null;
    }

    // ---------------------------------------------------------------- helpers

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private TMP_Text NewText(string name, Transform parent, float size)
    {
        GameObject go = new(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    private TMP_Text NewButton(string text, Transform parent, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        Image image = NewImage(text + " Button", parent, buttonSprite, Color.white);
        Place(image.rectTransform, size, position);
        Button button = image.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        TMP_Text label = NewText("Text", image.transform, 30f);
        label.text = text;
        Stretch(label.rectTransform);
        return label;
    }

    private static void Place(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static Sprite ringSprite;

    // A soft ring, drawn once in code.
    private static Sprite RingSprite()
    {
        if (ringSprite != null) return ringSprite;
        const int size = 128;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f)) / (size / 2f);
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(d - 0.85f) / 0.08f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();
        ringSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return ringSprite;
    }
}
