using UnityEngine;
using UnityEngine.UI;

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
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, scrollUI.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPoint, null, out Vector2 localPoint);
        menu.anchoredPosition = localPoint + offsetFromScroll;

        UpdateUseButton();

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
        else UpdateUseButton();
    }

    private void UpdateUseButton()
    {
        bool canUse = Interactions.Instance == null || Interactions.Instance.PlayerCanInteract();
        if (useButton.interactable != canUse) useButton.interactable = canUse;
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
