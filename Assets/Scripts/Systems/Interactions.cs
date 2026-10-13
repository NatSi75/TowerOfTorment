using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactions : Singleton<Interactions>
{
    public bool PlayerIsDragging { get; set; } = false;
    // Set when the battle rewards appear: the hand can no longer be hovered or played.
    public bool BattleEnded { get; set; } = false;
    public bool PlayerCanInteract()
    {
        if (PauseMenu.IsPaused || BattleEnded) return false;
        if (!ActionSystem.Instance.IsPerforming) return true;
        else return false;
    }

    public bool PlayerCanHover()
    {
        if (PauseMenu.IsPaused || BattleEnded) return false;
        if (PlayerIsDragging) return false;
        return true;
    }
}
