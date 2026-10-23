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

        SetLogo(logoFTIImage, logoFTI);
        SetLogo(logoGameDevImage, logoGameDev);
    }

    // Without a sprite the slot keeps its placeholder label (a child text) so it's clear what goes there.
    private static void SetLogo(Image image, Sprite sprite)
    {
        if (image == null) return;
        image.sprite = sprite;
        image.preserveAspect = true;
        image.enabled = sprite != null;
        foreach (Transform child in image.transform) child.gameObject.SetActive(sprite == null);
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
