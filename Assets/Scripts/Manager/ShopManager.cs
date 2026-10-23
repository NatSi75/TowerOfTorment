using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("UI Setup")]
    [Tooltip("Parent object yang memiliki komponen HorizontalLayoutGroup atau GridLayoutGroup")]
    public Transform shopContentContainer;
    public Transform shopContentContainer2;
    public GameObject shopCardPrefab;
    public GameObject shopItemPrefab;

    [Header("Card Pricing (min inclusive, max exclusive)")]
    public Vector2Int commonCardPrice = new(45, 56);
    public Vector2Int uncommonCardPrice = new(68, 83);
    public Vector2Int rareCardPrice = new(135, 166);

    [Header("Relic / Scroll Pricing")]
    public Vector2Int basePerkPrice = new(275, 300);
    public Vector2Int baseScrollPrice = new(135, 160);

    [Header("Shop Generation Limits")]
    [Tooltip("Cards are laid out from the left: commons, then uncommons, then rares")]
    public int commonCardsToSpawn = 2;
    public int uncommonCardsToSpawn = 2;
    public int rareCardsToSpawn = 1;
    public int perksToSpawn = 3;
    public int scrollsToSpawn = 3;

    private void Start()
    {
        GenerateShop();
    }

    private void GenerateShop()
    {
        // 1. Generate Cards: 2 common, 2 uncommon, 1 rare (left to right), picked by the card's own rarity
        List<CardData> heroCards = HeroCardPool();
        SpawnShopCards(heroCards, CardRarity.COMMON, commonCardsToSpawn, commonCardPrice);
        SpawnShopCards(heroCards, CardRarity.UNCOMMON, uncommonCardsToSpawn, uncommonCardPrice);
        SpawnShopCards(heroCards, CardRarity.RARE, rareCardsToSpawn, rareCardPrice);

        // 2. Generate Perks
        List<PerkData> availableRelics = new List<PerkData>();
        foreach (var perk in GameDataManager.Instance.AllPerkPool)
        {
            if (!GameDataManager.Instance.PerkDatas.Contains(perk))
            {
                availableRelics.Add(perk);
            }
        }
        List<PerkData> chosenRelics = new();
        int countToPick = Mathf.Min(perksToSpawn, availableRelics.Count);

        for (int i = 0; i < countToPick; i++)
        {
            int randomIndex = Random.Range(0, availableRelics.Count);
            chosenRelics.Add(availableRelics[randomIndex]);
            availableRelics.RemoveAt(randomIndex);
        }

        for (int i = 0; i < perksToSpawn; i++)
        {
            PerkData Perk = chosenRelics[i];
            SpawnShopItem(Perk.Image,Perk.name, Perk.Description, Random.Range(basePerkPrice.x, basePerkPrice.y), () => BuyPerk(Perk));
        }

        // 3. Generate Scrolls
        List<ScrollData> availableScrolls = new List<ScrollData>();
        foreach (var scroll in GameDataManager.Instance.ScrollRewardPool)
        {
                availableScrolls.Add(scroll);
        }

        List<ScrollData> chosenScrolls = new();
        int countToPick2 = Mathf.Min(scrollsToSpawn, availableScrolls.Count);

        for (int i = 0; i < countToPick2; i++)
        {
            int randomIndex = Random.Range(0, availableScrolls.Count);
            chosenScrolls.Add(availableScrolls[randomIndex]);
            availableScrolls.RemoveAt(randomIndex);
        }

        for (int i = 0; i < scrollsToSpawn; i++)
        {
            ScrollData randomScroll = chosenScrolls[i];
            SpawnShopItem(randomScroll.Image, randomScroll.name, randomScroll.Description, Random.Range(baseScrollPrice.x, baseScrollPrice.y), () => BuyScroll(randomScroll), () => GameDataManager.Instance.CanAddScroll);
        }
    }

    // Every card the current hero can get as a reward (the three reward pools together, no duplicates).
    private static List<CardData> HeroCardPool()
    {
        GameDataManager data = GameDataManager.Instance;
        bool knight = data.indexHero == 0;
        List<CardData> pool = new();
        foreach (List<CardData> rewardPool in knight
            ? new[] { data.MinorEnemyCardRewardPoolKnight, data.EliteEnemyCardRewardPoolKnight, data.BossEnemyCardRewardPoolKnight }
            : new[] { data.MinorEnemyCardRewardPoolWizard, data.EliteEnemyCardRewardPoolWizard, data.BossEnemyCardRewardPoolWizard })
        {
            if (rewardPool == null) continue;
            foreach (CardData card in rewardPool)
            {
                if (card != null && !pool.Contains(card)) pool.Add(card);
            }
        }
        return pool;
    }

    // Spawns `count` different cards of one rarity, each with a price from that rarity's range.
    private void SpawnShopCards(List<CardData> heroCards, CardRarity rarity, int count, Vector2Int priceRange)
    {
        List<CardData> candidates = heroCards.FindAll(card => card.RarityVisual == rarity);
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            CardData cardData = candidates[index];
            candidates.RemoveAt(index); // no two identical cards in the shop
            SpawnShopCard(new Card(cardData), Random.Range(priceRange.x, priceRange.y));
        }
    }

    private void SpawnShopCard(Card card, int price)
    {
        GameObject newCardObj = Instantiate(shopCardPrefab, shopContentContainer);
        ShopCardUI shopCardUI = newCardObj.GetComponent<ShopCardUI>();

        System.Action wrappedBuyLogic = () =>
        {
            int currentGold = GameDataManager.Instance.gold;

            if (currentGold >= price)
            {
                GameDataManager.Instance.gold -= price;
                AudioManager.PlaySfx(Sfx.ShopBuy);
                BuyCard(card.data);
                shopCardUI.MarkAsSold();
            }
            else
            {
                Debug.LogWarning("Gold tidak cukup!");
            }
        };

        shopCardUI.SetupCard(card, price, wrappedBuyLogic);
    }

    private void SpawnShopItem(Sprite icon, string name, string description, int price, System.Action buyLogic, System.Func<bool> canBuy = null)
    {
        GameObject newItemObj2 = Instantiate(shopItemPrefab, shopContentContainer2);
        ShopItemUI itemUI2 = newItemObj2.GetComponent<ShopItemUI>();

        System.Action wrappedBuyLogic = () =>
        {
            int currentGold = GameDataManager.Instance.gold;

            if (canBuy != null && !canBuy())
            {
                Debug.LogWarning("Slot scroll sudah penuh!");
            }
            else if (currentGold >= price)
            {
                GameDataManager.Instance.gold -= price;
                AudioManager.PlaySfx(Sfx.ShopBuy);
                buyLogic.Invoke();
                itemUI2.MarkAsSold(); 
            }
            else
            {
                Debug.LogWarning("Gold tidak cukup!");
            }
        };

        itemUI2.Setup(icon,name, description, price, wrappedBuyLogic);
    }

    // --- Logika Penambahan Item ---
    private void BuyCard(CardData card)
    {
        Card newCard = new(card);
        GameDataManager.Instance.CurrentDeck.Add(newCard);
    }

    private void BuyPerk(PerkData perk)
    {
        GameDataManager.Instance.AddPerk(perk);
    }

    private void BuyScroll(ScrollData scroll)
    {
        GameDataManager.Instance.AddScroll(scroll);
    }

    public void Done()
    {
        SceneManager.LoadScene("Map");
    }
}