using System.Collections.Generic;
using OneLine;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu]
    public class MapConfig : ScriptableObject
    {
        public List<NodeBlueprint> nodeBlueprints;
        [Tooltip("Background of this map. Leave empty to use the MapView background.")]
        public Sprite background;
        [Tooltip("Nodes that will be used on layers with Randomize Nodes > 0")]
        public List<NodeType> randomNodes = new List<NodeType>
            {NodeType.Mystery, NodeType.Store, NodeType.Treasure, NodeType.MinorEnemy, NodeType.RestSite, NodeType.PillarOfDespair};
        public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);

        [OneLineWithHeader]
        public IntMinMax numOfPreBossNodes;
        [OneLineWithHeader]
        public IntMinMax numOfStartingNodes;

        

        [Tooltip("Increase this number to generate more paths")]
        public int extraPaths;
        public List<MapLayer> layers;
    }
}