using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PerkChoiceUI : MonoBehaviour
{
    [SerializeField] private Image perkImage;
    [SerializeField] private TextMeshProUGUI perkTitleText;
    [SerializeField] private TextMeshProUGUI perkDescText;
    [SerializeField] private Button selectButton;

    public Button SelectButton => selectButton;

    public void Setup(PerkData perkData)
    {
        if (perkImage != null) perkImage.sprite = perkData.Image;
        if (perkTitleText != null) perkTitleText.text = perkData.Name;
        if (perkDescText != null) perkDescText.text = perkData.Description;
    }
}