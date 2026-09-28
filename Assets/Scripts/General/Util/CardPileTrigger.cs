using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Menggunakan IPointerClickHandler agar bekerja baik dengan UI Canvas
public class CardPileTrigger : MonoBehaviour, IPointerClickHandler
{
    public enum PileType { DrawPile, DiscardPile, ExhaustPile, UpgradeCard, RemoveCard }

    [Header("Settings")]
    [SerializeField] private PileType typeOfPile;
    [SerializeField] private CardPileViewerUI viewerUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardSystem.Instance == null) return;
        
        switch (typeOfPile)
        {
            case PileType.DrawPile:
                List<Card> currentDrawPile = CardSystem.Instance.drawPile;
                viewerUI.OpenViewer("Draw Pile", currentDrawPile);
                break;
            case PileType.DiscardPile:
                List<Card> currentDiscardPile = CardSystem.Instance.discardPile;
                viewerUI.OpenViewer("Discard Pile", currentDiscardPile);
                break;
            case PileType.ExhaustPile:
                List<Card> currentExhaustPile = CardSystem.Instance.exhaustPile;
                viewerUI.OpenViewer("Exhaust Pile", currentExhaustPile);
                break;
            default:
                break;
        }
    }
}