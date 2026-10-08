using System;
using System.Collections.Generic;
using UnityEngine;

// What the enemy hits with, for its attack sound.
public enum EnemyAttackSound
{
    Blunt, // fists, bodies, shields, staffs, spell blasts
    Sharp, // blades, spears, scythes, claws, fangs, spikes
}

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [field: SerializeField] public Sprite Image { get; private set; }
    [Tooltip("Sound of this enemy's attack hitting the hero")]
    [SerializeField] private EnemyAttackSound attackSound;
    public EnemyAttackSound AttackSound => attackSound;
    [SerializeField] private bool flipSprite;
    [SerializeField] public float scaleX;
    [SerializeField] public float scaleY;
    [SerializeField] public float intentionYPosition;

    [field: SerializeField] public RuntimeAnimatorController AnimatorController { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [SerializeField] private bool followAbilityPattern;
    [SerializeField] private List<EnemyAbilityData> enemyAbilityList;
    public bool FlipSprite => flipSprite;
    public List<EnemyAbilityData> EnemyAbilityList => enemyAbilityList;
    public EnemyAbilityData GetAbility()
    {
        return EnemyAbilityList.RandomItem();
    }

    public EnemyAbilityData GetAbility(int usedAbilityCount)
    {
        if (followAbilityPattern)
        {
            var index = usedAbilityCount % EnemyAbilityList.Count;
            return EnemyAbilityList[index];
        }

        return GetAbility();
    }
}

[Serializable]
public class EnemyAbilityData
{
    [Header("Settings")]
    [SerializeField] private string name;
    [SerializeField] private EnemyIntentionData intention;
    [SerializeField] private bool hideActionValue;
    [SerializeField] private List<EnemyActionData> actionList;
    public string Name => name;
    public EnemyIntentionData Intention => intention;
    public List<EnemyActionData> ActionList => actionList;
    public bool HideActionValue => hideActionValue;
    public EnemyActionData GetAction(EnemyAbilityData enemyAbilityData)
    {
        EnemyActionData enemyAction = enemyAbilityData.ActionList.RandomItem();
        return enemyAction;
    }
}

[Serializable]
public class EnemyActionData
{
    [SerializeField] private EnemyActionType actionType;
    [SerializeField] private int actionValue;
    public EnemyActionType ActionType => actionType;
    public int ActionValue => actionValue;
}
