using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Map
{
    public class MapView : MonoBehaviour
    {
        public enum MapOrientation
        {
            BottomToTop,
            TopToBottom,
            RightToLeft,
            LeftToRight
        }

        public MapManager mapManager;
        public MapOrientation orientation;

        [Tooltip(
            "List of all the MapConfig scriptable objects from the Assets folder that might be used to construct maps. " +
            "Similar to Acts in Slay The Spire (define general layout, types of bosses.)")]
        public List<MapConfig> allMapConfigs;
        public GameObject nodePrefab;
        [Tooltip("Offset of the start/end nodes of the map from the edges of the screen")]
        public float orientationOffset;
        [Header("Background Settings")]
        [Tooltip("If the background sprite is null, background will not be shown")]
        public Sprite background;
        public Color32 backgroundColor = Color.white;
        public float xSize;
        public float yOffset;
        [Header("Line Settings")]
        public GameObject linePrefab;
        [Tooltip("Line point count should be > 2 to get smooth color gradients")]
        [Range(3, 10)]
        public int linePointsCount = 10;
        [Tooltip("Distance from the node till the line starting point")]
        public float offsetFromNodes = 0.5f;
        [Header("Colors")]
        [Tooltip("Node Visited or Attainable color")]
        public Color32 visitedColor = Color.white;
        [Tooltip("Locked node color")]
        public Color32 lockedColor = Color.gray;
        [Tooltip("Visited or available path color")]
        public Color32 lineVisitedColor = Color.white;
        [Tooltip("Unavailable path color")]
        public Color32 lineLockedColor = Color.gray;
        [Tooltip("Color of the nodes the player can choose next (they pulse to this color)")]
        public Color32 attainableColor = new Color32(255, 200, 70, 255);
        [Tooltip("Color of the paths the player can take next")]
        public Color32 lineAttainableColor = new Color32(255, 200, 70, 255);

        protected GameObject firstParent;
        protected GameObject mapParent;
        private List<List<Vector2Int>> paths;
        private Camera cam;
        public readonly List<MapNode> MapNodes = new List<MapNode>();
        protected readonly List<LineConnection> lineConnections = new List<LineConnection>();

        public static MapView Instance;

        public Map Map { get; protected set; }

        private void Awake()
        {
            Instance = this;
            cam = Camera.main;
        }

        protected virtual void ClearMap()
        {
            if (firstParent != null)
                Destroy(firstParent);

            MapNodes.Clear();
            lineConnections.Clear();
        }

        public virtual void ShowMap(Map m)
        {
            if (m == null)
            {
                Debug.LogWarning("Map was null in MapView.ShowMap()");
                return;
            }

            Map = m;

            ClearMap();

            CreateMapParent();

            CreateNodes(m.nodes);

            DrawLines();

            SetOrientation();

            ResetNodesRotation();

            SetAttainableNodes();

            SetLineColors();

            CreateMapBackground(m);

            FocusCurrentNode();
        }

        protected virtual void FocusCurrentNode()
        {
            MapNode current = CurrentMapNode();
            if (current == null || mapParent == null) return;
            ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
            if (scrollNonUi == null || scrollNonUi.freezeY) return;
            float targetY = cam.transform.position.y - cam.orthographicSize * 0.3f;
            mapParent.transform.position += new Vector3(0f, targetY - current.transform.position.y, 0f);
        }

        private MapNode CurrentMapNode()
        {
            if (mapManager.CurrentMap == null || mapManager.CurrentMap.path.Count == 0) return null;
            return GetNode(mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1]);
        }

        [Header("Player Marker")]
        [Tooltip("Colour of the arrow that points at the node the player is on")]
        public Color playerMarkerColor = new Color(1f, 0.8f, 0.25f);
        private GameObject playerMarker;
        private static Sprite markerSprite;

        protected virtual void UpdatePlayerMarker()
        {
            if (playerMarker != null) Destroy(playerMarker);
            MapNode current = CurrentMapNode();
            if (current == null) return;

            Bounds nodeBounds = current.sr != null ? current.sr.bounds : new Bounds(current.transform.position, Vector3.one);
            float size = Mathf.Max(nodeBounds.size.x, nodeBounds.size.y) * 0.6f;

            playerMarker = new GameObject("Player Marker");
            playerMarker.transform.SetParent(current.transform, true);
            playerMarker.transform.position = new Vector3(nodeBounds.center.x, nodeBounds.max.y + size * 0.6f, current.transform.position.z);
            playerMarker.transform.rotation = Quaternion.identity;
            float parentScale = Mathf.Max(0.0001f, current.transform.lossyScale.x);
            playerMarker.transform.localScale = Vector3.one * (size / parentScale);

            SpriteRenderer marker = playerMarker.AddComponent<SpriteRenderer>();
            marker.sprite = MarkerSprite();
            marker.color = playerMarkerColor;
            marker.sortingOrder = 150;

            float bob = size * 0.25f / parentScale;
            playerMarker.transform.DOLocalMoveY(playerMarker.transform.localPosition.y + bob, 0.5f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(playerMarker);
        }

        private static Sprite MarkerSprite()
        {
            if (markerSprite != null) return markerSprite;
            const int size = 64;
            const float outline = 6f;
            Vector2 a = new Vector2(4f, 60f), b = new Vector2(60f, 60f), c = new Vector2(32f, 6f);
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            Color dark = new Color(0.12f, 0.06f, 0.03f, 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Mathf.Min(EdgeDistance(a, b, p), Mathf.Min(EdgeDistance(b, c, p), EdgeDistance(c, a, p)));
                    Color color = d > outline ? Color.white : dark;
                    if (d > outline && d < outline + 1f) color = Color.Lerp(dark, Color.white, d - outline);
                    color.a = Mathf.Clamp01(d);
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            markerSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return markerSprite;
        }

        private static float EdgeDistance(Vector2 from, Vector2 to, Vector2 p)
        {
            Vector2 edge = to - from;
            return (edge.x * (p.y - from.y) - edge.y * (p.x - from.x)) / -edge.magnitude;
        }

        protected virtual void CreateMapBackground(Map m)
        {
            MapConfig config = GetConfig(m.configName);
            Sprite sprite = config != null && config.background != null ? config.background : background;
            if (sprite == null) return;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(mapParent.transform);
            float span = m.DistanceBetweenFirstAndLastLayers();
            backgroundObject.transform.localPosition = new Vector3(0f, span / 2f, 0f);
            backgroundObject.transform.localRotation = Quaternion.identity;
            SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
            sr.color = backgroundColor;
            sr.sprite = sprite;
            float height = span + yOffset * 2f;
            if (sprite.border != Vector4.zero)
            {
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(xSize, height);
            }
            else
            {
                sr.drawMode = SpriteDrawMode.Simple;
                Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
                float viewWidth = cam.orthographic ? cam.orthographicSize * 2f * cam.aspect : xSize;
                float scale = Mathf.Max(height / spriteSize.y, Mathf.Max(xSize, viewWidth) / spriteSize.x);
                backgroundObject.transform.localScale = new Vector3(scale, scale, 1f);
            }

            ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
            if (scrollNonUi != null) scrollNonUi.boundsRenderer = sr;
        }

        protected virtual void CreateMapParent()
        {
            firstParent = new GameObject("OuterMapParent");
            mapParent = new GameObject("MapParentWithAScroll");
            mapParent.transform.SetParent(firstParent.transform);
            ScrollNonUI scrollNonUi = mapParent.AddComponent<ScrollNonUI>();
            scrollNonUi.freezeX = orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom;
            scrollNonUi.freezeY = orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft;
            BoxCollider boxCollider = mapParent.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(100, 100, 1);
        }

        protected void CreateNodes(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                MapNode mapNode = CreateMapNode(node);
                MapNodes.Add(mapNode);
            }
        }

        protected virtual MapNode CreateMapNode(Node node)
        {
            GameObject mapNodeObject = Instantiate(nodePrefab, mapParent.transform);
            MapNode mapNode = mapNodeObject.GetComponent<MapNode>();
            NodeBlueprint blueprint = GetBlueprint(node.blueprintName);
            mapNode.SetUp(node, blueprint);
            mapNode.transform.localPosition = node.position;
            return mapNode;
        }

        public void SetAttainableNodes()
        {
            foreach (MapNode node in MapNodes)
                node.SetState(NodeStates.Locked);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                foreach (MapNode node in MapNodes.Where(n => n.Node.point.y == 0))
                    node.SetState(NodeStates.Attainable);
            }
            else
            {
                foreach (Vector2Int point in mapManager.CurrentMap.path)
                {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Visited);
                }

                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                foreach (Vector2Int point in currentNode.outgoing)
                {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Attainable);
                }
            }

            UpdatePlayerMarker();
        }

        public virtual void SetLineColors()
        {
            foreach (LineConnection connection in lineConnections)
                connection.SetColor(lineLockedColor);

            if (mapManager.CurrentMap.path.Count == 0)
                return;

            Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
            Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

            foreach (Vector2Int point in currentNode.outgoing)
            {
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.from.Node == currentNode &&
                                                                            conn.to.Node.point.Equals(point));
                lineConnection?.SetColor(lineAttainableColor);
            }

            if (mapManager.CurrentMap.path.Count <= 1) return;

            for (int i = 0; i < mapManager.CurrentMap.path.Count - 1; i++)
            {
                Vector2Int current = mapManager.CurrentMap.path[i];
                Vector2Int next = mapManager.CurrentMap.path[i + 1];
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.@from.Node.point.Equals(current) &&
                                                                            conn.to.Node.point.Equals(next));
                lineConnection?.SetColor(lineVisitedColor);
            }
        }

        protected virtual void SetOrientation()
        {
            ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
            float span = mapManager.CurrentMap.DistanceBetweenFirstAndLastLayers();
            MapNode bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            Debug.Log("Map span in set orientation: " + span + " camera aspect: " + cam.aspect);

            firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            float offset = orientationOffset;
            switch (orientation)
            {
                case MapOrientation.BottomToTop:
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.yConstraints.max = 0;
                        scrollNonUi.yConstraints.min = -(span + 2f * offset);
                    }
                    firstParent.transform.localPosition += new Vector3(0, offset, 0);
                    break;
                case MapOrientation.TopToBottom:
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.yConstraints.min = 0;
                        scrollNonUi.yConstraints.max = span + 2f * offset;
                    }
                    firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                    break;
                case MapOrientation.RightToLeft:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                    firstParent.transform.localPosition -= new Vector3(offset, bossNode.transform.position.y, 0);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.xConstraints.max = span + 2f * offset;
                        scrollNonUi.xConstraints.min = 0;
                    }
                    break;
                case MapOrientation.LeftToRight:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                    firstParent.transform.localPosition += new Vector3(offset, -bossNode.transform.position.y, 0);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.xConstraints.max = 0;
                        scrollNonUi.xConstraints.min = -(span + 2f * offset);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawLines()
        {
            foreach (MapNode node in MapNodes)
            {
                foreach (Vector2Int connection in node.Node.outgoing)
                    AddLineConnection(node, GetNode(connection));
            }
        }

        private void ResetNodesRotation()
        {
            foreach (MapNode node in MapNodes)
                node.transform.rotation = Quaternion.identity;
        }

        protected virtual void AddLineConnection(MapNode from, MapNode to)
        {
            if (linePrefab == null) return;

            GameObject lineObject = Instantiate(linePrefab, mapParent.transform);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            Vector3 fromPoint = from.transform.position +
                                (to.transform.position - from.transform.position).normalized * offsetFromNodes;

            Vector3 toPoint = to.transform.position +
                              (from.transform.position - to.transform.position).normalized * offsetFromNodes;

            lineObject.transform.position = fromPoint;
            lineRenderer.useWorldSpace = false;

            lineRenderer.positionCount = linePointsCount;
            for (int i = 0; i < linePointsCount; i++)
            {
                lineRenderer.SetPosition(i,
                    Vector3.Lerp(Vector3.zero, toPoint - fromPoint, (float)i / (linePointsCount - 1)));
            }

            DottedLineRenderer dottedLine = lineObject.GetComponent<DottedLineRenderer>();
            if (dottedLine != null) dottedLine.ScaleMaterial();

            lineConnections.Add(new LineConnection(lineRenderer, null, from, to));
        }

        protected MapNode GetNode(Vector2Int p)
        {
            return MapNodes.FirstOrDefault(n => n.Node.point.Equals(p));
        }

        protected MapConfig GetConfig(string configName)
        {
            return allMapConfigs.FirstOrDefault(c => c.name == configName);
        }

        protected NodeBlueprint GetBlueprint(NodeType type)
        {
            MapConfig config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.nodeType == type);
        }

        protected NodeBlueprint GetBlueprint(string blueprintName)
        {
            Debug.Log(mapManager.CurrentMap.configName);
            MapConfig config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.name == blueprintName);
        }
    }
}
