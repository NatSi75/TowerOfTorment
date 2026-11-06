using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private Transform exhaustPilePoint;
    [SerializeField] private TMP_Text drawPileText;
    [SerializeField] private TMP_Text discardPileText;
    [SerializeField] private TMP_Text exhaustPileText;
    public readonly List<Card> drawPile = new();
    public readonly List<Card> discardPile = new();
    public readonly List<Card> exhaustPile = new();
    private readonly List<Card> hand = new();

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
    }

    // Publics
    public void Setup(List<Card> deckData)
    {
        exhaustPile.Clear();
        foreach (var card in deckData)
        {
            drawPile.Add(card);
        }
    }

    // Performers
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;
        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }

        if (notDrawnAmount > 0)
        {
            RefillDeck();
            for (int i = 0; i < notDrawnAmount; i++)
            {
                yield return DrawCard();
            }
        }
    }

    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    } 

    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        AudioManager.PlaySfx(Sfx.PlayCard);
        yield return ShowPlayedCard(cardView);
        if (playCardGA.ManualTarget != null)
        {
            foreach (var effect in cardView.Card.data.ManualTargetEffect)
            { 
                if (effect is AddStatusEffectEffect statusEffect)
                {
                    StatusEffectType typeStatus = statusEffect.statusEffectType;
                    if (playCardGA.ManualTarget is EnemyView enemyView && enemyView.ActionType == EnemyActionType.Attack)
                    {
                        if (typeStatus == StatusEffectType.STRENGTH)
                        {
                            enemyView.ActionValue += statusEffect.stackCount;
                            enemyView.UpdateActionValue(enemyView.ActionValue);
                        }
                        if (typeStatus == StatusEffectType.WEAK && enemyView.GetStatusEffectStacks(StatusEffectType.WEAK) == 0)
                        {
                            enemyView.ActionValue *= 0.75f;
                            enemyView.UpdateActionValue(enemyView.ActionValue);
                        }
                    }
                }
            }  
        }

        foreach (var effectWrapper in playCardGA.Card.OtherEffects)
        {
            if (effectWrapper.TargetMode is AllEnemiesTM allEnemiesTM && effectWrapper.Effect is AddStatusEffectEffect statusEffectStrength)
            {
                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                StatusEffectType typeStatus = statusEffectStrength.statusEffectType;
                foreach (EnemyView target in targets.Cast<EnemyView>())
                {
                    if (typeStatus == StatusEffectType.STRENGTH && target.ActionType == EnemyActionType.Attack)
                    {
                        target.ActionValue += statusEffectStrength.stackCount;
                        target.UpdateActionValue(target.ActionValue);
                    }
                }
            }
            if (effectWrapper.TargetMode is AllEnemiesTM allEnemiesTM2 && effectWrapper.Effect is AddStatusEffectEffect statusEffectWeak)
            {
                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                StatusEffectType typeStatus = statusEffectWeak.statusEffectType;
                foreach (EnemyView target in targets.Cast<EnemyView>())
                {
                    if (typeStatus == StatusEffectType.WEAK && target.ActionType == EnemyActionType.Attack && target.GetStatusEffectStacks(StatusEffectType.WEAK) == 0)
                    {
                        target.ActionValue *= 0.75f;
                        target.UpdateActionValue(target.ActionValue);
                    }
                }
            }
        }
         
        if (cardView.Card.data.Exhaust)
        {
            yield return ExhaustCard(cardView);
        }
        else
        {
            yield return DiscardCard(cardView);
        }

        if (playCardGA.Card.Type == CardType.Attack)
        {
            float attackLength = HeroSystem.Instance.HeroView.PlayAnimation("Attack");
            if (attackLength > 0f) yield return new WaitForSeconds(attackLength * 0.5f);
            bool isKnight = GameDataManager.Instance == null || GameDataManager.Instance.indexHero == 0;
            AudioManager.PlaySfx(isKnight ? Sfx.KnightAttack : Sfx.WizardAttack);
        }

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);

        if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.HAILSTORM) > 0)
        {
            foreach (var target in EnemySystem.Instance.Enemies)
            {
                CombatantView attacker = HeroSystem.Instance.HeroView;
                DealDamageGA damageGA = new(HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.HAILSTORM), new() { target }, attacker);
                ActionSystem.Instance.AddReaction(damageGA);
            }
        }

        if (HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.MIRRORIMAGE) > 0)
        {
            HeroSystem.Instance.HeroView.AddStatusEffect(StatusEffectType.ARMOR, HeroSystem.Instance.HeroView.GetStatusEffectStacks(StatusEffectType.MIRRORIMAGE), HeroSystem.Instance.HeroView);
        }

        foreach (var effectWrapper in playCardGA.Card.OtherEffects)
        {
            if (!IsHealthCost(effectWrapper)) continue;
            PerformEffectGA payHealthGA = new(effectWrapper.Effect, effectWrapper.TargetMode.GetTargets());
            ActionSystem.Instance.AddReaction(payHealthGA);
        }

        foreach (var effectWrapper in playCardGA.Card.ManualTargetEffect)
        {
            if ((cardView.Card.data.name == "Bane" || cardView.Card.data.name == "Bane +") && playCardGA.ManualTarget is EnemyView enemyView && enemyView.GetStatusEffectStacks(StatusEffectType.POISON) > 0)
            {
                PerformEffectGA performEffectGA = new(effectWrapper, new() { playCardGA.ManualTarget });
                ActionSystem.Instance.AddReaction(performEffectGA);
                PerformEffectGA performEffectGA2 = new(effectWrapper, new() { playCardGA.ManualTarget });
                ActionSystem.Instance.AddReaction(performEffectGA2);
            } else if (cardView.Card.data.name == "Catalyst" && playCardGA.ManualTarget is EnemyView enemyView2 && enemyView2.GetStatusEffectStacks(StatusEffectType.POISON) > 0) {
                PerformEffectGA performEffectGA = new(effectWrapper, new() { playCardGA.ManualTarget });
                ActionSystem.Instance.AddReaction(performEffectGA);
                float amountStack = enemyView2.GetStatusEffectStacks(StatusEffectType.POISON);
                enemyView2.AddStatusEffect(StatusEffectType.POISON, amountStack, enemyView2);
            } else if (cardView.Card.data.name == "Catalyst +" && playCardGA.ManualTarget is EnemyView enemyView3 && enemyView3.GetStatusEffectStacks(StatusEffectType.POISON) > 0)
            {
                PerformEffectGA performEffectGA = new(effectWrapper, new() { playCardGA.ManualTarget });
                ActionSystem.Instance.AddReaction(performEffectGA);
                float amountStack = enemyView3.GetStatusEffectStacks(StatusEffectType.POISON);
                enemyView3.AddStatusEffect(StatusEffectType.POISON, amountStack*2, enemyView3);
            } else if (cardView.Card.data.name == "Creeping Chill" || cardView.Card.data.name == "Creeping Chill +")
            {
                if (playCardGA.ManualTarget is EnemyView enemyView4 && enemyView4.GetStatusEffectStacks(StatusEffectType.POISON) > 0)
                {
                    PerformEffectGA performEffectGA = new(effectWrapper, new() { playCardGA.ManualTarget });
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            } else
            {
                PerformEffectGA performEffectGA = new(effectWrapper, new() { playCardGA.ManualTarget });
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
            
        }

        foreach (var effectWrapper in playCardGA.Card.OtherEffects)
        {
            if (IsHealthCost(effectWrapper)) continue; // already paid above
            List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
            if (cardView.Card.data.name == "Mirage" || cardView.Card.data.name == "Mirage +")
            {
                float amountPoison = 0;
                foreach (var target in targets)
                {
                    amountPoison += target.GetStatusEffectStacks(StatusEffectType.POISON);
                }
                HeroSystem.Instance.HeroView.AddStatusEffect(StatusEffectType.ARMOR, amountPoison, HeroSystem.Instance.HeroView);
            }
            PerformEffectGA performEffectGA = new(effectWrapper.Effect, targets);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
    }

    [Header("Play Card Feedback")]
    [Tooltip("Where a played card is shown, relative to the camera centre")]
    [SerializeField] private Vector2 playedCardOffset = new(0f, 0.5f);
    [SerializeField] private float playedCardScale = 1.15f;
    [SerializeField] private Color playedCardGlow = new(1f, 0.85f, 0.4f, 0.8f);
    private static Sprite glowSprite;

    // The played card moves to the middle of the screen, pops and glows, before its effects happen.
    private IEnumerator ShowPlayedCard(CardView cardView)
    {
        Camera cam = Camera.main;
        if (cardView == null || cam == null) yield break;
        cardView.SetAnimating(true);

        Transform card = cardView.transform;
        card.DOKill();
        card.rotation = Quaternion.identity;
        if (cardView.TryGetComponent(out UnityEngine.Rendering.SortingGroup sorting)) sorting.sortingOrder = 300;
        float scale = MobileLayout.HandScale * playedCardScale;
        Vector3 centerOffset = Vector3.Scale(cardView.VisualCenter, new Vector3(scale, scale, 1f));
        Vector3 target = cam.transform.position + (Vector3)playedCardOffset - centerOffset;
        target.z = card.position.z;

        SpriteRenderer glow = CreateGlow(cardView);
        Sequence sequence = DOTween.Sequence().SetLink(cardView.gameObject);
        sequence.Append(card.DOMove(target, 0.15f).SetEase(Ease.OutQuad));
        sequence.Join(card.DOScale(scale, 0.15f).SetEase(Ease.OutBack));
        if (glow != null)
        {
            sequence.Join(glow.DOFade(playedCardGlow.a, 0.1f));
            sequence.Append(glow.DOFade(0f, 0.25f));
        }
        sequence.Join(card.DOPunchScale(Vector3.one * 0.08f * scale, 0.2f, 6, 0.5f));
        yield return sequence.WaitForCompletion();
    }

    private SpriteRenderer CreateGlow(CardView cardView)
    {
        Bounds? bounds = null;
        foreach (SpriteRenderer sr in cardView.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.sprite == null || !sr.enabled) continue;
            if (bounds == null) bounds = sr.bounds;
            else { Bounds b = bounds.Value; b.Encapsulate(sr.bounds); bounds = b; }
        }
        if (bounds == null) return null;

        GameObject go = new("Play Glow");
        go.transform.SetParent(cardView.transform, false);
        go.transform.position = new Vector3(bounds.Value.center.x, bounds.Value.center.y, cardView.transform.position.z);
        Vector3 parentScale = cardView.transform.lossyScale;
        Vector3 size = bounds.Value.size * 1.12f;
        go.transform.localScale = new Vector3(size.x / Mathf.Max(0.0001f, parentScale.x), size.y / Mathf.Max(0.0001f, parentScale.y), 1f);

        SpriteRenderer glow = go.AddComponent<SpriteRenderer>();
        glow.sprite = GlowSprite();
        Color color = playedCardGlow;
        color.a = 0f;
        glow.color = color;
        glow.sortingOrder = -5; // behind the card inside its sorting group
        return glow;
    }

    // Soft rounded glow, drawn once in code.
    private static Sprite GlowSprite()
    {
        if (glowSprite != null) return glowSprite;
        const int size = 64;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - size / 2f) - size * 0.3f);
                float dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - size / 2f) - size * 0.3f);
                float edge = Mathf.Sqrt(dx * dx + dy * dy) / (size * 0.2f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - edge)));
            }
        }
        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return glowSprite;
    }

    private static bool IsHealthCost(AutoTargetEffect effectWrapper)
    {
        return effectWrapper.Effect is HealEffect heal && heal.healAmount < 0 && effectWrapper.TargetMode is HeroTM;
    }

    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        UpdatePileUI();
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        cardView.SetAnimating(true);
        yield return handView.AddCard(cardView);
        if (cardView != null) cardView.SetAnimating(false);
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        UpdatePileUI();
        discardPile.Clear();
    }

    private IEnumerator DiscardCard (CardView cardView)
    {
        cardView.SetAnimating(true);
        discardPile.Add(cardView.Card);
        UpdatePileUI();
        cardView.transform.DOScale(Vector3.zero, 1.5f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }

    private IEnumerator ExhaustCard(CardView cardView)
    {
        cardView.SetAnimating(true);
        exhaustPile.Add(cardView.Card);
        UpdatePileUI();
        cardView.transform.DOScale(Vector3.zero, 1.5f);
        Tween tween = cardView.transform.DOMove(exhaustPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }

    private void UpdatePileUI()
    {
        if (drawPileText != null) drawPileText.text = drawPile.Count.ToString();
        if (discardPileText != null) discardPileText.text = discardPile.Count.ToString();
        if (exhaustPileText != null) exhaustPileText.text = exhaustPile.Count.ToString();
    }
}
