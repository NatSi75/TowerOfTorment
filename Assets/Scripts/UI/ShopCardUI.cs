using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCardUI : MonoBehaviour
{
    [Header("Card Reference")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject vertical;
    [SerializeField] private Transform gridContent;

    [Header("Shop UI Elements")]
    public TMP_Text priceText;
    public Button buyButton;
    public GameObject soldOutOverlay;
    private GameObject spawnedCard;
    private System.Action onBuyAction;

    public void SetupCard(Card card, int price, System.Action buyCallback)
    {
        spawnedCard = Instantiate(cardPrefab, gridContent);

        if (spawnedCard.TryGetComponent(out RectTransform rectTransform))
        {
            rectTransform.localPosition = new Vector3(250,-50,0);
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }
        else
        {
            spawnedCard.transform.localPosition = Vector3.zero;
            spawnedCard.transform.localScale = Vector3.one;
            spawnedCard.transform.localRotation = Quaternion.identity;
        }

        if (spawnedCard.TryGetComponent(out CardView cardView))
        {
            cardView.Setup(card);
        }

        priceText.text = price.ToString() + " G";
        onBuyAction = buyCallback;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        if (soldOutOverlay != null) soldOutOverlay.SetActive(false);
    }

    private void OnBuyClicked()
    {
        onBuyAction?.Invoke();
    }

    public void MarkAsSold()
    {
        buyButton.interactable = false;
        vertical.SetActive(false);
        if (soldOutOverlay != null) soldOutOverlay.SetActive(true);
    }
}