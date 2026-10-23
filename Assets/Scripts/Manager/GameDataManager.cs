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
    public List<CardData> MinorEnemyCardRewardPoolWizard => minorEnemyCardRewardPoolWizard;
    public List<CardData> EliteEnemyCardRewardPoolWizard => eliteEnemyCardRewardPoolWizard;
    public List<CardData> BossEnemyCardRewardPoolWizard => bossEnemyCardRewardPoolWizard;

    public List<PerkData> AllPerkPool => allPerkPool;
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
        PerkDatas.Add(heroData.PerkHero);
    }

    // Picks the next minor encounter like Slay the Spire:
    //  - the first fights of an act come from the weak pool (the first entries of the act's list), later ones from the strong pool,
    //  - chosen at random, but never one of the last encounters fought,
    //  - everything starts over in a new act.
    // (Kept under its old name; it used to walk through the list in order.)
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

        // weak or strong pool
        int weakCount = Mathf.Clamp(weakEncounterPoolSize, 0, activePool.Count);
        bool useWeak = minorFightsThisAct < weakFightsPerAct && weakCount > 0;
        List<Enemy> candidates = useWeak
            ? activePool.GetRange(0, weakCount)
            : activePool.GetRange(weakCount, activePool.Count - weakCount);
        if (candidates.Count == 0) candidates = new List<Enemy>(activePool);

        // not one of the last encounters (unless the pool is too small for that)
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

    // Every way of getting a relic (shop, treasure, battle reward) goes through here,
    // so the one-time "upon pickup" effects are applied in one place.
    public void AddPerk(PerkData perk)
    {
        if (!PerkDatas.Contains(perk))
        {
            PerkDatas.Add(perk);
            ApplyPickupEffect(perk);
        }
    }

    private void ApplyPickupEffect(PerkData perk)
    {
        switch (NormalizePerkName(perk.Name))
        {
            case "Green Elixir": IncreaseMaxHP(7); break;
            case "Purple Elixir": IncreaseMaxHP(10); break;
            case "Crimson Elixir": IncreaseMaxHP(14); break;
            case "Fallen King's Crown": gold += 300; break;
        }
    }

    // Relic names may use a typographic apostrophe (’); compare them with a plain one.
    private static string NormalizePerkName(string name)
    {
        return string.IsNullOrEmpty(name) ? "" : name.Replace('’', '\'').Replace('‘', '\'').Replace('�', '\'').Trim();
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
        string wanted = NormalizePerkName(namePerk);
        foreach (PerkData perk in PerkDatas)
        {
            if (NormalizePerkName(perk.Name) == wanted)
            {
                return true;
            }
        }
        return false;
    }

    // Adds torment without going past the hero's maximum.
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

    // Returns false when all scroll slots are already used.
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
