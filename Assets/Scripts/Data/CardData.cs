using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Data/Card")]

public class CardData : ScriptableObject 
{
    [field: SerializeField] public string Description { get; private set;}
    [field: SerializeField] public int Mana { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public Sprite ImageUI { get; private set; }
    [field: SerializeField] public CardRarity RarityVisual { get; private set; }
    [field: SerializeField] public CardType Type { get; private set; }
    [field: SerializeField] private bool exhaust;
    [field: SerializeReference, SR] public List<Effect> ManualTargetEffect { get; private set; } = null;
    [field: SerializeField] public List<AutoTargetEffect> OtherEffects { get; private set; }
    [Header("Upgrade Settings")]
    [SerializeField] private CardData upgradedCardData;
    [SerializeField] bool isUpgradedVersion = false;

    public CardData UpgradedCardData => upgradedCardData;
    public bool IsUpgradedVersion => isUpgradedVersion;
    public bool Exhaust => exhaust;
}
