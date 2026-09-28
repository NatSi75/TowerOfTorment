using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    [SerializeField] private Sprite armorSprite, burnSprite, strengthSprite, dexteritySprite, weakSprite, vulnerableSprite, poisonSprite, 
                                    bleedSprite, chilledSprite, voidSprite, tormentSprite, regenSprite, ritualSprite, 
                                    curseSprite, hailstormSprite, mirrorimageSprite, accelerantSprite;
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();
    public void UpdateStatusEffectUI(StatusEffectType statusEffectType, int stackCount, CombatantView target)
    {
        if (stackCount == 0)
        {
            if(statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                statusEffectUIs.Remove(statusEffectType);
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            if (target != null)
            {
                if (!statusEffectUIs.ContainsKey(statusEffectType))
                {
                    StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                    statusEffectUIs.Add(statusEffectType, statusEffectUI);
                }
                Sprite sprite = GetSpriteByType(statusEffectType);
                statusEffectUIs[statusEffectType].Set(sprite, stackCount, statusEffectType);
            }
            
        }
    }

    public void UpdateStatusEffectUINoTarget(StatusEffectType statusEffectType, int stackCount)
    {
        if (stackCount == 0)
        {
            if (statusEffectUIs.ContainsKey(statusEffectType) && statusEffectUIs != null)
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                statusEffectUIs.Remove(statusEffectType);
                if (statusEffectUI != null)
                {
                    Destroy(statusEffectUI.gameObject);
                }
            }
        }
        else
        {
                if (!statusEffectUIs.ContainsKey(statusEffectType))
                {
                    StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                    statusEffectUIs.Add(statusEffectType, statusEffectUI);
                }
                Sprite sprite = GetSpriteByType(statusEffectType);
                statusEffectUIs[statusEffectType].Set(sprite, stackCount, statusEffectType);
        }
    }

    private Sprite GetSpriteByType(StatusEffectType statusEffectType)
    {
        return statusEffectType switch
        {
            StatusEffectType.ARMOR => armorSprite,
            StatusEffectType.BURN => burnSprite,
            StatusEffectType.STRENGTH => strengthSprite,
            StatusEffectType.DEXTERITY => dexteritySprite,
            StatusEffectType.WEAK => weakSprite,
            StatusEffectType.VULNERABLE => vulnerableSprite,
            StatusEffectType.POISON => poisonSprite,
            StatusEffectType.BLEED => bleedSprite,
            StatusEffectType.CHILLED => chilledSprite,
            StatusEffectType.VOID => voidSprite,
            StatusEffectType.TORMENT => tormentSprite,
            StatusEffectType.REGEN => regenSprite,
            StatusEffectType.RITUAL => ritualSprite,
            StatusEffectType.CURSE => curseSprite,
            StatusEffectType.HAILSTORM => hailstormSprite,
            StatusEffectType.MIRRORIMAGE => mirrorimageSprite,
            StatusEffectType.ACCELERANT => accelerantSprite,
            _ => null,
        };
    }
}
