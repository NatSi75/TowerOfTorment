// Implemented by objects whose tooltip text depends on their current state
// (status stacks, enemy intention, ...). Read every time the tooltip is shown or refreshed.
public interface ITooltipContent
{
    string TooltipTitle { get; }
    string TooltipDescription { get; }
}
