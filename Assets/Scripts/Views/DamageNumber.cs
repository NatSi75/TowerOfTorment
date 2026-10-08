using DG.Tweening;
using TMPro;
using UnityEngine;

// Floating combat text ("12", "Blocked") that pops up above a combatant, rises and fades out.
// Created in code, so no prefab is needed; it uses the same font as the combatant's HP label.
public static class DamageNumber
{
    public static readonly Color DamageColor = new(1f, 0.25f, 0.2f);
    public static readonly Color BlockedColor = new(0.55f, 0.75f, 1f);

    private const float FontSize = 18f;
    private const float RiseDistance = 1.6f;
    private const float Duration = 0.9f;
    private const int SortingOrder = 500; // above the combatants and their effects

    public static void Show(Vector3 position, string text, Color color, TMP_FontAsset font, float sizeMultiplier = 1f)
    {
        GameObject go = new("DamageNumber");
        go.transform.position = new Vector3(position.x, position.y, 0f);

        TextMeshPro label = go.AddComponent<TextMeshPro>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = FontSize * sizeMultiplier;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.color = color;
        label.fontSharedMaterial = OutlinedMaterial(label.font);
        label.rectTransform.sizeDelta = new Vector2(6f, 2f);
        label.sortingOrder = SortingOrder;

        // pop in, rise, then fade out
        Transform t = go.transform;
        t.localScale = Vector3.one * 0.4f;
        Sequence sequence = DOTween.Sequence().SetLink(go);
        sequence.Append(t.DOScale(1.15f, 0.12f).SetEase(Ease.OutBack));
        sequence.Append(t.DOScale(1f, 0.08f));
        sequence.Insert(0f, t.DOMoveY(position.y + RiseDistance, Duration).SetEase(Ease.OutCubic));
        sequence.Insert(Duration * 0.55f, DOTween.To(() => label.alpha, a => label.alpha = a, 0f, Duration * 0.45f));
        sequence.OnComplete(() => Object.Destroy(go));
    }

    private static Material outlinedMaterial;
    private static TMP_FontAsset outlinedFont;

    // One shared material with a thick dark outline (per font), so every number stays readable on any background.
    private static Material OutlinedMaterial(TMP_FontAsset font)
    {
        if (outlinedMaterial != null && outlinedFont == font) return outlinedMaterial;

        outlinedFont = font;
        // the saved material keeps the outline shader variant in builds (a material made only in code may lose it)
        Material saved = Resources.Load<Material>("DamageNumberMaterial");
        if (saved != null && saved.mainTexture == font.atlasTexture)
        {
            outlinedMaterial = saved;
            return outlinedMaterial;
        }
        outlinedMaterial = new Material(font.material) { name = font.name + " (damage number)" };
        outlinedMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
        outlinedMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.15f);
        outlinedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.3f);
        outlinedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(25, 10, 10, 255));
        return outlinedMaterial;
    }
}
