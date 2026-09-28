using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopBarManager : MonoBehaviour
{
    public static TopBarManager Instance { get; private set; }
    public static GameObject[] hiddenBattleObjects;
    public static bool isViewOnlyMode = false;
    [Header("UI Elements - Left")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text tormentText;
    [SerializeField] private TMP_Text goldText;

    [Header("UI Elements - Right")]
    [SerializeField] public Button mapButton;
    [SerializeField] private Button deckButton;

    [SerializeField] private GameObject viewerPanel;
    [SerializeField] private Transform gridContent;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Canvas canvas;
    private List<GameObject> spawnedCardViews = new List<GameObject>();
    GameObject newCard;
    [Header("Prefab References")]
    [SerializeField] private GameObject cardPrefab;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(canvas.gameObject);
        }
        else
        {
            Destroy(gameObject);
            Destroy(canvas.gameObject);
            return;
        }
    }

    private void Start()
    {
        viewerPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateHealthUI();
        UpdateTormentUI();
        UpdateGoldUI();
        ToggleVisibilityBasedOnScene();
    }

    private void UpdateHealthUI()
    {
        if (GameDataManager.Instance != null)
        {
            float currentHp = GameDataManager.Instance.CurrentHeroHP;
            int maxHp = GameDataManager.Instance.MaxHeroHP;

            hpText.text = "HP: " + $"{currentHp}/{maxHp}";
        }
    }

    private void UpdateTormentUI()
    {
        if (GameDataManager.Instance != null)
        {
            float tormentHp = GameDataManager.Instance.torment;
            tormentText.text = "Torment: " + $"{tormentHp}" + "/" + GameDataManager.Instance.heroData.MaxTorment;
        }
    }

    private void UpdateGoldUI()
    {
        if (GameDataManager.Instance != null)
        {
            goldText.text = $"{GameDataManager.Instance.gold} Gold";
        }
    }

    public void OnMapClicked()
    {
        if (isViewOnlyMode) return;
        isViewOnlyMode = true;
        if (mapButton != null)
        {
            mapButton.interactable = false;
        }
        Scene currentSceneName = SceneManager.GetActiveScene();
        if (currentSceneName.isLoaded)
        {
            hiddenBattleObjects = currentSceneName.GetRootGameObjects();

            foreach (GameObject obj in hiddenBattleObjects)
            {
                if (obj.GetComponent<Camera>() != null || obj.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                {
                    continue;
                }
                obj.SetActive(false); 
            }
        }
        SceneManager.LoadScene("Map", LoadSceneMode.Additive);
    }

    public void OnDeckClicked()
    {
        OpenViewerCardSelectionUI("Deck", GameDataManager.Instance.CurrentDeck);
    }

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

            /* Nonaktifkan komponen drag jika ada agar kartu di dalam viewer tidak bisa ditarik
            if (newCard.TryGetComponent(out CardDraggable dragComponent))
            {
                dragComponent.enabled = false;
            } */
        }
    }

    private void ClearSpawnedCards()
    {
        foreach (GameObject cardObj in spawnedCardViews)
        {
            Destroy(cardObj);
        }
        spawnedCardViews.Clear();
    }

    public void CloseViewer()
    {
        viewerPanel.SetActive(false);
        ClearSpawnedCards();
    }


    private void ToggleVisibilityBasedOnScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MainMenu")
        {
            GetComponent<Canvas>().enabled = false;
        }
        else
        {
            GetComponent<Canvas>().enabled = true;
        }

        if (currentSceneName == "Map")
        {
            mapButton.gameObject.SetActive(false);
        }
        else
        {
            mapButton.gameObject.SetActive(true);
        }
    }
}
