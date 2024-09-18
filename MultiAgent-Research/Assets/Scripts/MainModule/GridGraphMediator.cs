using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Visualizer.MapEditor;

namespace Visualizer
{
    /// <summary>
    /// シーン上のグリッドとグラフを仲介するクラス
    /// </summary>
    public class GridGraphMediator : MonoBehaviour
    {
        [SerializeField] private MapDataManager mapDataManager;

        private MapData mapData;
        private Dictionary<int, int> nodeIndexList;
        private Dictionary<int, int> indexNodeList;
        private Graph graph;

        private readonly Vector2Int[] direction = new[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            
            // 斜め移動
            // new Vector2Int(1, 1),
            // new Vector2Int(-1, -1),
            // new Vector2Int(-1, 1),
            // new Vector2Int(1, -1),
        };

        public bool Initialize()
        {
            mapData = mapDataManager.Load();
            nodeIndexList = CreateNodeIndexList();
            indexNodeList = nodeIndexList.ToDictionary(x => x.Value, x => x.Key);
            return ValidateEndPoints();
        }

        public MapData GetMapData()
        {
            return mapData;
        }

        public Graph ConstructGraph()
        {
            graph = new Graph(mapData.PassableCount);

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    GridType from = mapData.Grids[y, x];

                    //障害物のバスならNodeを作らない
                    if (from == GridType.Obstacle)
                    {
                        continue;
                    }

                    //４方向にEdgeをつなぐ
                    foreach (Vector2Int dir in direction)
                    {
                        Vector2Int pos = new Vector2Int(x, y) + dir;
                        bool isConnectable = 0 <= pos.x && pos.x < mapData.Width && 0 <= pos.y && pos.y < mapData.Height;

                        if (!isConnectable)
                        {
                            continue;
                        }

                        GridType to = mapData.Grids[pos.y, pos.x];

                        if (to == GridType.Road)
                        {
                            int fromIndex = y * mapData.Width + x;
                            int toIndex = pos.y * mapData.Width + pos.x;
                            graph.AddEdge(GetNode(fromIndex), GetNode(toIndex));
                        }
                    }
                }
            }

            return graph;
        }

        public int GetNode(Vector2Int pos)
        {
            int index = pos.y * mapData.Width + pos.x;
            return indexNodeList[index];
        }

        public Vector2Int GetPos(int node)
        {
            int index = nodeIndexList[node];
            return new Vector2Int(index % mapData.Width, index / mapData.Width);
        }

        private int GetNode(int index)
        {
            return indexNodeList[index];
        }

        private int GetIndex(int node)
        {
            return nodeIndexList[node];
        }

        private bool ValidateEndPoints()
        {
            var agents = mapData.Agents;
            bool isUniqueStarts = !agents.GroupBy(p => p).SelectMany(g => g.Skip(1)).Any();

            if (!isUniqueStarts)
            {
                Debug.LogError("スタートのデータが重複しています。");
            }

            return isUniqueStarts;
        }

        private Dictionary<int, int> CreateNodeIndexList()
        {
            var list = new Dictionary<int, int>();

            int nodeCount = 0;
            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    GridType gridType = mapData.Grids[y, x];

                    if (gridType == GridType.Road)
                    {
                        list.Add(nodeCount++, y * mapData.Width + x);
                    }
                }
            }

            return list;
        }
    }
}