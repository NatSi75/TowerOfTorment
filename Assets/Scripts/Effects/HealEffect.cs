using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealEffect : Effect
{
    [SerializeField] public int healAmount;
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        HealGA healGA = new(healAmount);
        foreach (var target in targets)
        {
            target.CurrentHealth += healAmount;

            if (target.CurrentHealth > target.MaxHealth)
            {
                target.CurrentHealth = target.MaxHealth;
            }
            target.UpdateHealthText();
        }
        return healGA;
    }
}
