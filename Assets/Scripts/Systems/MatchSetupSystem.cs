using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class MatchSetupSystem : MonoBehaviour
{
    private void Start()
    {
        // Heal HP
        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Blood Bowl"))
        {
            GameDataManager.Instance.CurrentHeroHP += 2;
            if (GameDataManager.Instance.CurrentHeroHP > GameDataManager.Instance.MaxHeroHP)
            {
                GameDataManager.Instance.CurrentHeroHP = GameDataManager.Instance.MaxHeroHP;
            }
        }
        
        if (GameDataManager.Instance.indexEnemy == 2)
        {
            if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Pulsing Behemoth Heart"))
                {
                GameDataManager.Instance.CurrentHeroHP += 25;
                if (GameDataManager.Instance.CurrentHeroHP > GameDataManager.Instance.MaxHeroHP)
                {
                    GameDataManager.Instance.CurrentHeroHP = GameDataManager.Instance.MaxHeroHP;
                }
            }
        }

        // Setup Hero
        HeroSystem.Instance.Setup(GameDataManager.Instance.heroData);
 
        // Setup Enemies
        if (GameDataManager.Instance != null && GameDataManager.Instance.NextEncounter.Count > 0)
        {
            EnemySystem.Instance.Setup(GameDataManager.Instance.NextEncounter);
        }
        
        CardSystem.Instance.Setup(GameDataManager.Instance.CurrentDeck);
        if (GameDataManager.Instance.PerkDatas != null)
        {
            foreach (PerkData perkData in GameDataManager.Instance.PerkDatas)
            {
                PerkSystem.Instance.AddPerk(new Perk(perkData));
            }
        }

        if (GameDataManager.Instance.ScrollDatas != null)
        {
            foreach (ScrollData scrollData in GameDataManager.Instance.ScrollDatas)
            {
                ScrollSystem.Instance.AddScroll(new Scroll(scrollData));
            }
        }


        if (GameDataManager.Instance != null)
        {
            if (GameDataManager.Instance.HasPerk("Cursed Tome") && GameDataManager.Instance.HasPerk("Spell Book"))
            {
                DrawCardsGA drawCardsGA = new(9);
                ActionSystem.Instance.Perform(drawCardsGA);
            } else if (GameDataManager.Instance.HasPerk("Cursed Tome") || GameDataManager.Instance.HasPerk("Spell Book"))
            {
                if (GameDataManager.Instance.HasPerk("Spell Book +"))
                {
                    DrawCardsGA drawCardsGA = new(8);
                    ActionSystem.Instance.Perform(drawCardsGA);
                } else
                {
                    DrawCardsGA drawCardsGA = new(7);
                    ActionSystem.Instance.Perform(drawCardsGA);
                }
            }
            else
            {
                DrawCardsGA drawCardsGA = new(5);
                ActionSystem.Instance.Perform(drawCardsGA);
            }
        } 

        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Fossilized Armor"))
        {
            HeroSystem.Instance.HeroView.AddStatusEffect(StatusEffectType.ARMOR, 10, HeroSystem.Instance.HeroView);
        }

        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Sapphire Ring"))
        {
            ManaSystem.Instance.GainMana(1);
        }

        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Dagger"))
        {
            List<EnemyView> enemies = EnemySystem.Instance.Enemies;
            foreach (EnemyView enemy in enemies)
            {
                enemy.AddStatusEffect(StatusEffectType.BLEED, 2, enemy);
            }
        }

        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Dagger +"))
        {
            List<EnemyView> enemies = EnemySystem.Instance.Enemies;
            foreach (EnemyView enemy in enemies)
            {
                enemy.AddStatusEffect(StatusEffectType.BLEED, 4, enemy);
            }
        }

        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Iron Dagger"))
        {
            HeroSystem.Instance.HeroView.AddStatusEffect(StatusEffectType.STRENGTH, 1, HeroSystem.Instance.HeroView);
        }

        if (GameDataManager.Instance != null && GameDataManager.Instance.HasPerk("Swift Boots"))
        {
            HeroSystem.Instance.HeroView.AddStatusEffect(StatusEffectType.DEXTERITY, 1, HeroSystem.Instance.HeroView);
        }
    }
}
