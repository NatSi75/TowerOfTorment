using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    public void Show(Card card, Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);

        bool isHeroStrength = false;
        float stackStrength = 0;
        bool isHeroWeak = false;
        bool isAnyEnemyVulnerable = false;
        float textDamage = 0;

        if (EnemySystem.Instance != null && EnemySystem.Instance.Enemies != null)
        {
            foreach (var enemy in EnemySystem.Instance.Enemies)
            {
                if (enemy != null)
                {
                    if (enemy.GetStatusEffectStacks(StatusEffectType.VULNERABLE) > 0) isAnyEnemyVulnerable = true;
                }
            }
        }

        if (HeroSystem.Instance.HeroView != null)
        {
            if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.STRENGTH) > 0) 
            {
                stackStrength = HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.STRENGTH);
                isHeroStrength = true;
            }

            
            if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.WEAK) > 0) isHeroWeak = true;
        }

        if (card.data.ManualTargetEffect != null && card.ManualTargetEffect.Count > 0)
        {
            foreach (var effect in card.data.ManualTargetEffect)
            {
                if (effect is DealDamageEffect dealDamageEffect && isHeroStrength)
                {
                    textDamage = dealDamageEffect.damageAmount;
                    textDamage += stackStrength;
                    string newText = Regex.Replace(card.description, @"\d+(?=\s*damage)", Mathf.RoundToInt(textDamage).ToString());
                    card.description = newText;
                }
                if (effect is DealDamageEffect dealDamageEffect2 && isHeroWeak)
                {
                    if (isHeroStrength)
                    {
                        textDamage *= 0.75f;
                    }
                    else
                    {
                        textDamage = dealDamageEffect2.damageAmount;
                        textDamage *= 0.75f;
                    }
                    string newText = Regex.Replace(card.description, @"\d+(?=\s*damage)", Mathf.RoundToInt(textDamage).ToString());
                    card.description = newText;
                }
                if (effect is DealDamageEffect dealDamageEffect3 && isAnyEnemyVulnerable)
                {
                    if (isHeroWeak || isHeroStrength)
                    {
                        textDamage *= 1.5f;
                    } else
                    {
                        textDamage = dealDamageEffect3.damageAmount;
                        textDamage *= 1.5f;
                    }
                    string newText = Regex.Replace(card.description, @"\d+(?=\s*damage)", Mathf.RoundToInt(textDamage).ToString());
                    card.description = newText;
                    break;
                } 
               
            }
        }
        if (card.data.OtherEffects != null && card.data.OtherEffects.Count > 0 && card.data.OtherEffects[0].Effect is DealDamageEffect dealDamageOtherEffect && isHeroStrength)
        {

            textDamage = dealDamageOtherEffect.damageAmount;
            textDamage += stackStrength;
            string newText = Regex.Replace(card.Description, @"\d+(?=\s*damage)", Mathf.RoundToInt(textDamage).ToString());
            card.description = newText;
        }
        if (card.data.OtherEffects != null && card.data.OtherEffects.Count > 0 && card.data.OtherEffects[0].Effect is DealDamageEffect dealDamageOtherEffect2 && isHeroWeak)
        {
            if (isHeroStrength)
            {
                textDamage *= 0.75f;
            }
            else
            {
                textDamage = dealDamageOtherEffect2.damageAmount;
                textDamage *= 0.75f;
            }
            string newText = Regex.Replace(card.Description, @"\d+(?=\s*damage)", Mathf.RoundToInt(textDamage).ToString());
            card.description = newText;
        }
        if (card.data.OtherEffects != null && card.data.OtherEffects.Count > 0 && card.data.OtherEffects[0].Effect is DealDamageEffect dealDamageOtherEffect3 && isAnyEnemyVulnerable)
        {
            if (isHeroWeak || isHeroStrength)
            {
                textDamage *= 1.5f;
            }
            else
            {
                textDamage = dealDamageOtherEffect3.damageAmount;
                textDamage *= 1.5f;
            }
            string newText = Regex.Replace(card.Description, @"\d+(?=\s*damage)", Mathf.RoundToInt(textDamage).ToString());
            card.description = newText;
        }
        
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position;
    }

    public void Hide()
    { 
        cardViewHover.gameObject.SetActive(false);
    }

}
