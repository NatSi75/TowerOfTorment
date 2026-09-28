using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text stackCountText;

    public void Set(Sprite sprite, int stackCount, StatusEffectType statusEffectType)
    {
        image.sprite = sprite;
        if (statusEffectType == StatusEffectType.TORMENT)
        {
            stackCountText.alpha = 0;
        }
        else
        {
            stackCountText.alpha = 255;
        }
        stackCountText.text = stackCount.ToString();
    }
}
