using DG.Tweening;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private SpriteRenderer intetionShow;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private Transform intentionImageTransform;

    public Sprite intentionType { get; set; }
    public float ActionValue { get; set; }
    public EnemyActionType ActionType { get; set; }
    public EnemyData EnemyData { get; set; }
    public EnemyAbilityData enemyAbility { get; set; }
    private int usedAbilityCount { get; set; } = -1;
    public int UsedAbilityCount()
    {
        usedAbilityCount++;
        return usedAbilityCount;
    }
    public void Setup(EnemyData enemyData)
    {
        if (intentionImageTransform != null)
        {
            Vector3 localPos = intentionImageTransform.localPosition;
            intentionImageTransform.localPosition = new Vector3(localPos.x, enemyData.intentionYPosition, localPos.z);
        }
        if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.TORMENT) == 1)
        {
            intetionShow.enabled = false;
            attackText.enabled = false;
        } else {
            intetionShow.enabled = true;
            attackText.enabled = true;
        }
        EnemyData = enemyData;
        enemyAbility = EnemyData.GetAbility(UsedAbilityCount());
        EnemyActionData enemyAction = enemyAbility.GetAction(enemyAbility);
        ActionType = enemyAction.ActionType;
        ActionValue = enemyAction.ActionValue;
        intentionType = enemyAbility.Intention.IntentionSprite;
        intetionShow.sprite = intentionType;
        if (enemyAbility.HideActionValue)
        {
            attackText.alpha = 0;
        }
        else
        {
            attackText.alpha = 255;
        }
        attackText.text = ActionValue.ToString();
        if (GameDataManager.Instance.HasPerk("Howling Visage"))
        {
            SetupBase(enemyData.Health, enemyData.Health - 9, enemyData.Image, enemyData.AnimatorController, enemyData.FlipSprite);
        } else
        {
            SetupBase(enemyData.Health, enemyData.Health, enemyData.Image, enemyData.AnimatorController, enemyData.FlipSprite);
        }
    }

    public void UpdateIntention(EnemyAbilityData newEnemyAbilityData)
    {
        if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.TORMENT) >= 1)
        {
            intetionShow.enabled = false;
            attackText.enabled = false;
        }
        else
        {
            intetionShow.enabled = true;
            attackText.enabled = true;
        }
        intentionType = newEnemyAbilityData.Intention.IntentionSprite;
        intetionShow.sprite = intentionType;
        EnemyActionData enemyAction = newEnemyAbilityData.GetAction(newEnemyAbilityData);
        ActionType = enemyAction.ActionType;
        if (newEnemyAbilityData.HideActionValue)
        {
            attackText.alpha = 0;
        }
        else
        {
            attackText.alpha = 255;
        }
        float finalValue = enemyAction.ActionValue;
        if (GetStatusEffectStacks(StatusEffectType.STRENGTH) != 0 && ActionType == EnemyActionType.Attack)
        {
            finalValue += GetStatusEffectStacks(StatusEffectType.STRENGTH);
        }
        if (GetStatusEffectStacks(StatusEffectType.WEAK) > 0 && ActionType == EnemyActionType.Attack)
        {
            finalValue *= 0.75f;
        }
        if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.VULNERABLE) > 0 && ActionType == EnemyActionType.Attack)
        {
            finalValue *= 1.5f;
        }
        ActionValue = finalValue;
        UpdateActionValue(ActionValue);
    }

    public void UpdateActionValue(float actionValue)
    {
        attackText.text = Mathf.RoundToInt(actionValue).ToString();
    }

    // Enemies without a hurt animation flash red instead.
    protected override void PlayHurt()
    {
        if (PlayAnimation("Hurt") > 0f) return;
        spriteRenderer.DOKill();
        spriteRenderer.color = Color.white;
        spriteRenderer.DOColor(new Color(1f, 0.35f, 0.35f), 0.08f).SetLoops(2, LoopType.Yoyo);
    }

    // Hides everything around the enemy and stops it from being targeted while it dies.
    public void PrepareForDeath()
    {
        HideCombatInfo();
        intetionShow.enabled = false;
        attackText.enabled = false;
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }
    }
}
