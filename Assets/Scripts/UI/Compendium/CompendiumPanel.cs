using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompendiumPanel : MonoBehaviour
{
    private enum Tab { Knight, Wizard, Relic, Scroll }

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button knightTab;
    [SerializeField] private Button wizardTab;
    [SerializeField] private Button relicTab;
    [SerializeField] private Button scrollTab;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color otherTabColor = new(0.55f, 0.55f, 0.55f);

    [Header("Cards (Knight / Wizard tabs)")]
    [SerializeField] private GameObject cardArea;
    [SerializeField] private Transform cardGrid;
    [SerializeField] private GameObject cardPrefab;

    [Header("Icons (Relic / Scroll tabs)")]
    [SerializeField] private GameObject iconArea;
    [SerializeField] private Transform iconGrid;
    [SerializeField] private ScrollRect iconScroll;
    [SerializeField] private CompendiumEntry iconPrefab;
    [SerializeField] private TMP_Text detailText;

    [Header("Content")]
    [SerializeField] private List<CardData> knightCards = new();
    [SerializeField] private List<CardData> wizardCards = new();
    [SerializeField] private List<RelicData> relics = new();
    [SerializeField] private List<ScrollData> scrolls = new();

    private static CompendiumPanel openPanel;
    private readonly List<GameObject> spawned = new();

    public static bool IsOpen => openPanel != null;

    public static void Open()
    {
        if (openPanel != null) return;
        CompendiumPanel prefab = Resources.Load<CompendiumPanel>("CompendiumPanel");
        if (prefab == null)
        {
            Debug.LogWarning("CompendiumPanel prefab not found in Resources.");
            return;
        }
        openPanel = Instantiate(prefab);
    }

    private void Start()
    {
        backButton.onClick.AddListener(Close);
        knightTab.onClick.AddListener(() => Show(Tab.Knight));
        wizardTab.onClick.AddListener(() => Show(Tab.Wizard));
        relicTab.onClick.AddListener(() => Show(Tab.Relic));
        scrollTab.onClick.AddListener(() => Show(Tab.Scroll));
        Show(Tab.Knight);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    private void Show(Tab tab)
    {
        SetTabColor(knightTab, tab == Tab.Knight);
        SetTabColor(wizardTab, tab == Tab.Wizard);
        SetTabColor(relicTab, tab == Tab.Relic);
        SetTabColor(scrollTab, tab == Tab.Scroll);

        foreach (GameObject item in spawned) Destroy(item);
        spawned.Clear();

        bool cards = tab == Tab.Knight || tab == Tab.Wizard;
        cardArea.SetActive(cards);
        iconArea.SetActive(!cards);
        detailText.text = "";
        detailText.gameObject.SetActive(!cards);

        switch (tab)
        {
            case Tab.Knight: ShowCards(knightCards, 0); break;
            case Tab.Wizard: ShowCards(wizardCards, 1); break;
            case Tab.Relic:
                foreach (RelicData relic in relics)
                {
                    if (relic != null) AddIcon(relic.Image, relic.Name, relic.Description);
                }
                FinishIcons(relics.Count > 0 ? "Hover or tap a relic to read it." : "No relics.");
                break;
            case Tab.Scroll:
                foreach (ScrollData scroll in scrolls)
                {
                    if (scroll != null) AddIcon(scroll.Image, scroll.Name, scroll.Description);
                }
                FinishIcons(scrolls.Count > 0 ? "Hover or tap a scroll to read it." : "No scrolls.");
                break;
        }
    }

    private void ShowCards(List<CardData> list, int heroIndex)
    {
        int count = 0;
        foreach (CardData data in list)
        {
            if (data == null) continue;
            GameObject card = Instantiate(cardPrefab, cardGrid);
            spawned.Add(card);
            card.transform.localPosition = Vector3.zero;
            card.transform.localScale = Vector3.one;
            card.transform.localRotation = Quaternion.identity;
            if (card.TryGetComponent(out CardView view)) view.Setup(new Card(data), heroIndex);
            if (card.TryGetComponent(out Button button)) button.interactable = false;
            count++;
        }
        CardGridLayout.Fit(cardGrid, count);
    }

    private void AddIcon(Sprite sprite, string title, string description)
    {
        CompendiumEntry entry = Instantiate(iconPrefab, iconGrid);
        entry.Setup(this, sprite, title, description);
        spawned.Add(entry.gameObject);
    }

    private void FinishIcons(string hint)
    {
        detailText.text = hint;
        Canvas.ForceUpdateCanvases();
        if (iconScroll != null) iconScroll.verticalNormalizedPosition = 1f;
    }

    public void ShowDetail(string title, string description)
    {
        detailText.text = $"<b>{title}</b>\n{description}";
    }

    private void SetTabColor(Button tab, bool selected)
    {
        TMP_Text label = tab.GetComponentInChildren<TMP_Text>();
        if (label != null) label.color = selected ? selectedTabColor : otherTabColor;
        if (tab.targetGraphic != null) tab.targetGraphic.color = selected ? Color.white : otherTabColor;
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openPanel == this) openPanel = null;
    }
}
