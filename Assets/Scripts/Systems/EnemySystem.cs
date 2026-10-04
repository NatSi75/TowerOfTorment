using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private GameObject victoryWindow;
    [SerializeField] private EnemyBoardView enemyBoardView;
    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<NewIntentionEnemyGA>(EnemyTurnIntentionPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
        ActionSystem.AttachPerformer<EnemyCastGA>(EnemyCastPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<NewIntentionEnemyGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.DetachPerformer<EnemyCastGA>();
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        if (GameDataManager.Instance != null)
        {
            if (GameDataManager.Instance.currentAct == 2 && GameDataManager.Instance.indexEnemy == 2)
            {
                enemyBoardView.AddEnemy(enemyDatas[0]);
            } else
            {
                foreach (var enemyData in enemyDatas)
                {
                    enemyBoardView.AddEnemy(enemyData);
                }
            }
        }
    }

    // Performers
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            enemy.ResetArmorEnemy(enemy.GetStatusEffectStacks(StatusEffectType.ARMOR));
            float bleedStacks = enemy.GetStatusEffectStacks(StatusEffectType.BLEED);
            if (bleedStacks > 0)
            {
                ApplyBleedGA applyBleedGA = new(enemy);
                ActionSystem.Instance.AddReaction(applyBleedGA);
            }

            if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.CURSE) > 0)
            {
                enemy.AddStatusEffect(StatusEffectType.POISON, HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.CURSE), enemy);
            }

            float poisonStacks = enemy.GetStatusEffectStacks(StatusEffectType.POISON);
            if (poisonStacks > 0)
            {
                if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.ACCELERANT) > 0)
                {
                    for (int i = 0; i <= HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.ACCELERANT); i++)
                    {
                        ApplyPoisonGA applyPoisonGA = new(poisonStacks, enemy);
                        ActionSystem.Instance.AddReaction(applyPoisonGA);
                    }
                } else
                {
                    ApplyPoisonGA applyPoisonGA = new(poisonStacks, enemy);
                    ActionSystem.Instance.AddReaction(applyPoisonGA);
                }
            }

            float vulnerableStacks = enemy.GetStatusEffectStacks(StatusEffectType.VULNERABLE);
            if (vulnerableStacks > 0)
            {
                ApplyVulnerableGA applyVulnerableAG = new(enemy);
                ActionSystem.Instance.AddReaction(applyVulnerableAG);
            }
            if (GameDataManager.Instance.HasPerk("Cursed Voodoo Doll"))
            {
                CombatantView attacker = enemy;
                DealDamageGA damageGA = new(3, new() { enemy }, attacker);
                ActionSystem.Instance.AddReaction(damageGA);
            }

            if (IsCastAction(enemy.ActionType))
            {
                EnemyCastGA enemyCastGA = new(enemy);
                ActionSystem.Instance.AddReaction(enemyCastGA);
            }

            switch (enemy.ActionType)
            {
                case EnemyActionType.Attack:
                    AttackHeroGA attackHeroGA = new(enemy); 
                    ActionSystem.Instance.AddReaction(attackHeroGA); 
                    break;
                case EnemyActionType.Block:
                    List<CombatantView> targets = new() { enemy };
                    AddStatusEffectGA addBlockGA = new (StatusEffectType.ARMOR, enemy.ActionValue, targets);
                    ActionSystem.Instance.AddReaction(addBlockGA);
                    break;
                 case EnemyActionType.DebuffVulnerable:
                    List<CombatantView> targetsDebuffVulnerable = new() { HeroSystem.Instance.HeroView };
                    AddStatusEffectGA debuffVulnerableGA = new AddStatusEffectGA(StatusEffectType.VULNERABLE, enemy.ActionValue, targetsDebuffVulnerable);
                    ActionSystem.Instance.AddReaction(debuffVulnerableGA); 
                    break;
                case EnemyActionType.DebuffWeak:
                    List<CombatantView> targetsDebuffWeak = new() { HeroSystem.Instance.HeroView };
                    AddStatusEffectGA debuffWeakGA = new AddStatusEffectGA(StatusEffectType.WEAK, enemy.ActionValue, targetsDebuffWeak); 
                    ActionSystem.Instance.AddReaction(debuffWeakGA);
                    break;
                case EnemyActionType.DebuffChilled:
                    List<CombatantView> targetsDebuffChilled = new() { HeroSystem.Instance.HeroView };
                    AddStatusEffectGA debuffChilledGA = new AddStatusEffectGA(StatusEffectType.CHILLED, enemy.ActionValue, targetsDebuffChilled);
                    ActionSystem.Instance.AddReaction(debuffChilledGA);
                    break;
                case EnemyActionType.DebuffBurn:
                    List<CombatantView> targetsDebuffBurn = new() { HeroSystem.Instance.HeroView };
                    AddStatusEffectGA debuffBurnGA = new AddStatusEffectGA(StatusEffectType.BURN, enemy.ActionValue, targetsDebuffBurn);
                    ActionSystem.Instance.AddReaction(debuffBurnGA);
                    break;
                case EnemyActionType.Heal:
                    List<CombatantView> targetsHeal = new() { enemy };
                    HealGA healGA = new(Mathf.RoundToInt(enemy.ActionValue));
                    HealEffect healEffect = new();
                    healEffect.healAmount = Mathf.RoundToInt(enemy.ActionValue);
                    GameAction effectAction = healEffect.GetGameAction(targetsHeal, enemy);
                    ActionSystem.Instance.AddReaction(effectAction);
                    break;
                case EnemyActionType.BuffStrength:
                    List<CombatantView> targetsBuffStrength = new() { enemy };
                    AddStatusEffectGA buffStrengthGA = new AddStatusEffectGA(StatusEffectType.STRENGTH, enemy.ActionValue, targetsBuffStrength);
                    ActionSystem.Instance.AddReaction(buffStrengthGA);
                    break;
                case EnemyActionType.DebuffVoid:
                    List<CombatantView> targetsDebuffVoid = new() { HeroSystem.Instance.HeroView };
                    AddStatusEffectGA debuffVoidGA = new AddStatusEffectGA(StatusEffectType.VOID, enemy.ActionValue, targetsDebuffVoid);
                    ActionSystem.Instance.AddReaction(debuffVoidGA);
                    break;
                case EnemyActionType.DebuffPoison:
                    List<CombatantView> targetsDebuffPoison = new() { HeroSystem.Instance.HeroView };
                    AddStatusEffectGA debuffPoisonGA = new AddStatusEffectGA(StatusEffectType.POISON, enemy.ActionValue, targetsDebuffPoison);
                    ActionSystem.Instance.AddReaction(debuffPoisonGA);
                    break;
                default:
                    break;
            }
        }
        NewIntentionEnemyGA newIntentionEnemyGA = new();
        ActionSystem.Instance.AddReaction(newIntentionEnemyGA);
        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        if (attacker != null)
        {
            float attackLength = attacker.PlayAnimation("Attack");
            Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
            yield return tween.WaitForCompletion();
            // the hit lands around the middle of the attack animation
            if (attackLength > 0f) yield return new WaitForSeconds(Mathf.Max(0f, attackLength * 0.5f - 0.15f));
            attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
            DealDamageGA dealDamageGA = new(Mathf.RoundToInt(attacker.ActionValue), new() { HeroSystem.Instance.HeroView }, attackHeroGA.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
    }

    private IEnumerator EnemyCastPerformer(EnemyCastGA enemyCastGA)
    {
        EnemyView caster = enemyCastGA.Caster;
        if (caster == null) yield break;
        float castLength = caster.PlayAnimation("Attack");
        // the debuff / buff is applied around the middle of the animation
        if (castLength > 0f) yield return new WaitForSeconds(castLength * 0.5f);
        if (caster.ActionType == EnemyActionType.DebuffBurn) AudioManager.PlaySfx(Sfx.Burn);
    }

    private static bool IsCastAction(EnemyActionType actionType)
    {
        return actionType is EnemyActionType.Debuff
            or EnemyActionType.DebuffWeak
            or EnemyActionType.DebuffVulnerable
            or EnemyActionType.DebuffChilled
            or EnemyActionType.DebuffBurn
            or EnemyActionType.DebuffVoid
            or EnemyActionType.DebuffPoison
            or EnemyActionType.BuffStrength;
    }

    private IEnumerator EnemyTurnIntentionPerformer(NewIntentionEnemyGA newIntentionEnemy)
    {
        float chilledStacks = HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.CHILLED);
        if (chilledStacks > 0)
        {
            if (GameDataManager.Instance.HasPerk("Spell Book +"))
            {
                DrawCardsGA drawCardsGA = new(5);
                ActionSystem.Instance.AddReaction(drawCardsGA);
            } else
            {
                DrawCardsGA drawCardsGA = new(4);
                ActionSystem.Instance.AddReaction(drawCardsGA);
            }
        }
        else
        {
            if (GameDataManager.Instance.HasPerk("Spell Book +"))
            {
                DrawCardsGA drawCardsGA = new(6);
                ActionSystem.Instance.AddReaction(drawCardsGA);
            }
            else
            {
                DrawCardsGA drawCardsGA = new(5);
                ActionSystem.Instance.AddReaction(drawCardsGA);
            }
        }
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            EnemyAbilityData newEnemyAbility = enemy.EnemyData.GetAbility(enemy.UsedAbilityCount());
            enemy.intentionType = newEnemyAbility.Intention.IntentionSprite;
            float weakStacks = enemy.GetStatusEffectStacks(StatusEffectType.WEAK);
            if (weakStacks > 0)
            {
                ApplyWeakGA applyWeakGA = new(enemy);
                ActionSystem.Instance.AddReaction(applyWeakGA);
            }
            enemy.UpdateIntention(newEnemyAbility);
        }
        yield return null;
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        // an enemy hit again in the same action can be "killed" twice; handle its death only once
        if (killEnemyGA.EnemyView == null || !Enemies.Contains(killEnemyGA.EnemyView)) yield break;
        EnemyData killedEnemy = killEnemyGA.EnemyView.EnemyData;
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
        if (GameDataManager.Instance != null)
        {
            // Act 2 boss: only the death of phase 1 brings in phase 2 (otherwise phase 2 respawns when it dies)
            if (GameDataManager.Instance.currentAct == 2 && GameDataManager.Instance.indexEnemy == 2
                && GameDataManager.Instance.NextEncounter.Count > 1
                && killedEnemy == GameDataManager.Instance.NextEncounter[0])
            {
                enemyBoardView.AddEnemy(GameDataManager.Instance.NextEncounter[1]);
                enemyBoardView.EnemyViews[0].AddStatusEffect(StatusEffectType.STRENGTH, enemyBoardView.GetStacksStrength(), enemyBoardView.EnemyViews[0]);
            }
        }
        if (Enemies == null || Enemies.Count == 0)
        {
            AudioManager.PlaySfx(Sfx.Victory);
            if (GameDataManager.Instance.currentAct == 2 && GameDataManager.Instance.indexEnemy == 2)
            {
                victoryWindow.SetActive(true);
                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("Main Menu");
                Destroy(GameDataManager.Instance.gameObject);
            }
            ActionSystem.Instance.ResetReaction();
            GameDataManager.Instance.CurrentHeroHP = HeroSystem.Instance.HeroView.CurrentHealth;
            GameDataManager.Instance.torment = HeroSystem.Instance.HeroView.savedTorment;
            GameDataManager.Instance.gauge = HeroSystem.Instance.HeroView.savedGauge;
            if (GameDataManager.Instance != null)
            {
                if (GameDataManager.Instance.currentAct == 1 && GameDataManager.Instance.indexEnemy == 2)
                {
                    GameDataManager.Instance.CurrentHeroHP = GameDataManager.Instance.MaxHeroHP;
                }
            }

            if (GameDataManager.Instance.HasPerk("Meat on the Bone"))
            {
                float hpPercentage = (float)GameDataManager.Instance.CurrentHeroHP / GameDataManager.Instance.MaxHeroHP;

                if (hpPercentage <= 0.5f)
                {
                    GameDataManager.Instance.CurrentHeroHP += 12;
                }
            }
            

            yield return new WaitForSeconds(2f);
            if (CardRewardUI.Instance != null)
            {
                CardRewardUI.Instance.ShowBattleReward();
            }
            else
            {
                SceneManager.LoadScene("Map");
            }
        }
    }
}
