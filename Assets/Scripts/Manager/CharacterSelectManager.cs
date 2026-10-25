using TMPro;
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

    // 0 = Knight, 1 = Wizard
    private int selectedIndex = 0;

    private void Start()
    {
        UpdateCharacterDisplay();
    }

    public void NextCharacter()
    {
        selectedIndex++;
        if (selectedIndex >= characters.Length)
        {
            selectedIndex = 0;
        }
        UpdateCharacterDisplay();
    }

    public void PreviousCharacter()
    {
        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = characters.Length - 1;
        }
        UpdateCharacterDisplay();
    }

    private void UpdateCharacterDisplay()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(false);
        }

        characters[selectedIndex].SetActive(true);
        relic.sprite = charactersData[selectedIndex].RelicHero.Image;
        relicDescription.text = charactersData[selectedIndex].RelicHero.Description;
        nameCharacter.text = charactersData[selectedIndex].name;
        hp.text = "HP: " + charactersData[selectedIndex].Health;
        torment.text = "Max Torment: " + charactersData[selectedIndex].MaxTorment;
        gold.text = "Gold: 99";
        description.text = charactersData[selectedIndex].Description;
    }

    public void ConfirmSelection()
    {
        PlayerPrefs.SetInt("SelectedHeroIndex", selectedIndex);
        PlayerPrefs.DeleteKey("Map");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Map");
    }
}
