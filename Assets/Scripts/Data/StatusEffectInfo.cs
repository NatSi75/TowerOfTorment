// Display names and tooltip descriptions for every status effect.
public static class StatusEffectInfo
{
    public static string GetName(StatusEffectType type)
    {
        return type switch
        {
            StatusEffectType.ARMOR => "Armor",
            StatusEffectType.BURN => "Burn",
            StatusEffectType.STRENGTH => "Strength",
            StatusEffectType.DEXTERITY => "Dexterity",
            StatusEffectType.WEAK => "Weak",
            StatusEffectType.VULNERABLE => "Vulnerable",
            StatusEffectType.POISON => "Poison",
            StatusEffectType.BLEED => "Bleed",
            StatusEffectType.CHILLED => "Chilled",
            StatusEffectType.VOID => "Void",
            StatusEffectType.TORMENT => "Tormented",
            StatusEffectType.REGEN => "Regen",
            StatusEffectType.RITUAL => "Ritual",
            StatusEffectType.CURSE => "Curse",
            StatusEffectType.HAILSTORM => "Hailstorm",
            StatusEffectType.MIRRORIMAGE => "Mirror Image",
            StatusEffectType.ACCELERANT => "Accelerant",
            _ => type.ToString(),
        };
    }

    public static string GetDescription(StatusEffectType type, int stacks)
    {
        string n = Highlight(stacks.ToString());
        return type switch
        {
            StatusEffectType.ARMOR => $"Blocks the next {n} damage. Removed at the start of its owner's next turn.",
            StatusEffectType.BURN => $"Takes {n} damage at the end of the enemy turn, then Burn decreases by 1.",
            StatusEffectType.STRENGTH => $"Attacks deal {n} additional damage.",
            StatusEffectType.DEXTERITY => $"Gain {n} additional Armor from cards.",
            StatusEffectType.WEAK => $"Attacks deal {Highlight("25%")} less damage. Lasts {n} turn(s).",
            StatusEffectType.VULNERABLE => $"Takes {Highlight("50%")} more damage from attacks. Lasts {n} turn(s).",
            StatusEffectType.POISON => $"Loses {n} HP at the start of its turn, then Poison decreases by 1.",
            StatusEffectType.BLEED => $"Takes {n} additional damage from each of your attacks. Decreases by 1 each turn.",
            StatusEffectType.CHILLED => $"Draw 1 fewer card at the start of your turn. Lasts {n} turn(s).",
            StatusEffectType.VOID => $"Start your turn with {n} less Mana. Decreases by 1 each turn.",
            StatusEffectType.TORMENT => "Your Torment is full. Enemy intentions are hidden from you.",
            StatusEffectType.REGEN => $"Heal {n} HP at the end of your turn, then Regen decreases by 1.",
            StatusEffectType.RITUAL => $"Gain {n} Strength at the end of each of your turns.",
            StatusEffectType.CURSE => $"At the start of the enemy turn, apply {n} Poison to every enemy.",
            StatusEffectType.HAILSTORM => $"Whenever you play a card, deal {n} damage to all enemies.",
            StatusEffectType.MIRRORIMAGE => $"Whenever you play a card, gain {n} Armor.",
            StatusEffectType.ACCELERANT => $"Poison on enemies triggers {n} extra time(s) each turn.",
            _ => string.Empty,
        };
    }

    public static string Highlight(string value)
    {
        return $"<color=#F0C060>{value}</color>";
    }
}
