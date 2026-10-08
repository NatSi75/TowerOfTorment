using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private CardView cardViewPrefab;

    public CardView CreateCardView(Card card, Vector3 position, Quaternion rotation)
    {
        CardView cardView = Instantiate(cardViewPrefab, position, rotation);
        cardView.transform.localScale = Vector3.zero;
        float scale = MobileLayout.HandScale;
        cardView.transform.DOScale(new Vector3(scale, scale), 0.15f);
        cardView.Setup(card);
        return cardView;
    }
}
