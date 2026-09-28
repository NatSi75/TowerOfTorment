using TMPro;
using UnityEngine;

// Look of the tooltip. The asset must live at Resources/TooltipStyle so TooltipSystem can load it.
[CreateAssetMenu(menuName = "Data/Tooltip Style")]
public class TooltipStyle : ScriptableObject
{
    [field: SerializeField] public TMP_FontAsset Font { get; private set; }
    [field: SerializeField] public Material TitleMaterial { get; private set; }
    [field: SerializeField] public Sprite Background { get; private set; }
    [field: SerializeField] public Color BackgroundColor { get; private set; } = new(0.08f, 0.06f, 0.09f, 0.94f);
    [field: SerializeField] public Color BorderColor { get; private set; } = new(0.78f, 0.62f, 0.33f, 1f);
    [field: SerializeField] public Color TitleColor { get; private set; } = new(0.94f, 0.83f, 0.55f, 1f);
    [field: SerializeField] public Color DescriptionColor { get; private set; } = new(0.93f, 0.91f, 0.87f, 1f);
    [field: SerializeField] public float TitleSize { get; private set; } = 32f;
    [field: SerializeField] public float DescriptionSize { get; private set; } = 26f;
    [field: SerializeField] public float MaxWidth { get; private set; } = 440f;
}
