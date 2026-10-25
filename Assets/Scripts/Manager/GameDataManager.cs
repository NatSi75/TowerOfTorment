using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    [Header("Characther Data")]
    [SerializeField] public HeroData heroDataKnight;
    [SerializeField] public HeroData heroDataWizard;
    int chosenHeroIndex;
    public HeroData heroData;
    public RelicData RelicHero => heroData.RelicHero;
    public RelicData RelicHeroUpgrade => heroData.RelicHeroUpgrade;

    [Header("Player Persistent Data")]
    public float CurrentHeroHP = -1;
    public int MaxHeroHP = 30;
    public int torment = 0;
    public int gold = 0;
    public List<Card> CurrentDeck = new();
    public List<RelicData> RelicDatas = new();
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
    [SerializeField] private List<RelicData> allRelicPool;
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
    public List<CardData> MinorEnemyCardRewardPoolWizard => minorEnemyCardRewardPoolWizard;
    public List<CardData> EliteEnemyCardRewardPoolWizard => eliteEnemyCardRewardPoolWizard;
    public List<CardData> BossEnemyCardRewardPoolWizard => bossEnemyCardRewardPoolWizard;

    public List<RelicData> AllRelicPool => allRelicPool;
    public int indexHero; // 0 = Knight, 1 = Wizard
    public int indexEnemy; // 0 = Minor Enemy, 1 = Elite Enemy, 2 = Boss

    [Header("Minor Enemy Selection (like Slay the Spire)")]
    [Tooltip("The first encounters of each act's minor list are the easy (weak) pool")]
    [SerializeField] private int weakEncounterPoolSize = 3;
    [Tooltip("How many minor fights at the start of each act use the weak pool; the rest use the strong pool")]
    [SerializeField] private int weakFightsPerAct = 3;
    [Tooltip("A new encounter is never one of the last N minor encounters fought")]
    [SerializeField] private int noRepeatOfLast = 2;
    private int minorFightsThisAct;
    private int minorFightsAct = -1;
    private readonly List<Enemy> recentMinorEncounters = new();

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
        RelicDatas.Add(heroData.RelicHero);
    }

    public void SetSequentialMinorEnemy()
    {
        List<Enemy> activePool = (currentAct == 2) ? act2MinorEnemies : act1MinorEnemies;
        NextEncounter.Clear();
        indexEnemy = 0;
        if (activePool == null || activePool.Count == 0) return;

        if (minorFightsAct != currentAct)
        {
            minorFightsAct = currentAct;
            minorFightsThisAct = 0;
            recentMinorEncounters.Clear();
        }

        int weakCount = Mathf.Clamp(weakEncounterPoolSize, 0, activePool.Count);
        bool useWeak = minorFightsThisAct < weakFightsPerAct && weakCount > 0;
        List<Enemy> candidates = useWeak
            ? activePool.GetRange(0, weakCount)
            : activePool.GetRange(weakCount, activePool.Count - weakCount);
        if (candidates.Count == 0) candidates = new List<Enemy>(activePool);

        List<Enemy> fresh = candidates.FindAll(encounter => !recentMinorEncounters.Contains(encounter));
        if (fresh.Count > 0) candidates = fresh;

        Enemy chosen = candidates[Random.Range(0, candidates.Count)];
        NextEncounter.AddRange(chosen.ListEnemyData);

        recentMinorEncounters.Add(chosen);
        while (recentMinorEncounters.Count > Mathf.Max(0, noRepeatOfLast)) recentMinorEncounters.RemoveAt(0);
        minorFightsThisAct++;
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

    public void AddRelic(RelicData relic)
    {
        if (!RelicDatas.Contains(relic))
        {
            RelicDatas.Add(relic);
            ApplyPickupEffect(relic);
        }
    }

    private void ApplyPickupEffect(RelicData relic)
    {
        switch (NormalizeRelicName(relic.Name))
        {
            case "Green Elixir": IncreaseMaxHP(7); break;
            case "Purple Elixir": IncreaseMaxHP(10); break;
            case "Crimson Elixir": IncreaseMaxHP(14); break;
            case "Fallen King's Crown": gold += 300; break;
        }
    }

    private static string NormalizeRelicName(string name)
    {
        return string.IsNullOrEmpty(name) ? "" : name.Replace('’', '\'').Replace('‘', '\'').Replace('�', '\'').Trim();
    }

    public void RemoveRelic(RelicData relic)
    {
        if (RelicDatas.Contains(relic))
        {
            RelicDatas.Remove(relic);
        }
    }

    public bool HasRelic(string nameRelic)
    {
        string wanted = NormalizeRelicName(nameRelic);
        foreach (RelicData relic in RelicDatas)
        {
            if (NormalizeRelicName(relic.Name) == wanted)
            {
                return true;
            }
        }
        return false;
    }

    public void AddTorment(int amount)
    {
        int max = heroData != null ? heroData.MaxTorment : int.MaxValue;
        torment = Mathf.Clamp(torment + amount, 0, max);
    }

    public void IncreaseMaxHP(int amount)
    {
        CurrentHeroHP += amount;
        MaxHeroHP += amount;
    }

    public const int MaxScrolls = 3;
    public bool CanAddScroll => ScrollDatas.Count < MaxScrolls;

    public bool AddScroll(ScrollData scroll)
    {
        if (!CanAddScroll) return false;
        ScrollDatas.Add(scroll);
        return true;
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
