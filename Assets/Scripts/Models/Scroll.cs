using System.Collections.Generic;
using UnityEngine;

public class Scroll
{
    public Sprite Image => data.Image;
    public readonly ScrollData data;
    public List<Effect> ManualTargetEffect => data.ManualTargetEffect;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    public Scroll(ScrollData scrollData)
    {
        data = scrollData;
    }
}
