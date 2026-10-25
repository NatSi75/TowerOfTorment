using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicSystem : Singleton<RelicSystem>
{
    [SerializeField] private RelicsUI relicsUI;
    private readonly List<Relic> relics = new();
    public void AddRelic(Relic relic)
    {
        relics.Add(relic);
        relicsUI.AddRelicUI(relic);
    }
    public void RemoveRelic(Relic relic)
    {
        relics.Remove(relic);
        relicsUI.RemoveRelicUI(relic);
    }
}
