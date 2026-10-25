using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class MenuTextInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scene Settings")]
    [Tooltip("Nama scene yang akan diload saat diklik")]
    public string sceneToLoad;

    [Tooltip("Centang untuk membuka panel Setting (volume) alih-alih pindah scene")]
    public bool opensSettings;

    [Tooltip("Centang untuk membuka panel Compendium")]
    public bool opensCompendium;

    [Tooltip("Centang untuk membuka panel About")]
    public bool opensAbout;

    [Header("Hover Settings")]
    [Tooltip("Masukkan object Selected_line dari Hierarchy")]
    public GameObject selectedLine;

    [Tooltip("Masukkan gambar cursor (Texture2D) untuk state hover")]
    public Texture2D hoverCursor;

    [Tooltip("Posisi offset untuk garis bawah (sesuaikan jika garis terlalu atas/bawah)")]
    public Vector2 lineOffset = new Vector2(0, -25f);

    [Header("Vertex Color Settings")]
    [Tooltip("Warna vertex saat kursor masuk (Hover)")]
    public Color hoverColorBL = Color.yellow; // Kiri Bawah
    public Color hoverColorTL = Color.yellow; // Kiri Atas
    public Color hoverColorTR = Color.yellow; // Kanan Atas
    public Color hoverColorBR = Color.yellow; // Kanan Bawah

    [Tooltip("Warna vertex default teks (saat kursor keluar)")]
    public Color normalColorBL = Color.white; // Kiri Bawah
    public Color normalColorTL = Color.white; // Kiri Atas
    public Color normalColorTR = Color.white; // Kanan Atas
    public Color normalColorBR = Color.white; // Kanan Bawah

    private TMP_Text textComponent;
    private Vector2 cursorHotspot = Vector2.zero;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (selectedLine != null && selectedLine.transform.parent == this.transform)
        {
            selectedLine.SetActive(false);
        }

        ApplyVertexColors(normalColorBL, normalColorTL, normalColorTR, normalColorBR);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverCursor != null)
        {
            Cursor.SetCursor(hoverCursor, cursorHotspot, CursorMode.Auto);
        }

        if (selectedLine != null)
        {
            selectedLine.transform.SetParent(this.transform, false);
            selectedLine.GetComponent<RectTransform>().anchoredPosition = lineOffset;
            selectedLine.SetActive(true);
        }

        ApplyVertexColors(hoverColorBL, hoverColorTL, hoverColorTR, hoverColorBR);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (selectedLine != null)
        {
            selectedLine.SetActive(false);
        }

        ApplyVertexColors(normalColorBL, normalColorTL, normalColorTR, normalColorBR);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (opensSettings)
        {
            SettingsPanel.Open();
            return;
        }
        if (opensCompendium)
        {
            CompendiumPanel.Open();
            return;
        }
        if (opensAbout)
        {
            AboutPanel.Open();
            return;
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Nama scene belum diisi pada: " + gameObject.name);
        }
    }

    private void ApplyVertexColors(Color bottomLeft, Color topLeft, Color topRight, Color bottomRight)
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

            vertexColors[vertexIndex + 0] = bottomLeft;
            vertexColors[vertexIndex + 1] = topLeft;
            vertexColors[vertexIndex + 2] = topRight;
            vertexColors[vertexIndex + 3] = bottomRight;
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}