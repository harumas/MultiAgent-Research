using System;
using System.Diagnostics;
using System.Linq;
using PathFinder.Core;
using PathFinding;
using PathFinding.Algorithm;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;
using Vector2Int = PathFinder.Core.Vector2Int;

namespace MainModule
{
    public class PerformanceTest : MonoBehaviour
    {
        [SerializeField] private SolverType solverType;
        [SerializeField] private MapDataManager mapDataManager;
        [SerializeField] private int interval;
        [SerializeField] private int attemptCount;

        private GridGraphMediator mediator;
        private ISolver solver;

        private int enemyNode;
        private int playerNode;

        private void Awake()
        {
            // マップデータを読み込む
            MapData mapData = mapDataManager.Load();
            mediator = new GridGraphMediator(mapData);

            // グラフを構築する
            GraphConstructor constructor = new GraphConstructor(mapData, mediator);
            Graph graph = constructor.ConstructGraph();

            // ソルバーの作成
            solver = CreateSolver(graph);

            Random random = new Random(123);

            Stopwatch sw = new Stopwatch();
            long[] times = new long[attemptCount];

            for (int i = 0; i < attemptCount; i++)
            {
                sw.Reset();
                sw.Start();
                enemyNode = mediator.GetNode(mapData.Enemy);
                playerNode = mediator.GetNode(mapData.Player);

                // 経路探索を実行
                for (int j = 0; j < interval; j++)
                {
                    var path = solver.Solve(enemyNode, playerNode);
                    enemyNode = path[0];

                    var next = graph.GetNextNodes(playerNode);
                    var index = random.Next(0, next.Count);
                    var node = next.ElementAt(index);

                    playerNode = node;
                }

                sw.Stop();
                times[i] = sw.ElapsedMilliseconds;
            }

            Debug.Log($"Ave: {times.Average()}");

            var orderedTimes = times.OrderBy(key => key).ToList();
            float median;

            if (attemptCount % 2 == 0)
            {
                median = (orderedTimes[attemptCount / 2 - 1] + orderedTimes[attemptCount / 2]) / 2;
            }
            else
            {
                median = orderedTimes[attemptCount / 2];
            }
            
            Debug.Log($"Med: {median}");
            Debug.Log($"Sum: {times.Sum()}");
        }

        private ISolver CreateSolver(Graph graph)
        {
            switch (solverType)
            {
                case SolverType.RangeGoal:
                    return new RangeGoalAlgorithm(graph, mediator);
                case SolverType.RangeGoalWithBFS:
                    return new RangeGoalAlgorithmWithBFS(graph, mediator);
                case SolverType.AStar:
                    return new NormalAStar(graph, mediator);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}