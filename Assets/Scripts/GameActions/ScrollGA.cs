using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayScrollGA : GameAction
{
    public EnemyView ManualTarget { get; private set; }
    public Scroll Scroll { get; set; }

    public PlayScrollGA(Scroll scroll)
    {
        Scroll = scroll;
        ManualTarget = null;
    }

    public PlayScrollGA(Scroll scroll, EnemyView target)
    {
        Scroll = scroll;
        ManualTarget = null;
    }
}

