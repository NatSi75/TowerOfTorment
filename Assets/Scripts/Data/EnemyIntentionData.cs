using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/EnemyIntention")]
public class EnemyIntentionData : ScriptableObject
{
    [SerializeField] private EnemyIntentionType enemyIntentionType;
    [SerializeField] private Sprite intentionSprite;

    public EnemyIntentionType EnemyIntentionType => enemyIntentionType;

    public Sprite IntentionSprite => intentionSprite;
}
