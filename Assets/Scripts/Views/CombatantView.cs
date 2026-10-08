using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] private StatusEffectsUI statusEffectsUI;
    [SerializeField] private Animator animatorController;
    public int MaxHealth {  get; private set; }
    public float CurrentHealth { get; set; }
    public float currentArmor { get; private set; }
    private Dictionary<StatusEffectType, float> statusEffects = new();
    private bool isDying;

    protected void SetupBase(int health, float currentHealth, Sprite image, RuntimeAnimatorController animatorController, bool flipSprite)
    {
        MaxHealth = health;
        CurrentHealth = currentHealth;
        spriteRenderer.sprite = image;
        spriteRenderer.flipX = flipSprite;
        this.animatorController.runtimeAnimatorController = animatorController;
        UpdateHealthText();
    }

    public void UpdateHealthText()
    {
        healthText.text = "HP: " + CurrentHealth + "/" + MaxHealth;
    }

    public bool HasAnimation(string stateName)
    {
        return animatorController != null
            && animatorController.runtimeAnimatorController != null
            && animatorController.HasState(0, Animator.StringToHash(stateName));
    }

    // Triggers the "Attack" / "Hurt" / "Death" animation and returns its length (0 when it has none).
    // Once Death has played nothing else can interrupt it.
    public float PlayAnimation(string stateName)
    {
        if (isDying || !HasAnimation(stateName)) return 0f;
        if (stateName == "Death") isDying = true;
        animatorController.SetTrigger(stateName);
        foreach (AnimationClip clip in animatorController.runtimeAnimatorController.animationClips)
        {
            if (clip.name.EndsWith(" " + stateName)) return clip.length;
        }
        return 0f;
    }

    public void HideCombatInfo()
    {
        healthText.gameObject.SetActive(false);
        statusEffectsUI.gameObject.SetActive(false);
    }

    protected virtual void PlayHurt()
    {
        PlayAnimation("Hurt");
    }

    public void Damage(float damageAmount)
    {
        float remainingDamage = damageAmount;
        currentArmor = GetStatusEffectStacks(StatusEffectType.ARMOR);
        if (currentArmor > 0)
        {
            if (currentArmor >= damageAmount)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, remainingDamage);
                remainingDamage = 0;
            } else if (currentArmor < damageAmount)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
                remainingDamage -= currentArmor;
            }
        }
        float healthBefore = CurrentHealth;
        if (remainingDamage > 0)
        {
            CurrentHealth -= remainingDamage;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
            if (CurrentHealth > 0)
            {
                PlayHurt();
            }
        }

        if (this != null && gameObject != null)
        {
            ShowDamageNumber(healthBefore - CurrentHealth, damageAmount - remainingDamage);
            transform.DOShakePosition(0.2f, 0.5f);
            UpdateHealthText();
        }
    }

    // Red number for the health lost; "Blocked" when the armor took all of it.
    private void ShowDamageNumber(float healthLost, float blocked)
    {
        if (healthLost <= 0f && blocked <= 0f) return;

        // around the upper part of the body, a little to the side so quick multi-hits don't stack
        Bounds body = spriteRenderer.bounds;
        Vector3 position = new(
            body.center.x + Random.Range(-0.5f, 0.5f),
            body.center.y + body.extents.y * 0.7f,
            0f);

        if (healthLost > 0f)
        {
            DamageNumber.Show(position, Mathf.RoundToInt(healthLost).ToString(), DamageNumber.DamageColor, healthText.font);
        }
        else
        {
            DamageNumber.Show(position, "Blocked", DamageNumber.BlockedColor, healthText.font, 0.7f);
        }
    }

    public void ResetArmorHero()
    {
        if (currentArmor >= 0)
        {
            RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
        }
    }

    public void ResetArmorEnemy(float currentArmor)
    {
        if (currentArmor >= 0)
        {
            RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
        }
    }

    public void AddStatusEffect(StatusEffectType type, float stackCount,CombatantView target)
    {
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] += stackCount;
        }
        else
        {
            statusEffects.Add(type, stackCount);
        }
        statusEffectsUI.UpdateStatusEffectUI(type, Mathf.RoundToInt(GetStatusEffectStacks(type)), target);
    }
    public void RemoveStatusEffect(StatusEffectType type, float stackCount)
    {
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            if (statusEffects[type] <= 0)
            {
                statusEffects.Remove(type);
            }
        }
            statusEffectsUI.UpdateStatusEffectUINoTarget(type, Mathf.RoundToInt(GetStatusEffectStacks(type)));
    }
    public float GetStatusEffectStacks(StatusEffectType type)
    {
        if (statusEffects.ContainsKey(type)) return statusEffects[type];
        else return 0;
    }
}
