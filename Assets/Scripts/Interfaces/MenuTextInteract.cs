using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro; // Tambahkan namespace ini untuk mengakses TextMeshPro

[RequireComponent(typeof(TMP_Text))] // Memastikan objek selalu memiliki komponen TextMeshPro
public class MenuTextInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scene Settings")]
    [Tooltip("Nama scene yang akan diload saat diklik")]
    public string sceneToLoad;

    [Tooltip("Centang untuk membuka panel Setting (volume) alih-alih pindah scene")]
    public bool opensSettings;

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
        // Ambil komponen TextMeshPro dari objek ini
        textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        // Pastikan garis disembunyikan saat game baru mulai
        if (selectedLine != null && selectedLine.transform.parent == this.transform)
        {
            selectedLine.SetActive(false);
        }

        // Aplikasikan warna normal di awal saat game mulai
        ApplyVertexColors(normalColorBL, normalColorTL, normalColorTR, normalColorBR);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. Ubah Cursor saat di-hover
        if (hoverCursor != null)
        {
            Cursor.SetCursor(hoverCursor, cursorHotspot, CursorMode.Auto);
        }

        // 2. Jadikan Selected_line sebagai child dari teks ini, lalu aktifkan
        if (selectedLine != null)
        {
            selectedLine.transform.SetParent(this.transform, false);
            selectedLine.GetComponent<RectTransform>().anchoredPosition = lineOffset;
            selectedLine.SetActive(true);
        }

        // 3. Ubah Vertex Color ke warna Hover
        ApplyVertexColors(hoverColorBL, hoverColorTL, hoverColorTR, hoverColorBR);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 1. Kembalikan Cursor ke default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // 2. Sembunyikan Selected_line saat kursor pergi
        if (selectedLine != null)
        {
            selectedLine.SetActive(false);
        }

        // 3. Kembalikan Vertex Color ke warna Normal
        ApplyVertexColors(normalColorBL, normalColorTL, normalColorTR, normalColorBR);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Kembalikan kursor ke default sebelum pindah scene
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (opensSettings)
        {
            SettingsPanel.Open();
            return;
        }

        // 3. Pindah Scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Nama scene belum diisi pada: " + gameObject.name);
        }
    }

    // Fungsi internal untuk memodifikasi mesh color per vertex
    private void ApplyVertexColors(Color bottomLeft, Color topLeft, Color topRight, Color bottomRight)
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Abaikan spasi
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