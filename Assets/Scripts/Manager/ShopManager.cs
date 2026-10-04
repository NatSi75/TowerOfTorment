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

    [Header("Pricing")]
    public Vector2Int baseCardPrice = new (60, 75);
    public Vector2Int baseCardPriceElite = new(80, 95);
    public Vector2Int baseCardPriceBoss = new(100, 115);
    public Vector2Int basePerkPrice = new(275, 300);
    public Vector2Int baseScrollPrice = new(135, 160);

    [Header("Shop Generation Limits")]
    public int cardsToSpawn = 6;
    public int perksToSpawn = 3;
    public int scrollsToSpawn = 3;

    CardData randomCardData;

    private void Start()
    {
        GenerateShop();
    }

    private void GenerateShop()
    {
        // 1. Generate Cards
        for (int i = 0; i < cardsToSpawn; i++)
        {
            if (i < 2) // 2 Common Card
            {
                if (GameDataManager.Instance.indexHero == 0) // Knight
                {
                    randomCardData = GameDataManager.Instance.MinorEnemyCardRewardPoolKnight[Random.Range(0, GameDataManager.Instance.MinorEnemyCardRewardPoolKnight.Count)];
                }
                else // Wizard
                {
                    randomCardData = GameDataManager.Instance.MinorEnemyCardRewardPoolWizard[Random.Range(0, GameDataManager.Instance.MinorEnemyCardRewardPoolWizard.Count)];
                }
                
                Card newCardModel = new(randomCardData);
                SpawnShopCard(newCardModel, Random.Range(baseCardPrice.x, baseCardPrice.y));
            } else if (i < 4) // 2 Uncommon Card
            {
                if (GameDataManager.Instance.indexHero == 0) // Knight
                {
                    randomCardData = GameDataManager.Instance.EliteEnemyCardRewardPoolKnight[Random.Range(0, GameDataManager.Instance.EliteEnemyCardRewardPoolKnight.Count)];
                }
                else // Wizard
                {
                    randomCardData = GameDataManager.Instance.EliteEnemyCardRewardPoolWizard[Random.Range(0, GameDataManager.Instance.EliteEnemyCardRewardPoolWizard.Count)];
                }
                Card newCardModel = new(randomCardData);
                SpawnShopCard(newCardModel, Random.Range(baseCardPriceElite.x, baseCardPriceElite.y));
            } else // 1 Card Rare
            {
                if (GameDataManager.Instance.indexHero == 0) // Knight
                {
                    randomCardData = GameDataManager.Instance.BossEnemyCardRewardPoolKnight[Random.Range(0, GameDataManager.Instance.BossEnemyCardRewardPoolKnight.Count)];
                }
                else // Wizard
                {
                    randomCardData = GameDataManager.Instance.BossEnemyCardRewardPoolWizard[Random.Range(0, GameDataManager.Instance.BossEnemyCardRewardPoolWizard.Count)];
                }
                Card newCardModel = new(randomCardData);
                SpawnShopCard(newCardModel, Random.Range(baseCardPriceBoss.x, baseCardPriceBoss.y));
            }
        }

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