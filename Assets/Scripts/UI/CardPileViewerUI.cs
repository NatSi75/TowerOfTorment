using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPileViewerUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject viewerPanel;
    [SerializeField] private Transform gridContent; 
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Prefab References")]
    [SerializeField] private GameObject cardPrefab; 

    private List<GameObject> spawnedCardViews = new List<GameObject>();

    private void Start()
    {
        viewerPanel.SetActive(false);
    }
   
    public void OpenViewer(string pileName, List<Card> cardDataList)
    {
        viewerPanel.SetActive(true);
        titleText.text = $"{pileName}";

        ClearSpawnedCards();

        foreach (Card cardData in cardDataList)
        {
            GameObject newCard = Instantiate(cardPrefab, gridContent);
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

    public void CloseViewer()
    {
        viewerPanel.SetActive(false);
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
}