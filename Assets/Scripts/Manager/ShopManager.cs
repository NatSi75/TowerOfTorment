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
    public Vector2Int baseRelicPrice = new(275, 300);
    public Vector2Int baseScrollPrice = new(135, 160);

    [Header("Shop Generation Limits")]
    [Tooltip("Cards are laid out from the left: commons, then uncommons, then rares")]
    public int commonCardsToSpawn = 2;
    public int uncommonCardsToSpawn = 2;
    public int rareCardsToSpawn = 1;
    public int relicsToSpawn = 3;
    public int scrollsToSpawn = 3;

    private void Start()
    {
        GenerateShop();
    }

    private void GenerateShop()
    {
        List<CardData> heroCards = HeroCardPool();
        SpawnShopCards(heroCards, CardRarity.COMMON, commonCardsToSpawn, commonCardPrice);
        SpawnShopCards(heroCards, CardRarity.UNCOMMON, uncommonCardsToSpawn, uncommonCardPrice);
        SpawnShopCards(heroCards, CardRarity.RARE, rareCardsToSpawn, rareCardPrice);

        List<RelicData> availableRelics = new List<RelicData>();
        foreach (var relic in GameDataManager.Instance.AllRelicPool)
        {
            if (!GameDataManager.Instance.RelicDatas.Contains(relic))
            {
                availableRelics.Add(relic);
            }
        }
        List<RelicData> chosenRelics = new();
        int countToPick = Mathf.Min(relicsToSpawn, availableRelics.Count);

        for (int i = 0; i < countToPick; i++)
        {
            int randomIndex = Random.Range(0, availableRelics.Count);
            chosenRelics.Add(availableRelics[randomIndex]);
            availableRelics.RemoveAt(randomIndex);
        }

        for (int i = 0; i < relicsToSpawn; i++)
        {
            RelicData Relic = chosenRelics[i];
            SpawnShopItem(Relic.Image,Relic.name, Relic.Description, Random.Range(baseRelicPrice.x, baseRelicPrice.y), () => BuyRelic(Relic));
        }

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

    private void SpawnShopCards(List<CardData> heroCards, CardRarity rarity, int count, Vector2Int priceRange)
    {
        List<CardData> candidates = heroCards.FindAll(card => card.RarityVisual == rarity);
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            CardData cardData = candidates[index];
            candidates.RemoveAt(index);
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

    private void BuyCard(CardData card)
    {
        Card newCard = new(card);
        GameDataManager.Instance.CurrentDeck.Add(newCard);
    }

    private void BuyRelic(RelicData relic)
    {
        GameDataManager.Instance.AddRelic(relic);
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