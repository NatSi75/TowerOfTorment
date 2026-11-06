using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CardZoom : MonoBehaviour
{
    private const float ScreenHeightFraction = 0.8f;
    private const int WorldSortingOrder = 1000;
    private const float LongPressTime = 0.4f;
    private const float LongPressMaxMove = 25f;
    private static readonly Color DimColor = new(0f, 0f, 0f, 0.7f);

    private Canvas overlay;
    private Image overlayDim;
    private GameObject zoomed;
    private GameObject worldDim;
    private CardView source;
    private readonly List<RaycastResult> raycastResults = new();

    private static float pressTime = -1f;
    private static Vector2 pressPosition;
    private static bool pressMoved;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject go = new("CardZoom");
        DontDestroyOnLoad(go);
        go.AddComponent<CardZoom>();
    }

    public static bool ZoomHeld =>
        Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetMouseButton(1) || LongPressHeld;

    private static bool LongPressHeld =>
        MobileLayout.IsSmallScreen && pressTime >= 0f && !pressMoved && Time.unscaledTime - pressTime >= LongPressTime;

    private static void TrackLongPress()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressTime = Time.unscaledTime;
            pressPosition = Input.mousePosition;
            pressMoved = false;
        }
        else if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(pressPosition, Input.mousePosition) > LongPressMaxMove) pressMoved = true;
        }
        else
        {
            pressTime = -1f;
        }
    }

    private void Update()
    {
        bool wasLongPress = LongPressHeld;
        TrackLongPress();
        if (wasLongPress && Input.GetMouseButtonUp(0) && zoomed != null) return;
        CardView hovered = ZoomHeld && !PauseMenu.IsPaused ? FindHoveredCard() : null;
        if (hovered == source) return;
        Hide();
        if (hovered != null) Show(hovered);
    }

    private CardView FindHoveredCard()
    {
        if (CardViewHoverSystem.Instance != null && CardViewHoverSystem.Instance.ShownCard != null)
            return CardViewHoverSystem.Instance.ShownCard;

        if (EventSystem.current != null)
        {
            PointerEventData pointer = new(EventSystem.current) { position = Input.mousePosition };
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, raycastResults);
            foreach (RaycastResult hit in raycastResults)
            {
                if (overlay != null && hit.gameObject.transform.IsChildOf(overlay.transform)) continue;
                CardView card = hit.gameObject.GetComponentInParent<CardView>();
                if (card != null && card.Card != null) return card;
            }
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
            foreach (Collider2D hit in Physics2D.OverlapPointAll(new Vector2(world.x, world.y)))
            {
                CardView card = hit.GetComponentInParent<CardView>();
                if (card != null && card.Card != null) return card;
            }
        }
        return null;
    }

    private void Show(CardView card)
    {
        source = card;
        if (card.transform is RectTransform) ShowUI(card);
        else ShowWorld(card);
    }

    private void ShowUI(CardView card)
    {
        EnsureOverlay();
        overlay.gameObject.SetActive(true);
        overlayDim.raycastTarget = LongPressHeld;
        zoomed = Instantiate(card.gameObject, overlay.transform, false);
        zoomed.name = "Zoomed Card";
        CanvasGroup group = zoomed.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        RectTransform rect = (RectTransform)zoomed.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        rect.anchoredPosition = Vector2.zero;
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(overlay.transform, rect);
        if (bounds.size.y <= 0f) return;
        float scale = Screen.height * ScreenHeightFraction / bounds.size.y;
        rect.localScale = Vector3.one * scale;
        rect.anchoredPosition = -(Vector2)bounds.center * scale;
    }

    private void ShowWorld(CardView card)
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic || CardViewCreator.Instance == null) return;

        CardView zoomCard = Instantiate(CardViewCreator.Instance.CardViewPrefab);
        zoomCard.name = "Zoomed Card";
        zoomCard.Setup(card.Card, card.HeroIndex);
        foreach (Collider2D col in zoomCard.GetComponentsInChildren<Collider2D>()) col.enabled = false;
        zoomCard.enabled = false;
        zoomed = zoomCard.gameObject;
        if (!zoomed.TryGetComponent(out SortingGroup sorting)) sorting = zoomed.AddComponent<SortingGroup>();
        sorting.sortingOrder = WorldSortingOrder;

        Bounds? cardBounds = SpriteBounds(zoomed);
        if (cardBounds == null) return;
        float viewHeight = cam.orthographicSize * 2f;
        float scale = viewHeight * ScreenHeightFraction / cardBounds.Value.size.y;
        zoomed.transform.localScale *= scale;
        Bounds scaled = SpriteBounds(zoomed).Value;
        Vector3 camPos = cam.transform.position;
        zoomed.transform.position += new Vector3(camPos.x - scaled.center.x, camPos.y - scaled.center.y, 0f);

        worldDim = new GameObject("Zoom Dim");
        SpriteRenderer dim = worldDim.AddComponent<SpriteRenderer>();
        dim.sprite = WhiteSprite();
        dim.color = DimColor;
        dim.sortingOrder = WorldSortingOrder - 1;
        worldDim.transform.position = new Vector3(camPos.x, camPos.y, 0f);
        worldDim.transform.localScale = new Vector3(viewHeight * cam.aspect + 2f, viewHeight + 2f, 1f);
    }

    private static Bounds? SpriteBounds(GameObject root)
    {
        Bounds? result = null;
        foreach (SpriteRenderer sr in root.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.sprite == null || !sr.enabled) continue;
            if (result == null) result = sr.bounds;
            else { Bounds b = result.Value; b.Encapsulate(sr.bounds); result = b; }
        }
        return result;
    }

    private void Hide()
    {
        source = null;
        if (zoomed != null) Destroy(zoomed);
        if (worldDim != null) Destroy(worldDim);
        zoomed = null;
        worldDim = null;
        if (overlay != null) overlay.gameObject.SetActive(false);
    }

    private void EnsureOverlay()
    {
        if (overlay != null) return;
        GameObject go = new("Card Zoom Overlay");
        go.transform.SetParent(transform, false);
        overlay = go.AddComponent<Canvas>();
        overlay.renderMode = RenderMode.ScreenSpaceOverlay;
        overlay.sortingOrder = 30000;

        GameObject dimGo = new("Dim");
        dimGo.transform.SetParent(go.transform, false);
        Image dim = dimGo.AddComponent<Image>();
        dim.color = DimColor;
        dim.raycastTarget = false;
        overlayDim = dim;
        RectTransform dimRect = dim.rectTransform;
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
    }

    private static Sprite whiteSprite;
    private static Sprite WhiteSprite()
    {
        if (whiteSprite == null)
        {
            Texture2D texture = Texture2D.whiteTexture;
            whiteSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        }
        return whiteSprite;
    }
}
