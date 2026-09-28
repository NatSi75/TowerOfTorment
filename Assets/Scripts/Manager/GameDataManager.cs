using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    [Header("Characther Data")]
    [SerializeField] public HeroData heroDataKnight;
    [SerializeField] public HeroData heroDataWizard;
    int chosenHeroIndex;
    public HeroData heroData;
    public PerkData PerkHero => heroData.PerkHero;
    public PerkData PerkHeroUpgrade => heroData.PerkHeroUpgrade;

    [Header("Player Persistent Data")]
    public float CurrentHeroHP = -1;
    public int MaxHeroHP = 30;
    public int torment = 0;
    public int gauge = 0;
    public int gold = 0;
    public List<Card> CurrentDeck = new();
    public List<PerkData> PerkDatas = new();
    public List<ScrollData> ScrollDatas = new();
    
    [Header("Act Settings")]
    public int currentAct = 2;
    [field: SerializeReference, SR] public List<Enemy> act1MinorEnemies { get; private set; }
    [field: SerializeField] public List<EnemyData> act1EliteEnemies { get; private set; }
    [field: SerializeField] public List<EnemyData> act1BossEnemies { get; private set; }
    [field: SerializeReference, SR] public List<Enemy> act2MinorEnemies { get; private set; }
    [field: SerializeField] public List<EnemyData> act2EliteEnemies { get; private set; }
    [field: SerializeField] public List<EnemyData> act2BossEnemies { get; private set; }
    public List<EnemyData> NextEncounter { get; set; } = new List<EnemyData>();

    [Header("Reward Setting")]
    [Header("List Relic")]
    [SerializeField] private List<PerkData> allPerkPool;
    [Header("Scroll")]
    [SerializeField] private List<ScrollData> scrollRewardPool;
    public List<ScrollData> ScrollRewardPool => scrollRewardPool;

    [Header("Card Knight")]
    [SerializeField] private List<CardData> minorEnemyCardRewardPoolKnight;
    [SerializeField] private List<CardData> eliteEnemyCardRewardPoolKnight;
    [SerializeField] private List<CardData> bossEnemyCardRewardPoolKnight;
    public List<CardData> MinorEnemyCardRewardPoolKnight => minorEnemyCardRewardPoolKnight;
    public List<CardData> EliteEnemyCardRewardPoolKnight => eliteEnemyCardRewardPoolKnight;
    public List<CardData> BossEnemyCardRewardPoolKnight => bossEnemyCardRewardPoolKnight;

    [Header("Card Wizard")]
    [SerializeField] private List<CardData> minorEnemyCardRewardPoolWizard;
    [SerializeField] private List<CardData> eliteEnemyCardRewardPoolWizard;
    [SerializeField] private List<CardData> bossEnemyCardRewardPoolWizard;
    public List<CardData> MinorEnemyCardRewardPoolWizard => minorEnemyCardRewardPoolKnight;
    public List<CardData> EliteEnemyCardRewardPoolWizard => eliteEnemyCardRewardPoolKnight;
    public List<CardData> BossEnemyCardRewardPoolWizard => bossEnemyCardRewardPoolKnight;

    public List<PerkData> AllPerkPool => allPerkPool;
    public int indexHero; // 0 = Knight, 1 = Wizard
    public int indexEnemy; // 0 = Minor Enemy, 1 = Elite Enemy, 2 = Boss
    private int currentEnemyIndex = 0;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;

        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultData();
        }
    }

    private void InitializeDefaultData()
    {
        chosenHeroIndex = PlayerPrefs.GetInt("SelectedHeroIndex", 0);
        indexHero = chosenHeroIndex;
        if (chosenHeroIndex == 0)
        {
            heroData = heroDataKnight;
        } else
        {   heroData = heroDataWizard;

        }
        foreach (var cardData in heroData.Deck)
        {
            Card card = new(cardData);
            CurrentDeck.Add(card);
        }
        torment = heroData.Torment;
        MaxHeroHP = heroData.Health;
        CurrentHeroHP = MaxHeroHP;
        PerkDatas.Add(heroData.PerkHero);
    }

    public void SetSequentialMinorEnemy()
    {
        List<Enemy> activePool = (currentAct == 2) ? act2MinorEnemies : act1MinorEnemies;
        NextEncounter.Clear();
        indexEnemy = 0;
        if (activePool != null && activePool.Count > 0)
        {
            int index = currentEnemyIndex % activePool.Count;
            List<EnemyData> selectedEncounter = activePool[index].ListEnemyData;
            NextEncounter.AddRange(selectedEncounter);
            currentEnemyIndex++;
        }
    }

    public void SetEliteEnemy()
    {
        List<EnemyData> activePool = (currentAct == 2) ? act2EliteEnemies : act1EliteEnemies;
        NextEncounter.Clear();
        indexEnemy = 1;
        if (activePool != null && activePool.Count > 0)
        {
            NextEncounter.AddRange(activePool);
        }
    }

    public void SetBossEnemy()
    {
        List<EnemyData> activePool = (currentAct == 2) ? act2BossEnemies : act1BossEnemies;
        NextEncounter.Clear();
        indexEnemy = 2;
        if (activePool != null && activePool.Count > 0)
        {
            NextEncounter.AddRange(activePool);
        }
    }

    public void AddPerk(PerkData perk)
    {
        if (!PerkDatas.Contains(perk))
        {
            PerkDatas.Add(perk);
        }
    }

    public void RemovePerk(PerkData perk)
    {
        if (PerkDatas.Contains(perk))
        {
            PerkDatas.Remove(perk);
        }
    }

    public bool HasPerk(string namePerk)
    {
        foreach (PerkData perk in PerkDatas)
        {
            if (perk.Name == namePerk)
            {
                return true;
            }
        }
        return false;
    }

    public void IncreaseMaxHP(int amount)
    {
        CurrentHeroHP += amount;
        MaxHeroHP += amount;
    }

    public void AddScroll(ScrollData scroll)
    {
        ScrollDatas.Add(scroll);
    }

    public void RemoveScroll(ScrollData scroll)
    {
        if (ScrollDatas.Contains(scroll))
        {
            ScrollDatas.Remove(scroll);
        }
    }

    public ScrollData RandomScroll()
    {
        int randomIndex = Random.Range(0, scrollRewardPool.Count);
        return scrollRewardPool[randomIndex];
    }
}
