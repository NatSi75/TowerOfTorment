using UnityEngine;
using UnityEngine.UI;

// Small Use / Remove menu shown next to a clicked scroll. The prefab lives at Resources/ScrollActionMenu.
// Clicking anywhere outside the menu closes it.
public class ScrollActionMenu : MonoBehaviour
{
    [SerializeField] private RectTransform menu;
    [SerializeField] private Button useButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private Button outsideButton;
    [SerializeField] private Vector2 offsetFromScroll = new(150f, 0f);

    private static ScrollActionMenu openMenu;
    private ScrollUI target;

    public static void Open(ScrollUI scrollUI)
    {
        if (openMenu != null) openMenu.Close();
        ScrollActionMenu prefab = Resources.Load<ScrollActionMenu>("ScrollActionMenu");
        if (prefab == null)
        {
            Debug.LogWarning("ScrollActionMenu prefab not found in Resources.");
            return;
        }
        openMenu = Instantiate(prefab);
        openMenu.Setup(scrollUI);
    }

    private void Setup(ScrollUI scrollUI)
    {
        target = scrollUI;

        // place the menu next to the scroll (the scroll sits on a Screen Space Overlay canvas)
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, scrollUI.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPoint, null, out Vector2 localPoint);
        menu.anchoredPosition = localPoint + offsetFromScroll;

        // scrolls can only be used while nothing else is happening (same rule as playing a card)
        useButton.interactable = Interactions.Instance == null || Interactions.Instance.PlayerCanInteract();

        useButton.onClick.AddListener(() =>
        {
            if (target != null) target.UseScroll();
            Close();
        });
        removeButton.onClick.AddListener(() =>
        {
            if (target != null) target.RemoveScroll();
            Close();
        });
        outsideButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        if (target == null) Close();
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openMenu == this) openMenu = null;
    }
}
