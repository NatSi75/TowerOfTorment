using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemyGA : GameAction
{
    public EnemyView EnemyView { get; private set; }
    public KillEnemyGA(EnemyView enemyView)
    {
        EnemyView = enemyView;
    }
}

// Plays the enemy's attack animation for actions that are not a direct attack (debuffs, buffs).
public class EnemyCastGA : GameAction
{
    public EnemyView Caster { get; private set; }
    public EnemyCastGA(EnemyView caster)
    {
        Caster = caster;
    }
}

public class EnemyTurnGA : GameAction {}

public class NewIntentionEnemyGA : GameAction {}