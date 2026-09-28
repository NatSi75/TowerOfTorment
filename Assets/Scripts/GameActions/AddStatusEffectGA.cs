using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectGA : GameAction
{
    public StatusEffectType StatusEffectType { get; private set; }
    public float StackCount { get; private set; }
    public List<CombatantView> Targets { get; private set; }
    public AddStatusEffectGA(StatusEffectType statusEffectType, float stackCount, List<CombatantView> targets)
    {
        StatusEffectType = statusEffectType;
        StackCount = stackCount;
        Targets = targets;
    }
}
