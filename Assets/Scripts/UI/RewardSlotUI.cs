using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardSlotUI : MonoBehaviour
{
    [Tooltip("Masukkan komponen Image (kotak putih di UI)")]
    public Image itemIcon;

    [Tooltip("Masukkan komponen TextMeshPro (tulisan 'Obtain' di UI)")]
    public TMP_Text itemDescription;

    public void SetupSlot(Sprite icon, string description)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.color = Color.white;
        }

        if (itemDescription != null)
        {
            itemDescription.text = description;
        }

        gameObject.SetActive(true);
    }

    public void HideSlot()
    {
        gameObject.SetActive(false);
    }
}