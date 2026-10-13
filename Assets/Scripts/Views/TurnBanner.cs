using DG.Tweening;
using TMPro;
using UnityEngine;

// "Turn N" banner (like Slay the Spire) shown between the hero and the enemies whenever the player's turn starts.
// Created by MatchSetupSystem at the start of a battle; counts the turns itself.
public class TurnBanner : MonoBehaviour
{
    private const float FontSize = 16f;
    private const float StripHeight = 2.2f;
    private const float StripWidth = 7f;
    private const float StripAlpha = 0.55f;
    private const float ShowTime = 0.9f;   // fully visible
    private const float FadeTime = 0.35f;  // in and out
    private const int SortingOrder = 400;  // above the combatants, below the damage numbers

    public int Turn { get; private set; }

    private static Sprite whiteSprite;

    public static TurnBanner Begin()
    {
        TurnBanner banner = new GameObject("TurnBanner").AddComponent<TurnBanner>();
        banner.StartTurn();
        return banner;
    }

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    // the enemies are done: the player's next turn begins
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        if (this == null) return; // left over from an earlier battle
        if (Interactions.Instance != null && Interactions.Instance.BattleEnded) return;
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroView != null && HeroSystem.Instance.HeroView.CurrentHealth <= 0) return;
        StartTurn();
    }

    private void StartTurn()
    {
        Turn++;
        Show("Turn " + Turn);
    }

    private void Show(string text)
    {
        HeroView hero = HeroSystem.Instance != null ? HeroSystem.Instance.HeroView : null;

        GameObject root = new("Turn " + Turn);
        root.transform.SetParent(transform, false);
        root.transform.position = BannerPosition(hero);

        // dark strip behind the text
        SpriteRenderer strip = new GameObject("Strip").AddComponent<SpriteRenderer>();
        strip.transform.SetParent(root.transform, false);
        strip.sprite = WhiteSprite();
        strip.color = new Color(0f, 0f, 0f, 0f);
        strip.transform.localScale = new Vector3(StripWidth, StripHeight, 1f);
        strip.sortingOrder = SortingOrder;

        TextMeshPro label = new GameObject("Text").AddComponent<TextMeshPro>();
        label.transform.SetParent(root.transform, false);
        TMP_FontAsset font = hero != null ? hero.LabelFont : null;
        if (font != null)
        {
            label.font = font;
            label.fontSharedMaterial = DamageNumber.OutlinedMaterial(font);
        }
        label.text = text;
        label.fontSize = FontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.color = new Color(1f, 0.92f, 0.75f);
        label.alpha = 0f;
        label.rectTransform.sizeDelta = new Vector2(StripWidth, StripHeight);
        label.sortingOrder = SortingOrder + 1;

        // slide in from the left, hold, fade out to the right
        Transform t = label.transform;
        t.localPosition = new Vector3(-1.2f, 0f, 0f);
        Sequence sequence = DOTween.Sequence().SetLink(root);
        sequence.Append(DOTween.To(() => label.alpha, a => label.alpha = a, 1f, FadeTime));
        sequence.Join(strip.DOFade(StripAlpha, FadeTime));
        sequence.Join(t.DOLocalMoveX(0f, FadeTime).SetEase(Ease.OutCubic));
        sequence.AppendInterval(ShowTime);
        sequence.Append(DOTween.To(() => label.alpha, a => label.alpha = a, 0f, FadeTime));
        sequence.Join(strip.DOFade(0f, FadeTime));
        sequence.Join(t.DOLocalMoveX(1.2f, FadeTime).SetEase(Ease.InCubic));
        sequence.OnComplete(() => Destroy(root));
    }

    // Halfway between the hero and the nearest enemy, around the height of their bodies
    // (so it stays clear of the enemies' intents).
    private static Vector3 BannerPosition(HeroView hero)
    {
        if (hero == null) return Vector3.zero;
        Vector3 heroPos = hero.transform.position;
        float enemyX = heroPos.x + 12f;
        if (EnemySystem.Instance != null && EnemySystem.Instance.Enemies != null)
        {
            foreach (EnemyView enemy in EnemySystem.Instance.Enemies)
            {
                if (enemy != null) enemyX = Mathf.Min(enemyX, enemy.transform.position.x);
            }
        }
        float bodyY = hero.spriteRenderer != null ? hero.spriteRenderer.bounds.center.y : heroPos.y;
        return new Vector3((heroPos.x + enemyX) / 2f, bodyY + 0.5f, 0f);
    }

    private static Sprite WhiteSprite()
    {
        if (whiteSprite == null)
        {
            Texture2D texture = Texture2D.whiteTexture;
            whiteSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        }
        return whiteSprite;
    }
}
