using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour, ITooltipContent
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text stackCountText;
    private StatusEffectType type;
    private int stacks;

    public string TooltipTitle => StatusEffectInfo.GetName(type);
    public string TooltipDescription => StatusEffectInfo.GetDescription(type, stacks);

    private void Awake()
    {
        if (GetComponent<TooltipTrigger>() == null) gameObject.AddComponent<TooltipTrigger>();
    }

    public void Set(Sprite sprite, int stackCount, StatusEffectType statusEffectType)
    {
        type = statusEffectType;
        stacks = stackCount;
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
