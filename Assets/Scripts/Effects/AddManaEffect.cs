using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddManaEffect : Effect
{
    [SerializeField] private int manaAmount;
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        AddManaGA manaGA = new(manaAmount);
        ManaSystem.Instance.GainMana(manaAmount);
        return manaGA;
    }
}
