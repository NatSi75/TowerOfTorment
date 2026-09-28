using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero")]

public class HeroData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string Description {  get; private set; }
    [field: SerializeField] public Sprite Image {  get; private set; }
    [SerializeField] private bool flipSprite;
    [field: SerializeField] public RuntimeAnimatorController Animator { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Torment { get; private set; }
    [field: SerializeField] public int MaxTorment { get; private set; }
    [field: SerializeField] public float scaleX;
    [field: SerializeField] public float scaleY;
    [field: SerializeField] public List<CardData> Deck { get; private set; }
    [field: SerializeField] public PerkData PerkHero { get; private set; }
    [field: SerializeField] public PerkData PerkHeroUpgrade { get; private set; }
    public bool FlipSprite => flipSprite;
}
