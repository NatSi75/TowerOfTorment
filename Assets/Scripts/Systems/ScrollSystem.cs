using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public class ScrollSystem : Singleton<ScrollSystem>
{
    [SerializeField] private ScrollsUI scrollsUI;
    private readonly List<Scroll> scrolls = new();
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PlayScrollGA>(PlayScrollPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<PlayScrollGA>();
    }

    private IEnumerator PlayScrollPerformer(PlayScrollGA playScrollGA)
    {
        ScrollData scrollData = playScrollGA.Scroll.data;
        if (playScrollGA.ManualTarget != null)
        {
            foreach (var effect in scrollData.ManualTargetEffect)
            {
                if (effect is AddStatusEffectEffect statusEffect)
                {
                    StatusEffectType typeStatus = statusEffect.statusEffectType;
                    if (playScrollGA.ManualTarget is EnemyView enemyView && enemyView.ActionType == EnemyActionType.Attack)
                    {
                        if (typeStatus == StatusEffectType.STRENGTH)
                        {
                            enemyView.ActionValue += statusEffect.stackCount;
                            enemyView.UpdateActionValue(enemyView.ActionValue);
                        }
                        if (typeStatus == StatusEffectType.WEAK && enemyView.GetStatusEffectStacks(StatusEffectType.WEAK) == 0)
                        {
                            enemyView.ActionValue *= 0.75f;
                            enemyView.UpdateActionValue(enemyView.ActionValue);
                        }
                    }
                }
            }

        }

        foreach (var effectWrapper in playScrollGA.Scroll.OtherEffects)
        {
            if (effectWrapper.TargetMode is AllEnemiesTM allEnemiesTM && effectWrapper.Effect is AddStatusEffectEffect statusEffectStrength)
            {
                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                StatusEffectType typeStatus = statusEffectStrength.statusEffectType;
                foreach (EnemyView target in targets.Cast<EnemyView>())
                {
                    if (typeStatus == StatusEffectType.STRENGTH && target.ActionType == EnemyActionType.Attack)
                    {
                        target.ActionValue += statusEffectStrength.stackCount;
                        target.UpdateActionValue(target.ActionValue);
                    }
                }
            }
            if (effectWrapper.TargetMode is AllEnemiesTM allEnemiesTM2 && effectWrapper.Effect is AddStatusEffectEffect statusEffectWeak)
            {
                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                StatusEffectType typeStatus = statusEffectWeak.statusEffectType;
                foreach (EnemyView target in targets.Cast<EnemyView>())
                {
                    if (typeStatus == StatusEffectType.WEAK && target.ActionType == EnemyActionType.Attack && target.GetStatusEffectStacks(StatusEffectType.WEAK) == 0)
                    {
                        target.ActionValue *= 0.75f;
                        target.UpdateActionValue(target.ActionValue);
                    }
                }
            }
        }

        foreach (var effectWrapper in playScrollGA.Scroll.ManualTargetEffect)
        {
            PerformEffectGA performEffectGA = new(effectWrapper, new() { playScrollGA.ManualTarget }) { RawDamage = true };
            ActionSystem.Instance.AddReaction(performEffectGA);
        }

        foreach (var effectWrapper in playScrollGA.Scroll.OtherEffects)
        {
            List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
            PerformEffectGA performEffectGA = new(effectWrapper.Effect, targets) { RawDamage = true };
            ActionSystem.Instance.AddReaction(performEffectGA);
        }

        yield return null;
    }

    public void AddScroll(Scroll scroll)
    {
        scrolls.Add(scroll);
        scrollsUI.AddScrollUI(scroll);
        //perk.OnAdd();
    }
    public void RemoveScroll(Scroll scroll)
    {
        scrolls.Remove(scroll);
        scrollsUI.RemoveScrollUI(scroll);
        //perk.OnRemove();
    }
}
