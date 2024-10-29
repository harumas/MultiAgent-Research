using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;
using UnityEngine;
using Visualizer;
using Visualizer.MapEditor;

namespace MainModule
{
    public class Starter : MonoBehaviour
    {
        [Header("インスタンス")] [SerializeField] private GridGraphMediator mediator;
        [SerializeField] private MapVisualizer visualizer;
        [SerializeField] private GameObject agentPrefab;
        [SerializeField] private Transform agentParent;

        private bool isInitialized;
        private SampleAlgorithm solver;
        private Agent player;
        private Agent enemy;
        private Vector2Int currentGoal;
        private Vector2Int halfGoal;

        private void Start()
        {
            mediator.Initialize();
            isInitialized = true;

            if (!isInitialized)
            {
                Debug.LogError("初期化に失敗しました");
                return;
            }

            //ビジュアライザーの初期化
            MapData mapData = mediator.GetMapData();
            visualizer.Create(mapData.Width, mapData.Height);

            // グラフを構築する
            Graph graph = mediator.ConstructGraph();
            solver = CreateSolver(graph);

            currentGoal = mapData.Player;
            CreateAgents(mapData.Player, mapData.Enemy);
            AssignHalfGoal();
            Solve(mapData);
        }


        // 幅優先探索で障害物を避け、最も近い通行可能なセルを見つける関数
        private Vector2Int? FindNearestGoal(GridType[,] grid, Vector2Int point)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // 探索用のキューと探索済みの座標を記録するセット
            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            // 初期位置をキューに追加し、訪問済みとしてマーク
            queue.Enqueue(point);
            visited.Add(point);

            // 移動可能な方向（上下左右）
            var direction = new[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1)
            };

