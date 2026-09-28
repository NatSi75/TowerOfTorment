using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour, ITooltipContent
{
    [SerializeField] private Image image;
    public Perk Perk {  get; private set; }
    public string TooltipTitle => Perk?.Name;
    public string TooltipDescription => Perk?.Description;

    private void Awake()
    {
        if (GetComponent<TooltipTrigger>() == null) gameObject.AddComponent<TooltipTrigger>();
    }

    public void Setup(Perk perk)
    {
        Perk = perk;
        image.sprite = perk.Image;
    }
}
