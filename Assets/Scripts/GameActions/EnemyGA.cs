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

public class EnemyCastGA : GameAction
{
    public EnemyView Caster { get; private set; }
    public EnemyCastGA(EnemyView caster)
    {
        Caster = caster;
    }
}

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