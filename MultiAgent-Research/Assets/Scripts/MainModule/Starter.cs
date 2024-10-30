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
        private Agent enemy;

        private void Start()
        {
            // マップデータの読み込み
            MapData mapData = mapDataManager.Load();
            mediator = new GridGraphMediator(mapData);

            //ビジュアライザーの初期化
            visualizer.Create(mapData.Width, mapData.Height);

            // グラフを構築する
            GraphConstructor constructor = new GraphConstructor(mapData, mediator);
            Graph graph = constructor.ConstructGraph();

            // ソルバーの作成
            solver = CreateSolver(graph, mapData.Grids);

            // エージェントの作成
            (player, enemy) = agentFactory.CreateAgents(mapData);

            // 解決
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
            var path = solver.Solve(mediator.GetNode(enemy.Position), mediator.GetNode(player.Position));
            var sampleAlgorithm = (solver as SampleAlgorithm);

            // パスデータを書き込む
            UpdatePaint(grids, path, GridType.Path);
            UpdatePaint(grids, sampleAlgorithm.RangeGoals.SelectMany(item => item), GridType.Circle);

            var waypoints = path.Select(node => mediator.GetPos(node)).ToList();
            enemy.SetWaypoints(waypoints);
        }

        private void UpdatePaint(GridType[,] grids, IEnumerable<int> data, GridType type)
        {
            RemoveFlags(grids, type);

            foreach (int index in data)
            {
                Vector2Int pos = mediator.GetPos(index);
                grids[pos.y, pos.x] |= type;
            }
        }

        private void RemoveFlags(GridType[,] grids, GridType type)
        {
            for (int y = 0; y < grids.GetLength(0); y++)
            {
                for (int x = 0; x < grids.GetLength(1); x++)
                {
                    grids[y, x] &= ~type;
                }
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
                player.SetWaypoints(new List<Vector2Int>(1) { goal });
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