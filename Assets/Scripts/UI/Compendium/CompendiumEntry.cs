using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompendiumEntry : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private Image icon;

    private CompendiumPanel panel;
    private string title;
    private string description;

    public void Setup(CompendiumPanel owner, Sprite sprite, string entryTitle, string entryDescription)
    {
        panel = owner;
        title = entryTitle;
        description = entryDescription;
        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.enabled = sprite != null;
    }

    public void OnPointerEnter(PointerEventData eventData) => panel.ShowDetail(title, description);

    public void OnPointerClick(PointerEventData eventData) => panel.ShowDetail(title, description);
}
