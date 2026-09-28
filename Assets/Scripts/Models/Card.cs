using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Card
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public Sprite ImageUI => data.ImageUI;
    public CardRarity RarityVisual => data.RarityVisual;
    public CardType Type => data.Type;
    public List<Effect> ManualTargetEffect => data.ManualTargetEffect;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    public CardData UpgradedCardData => data.UpgradedCardData;
    public bool IsUpgradeVersion => data.IsUpgradedVersion;
    public int Mana { get; private set; }
    public string description { get;  set; }
    public readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
        Mana = cardData.Mana;
        description = cardData.Description;
    }
}
