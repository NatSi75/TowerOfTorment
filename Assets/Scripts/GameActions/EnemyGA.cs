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

// One enemy carries out its intent (attack, block, debuff...). Queued per enemy during the enemy turn,
// after its Poison / Bleed, so an enemy that dies from those skips its action.
public class EnemyActionGA : GameAction
{
    public EnemyView Enemy { get; private set; }
    public EnemyActionGA(EnemyView enemy)
    {
        Enemy = enemy;
    }
}

public class EnemyTurnGA : GameAction {}

public class NewIntentionEnemyGA : GameAction {}