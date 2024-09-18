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
        private List<Agent> moveAgents;
        private Vector2Int currentGoal;
        private List<Vector2Int> halfGoals = new List<Vector2Int>();
        private List<AgentContext> contexts;

        private void Start()
        {
            isInitialized = mediator.Initialize();

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

            currentGoal = mapData.Goal;
            CreateAgents(mapData.Agents);
            AssignHalfGoal();
            contexts = CreateContexts(mapData.Agents);
            Solve(mapData);
        }


        // 幅優先探索で障害物を避け、最も近い通行可能なセルを見つける関数
        private Vector2Int? FindNearestGoal(GridType[,] grid, Vector2Int point)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // 探索用のキューと探索済みの座標を記録するセット
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            // 初期位置をキューに追加し、訪問済みとしてマーク
            queue.Enqueue(point);
            visited.Add(point);

            // 移動可能な方向（上下左右）
            Vector2Int[] direction = new[]
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
            
            for (var i = 0; i < moveAgents.Count; i++)
            {
                var agent = moveAgents[i];
                Vector2 bullet = agent.Position + (Vector2)(goal - agent.Position) * 0.5f;
                Vector2Int point = new Vector2Int(Mathf.RoundToInt(bullet.x), Mathf.RoundToInt(bullet.y));

                if ((grid[point.y, point.x] & GridType.Obstacle) == 0)
                {
                    halfGoals[i] = point;
                }
                else
                {
                    Vector2Int? nearestGoal = FindNearestGoal(mediator.GetMapData().Grids, point);


                    if (nearestGoal != null)
                    {
                        if (nearestGoal == agent.Position)
                        {
                            nearestGoal = goal;
                        }

                        halfGoals[i] = nearestGoal.Value;
                    }
                    else
                    {
                        halfGoals[i] = goal;
                    }
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
            var result = solver.Solve(contexts);
            PaintPath(mapData.Grids, result);

            foreach ((int agentIndex, List<int> path) in result)
            {
                moveAgents[agentIndex].SetWaypoints(path.Select(node => mediator.GetPos(node)).ToList());
            }
        }

        /// <summary>
        /// Solverに渡すContextを作成します
        /// </summary>
        /// <returns></returns>
        private void CreateAgents(IReadOnlyList<Vector2Int> agentPoints)
        {
            moveAgents = new List<Agent>(agentPoints.Count);

            player = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
            player.Initialize(-1, currentGoal);

            for (var index = 0; index < agentPoints.Count; index++)
            {
                var point = agentPoints[index];
                //エージェントのオブジェクトを生成
                Agent moveAgent = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
                moveAgent.Initialize(index, point);

                moveAgents.Add(moveAgent);
                halfGoals.Add(currentGoal);
            }
        }

        private List<AgentContext> CreateContexts(IReadOnlyList<Vector2Int> agentPoints)
        {
            List<AgentContext> contexts = new List<AgentContext>(agentPoints.Count);

            for (var i = 0; i < moveAgents.Count; i++)
            {
                contexts.Add(new AgentContext(i, mediator.GetNode(agentPoints[i]), mediator.GetNode(halfGoals[i])));       
            }

            return contexts;
        }

        private void UpdateContexts(List<AgentContext> contexts)
        {
            player.SetWaypoints(new List<Vector2Int>(1) { currentGoal });

            for (var i = 0; i < moveAgents.Count; i++)
            {
                contexts[i] = new AgentContext(contexts[i].AgentIndex, mediator.GetNode(moveAgents[i].Position), mediator.GetNode(halfGoals[i]));
            }
        }

        private void PaintPath(GridType[,] grids, List<(int agentIndex, List<int> path)> result)
        {
            RemovePathData(grids);

            foreach (List<int> path in result.Select(item => item.path))
            {
                foreach (int index in path)
                {
                    Vector2Int pos = mediator.GetPos(index);
                    grids[pos.y, pos.x] |= GridType.Path;
                }
            }
        }

        private void RemovePathData(GridType[,] grids)
        {
            for (int y = 0; y < grids.GetLength(0); y++)
            {
                for (int x = 0; x < grids.GetLength(1); x++)
                {
                    grids[y, x] &= ~GridType.Path;
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
                AssignHalfGoal();
                UpdateContexts(contexts);
                Solve(mapData);
            }
        }

        private void Update()
        {
            MovePlayerAgent();

            MapData mapData = mediator.GetMapData();
            visualizer.UpdateMap(mapData.Grids, convertJob);
        }
    }
}