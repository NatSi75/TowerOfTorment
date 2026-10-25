using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardRewardUI : MonoBehaviour
{
    public static CardRewardUI Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Panel utama Reward (untuk diaktifkan saat battle selesai)")]
    public GameObject rewardPanel;

    [Tooltip("Slot baris pertama untuk Gold")]
    public RewardSlotUI goldSlot;

    [Tooltip("Slot baris kedua untuk Scroll")]
    public RewardSlotUI scrollSlot;

    [Header("Gold Sprite Settings")]
    public Sprite minorCoinSprite;
    public Sprite eliteCoinSprite;
    public Sprite bossCoinSprite;

    [Header("Gold Amount Settings")]
    public Vector2Int minorGoldRange = new Vector2Int(20, 30);
    public Vector2Int eliteGoldRange = new Vector2Int(40, 50);
    public Vector2Int bossGoldRange = new Vector2Int(65, 80);

    [Header("Scroll Drop Chance")]
    [Range(0f, 1f)] public float minorScrollDropChance = 0.5f;
    [Range(0f, 1f)] public float eliteScrollDropChance = 1f;
    [Range(0f, 1f)] public float bossScrollDropChance = 1f;

    [Header("UI Layout")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private List<CardView> rewardCardSlots;

    [Header("UI Reward Relic Settings")]
    [SerializeField] private GameObject rewardPanelObject;
    [SerializeField] private List<RelicChoiceUI> relicSlots;

    private List<CardData> poolCopy;
    private List<Card> chosenListToDisplay = new List<Card>();

    private void ShowBattleEndOverlay()
    {
        if (battleEndOverlay != null || rewardPanel == null) return;
        Transform canvas = rewardPanel.transform.parent;

        battleEndOverlay = new GameObject("Battle End Overlay", typeof(RectTransform));
        RectTransform rect = (RectTransform)battleEndOverlay.transform;
        rect.SetParent(canvas, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = battleEndOverlay.AddComponent<Image>();
        image.color = battleEndOverlayColor;
        image.raycastTarget = true;
        rect.SetAsLastSibling();

        List<Transform> panels = new();
        foreach (GameObject panel in new[] { rewardPanelObject, panelObject, rewardPanel })
        {
            if (panel != null && panel.transform.parent == canvas) panels.Add(panel.transform);
        }
        panels.Sort((a, b) => a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));
        foreach (Transform panel in panels) panel.SetAsLastSibling();
    }

    private void Awake()
    {
        Instance = this;
        if (panelObject != null) panelObject.SetActive(false);
        if (rewardPanel != null) rewardPanel.SetActive(false);
    }

    [Header("Battle End Overlay")]
    [Tooltip("Darkens the battle behind the rewards and blocks it (cards, End Turn...)")]
    [SerializeField] private Color battleEndOverlayColor = new(0f, 0f, 0f, 0.6f);
    private GameObject battleEndOverlay;

    public void ShowBattleReward()
    {
        if (Interactions.Instance != null) Interactions.Instance.BattleEnded = true;
        foreach (CardView card in FindObjectsByType<CardView>(FindObjectsInactive.Exclude)) card.CancelHover();
        if (CardViewHoverSystem.Instance != null) CardViewHoverSystem.Instance.Hide();
        ShowBattleEndOverlay();
        rewardPanel.SetActive(true);
        int goldAmount = 0;
        Sprite coinSprite = null;

        switch (GameDataManager.Instance.indexEnemy)
        {
            case 0:
                goldAmount = Random.Range(minorGoldRange.x, minorGoldRange.y);
                coinSprite = minorCoinSprite;
                break;
            case 1:
                goldAmount = Random.Range(eliteGoldRange.x, eliteGoldRange.y);
                coinSprite = eliteCoinSprite;
                break;
            case 2:
                goldAmount = Random.Range(bossGoldRange.x, bossGoldRange.y);
                coinSprite = bossCoinSprite;
                break;
        }
        if (GameDataManager.Instance.HasRelic("Greed's Pounch"))
        {
            goldAmount += 15;
        }
        goldSlot.SetupSlot(coinSprite, $"{goldAmount} Gold");
        GameDataManager.Instance.gold += goldAmount;

        float scrollChance = GameDataManager.Instance.indexEnemy switch
        {
            0 => minorScrollDropChance,
            1 => eliteScrollDropChance,
            _ => bossScrollDropChance,
        };
        ScrollData droppedScroll = Random.value < scrollChance ? GameDataManager.Instance.RandomScroll() : null;
        if (droppedScroll == null)
        {
            scrollSlot.HideSlot();
        }
        else if (GameDataManager.Instance.AddScroll(droppedScroll))
        {
            scrollSlot.SetupSlot(droppedScroll.Image, droppedScroll.name);
        }
        else
        {
            scrollSlot.SetupSlot(droppedScroll.Image, $"{droppedScroll.name} (full)");
        }
    }

    public void Next()
    {
        rewardPanel.SetActive(false);
        OpenCardRewardUI();
    }

    public void OpenCardRewardUI()
    {
        if (GameDataManager.Instance == null)
        {
            SceneManager.LoadScene("Map");
            return;
        }

        panelObject.SetActive(true);
        chosenListToDisplay.Clear();

        if (GameDataManager.Instance.indexEnemy == 0) // Minor Enemy
        {
            if (GameDataManager.Instance.indexHero == 0) // Knight
            {
                poolCopy = new(GameDataManager.Instance.MinorEnemyCardRewardPoolKnight);
            } else // Wizard
            {
                poolCopy = new(GameDataManager.Instance.MinorEnemyCardRewardPoolWizard);
            }
                
        } else if (GameDataManager.Instance.indexEnemy == 1) // Elite Enemy
        {
            if (GameDataManager.Instance.indexHero == 0) // Knight
            {
                poolCopy = new(GameDataManager.Instance.EliteEnemyCardRewardPoolKnight);
            }
            else // Wizard
            {
                poolCopy = new(GameDataManager.Instance.EliteEnemyCardRewardPoolWizard);
            }
        } else // Boss Enemy
        {
            if (GameDataManager.Instance.indexHero == 0) // Knight
            {
                poolCopy = new(GameDataManager.Instance.BossEnemyCardRewardPoolKnight);
            }
            else // Wizard
            {
                poolCopy = new(GameDataManager.Instance.BossEnemyCardRewardPoolWizard);
            }
        }

            int cardsToPick = Mathf.Min(3, poolCopy.Count);

        for (int i = 0; i < cardsToPick; i++)
        {
            int randomIndex = Random.Range(0, poolCopy.Count);
            Card runtimeCard = new Card(poolCopy[randomIndex]);
            chosenListToDisplay.Add(runtimeCard);
            poolCopy.RemoveAt(randomIndex);
        }

        for (int i = 0; i < rewardCardSlots.Count; i++)
        {
            if (i < chosenListToDisplay.Count)
            {
                rewardCardSlots[i].gameObject.SetActive(true);
                rewardCardSlots[i].Setup(chosenListToDisplay[i]);

                if (rewardCardSlots[i].TryGetComponent(out Button btn))
                {
                    int index = i;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnRewardCardSelected(chosenListToDisplay[index]));
                }
            }
            else
            {
                rewardCardSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnRewardCardSelected(Card selectedCard)
    {
        AudioManager.PlaySfx(Sfx.RewardPickup);
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.CurrentDeck.Add(selectedCard);
        }
        panelObject.SetActive(false);

        if (GameDataManager.Instance.indexEnemy == 0) // Minor Enemy
        {
            SceneManager.LoadScene("Map");
        }
        else // Elite & Boss Enemy
        {
            ShowRelicRewards();
        }
    }

    public void Skip()
    {
        panelObject.SetActive(false);
        if (GameDataManager.Instance.indexEnemy == 0) // Minor Enemy
        {
            SceneManager.LoadScene("Map");
        }
        else // Elite & Boss Enemy
        {
            ShowRelicRewards();
        }
    }

    private void ShowRelicRewards()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.AllRelicPool.Count == 0)
        {
            SceneManager.LoadScene("Map");
            return;
        }

        List<RelicData> availableRelics = new List<RelicData>();
        foreach (var relic in GameDataManager.Instance.AllRelicPool)
        {
            if (!GameDataManager.Instance.RelicDatas.Contains(relic))
            {
                availableRelics.Add(relic);
            }
        }

        List<RelicData> chosenRelics = new();
        int countToPick = 0;
        if (GameDataManager.Instance.indexEnemy == 2) // Boss Enemy
        {
            countToPick = Mathf.Min(2, availableRelics.Count);
        } else
        {
            countToPick = Mathf.Min(3, availableRelics.Count);
        }

        for (int i = 0; i < countToPick; i++)
        {
            int randomIndex = Random.Range(0, availableRelics.Count);
            chosenRelics.Add(availableRelics[randomIndex]);
            availableRelics.RemoveAt(randomIndex);
        }

        rewardPanelObject.SetActive(true);

        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (i < chosenRelics.Count)
            {
                relicSlots[i].gameObject.SetActive(true);
                relicSlots[i].Setup(chosenRelics[i]);

                int index = i;
                relicSlots[i].SelectButton.onClick.RemoveAllListeners();
                relicSlots[i].SelectButton.onClick.AddListener(() => OnRelicSelected(chosenRelics[index]));
            }
            else
            {
                relicSlots[i].gameObject.SetActive(false);
            }
        }
        if (GameDataManager.Instance.indexEnemy == 2) // Boss Enemy
        {
            relicSlots[2].gameObject.SetActive(true);
            relicSlots[2].Setup(GameDataManager.Instance.RelicHeroUpgrade);
            relicSlots[2].SelectButton.onClick.RemoveAllListeners();
            relicSlots[2].SelectButton.onClick.AddListener(() => OnRelicSelected(GameDataManager.Instance.RelicHeroUpgrade));
            GameDataManager.Instance.RemoveRelic(GameDataManager.Instance.RelicHero);
        }
    }

    private void OnRelicSelected(RelicData selectedRelic)
    {
        AudioManager.PlaySfx(Sfx.RewardPickup);
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.AddRelic(selectedRelic);
        }

        rewardPanelObject.SetActive(false);
        SceneManager.LoadScene("Map");
    }
}
