using UnityEngine;

public class Relic
{
    public Sprite Image => data.Image;
    public string Name => data.Name;
    public string Description => data.Description;
    private readonly RelicData data;
    public Relic(RelicData relicData)
    {
        data = relicData;
    }
}
