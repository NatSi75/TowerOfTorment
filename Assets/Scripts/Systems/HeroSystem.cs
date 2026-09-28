using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field: SerializeField] public HeroView HeroView { get; private set;}
    public List<EnemyView> Enemies => EnemySystem.Instance.Enemies;
    void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    public void Setup(HeroData heroData)
    {
        HeroView.Setup(heroData);
        HeroSystem.Instance.HeroView.gameObject.SetActive(true);
    }

    // Reactions
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
        HeroView.UpdateTormentText(1);
        if (HeroView.savedTorment >= HeroView.savedMaxTorment)
        {
            List<CombatantView> targetsDebuffTorment = new() { HeroSystem.Instance.HeroView };
            AddStatusEffectGA debuffTormentGA = new AddStatusEffectGA(StatusEffectType.TORMENT, 1, targetsDebuffTorment);
            ActionSystem.Instance.AddReaction(debuffTormentGA);
        }
        else
        {
            HeroView.RemoveStatusEffect(StatusEffectType.TORMENT, 1);
        }

        float regenStacks = HeroView.GetStatusEffectStacks(StatusEffectType.REGEN);
        if (regenStacks > 0)
        {
            ApplyRegenGA applyRegenGA = new(HeroView);
            ActionSystem.Instance.AddReaction(applyRegenGA);
            List<CombatantView> targetsHeal = new() { HeroView };
            HealGA healGA = new(Mathf.RoundToInt(regenStacks));
            HealEffect healEffect = new();
            healEffect.healAmount = Mathf.RoundToInt(regenStacks);
            GameAction effectAction = healEffect.GetGameAction(targetsHeal, HeroView);
            ActionSystem.Instance.AddReaction(effectAction);
        }
        float ritualStacks = HeroView.GetStatusEffectStacks(StatusEffectType.RITUAL);
        if (ritualStacks > 0)
        {
            List<CombatantView> targetsBuffStrength = new() { HeroView };
            AddStatusEffectGA buffStrengthGA = new AddStatusEffectGA(StatusEffectType.STRENGTH, ritualStacks, targetsBuffStrength);
            ActionSystem.Instance.AddReaction(buffStrengthGA);
        }
        float vulnerableStacks = HeroView.GetStatusEffectStacks(StatusEffectType.VULNERABLE);
        if (vulnerableStacks > 0)
        {
            ApplyVulnerableGA applyVulnerableGA = new(HeroView);
            ActionSystem.Instance.AddReaction(applyVulnerableGA);
        }
        float weakStacks = HeroView.GetStatusEffectStacks(StatusEffectType.WEAK);
        if (weakStacks > 0)
        {
            ApplyWeakGA applyWeakGA = new(HeroView);
            ActionSystem.Instance.AddReaction(applyWeakGA);
        }
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        float burnStacks = HeroView.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStacks > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStacks, HeroView);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }
        float poisonStacks = HeroView.GetStatusEffectStacks(StatusEffectType.POISON);
        if (poisonStacks > 0)
        {
            ApplyPoisonGA applyPoisonGA = new(poisonStacks, HeroView);
            ActionSystem.Instance.AddReaction(applyPoisonGA);
        }
        float chilledStacks = HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.CHILLED);
        if (chilledStacks > 0)
        {
            ApplyChilledGA applyChilledGA = new(HeroView);
            ActionSystem.Instance.AddReaction(applyChilledGA);
        }
        HeroView.ResetArmorHero();
    }
}
