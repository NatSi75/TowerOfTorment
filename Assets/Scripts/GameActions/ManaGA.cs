using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddManaGA : GameAction
{
    public int Amount { get; set; }
    public AddManaGA(int amount)
    {
        Amount = amount;
    }
}

public class SpendManaGA : GameAction
{
    public int Amount { get; set; }
    public SpendManaGA(int amount)
    {
        Amount = amount;
    }
}

public class RefillManaGA : GameAction
{
    
}
