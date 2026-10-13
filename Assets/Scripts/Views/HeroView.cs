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
        UpdateTormentText(0);
        UpdateGaugeText(0);
        // entering a battle with full torment: tormented right away
        RefreshTormentStatus(false);
    }

    public bool IsTormentFull => savedMaxTorment > 0 && savedTorment >= savedMaxTorment;

    // Adds torment points and applies / removes the Torment status immediately.
    public void AddTorment(int amount)
    {
        UpdateTormentText(amount);
        RefreshTormentStatus(true);
    }

    // Torment is on/off: exactly one stack while torment is full (each hero has its own maximum), none otherwise.
    public void RefreshTormentStatus(bool playSound)
    {
        float stacks = GetStatusEffectStacks(StatusEffectType.TORMENT);
        if (IsTormentFull)
        {
            if (stacks == 0)
            {
                AddStatusEffect(StatusEffectType.TORMENT, 1, this);
                if (playSound) AudioManager.PlaySfx(Sfx.TormentFull);
                SetEnemyIntentsHidden(true);
            }
            else if (stacks > 1)
            {
                RemoveStatusEffect(StatusEffectType.TORMENT, stacks - 1);
            }
        }
        else if (stacks > 0)
        {
            RemoveStatusEffect(StatusEffectType.TORMENT, stacks);
            SetEnemyIntentsHidden(false);
        }
    }

    private static void SetEnemyIntentsHidden(bool hidden)
    {
        if (EnemySystem.Instance == null || EnemySystem.Instance.Enemies == null) return;
        foreach (EnemyView enemy in EnemySystem.Instance.Enemies)
        {
            if (enemy != null) enemy.SetIntentHidden(hidden);
        }
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
