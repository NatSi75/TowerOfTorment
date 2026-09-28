using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatusEffectSystem : MonoBehaviour
{
    [SerializeField] private GameObject vulnerableVFX;
    [SerializeField] private GameObject burnVFX;
    [SerializeField] private GameObject weakVFX;
    [SerializeField] private GameObject poisonVFX;
    [SerializeField] private GameObject bleedVFX;
    [SerializeField] private GameObject chilledVFX;
    [SerializeField] private GameObject voidVFX;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
    }
    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        foreach (CombatantView target in addStatusEffectGA.Targets)
        {
            if (target != null)
            {
                target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount, target);
                switch (addStatusEffectGA.StatusEffectType)
                {
                    case StatusEffectType.BLEED:
                        Instantiate(bleedVFX, target.transform.position, Quaternion.identity);
                        break;
                    case StatusEffectType.BURN:
                        Instantiate(burnVFX, target.transform.position, Quaternion.identity);
                        break;
                    case StatusEffectType.CHILLED:
                        Instantiate(chilledVFX, target.transform.position, Quaternion.identity);
                        break;
                    case StatusEffectType.POISON:
                        Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
                        break;
                    case StatusEffectType.VULNERABLE:
                        Instantiate(vulnerableVFX, target.transform.position, Quaternion.identity);
                        break;
                    case StatusEffectType.WEAK:
                        Instantiate(weakVFX, target.transform.position, Quaternion.identity);
                        break;
                    case StatusEffectType.VOID:
                        Instantiate(voidVFX, target.transform.position, Quaternion.identity);
                        break;
                    default:
                        yield return null; //ADD VFX for adding status effect
                        break;
                }
            }
        }
        yield return null;
    }
}
