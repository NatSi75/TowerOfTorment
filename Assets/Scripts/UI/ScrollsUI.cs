using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Shows the scroll slots (GameDataManager.MaxScrolls). Empty slots show a faded scroll placeholder.
public class ScrollsUI : MonoBehaviour
{
    [SerializeField] private ScrollUI scrollUIPrefab;
    [Header("Empty Slot")]
    [SerializeField] private Sprite placeholderSprite;
    [SerializeField] private Color placeholderColor = new(0f, 0f, 0f, 0.45f);

    private readonly List<ScrollUI> scrollUIs = new();
    private readonly List<RectTransform> slots = new();
    private readonly List<Image> placeholders = new();

    private void Awake()
    {
        Vector2 slotSize = ((RectTransform)scrollUIPrefab.transform).sizeDelta;
        for (int i = 0; i < GameDataManager.MaxScrolls; i++)
        {
            RectTransform slot = new GameObject($"Scroll Slot {i + 1}", typeof(RectTransform)).GetComponent<RectTransform>();
            slot.SetParent(transform, false);
            slot.sizeDelta = slotSize;
            slots.Add(slot);

            Image placeholder = new GameObject("Placeholder", typeof(RectTransform)).AddComponent<Image>();
            placeholder.rectTransform.SetParent(slot, false);
            Stretch(placeholder.rectTransform);
            placeholder.sprite = placeholderSprite;
            placeholder.color = placeholderColor;
            placeholder.preserveAspect = true;
            placeholder.raycastTarget = false;
            placeholders.Add(placeholder);
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            placeholders[i].enabled = SlotScroll(i) == null;
        }
    }

    public void AddScrollUI(Scroll scroll)
    {
        int freeSlot = slots.FindIndex(slot => slot.GetComponentInChildren<ScrollUI>() == null);
        Transform parent = freeSlot >= 0 ? slots[freeSlot] : transform;
        ScrollUI scrollUI = Instantiate(scrollUIPrefab, parent);
        if (freeSlot >= 0) Stretch((RectTransform)scrollUI.transform);
        scrollUI.Setup(scroll);
        scrollUIs.Add(scrollUI);
    }

    public void RemoveScrollUI(Scroll scroll)
    {
        ScrollUI scrollUI = scrollUIs.Where(pui => pui.Scroll == scroll).FirstOrDefault();
        if (scrollUI != null)
        {
            scrollUIs.Remove(scrollUI);
            Destroy(scrollUI.gameObject);
        }
    }

    private ScrollUI SlotScroll(int index)
    {
        return slots[index].GetComponentInChildren<ScrollUI>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