            // 幅優先探索
            while (queue.Count > 0)
            {
                Vector2Int p = queue.Dequeue();

                // 通行可能なセルを見つけた場合
                if ((grid[p.y, p.x] & GridType.Obstacle) == 0)
                {
                    return p;
                }

                // 隣接するセルを探索
                foreach (var d in direction)
                {
                    Vector2Int n = p + d;

                    // グリッド範囲内かつ未探索である場合
                    if (n.x >= 0 && n.x < rows && n.y >= 0 && n.y < cols && !visited.Contains(n))
                    {
                        visited.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }

            // 通行可能なセルが見つからない場合
            return null;
        }

        private void AssignHalfGoal()
        {
            GridType[,] grid = mediator.GetMapData().Grids;
            Vector2Int goal = currentGoal;

            Vector2 bullet = enemy.Position + (Vector2)(goal - enemy.Position) * 0.5f;
            Vector2Int point = new Vector2Int(Mathf.RoundToInt(bullet.x), Mathf.RoundToInt(bullet.y));

            if ((grid[point.y, point.x] & GridType.Obstacle) == 0)
            {
                halfGoal = point;
            }
            else
            {
                Vector2Int? nearestGoal = FindNearestGoal(mediator.GetMapData().Grids, point);


                if (nearestGoal != null)
                {
                    if (nearestGoal == player.Position)
                    {
                        nearestGoal = goal;
                    }

                    halfGoal = nearestGoal.Value;
                }
                else
                {
                    halfGoal = goal;
                }
            }
        }

        private SampleAlgorithm CreateSolver(Graph graph)
        {
            // 座標からアルゴリズムで使用するノードを作成
            List<Node> nodes = new List<Node>(graph.NodeCount);

            for (int i = 0; i < graph.NodeCount; i++)
            {
                Vector2Int pos = mediator.GetPos(i);
                nodes.Add(new Node(i, new System.Numerics.Vector2(pos.x, pos.y)));
            }

            return new SampleAlgorithm(graph, nodes);
        }

        private void Solve(MapData mapData)
        {
            // 経路探索を実行
            var result = solver.Solve(mediator.GetNode(enemy.Position), mediator.GetNode(halfGoal));
            PaintPath(mapData.Grids, result);
            PaintCircle(mapData.Grids, enemy.Position, 4);

            enemy.SetWaypoints(result.Select(node => mediator.GetPos(node)).ToList());
        }

        /// <summary>
        /// Solverに渡すContextを作成します
        /// </summary>
        /// <returns></returns>
        private void CreateAgents(Vector2Int playerPos, Vector2Int enemyPos)
        {
            player = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
            player.Initialize(true, playerPos);

            enemy = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
            enemy.Initialize(false, enemyPos);

            halfGoal = currentGoal;
        }

        private void UpdateContexts()
        {
            player.SetWaypoints(new List<Vector2Int>(1) { currentGoal });
        }

        private void PaintPath(GridType[,] grids, List<int> result)
        {
            RemoveBitData(grids, GridType.Path);

            foreach (Vector2Int pos in result.Select(index => mediator.GetPos(index)))
            {
                grids[pos.y, pos.x] |= GridType.Path;
            }
        }

        private void PaintCircle(GridType[,] grids, Vector2Int center, int radius)
        {
            RemoveBitData(grids, GridType.Circle);

            Vector2Int pos = Vector2Int.zero;
            int d = 0;

            d = 3 - 2 * radius;
            pos.y = radius;

            SetCirclePoint(grids, center.x, radius + center.y);
            SetCirclePoint(grids, center.x, -radius + center.y);
            SetCirclePoint(grids, radius + center.x, center.y);
            SetCirclePoint(grids, -radius + center.x, center.y);

            for (pos.x = 0; pos.x <= pos.y; pos.x++)
            {
                if (d < 0)
                {
                    d += 6 + 4 * pos.x;
                }
                else
                {
                    d += 10 + 4 * pos.x - 4 * pos.y--;
                }

                SetCirclePoint(grids, pos.y + center.x, pos.x + center.y);
                SetCirclePoint(grids, pos.x + center.x, pos.y + center.y);
                SetCirclePoint(grids, -pos.x + center.x, pos.y + center.y);
                SetCirclePoint(grids, -pos.y + center.x, pos.x + center.y);
                SetCirclePoint(grids, -pos.y + center.x, -pos.x + center.y);
                SetCirclePoint(grids, -pos.x + center.x, -pos.y + center.y);
                SetCirclePoint(grids, pos.x + center.x, -pos.y + center.y);
                SetCirclePoint(grids, pos.y + center.x, -pos.x + center.y);
            }
        }

        private void SetCirclePoint(GridType[,] grids, int x, int y)
        {
            if (x < 0 || x >= grids.GetLength(1) || y < 0 || y >= grids.GetLength(0))
            {
                return;
            }

            grids[y, x] |= GridType.Circle;
        }

        private void RemoveBitData(GridType[,] grids, GridType type)
        {
            for (int y = 0; y < grids.GetLength(0); y++)
            {
                for (int x = 0; x < grids.GetLength(1); x++)
                {
                    grids[y, x] &= ~type;
                }
            }
        }

        private ObstacleMapConvertJob convertJob = new ObstacleMapConvertJob();

        private void MovePlayerAgent()
        {
            Vector2Int input = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                input = Vector2Int.left;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                input = Vector2Int.right;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                input = Vector2Int.up;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                input = Vector2Int.down;
            }

            Vector2Int goal = currentGoal + input;
            MapData mapData = mediator.GetMapData();

            if ((mapData.Grids[goal.y, goal.x] & GridType.Obstacle) == GridType.Obstacle)
            {
                return;
            }

            if (input != Vector2Int.zero)
            {
                currentGoal += input;
            }

            AssignHalfGoal();
            UpdateContexts();
            Solve(mapData);
        }

        private void Update()
        {
            MovePlayerAgent();

            MapData mapData = mediator.GetMapData();
            visualizer.UpdateMap(mapData.Grids, convertJob);
        }
    }
}