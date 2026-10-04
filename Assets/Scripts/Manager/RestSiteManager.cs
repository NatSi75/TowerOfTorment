using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CardPileTrigger;

public class RestSiteManager : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private GameObject buttonsCanvasGroup;
    [SerializeField] private int healPercentage = 30;
    [SerializeField] private int tormentAmount = 25;

    [Header("UI Elements")]
    [SerializeField] private GameObject viewerPanel;
    [SerializeField] private Transform gridContent;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Prefab References")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Preview UI")]
    [SerializeField] private GameObject upgradePreviewPanel;
    [SerializeField] private GameObject removePreviewPanel;
    [SerializeField] private CardView beforeCardView;
    [SerializeField] private CardView afterCardView;
    [SerializeField] private Button confirmUpgradeButton;
    [SerializeField] private CardView previewCardView;
    [SerializeField] private Button confirmRemoveButton;
    [SerializeField] private Button cancelPreviewButton;

    [Header("Button References")]
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private GameObject healButton;
    [SerializeField] private GameObject removeButton;

    private List<GameObject> spawnedCardViews = new List<GameObject>();
    GameObject newCard;
    public int click = 0;
    public bool firstTime = true;

    private Card cardSelectedForUpgrade;
    private Card cardSelectedForRemove;
    public enum RestSiteState { ChoosingOption, SelectingCardForUpgrade, SelectingCardForRemove }
    public RestSiteState currentState = RestSiteState.ChoosingOption;
    private int optionsUsedCount = 0;
    private const int MAX_OPTIONS = 3;

    private void Start()
    {
        viewerPanel.SetActive(false);
        healButton.SetActive(false);
    }

    // LOGIKA PILIHAN MEMULIHKAN HP (HEAL)
    public void OnHealClicked()
    {
        if (GameDataManager.Instance != null)
        {
            int healAmount = Mathf.RoundToInt(GameDataManager.Instance.MaxHeroHP * (healPercentage / 100f));

            if (GameDataManager.Instance.HasPerk("Vitality Vial"))
            {
                healAmount += 15;
            }

            GameDataManager.Instance.CurrentHeroHP = Mathf.Min(GameDataManager.Instance.CurrentHeroHP + healAmount, GameDataManager.Instance.MaxHeroHP);
            AudioManager.PlaySfx(Sfx.RestHeal);
            int tormentCurrent = GameDataManager.Instance.torment;
            if (tormentCurrent < 25)
            {
                GameDataManager.Instance.torment = 0;
            } else
            {
                GameDataManager.Instance.torment -= tormentAmount;
            }
            HandleOptionSelected(healButton, upgradePreviewPanel);
        }
    }

    // LOGIKA PILIHAN UPGRADE KARTU
    public void OnUpgradeClicked()
    {
        currentState = RestSiteState.SelectingCardForUpgrade;
        List<Card> deckForUpgrade = new();
        foreach (Card card in GameDataManager.Instance.CurrentDeck) { 
            if (!card.IsUpgradeVersion)
            {
                deckForUpgrade.Add(card);
            }
        }    
        OpenViewerCardSelectionUI("Pilih 1 kartu untuk di upgrade", "Upgrade Card", deckForUpgrade);
        DisableMainButtons();
    }

    public void ShowUpgradePreview(Card selectedCard)
    {
        if (currentState != RestSiteState.SelectingCardForUpgrade) return;
        if (selectedCard == null || selectedCard.data == null) return;

        CardData upgradedData = selectedCard.data.UpgradedCardData;
        if (upgradedData == null)
        {
            Debug.LogWarning("Kartu ini tidak memiliki versi upgrade!");
            return;
        }

        cardSelectedForUpgrade = selectedCard;
        upgradePreviewPanel.SetActive(true);
        beforeCardView.Setup(selectedCard);
        Card upgradedPreviewCard = new (upgradedData);
        afterCardView.Setup(upgradedPreviewCard);
    }

    public void ExecuteFinalUpgrade()
    {
        if (cardSelectedForUpgrade == null) return;

        var deck = GameDataManager.Instance.CurrentDeck;
        int cardIndex = deck.IndexOf(cardSelectedForUpgrade);

        if (cardIndex != -1 && cardSelectedForUpgrade.data.UpgradedCardData != null)
        {
            Card upgradedNewCard = new (cardSelectedForUpgrade.data.UpgradedCardData);
            deck[cardIndex] = upgradedNewCard;
            GameDataManager.Instance.CurrentDeck = deck;

            upgradePreviewPanel.SetActive(false);
            HandleOptionSelected(upgradeButton, upgradePreviewPanel);
            CloseViewer();
        }
    }

    // LOGIKA PILIHAN REMOVE KARTU
    public void OnRemoveClicked()
    {
        currentState = RestSiteState.SelectingCardForRemove;
        List<Card> deckForRemove = GameDataManager.Instance.CurrentDeck;
        OpenViewerCardSelectionUI("Pilih 1 kartu untuk di remove", "Remove Card", deckForRemove);
        DisableMainButtons();
    }

    public void ShowRemovePreview(Card selectedCard)
    {
        if (currentState != RestSiteState.SelectingCardForRemove) return;
        if (selectedCard == null || selectedCard.data == null) return;

        cardSelectedForRemove = selectedCard;
        removePreviewPanel.SetActive(true);
        previewCardView.Setup(selectedCard);
    }

    public void ExecuteFinalRemove()
    {
        if (cardSelectedForRemove == null) return;

        var deck = GameDataManager.Instance.CurrentDeck;
        if (deck.Contains(cardSelectedForRemove))
        {
            deck.Remove(cardSelectedForRemove);
            GameDataManager.Instance.CurrentDeck = deck;
        }

        HandleOptionSelected(removeButton, removePreviewPanel);
        CloseViewer();
    }

    // UTILITY
    public void OpenViewerCardSelectionUI(string SaveMessage, string pileName, List<Card> cardDataList)
    {
        titleText.text = $"{pileName}";
        viewerPanel.SetActive(true);
        ClearSpawnedCards();

        foreach (Card cardData in cardDataList)
        {
            newCard = Instantiate(cardPrefab, gridContent);
            spawnedCardViews.Add(newCard);

            if (newCard.TryGetComponent(out RectTransform rectTransform))
            {
                rectTransform.localPosition = Vector3.zero;
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
            }
            else
            {
                newCard.transform.localPosition = Vector3.zero;
                newCard.transform.localScale = Vector3.one;
                newCard.transform.localRotation = Quaternion.identity;
            }

            if (newCard.TryGetComponent(out CardView cardView))
            {
                cardView.Setup(cardData);
            }

            if (newCard.TryGetComponent(out Button cardButton))
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(cardView.OnCardClick);
            }

            /* Nonaktifkan komponen drag jika ada agar kartu di dalam viewer tidak bisa ditarik
            if (newCard.TryGetComponent(out CardDraggable dragComponent))
            {
                dragComponent.enabled = false;
            } */
        }
    }

    public void CloseViewer()
    {
        viewerPanel.SetActive(false);
        ClearSpawnedCards();
        EnableMainButtons();
    }

    private void ClearSpawnedCards()
    {
        foreach (GameObject cardObj in spawnedCardViews)
        {
            Destroy(cardObj);
        }
        spawnedCardViews.Clear();
    }

    public void DisableMainButtons()
    {
        click++;
        if (firstTime)
        {
            firstTime = false;
        } else
        {
            if (buttonsCanvasGroup != null & click > 0)
            {
                click = 0;
                buttonsCanvasGroup.SetActive(false);
            }
        }
        
    }

    public void EnableMainButtons()
    {
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.SetActive(true);
        }
    }

    public void ClosePreviewPanel()
    {
        upgradePreviewPanel.SetActive(false);
        removePreviewPanel.SetActive(false);
        cardSelectedForUpgrade = null;
        cardSelectedForRemove = null;
    }

    private void HandleOptionSelected(GameObject clickedButton, GameObject panel)
    {
        if (clickedButton != null)
        {
            clickedButton.SetActive(false);
            panel.SetActive(false);
        }

        optionsUsedCount++;

        if (optionsUsedCount == 2)
        {
            healButton.SetActive(true);
        }

        if (optionsUsedCount >= MAX_OPTIONS)
        {
            LeaveRestSite();
        }
    }

    private void LeaveRestSite()
    {
        SceneManager.LoadScene("Map");
    }
}


