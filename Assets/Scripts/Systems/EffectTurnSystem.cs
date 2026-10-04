using System.Collections;
using UnityEngine;

public class EffectTurnSystem : MonoBehaviour
{
    [SerializeField] private GameObject vulnerableVFX;
    [SerializeField] private GameObject burnVFX;
    [SerializeField] private GameObject weakVFX;
    [SerializeField] private GameObject poisonVFX;
    [SerializeField] private GameObject bleedVFX;
    [SerializeField] private GameObject chilledVFX;
    [SerializeField] private GameObject voidVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyVulnerableGA>(ApplyVulnerablePerformer);
        ActionSystem.AttachPerformer<ApplyBurnGA>(ApplyBurnPerformer);
        ActionSystem.AttachPerformer<ApplyWeakGA>(ApplyWeakPerformer);
        ActionSystem.AttachPerformer<ApplyPoisonGA>(ApplyPoisonPerformer);
        ActionSystem.AttachPerformer<ApplyBleedGA>(ApplyBleedPerformer);
        ActionSystem.AttachPerformer<ApplyChilledGA>(ApplyChilledPerformer);
        ActionSystem.AttachPerformer<ApplyVoidGA>(ApplyVoidPerformer);
        ActionSystem.AttachPerformer<ApplyRegenGA>(ApplyRegenPerformer);
        ActionSystem.AttachPerformer<ApplyRitualGA>(ApplyRitualPerformer);
        ActionSystem.AttachPerformer<ApplyCurseGA>(ApplyCursePerformer);
        ActionSystem.AttachPerformer<ApplyHailstormGA>(ApplyHailstormPerformer);
        ActionSystem.AttachPerformer<ApplyMirrorImageGA>(ApplyMirrorImagePerformer);
        ActionSystem.AttachPerformer<ApplyAccelerantGA>(ApplyAccelerantPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyVulnerableGA>();
        ActionSystem.DetachPerformer<ApplyBurnGA>();
        ActionSystem.DetachPerformer<ApplyWeakGA>();
        ActionSystem.DetachPerformer<ApplyPoisonGA>();
        ActionSystem.DetachPerformer<ApplyBleedGA>();
        ActionSystem.DetachPerformer<ApplyChilledGA>();
        ActionSystem.DetachPerformer<ApplyVoidGA>();
        ActionSystem.DetachPerformer<ApplyRegenGA>();
        ActionSystem.DetachPerformer<ApplyRitualGA>();
        ActionSystem.DetachPerformer<ApplyCurseGA>();
        ActionSystem.DetachPerformer<ApplyHailstormGA>();
        ActionSystem.DetachPerformer<ApplyMirrorImageGA>();
        ActionSystem.DetachPerformer<ApplyAccelerantGA>();
    }
    private IEnumerator ApplyVulnerablePerformer(ApplyVulnerableGA applyVulnerableGA)
    {
        CombatantView target = applyVulnerableGA.Target;
        if (target != null)
        {
            Instantiate(vulnerableVFX, target.transform.position, Quaternion.identity);
            target.RemoveStatusEffect(StatusEffectType.VULNERABLE, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyBurnPerformer(ApplyBurnGA applyBurnGA)
    {
        CombatantView caster = null;
        CombatantView target = applyBurnGA.Target;
        if (target != null) 
        {
            Instantiate(burnVFX, target.transform.position, Quaternion.identity);
            DealDamageGA damageGA = new(applyBurnGA.BurnDamage, new() { target }, caster);
            //target.Damage(applyBurnGA.BurnDamage);
            target.RemoveStatusEffect(StatusEffectType.BURN, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyWeakPerformer(ApplyWeakGA applyWeakGA)
    {
        CombatantView target = applyWeakGA.Target;
        if (target != null)
        {
            Instantiate(weakVFX, target.transform.position, Quaternion.identity);
            target.RemoveStatusEffect(StatusEffectType.WEAK, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA applyPoisonGA)
    {
        CombatantView target = applyPoisonGA.Target;
        if (target != null)
        {
            Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
            DealDamageGA damageGA = new(applyPoisonGA.PoisonDamage, new() { target }, target);
            ActionSystem.Instance.AddReaction(damageGA);
            //target.Damage(applyPoisonGA.PoisonDamage);
            target.RemoveStatusEffect(StatusEffectType.POISON, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyBleedPerformer(ApplyBleedGA applyBleedGA)
    {
        CombatantView target = applyBleedGA.Target;
        if (target != null)
        {
            Instantiate(bleedVFX, target.transform.position, Quaternion.identity);
            target.RemoveStatusEffect(StatusEffectType.BLEED, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyChilledPerformer(ApplyChilledGA applyChilledGA)
    {
        CombatantView target = applyChilledGA.Target;
        if (target != null)
        {
            Instantiate(chilledVFX, target.transform.position, Quaternion.identity);
            target.RemoveStatusEffect(StatusEffectType.CHILLED, 1);
        }
        // Chilled only counts down (no damage) and ticks after the new hand is drawn,
        // so don't make the player wait for it.
        yield return null;
    }
    private IEnumerator ApplyVoidPerformer(ApplyVoidGA applyVoidGA)
    {
        CombatantView target = applyVoidGA.Target;
        if (target != null)
        {
            Instantiate(voidVFX, target.transform.position, Quaternion.identity);
            target.RemoveStatusEffect(StatusEffectType.VOID, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyRegenPerformer(ApplyRegenGA applyRegenGA)
    {
        CombatantView target = applyRegenGA.Target;
        if (target != null)
        {
            target.RemoveStatusEffect(StatusEffectType.REGEN, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyRitualPerformer(ApplyRitualGA applyRitualGA)
    {
        CombatantView target = applyRitualGA.Target;
        if (target != null)
        {
            target.RemoveStatusEffect(StatusEffectType.RITUAL, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyCursePerformer(ApplyCurseGA applyCurseGA)
    {
        CombatantView target = applyCurseGA.Target;
        if (target != null)
        {
            target.RemoveStatusEffect(StatusEffectType.CURSE, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyHailstormPerformer(ApplyHailstormGA applyHailstormGA)
    {
        CombatantView target = applyHailstormGA.Target;
        if (target != null)
        {
            target.RemoveStatusEffect(StatusEffectType.HAILSTORM, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyMirrorImagePerformer(ApplyMirrorImageGA applyMirrorImageGA)
    {
        CombatantView target = applyMirrorImageGA.Target;
        if (target != null)
        {
            target.RemoveStatusEffect(StatusEffectType.MIRRORIMAGE, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator ApplyAccelerantPerformer(ApplyAccelerantGA applyAccelerantGA)
    {
        CombatantView target = applyAccelerantGA.Target;
        if (target != null)
        {
            target.RemoveStatusEffect(StatusEffectType.ACCELERANT, 1);
        }
        yield return new WaitForSeconds(0.5f);
    }
}
