using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroView : CombatantView 
{
    [SerializeField] private TMP_Text tormentText;
    [SerializeField] private TMP_Text gaugeText;
    public List<CardData> deck;
    public float savedHP;
    public int savedMaxHP;
    public int savedTorment;
    public int savedMaxTorment;
    public int savedGauge;
    public void Setup(HeroData heroData)
    {
        deck = heroData.Deck;
        if (GameDataManager.Instance != null )
        {
            savedHP = GameDataManager.Instance.CurrentHeroHP;
            savedMaxHP = GameDataManager.Instance.MaxHeroHP;
            savedTorment = GameDataManager.Instance.torment;
            savedMaxTorment = GameDataManager.Instance.heroData.MaxTorment;
            savedGauge = GameDataManager.Instance.gauge;
        } else
        {
            savedHP = heroData.Health;
            savedMaxHP = heroData.Health;
        }
        HeroSystem.Instance.HeroView.spriteRenderer.transform.localScale = new(heroData.scaleX, heroData.scaleY);
        SetupBase(savedMaxHP, savedHP, heroData.Image, heroData.Animator, heroData.FlipSprite);
        if (savedTorment >= 50)
        {
            HeroSystem.Instance.HeroView.AddStatusEffect(StatusEffectType.TORMENT, 1, HeroSystem.Instance.HeroView);
        }
        UpdateTormentText(0);
        UpdateGaugeText(0);
    }

    public void UpdateTormentText(int torment)
    {
        int newTorment = savedTorment + torment;
        if (newTorment <= savedMaxTorment)
        {
            tormentText.text = "Torment: " + newTorment + "/" + savedMaxTorment;
            savedTorment = newTorment;
        }
        else
        {
            tormentText.text = "Torment: " + savedMaxTorment + "/" + savedMaxTorment;
            savedTorment = savedMaxTorment + 1;
        }
    }

    public void UpdateGaugeText(int gauge)
    {
        int newGauge = savedGauge + gauge;
        if (newGauge <= 25)
        {
            gaugeText.text = "Gauge: " + newGauge + "/" + 25;
            savedGauge = newGauge;
        }
        else
        {
            gaugeText.text = "Gauge: " + 25 + "/" + 25;
            savedGauge = 26;
        }
    }

}
