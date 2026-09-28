using Map;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Character Setup")]
    public GameObject[] characters;
    public HeroData[] charactersData;
    public Image relic;
    public TMP_Text relicDescription;
    public TMP_Text nameCharacter;
    public TMP_Text hp;
    public TMP_Text torment;
    public TMP_Text gold;
    public TMP_Text description;

    // Menyimpan index karakter yang sedang dipilih (0 = Knight, 1 = Wizard)
    private int selectedIndex = 0;

    private void Start()
    {
        // Pastikan saat scene mulai, hanya karakter pertama yang terlihat
        UpdateCharacterDisplay();
    }

    // Fungsi untuk tombol "Next" (Panah Kanan)
    public void NextCharacter()
    {
        selectedIndex++;
        // Jika melebihi jumlah karakter, kembali ke index 0
        if (selectedIndex >= characters.Length)
        {
            selectedIndex = 0;
        }
        UpdateCharacterDisplay();
    }

    // Fungsi untuk tombol "Previous" (Panah Kiri)
    public void PreviousCharacter()
    {
        selectedIndex--;
        // Jika kurang dari 0, pergi ke karakter terakhir
        if (selectedIndex < 0)
        {
            selectedIndex = characters.Length - 1;
        }
        UpdateCharacterDisplay();
    }

    // Memperbarui visual karakter di layar
    private void UpdateCharacterDisplay()
    {
        // Matikan semua karakter
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(false);
        }

        // Nyalakan hanya karakter yang sesuai dengan index pilihan
        characters[selectedIndex].SetActive(true);
        relic.sprite = charactersData[selectedIndex].PerkHero.Image;
        relicDescription.text = charactersData[selectedIndex].PerkHero.Description;
        nameCharacter.text = charactersData[selectedIndex].name;
        hp.text = "HP: " + charactersData[selectedIndex].Health;
        torment.text = "Max Torment: " + charactersData[selectedIndex].MaxTorment;
        gold.text = "Gold: 99";
        description.text = charactersData[selectedIndex].Description;
    }

    // Fungsi untuk tombol "SELECT / PLAY"
    public void ConfirmSelection()
    {
        // Simpan index karakter terpilih ke PlayerPrefs agar bisa dibaca di Scene lain
        PlayerPrefs.SetInt("SelectedHeroIndex", selectedIndex);
        PlayerPrefs.DeleteKey("Map");
        PlayerPrefs.Save();

        // Pindah ke scene berikutnya
        
        SceneManager.LoadScene("Map");
    }
}