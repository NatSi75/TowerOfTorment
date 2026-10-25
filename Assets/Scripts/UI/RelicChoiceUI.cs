using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicChoiceUI : MonoBehaviour
{
    [SerializeField] private Image relicImage;
    [SerializeField] private TextMeshProUGUI relicTitleText;
    [SerializeField] private TextMeshProUGUI relicDescText;
    [SerializeField] private Button selectButton;

    public Button SelectButton => selectButton;

    public void Setup(RelicData relicData)
    {
        if (relicImage != null) relicImage.sprite = relicData.Image;
        if (relicTitleText != null) relicTitleText.text = relicData.Name;
        if (relicDescText != null) relicDescText.text = relicData.Description;
    }
}
