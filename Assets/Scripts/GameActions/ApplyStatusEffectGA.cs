public class AddStrengthGA : GameAction
{
    public int Amount { get; set; }
    public AddStrengthGA(int amount)
    {
        Amount = amount;
    }
}

public class AddDexterityGA : GameAction
{
    public int Amount { get; set; }
    public AddDexterityGA(int amount)
    {
        Amount = amount;
    }
}

public class ApplyVulnerableGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyVulnerableGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyBurnGA : GameAction
{
    public float BurnDamage { get; private set; }
    public CombatantView Target { get; private set; }
    public ApplyBurnGA(float burnDamage, CombatantView target)
    {
        BurnDamage = burnDamage;
        Target = target;
    }
}

public class ApplyWeakGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyWeakGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyPoisonGA : GameAction
{
    public float PoisonDamage { get; private set; }
    public CombatantView Target { get; private set; }
    public ApplyPoisonGA(float poisonDamage, CombatantView target)
    {
        PoisonDamage = poisonDamage;
        Target = target;
    }
}

public class ApplyBleedGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyBleedGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyChilledGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyChilledGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyVoidGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyVoidGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyTormentGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyTormentGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyRegenGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyRegenGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyRitualGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyRitualGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyCurseGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyCurseGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyHailstormGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyHailstormGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyMirrorImageGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyMirrorImageGA(CombatantView target)
    {
        Target = target;
    }
}

public class ApplyAccelerantGA : GameAction
{
    public CombatantView Target { get; private set; }
    public ApplyAccelerantGA(CombatantView target)
    {
        Target = target;
    }
}