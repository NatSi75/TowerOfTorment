using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static RestSiteManager;

public class PillarManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private GameObject viewerPanel;
    [SerializeField] private Transform gridContent;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Prefab References")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Preview UI Remove")]
    [SerializeField] private GameObject removePreviewPanel;
    [SerializeField] private CardView previewCardView;
    [SerializeField] private Button confirmRemoveButton;
    [SerializeField] private Button cancelPreviewButton;

    [Header("Preview UI Upgrade")]
    [SerializeField] private GameObject upgradePreviewPanel;
    [SerializeField] private CardView beforeCardView;
    [SerializeField] private CardView afterCardView;
    [SerializeField] private Button confirmUpgradeButton;

    [Header("Button References")]
    [SerializeField] private GameObject confirmButton;

    public enum PillarState { ChoosingOption, SelectingCardForUpgrade, SelectingCardForRemove }
    public PillarState currentState = PillarState.ChoosingOption;

    private List<GameObject> spawnedCardViews = new List<GameObject>();
    GameObject newCard;
    private int actionCount = 0;
    public int click = 0;
    public bool firstTime = true;
    private Card cardSelectedForUpgrade;
    private Card cardSelectedForRemove;

    // LOGIKA PILIHAN UPGRADE KARTU
    public void OnClicked()
    {
        AudioManager.PlaySfx(Sfx.PillarSacrifice);
        if (GameDataManager.Instance.currentAct == 1)
        {
            OnUpgradeClicked();
        } else
        {
            OnRemoveClicked();
        }
    }

    public void OnUpgradeClicked()
    {
        currentState = PillarState.SelectingCardForUpgrade;
        List<Card> deckForUpgrade = new();
        foreach (Card card in GameDataManager.Instance.CurrentDeck)
        {
            if (!card.IsUpgradeVersion)
            {
                deckForUpgrade.Add(card);
            }
        }
        OpenViewerCardSelectionUI("Select 3 Card to Upgrade", deckForUpgrade);
        DisableMainButtons();
    }

    public void ShowUpgradePreview(Card selectedCard)
    {
        if (currentState != PillarState.SelectingCardForUpgrade) return;
        if (selectedCard == null || selectedCard.data == null) return;

        CardData upgradedData = selectedCard.data.UpgradedCardData;
        if (upgradedData == null)
        {
            return;
        }

        cardSelectedForUpgrade = selectedCard;
        upgradePreviewPanel.SetActive(true);
        beforeCardView.Setup(selectedCard);
        Card upgradedPreviewCard = new(upgradedData);
        afterCardView.Setup(upgradedPreviewCard);
    }

    public void ExecuteFinalUpgrade()
    {
        if (cardSelectedForUpgrade == null) return;

        var deck = GameDataManager.Instance.CurrentDeck;
        int cardIndex = deck.IndexOf(cardSelectedForUpgrade);

        if (cardIndex != -1 && cardSelectedForUpgrade.data.UpgradedCardData != null)
        {
            Card upgradedNewCard = new(cardSelectedForUpgrade.data.UpgradedCardData);
            deck[cardIndex] = upgradedNewCard;
            GameDataManager.Instance.CurrentDeck = deck;
            CloseViewer();
            if (actionCount < 2)
            {
                OnUpgradeClicked();
                actionCount++;
            } else {
                GameDataManager.Instance.torment += 10;
                SceneManager.LoadScene("Map");
            }
        }
    }

    // LOGIKA PILIHAN REMOVE KARTU
    public void OnRemoveClicked()
    {
        currentState = PillarState.SelectingCardForRemove;
        List<Card> deckForRemove = GameDataManager.Instance.CurrentDeck;
        OpenViewerCardSelectionUI("Select 3 Card to Remove", deckForRemove);
        DisableMainButtons();
    }

    public void ShowRemovePreview(Card selectedCard)
    {
        if (currentState != PillarState.SelectingCardForRemove) return;
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
        CloseViewer();
        if (actionCount < 2)
        {
            OnRemoveClicked();
            actionCount++;
        }
        else
        {
            GameDataManager.Instance.torment += 10;
            SceneManager.LoadScene("Map");
        }
    }

    // UTILITY
    public void OpenViewerCardSelectionUI(string pileName, List<Card> cardDataList)
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

        CardGridLayout.Fit(gridContent, cardDataList.Count);
    }

    public void CloseViewer()
    {
        viewerPanel.SetActive(false);
        upgradePreviewPanel.SetActive(false);
        removePreviewPanel.SetActive(false);
        cardSelectedForUpgrade = null;
        cardSelectedForRemove = null;
        ClearSpawnedCards();
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
        }
        else
        {
            if (confirmButton != null & click > 0)
            {
                click = 0;
                popupPanel.SetActive(false);
            }
        }
    }

    public void ClosePreviewPanel()
    {
        upgradePreviewPanel.SetActive(false);
        removePreviewPanel.SetActive(false);
        cardSelectedForUpgrade = null;
        cardSelectedForRemove = null;
    }
}
