using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealGA : GameAction
{
    public int Amount { get; set; }
    public HealGA(int amount)
    {
        Amount = amount;
    }
}
