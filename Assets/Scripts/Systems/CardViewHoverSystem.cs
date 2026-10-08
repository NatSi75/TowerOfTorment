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
        baseScale ??= cardViewHover.transform.localScale;
        float scale = MobileLayout.HoverScale;
        cardViewHover.transform.localScale = baseScale.Value * scale;
        cardViewHover.transform.position = position;
        if (scale > 1f) KeepOnScreen();
    }

    private Vector3? baseScale;

    // Moves the (enlarged) preview card so it is fully inside the camera view.
    private void KeepOnScreen()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        Bounds? cardBounds = null;
        foreach (SpriteRenderer sr in cardViewHover.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.sprite == null || !sr.enabled) continue;
            if (cardBounds == null) cardBounds = sr.bounds;
            else { Bounds b = cardBounds.Value; b.Encapsulate(sr.bounds); cardBounds = b; }
        }
        if (cardBounds == null) return;

        const float margin = 0.3f;
        Bounds bounds = cardBounds.Value;
        Vector3 camPos = cam.transform.position;
        float halfHeight = cam.orthographicSize - margin;
        float halfWidth = cam.orthographicSize * cam.aspect - margin;
        Vector3 shift = Vector3.zero;
        shift.x = Shift(bounds.min.x, bounds.max.x, camPos.x - halfWidth, camPos.x + halfWidth);
        shift.y = Shift(bounds.min.y, bounds.max.y, camPos.y - halfHeight, camPos.y + halfHeight);
        cardViewHover.transform.position += shift;
    }

    // How far [min, max] has to move to lie inside [viewMin, viewMax]; if it does not fit, its top edge is kept visible.
    private static float Shift(float min, float max, float viewMin, float viewMax)
    {
        if (max > viewMax || max - min > viewMax - viewMin) return viewMax - max;
        if (min < viewMin) return viewMin - min;
        return 0f;
    }

    public void Hide()
    { 
        cardViewHover.gameObject.SetActive(false);
    }

}
