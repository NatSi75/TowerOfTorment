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

public class EnemyTurnGA : GameAction {}

public class NewIntentionEnemyGA : GameAction {}