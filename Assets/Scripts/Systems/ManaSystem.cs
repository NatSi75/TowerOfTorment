using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    [SerializeField] private ManaUI manaUI;
    [SerializeField] private Color notEnoughManaColor = new(1f, 0.35f, 0.3f);
    private const int MAX_MANA = 3;
    private int currentMana = MAX_MANA;

    void Start()
    {
        manaUI.UpdateManaText(currentMana, MAX_MANA);
    }

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<RefillManaGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public bool HasEnoughMana(int mana)
    {
        return currentMana >= mana;
    }

    public void ShowNotEnoughMana()
    {
        manaUI.ShowNotEnough();
        Camera cam = Camera.main;
        Vector3 position = cam != null ? cam.transform.position + Vector3.up * 1.5f : Vector3.zero;
        TMP_FontAsset font = HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null ? HeroSystem.Instance.HeroView.LabelFont : null;
        DamageNumber.Show(position, "Not enough mana!", notEnoughManaColor, font, 0.8f);
    }

    public void GainMana(int mana)
    {
        currentMana += mana;
        manaUI.UpdateManaText(currentMana, MAX_MANA);
    }

    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        currentMana -= spendManaGA.Amount;
        manaUI.UpdateManaText(currentMana, MAX_MANA);
        yield return null;
    }

    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        currentMana = MAX_MANA;
        if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.VOID) > 0)
        {
            currentMana -= Mathf.RoundToInt(HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.VOID));
            ApplyVoidGA applyVoidGA = new(HeroSystem.Instance.HeroView);
            ActionSystem.Instance.AddReaction(applyVoidGA);
        }
        manaUI.UpdateManaText(currentMana, MAX_MANA);
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);
    }
}
