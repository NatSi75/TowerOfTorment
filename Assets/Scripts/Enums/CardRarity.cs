using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardRarity
{
    COMMON,
    UNCOMMON,
    RARE
}

[System.Serializable]
public struct RarityVisual
{
    public CardRarity rarity;
    public GameObject backgroundSprite;
}
