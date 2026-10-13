// The hero's armor runs out at the start of the player's turn. Queued as an action (after Burn)
// so Burn at the end of the enemy turn is still blocked by the armor the hero had.
public class ResetHeroArmorGA : GameAction {}
