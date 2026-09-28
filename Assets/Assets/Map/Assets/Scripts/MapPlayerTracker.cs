using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Map
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;
        public GameObject closeMapButton;

        public bool Locked { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if(TopBarManager.isViewOnlyMode)
            {
                MapPlayerTracker.Instance.Locked = true;
                if (closeMapButton != null) closeMapButton.SetActive(true);
            }
            else
            {
                if (closeMapButton != null) closeMapButton.SetActive(false);
                MapPlayerTracker.Instance.Locked = false;
            }
        }

        public void SelectNode(MapNode mapNode)
        {
            if (Locked) return;
            if (mapManager.CurrentMap.path.Count == 0)
            {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (mapNode.Node.point.y == 0)
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
            else
            {
                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point)))
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
        }

        private void SendPlayerToNode(MapNode mapNode)
        {
            Locked = lockAfterSelecting;
            mapManager.CurrentMap.path.Add(mapNode.Node.point);
            mapManager.SaveMap();
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
        }

        private static void EnterNode(MapNode mapNode)
        {
            // we have access to blueprint name here as well
            Debug.Log("Entering node: " + mapNode.Node.blueprintName + " of type: " + mapNode.Node.nodeType);
            // load appropriate scene with context based on nodeType:
            // or show appropriate GUI over the map: 
            // if you choose to show GUI in some of these cases, do not forget to set "Locked" in MapPlayerTracker back to false
            switch (mapNode.Node.nodeType)
            {
                case NodeType.MinorEnemy:
                    if (GameDataManager.Instance != null)
                    {
                        GameDataManager.Instance.SetSequentialMinorEnemy();
                    }
                    SceneManager.LoadScene("Battle");
                    break;
                case NodeType.EliteEnemy:
                    if (GameDataManager.Instance != null)
                    {
                        GameDataManager.Instance.SetEliteEnemy();
                    }
                    SceneManager.LoadScene("Battle");
                    break;
                case NodeType.PillarOfDespair:
                    SceneManager.LoadScene("Pillar of Despair");
                    break;
                case NodeType.Treasure:
                    SceneManager.LoadScene("Treasure");
                    break;
                case NodeType.Store:
                    SceneManager.LoadScene("Shop");
                    break;
                case NodeType.RestSite:
                    SceneManager.LoadScene("RestSite");
                    break;
                case NodeType.Boss:
                    if (GameDataManager.Instance != null)
                    {
                        GameDataManager.Instance.SetBossEnemy();
                    }
                    SceneManager.LoadScene("Battle");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayWarningThatNodeCannotBeAccessed()
        {
            Debug.Log("Selected node cannot be accessed");
        }

        public void CloseViewOnlyMap()
        {
            TopBarManager.isViewOnlyMode = false;
            if (TopBarManager.hiddenBattleObjects != null)
            {
                foreach (GameObject obj in TopBarManager.hiddenBattleObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
                TopBarManager.hiddenBattleObjects = null;
            }
            GameObject outerMap = GameObject.Find("OuterMapParent");
            if (outerMap != null)
            {
                Destroy(outerMap);
            }
            GameObject topBarManagerObj = GameObject.Find("TopBar");
            if (topBarManagerObj != null)
            {
                TopBarManager topBarScript = topBarManagerObj.GetComponent<TopBarManager>();
                if (topBarScript != null && topBarScript.mapButton != null)
                {
                    topBarScript.mapButton.interactable = true;
                    topBarScript.mapButton.gameObject.SetActive(true);
                }
            }
            SceneManager.UnloadSceneAsync("Map");
        }
    }
}