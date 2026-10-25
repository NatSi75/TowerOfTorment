using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour, ITooltipContent
{
    [SerializeField] private Image image;
    public Relic Relic {  get; private set; }
    public string TooltipTitle => Relic?.Name;
    public string TooltipDescription => Relic?.Description;

    private void Awake()
    {
        if (GetComponent<TooltipTrigger>() == null) gameObject.AddComponent<TooltipTrigger>();
    }

    public void Setup(Relic relic)
    {
        Relic = relic;
        image.sprite = relic.Image;
    }
}
