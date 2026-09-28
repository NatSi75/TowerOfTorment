using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrollsUI : MonoBehaviour
{
    [SerializeField] private ScrollUI scrollUIPrefab;
    private readonly List<ScrollUI> scrollUIs = new();
    public void AddScrollUI(Scroll scroll)
    {
        ScrollUI scrollUI = Instantiate(scrollUIPrefab, transform);
        scrollUI.Setup(scroll);
        scrollUIs.Add(scrollUI);
    }
    public void RemoveScrollUI(Scroll scroll)
    {
        ScrollUI scrollUI = scrollUIs.Where(pui => pui.Scroll == scroll).FirstOrDefault();
        if (scrollUI != null)
        {
            scrollUIs.Remove(scrollUI);
            Destroy(scrollUI.gameObject);
        }
    }
}
