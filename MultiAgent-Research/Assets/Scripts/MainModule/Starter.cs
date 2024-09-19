using System;
using System.Collections.Generic;
using System.Linq;
using PathFinding.Algorithm;
using PathFinder.Core;
using PathFinding;
using UnityEngine;
using Visualizer;
using Vector2Int = PathFinder.Core.Vector2Int;

namespace MainModule
{
    public enum SolverType
    {
        MySolver,
        NormalAStar
    }

    public class Starter : MonoBehaviour
    {
        [SerializeField] private SolverType solverType;
        [SerializeField] private MapDataManager mapDataManager;
        [SerializeField] private AgentFactory agentFactory;
        [SerializeField] private MapVisualizer visualizer;

        private GridGraphMediator mediator;
        private ISolver solver;
        private Agent player;
        private List<Agent> moveAgents;
        private List<AgentContext> contexts;

        private void Start()
        {
            // マップデータの読み込み
            MapData mapData = mapDataManager.Load();

            if (!mapData.IsValid())
            {
                Debug.LogError("初期化に失敗しました");
                return;
            }


            mediator = new GridGraphMediator(mapData);

            //ビジュアライザーの初期化
            visualizer.Create(mapData.Width, mapData.Height);

            // グラフを構築する
            GraphConstructor constructor = new GraphConstructor(mapData, mediator);
            Graph graph = constructor.ConstructGraph();

            // ソルバーの作成
            solver = CreateSolver(graph, mapData.Grids);

            // エージェントの作成
            (player, moveAgents) = agentFactory.CreateAgents(mapData);

            // 解決
            contexts = CreateContexts(mapData.Agents, mapData.Goal);
            Solve(mapData.Grids);
        }

        private ISolver CreateSolver(Graph graph, GridType[,] grids)
        {
            switch (solverType)
            {
                case SolverType.MySolver:
                    return new SampleAlgorithm(graph, grids, mediator);
                case SolverType.NormalAStar:
                    return new NormalAStar(graph, mediator);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Solve(GridType[,] grids)
        {
            // 経路探索を実行
            var result = solver.Solve(contexts);

            // パスデータを書き込む
            PaintPath(grids, result);

            foreach ((int agentIndex, List<int> path) in result)
            {
                var waypoints = path.Select(node => mediator.GetPos(node)).ToList();
                moveAgents[agentIndex].SetWaypoints(waypoints);
            }
        }

        private List<AgentContext> CreateContexts(IReadOnlyList<Vector2Int> agentPoints, Vector2Int goal)
        {
            List<AgentContext> contexts = new List<AgentContext>(agentPoints.Count);

            for (var i = 0; i < moveAgents.Count; i++)
            {
                int agentNode = mediator.GetNode(agentPoints[i]);
                int goalNode = mediator.GetNode(goal);

                contexts.Add(new AgentContext(i, agentNode, goalNode));
            }

            return contexts;
        }

        private void UpdateContexts(List<AgentContext> contexts, Vector2Int goal)
        {
            player.SetWaypoints(new List<Vector2Int>(1) { goal });

            for (var i = 0; i < moveAgents.Count; i++)
            {
                int agentNode = mediator.GetNode(moveAgents[i].Position);
                int goalNode = mediator.GetNode(goal);

                contexts[i] = new AgentContext(contexts[i].AgentIndex, agentNode, goalNode);
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
            Vector2Int input = new Vector2Int(0, 0);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                input = new Vector2Int(-1, 0);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                input = new Vector2Int(1, 0);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                input = new Vector2Int(0, 1);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                input = new Vector2Int(0, -1);
            }

            Vector2Int goal = player.Position + input;
            GridType[,] grids = mapDataManager.CurrentMapData.Grids;

            if ((grids[goal.y, goal.x] & GridType.Obstacle) == GridType.Obstacle)
            {
                return;
            }

            if (input.x != 0 || input.y != 0)
            {
                UpdateContexts(contexts, goal);
                Solve(grids);
            }
        }

        private void Update()
        {
            MovePlayerAgent();

            GridType[,] mapData = mapDataManager.CurrentMapData.Grids;
            visualizer.UpdateMap(mapData, convertJob);
        }
    }
}