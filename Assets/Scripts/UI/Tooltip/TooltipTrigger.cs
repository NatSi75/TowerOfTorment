using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string title;
    [SerializeField, TextArea(2, 5)] private string description;

    private ITooltipContent content;

    private void Awake()
    {
        content = GetComponentInParent<ITooltipContent>();
    }

    public void SetContent(string newTitle, string newDescription)
    {
        title = newTitle;
        description = newDescription;
    }

    public void GetText(out string tooltipTitle, out string tooltipDescription)
    {
        content ??= GetComponentInParent<ITooltipContent>();
        tooltipTitle = content != null ? content.TooltipTitle : title;
        tooltipDescription = content != null ? content.TooltipDescription : description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Show(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide(this);
    }

    private void OnMouseEnter()
    {
        TooltipSystem.Show(this);
    }

    private void OnMouseExit()
    {
        TooltipSystem.Hide(this);
    }

    private void OnDisable()
    {
        TooltipSystem.Hide(this);
    }
}
