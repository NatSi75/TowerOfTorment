using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class ScrollUI : MonoBehaviour, ITooltipContent
{
    [SerializeField] private Image image;
    public Scroll Scroll { get; private set; }
    public string TooltipTitle => Scroll?.data.Name;
    public string TooltipDescription => Scroll?.data.Description;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private Vector3 dragOffset;

    private void Awake()
    {
        if (GetComponent<TooltipTrigger>() == null) gameObject.AddComponent<TooltipTrigger>();
    }

    public void Setup(Scroll scroll)
    {
        Scroll = scroll;
        image.sprite = scroll.Image;
    }

    public void UseScroll()
    {
        PlayScrollGA playScrollGA = new(Scroll);
        ActionSystem.Instance.Perform(playScrollGA);
        GameDataManager.Instance.RemoveScroll(Scroll.data);
        Destroy(gameObject);
    }
}
