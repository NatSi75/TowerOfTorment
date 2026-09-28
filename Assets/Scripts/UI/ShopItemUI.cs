using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    [SerializeField] private GameObject vertical;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text descriptionText;
    public Button buyButton;
    public GameObject soldOutOverlay;

    private System.Action onBuyAction;

    public void Setup(Sprite icon,string name, string description, int price, System.Action buyCallback)
    {
        itemIcon.sprite = icon;
        nameText.text = name;
        priceText.text = price + " G";
        descriptionText.text = description;

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