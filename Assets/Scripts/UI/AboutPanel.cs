using TMPro;
using UnityEngine;
using UnityEngine.UI;

// About window: FTI UNTAR and Game Development UNTAR logos, the supervisors and the author.
// The prefab lives at Resources/AboutPanel; open it with AboutPanel.Open(). Edit the texts and logos on the prefab.
public class AboutPanel : MonoBehaviour
{
    [Header("Content (edit on the prefab)")]
    [SerializeField] private string[] supervisors = { "-", "-" };
    [SerializeField] private string author = "Natanael Sion";
    [SerializeField] private Sprite logoFTI;
    [SerializeField] private Sprite logoGameDev;

    [Header("References")]
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private Image logoFTIImage;
    [SerializeField] private Image logoGameDevImage;
    [Tooltip("Optional plain backdrop behind a logo (e.g. white behind a dark logo); sized to the logo")]
    [SerializeField] private Image logoFTIBackground;
    [SerializeField] private Image logoGameDevBackground;
    [SerializeField] private Vector2 logoBackgroundPadding = new(28f, 20f);

    private static AboutPanel openPanel;

    public static bool IsOpen => openPanel != null;

    public static void Open()
    {
        if (openPanel != null) return;
        AboutPanel prefab = Resources.Load<AboutPanel>("AboutPanel");
        if (prefab == null)
        {
            Debug.LogWarning("AboutPanel prefab not found in Resources.");
            return;
        }
        openPanel = Instantiate(prefab);
    }

    private void Start()
    {
        backButton.onClick.AddListener(Close);

        System.Text.StringBuilder text = new();
        text.AppendLine("Dosen Pembimbing:");
        for (int i = 0; i < supervisors.Length; i++) text.AppendLine($"   {i + 1}. {supervisors[i]}");
        text.AppendLine();
        text.Append("Penulis: ").Append(author);
        creditsText.text = text.ToString();

        SetLogo(logoFTIImage, logoFTI, logoFTIBackground);
        SetLogo(logoGameDevImage, logoGameDev, logoGameDevBackground);
    }

    // Without a sprite the slot keeps its placeholder label (a child text) so it's clear what goes there.
    private void SetLogo(Image image, Sprite sprite, Image background)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.preserveAspect = true;
        image.enabled = sprite != null;
        foreach (Transform child in image.transform) child.gameObject.SetActive(sprite == null);

        if (background == null) return;
        background.gameObject.SetActive(sprite != null);
        if (sprite == null) return;
        // fit the backdrop to the part of the slot the logo really covers (it keeps its aspect)
        Vector2 slot = image.rectTransform.rect.size;
        float aspect = sprite.rect.width / sprite.rect.height;
        Vector2 drawn = slot.x / slot.y > aspect ? new Vector2(slot.y * aspect, slot.y) : new Vector2(slot.x, slot.x / aspect);
        RectTransform rect = background.rectTransform;
        rect.position = image.rectTransform.TransformPoint(image.rectTransform.rect.center);
        rect.sizeDelta = drawn + logoBackgroundPadding;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openPanel == this) openPanel = null;
    }
}
