using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectEffect : Effect
{
    [field: SerializeField] public StatusEffectType statusEffectType { get; set; }
    [SerializeField] public int stackCount;
    private int stackCountArmor;
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (statusEffectType == StatusEffectType.ARMOR && HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.DEXTERITY) > 0)
        {
            stackCountArmor = stackCount;
            stackCountArmor += Mathf.RoundToInt(HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.DEXTERITY));
            return new AddStatusEffectGA(statusEffectType, stackCountArmor, targets);
        }
        return new AddStatusEffectGA(statusEffectType, stackCount, targets);
    }
}
