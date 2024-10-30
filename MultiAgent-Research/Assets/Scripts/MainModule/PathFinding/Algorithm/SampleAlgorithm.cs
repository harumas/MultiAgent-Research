using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    public class SampleAlgorithm : ISolver
    {
        private readonly GridType[,] grids;
        private readonly GridGraphMediator mediator;
        private readonly ConstrainedAStar pathFinder;
        private readonly RangeGoalFinder rangeGoalFinder;

        public List<List<int>> RangeGoals;

        public SampleAlgorithm(Graph graph, GridType[,] grids, GridGraphMediator mediator)
        {
            this.grids = grids;
            this.mediator = mediator;

            pathFinder = new ConstrainedAStar(graph, CreateNodes(graph));
            rangeGoalFinder = new RangeGoalFinder(graph, mediator);
        }

        public List<int> Solve(int start, int goal)
        {
            float alpha = 0.5f;
            Vector2Int startPos = mediator.GetPos(start);
            Vector2Int goalPos = mediator.GetPos(goal);

            Vector2Int diff = goalPos - startPos;
            double distance = Math.Sqrt(diff.x * diff.x + diff.y * diff.y) * alpha;
            int radius = (int)Math.Round(distance);
            RangeGoals = rangeGoalFinder.GetRangeGoals(startPos, radius);

            Dictionary<List<int>, int> nearestGoals = RangeGoals.ToDictionary(
                rangeGoal => rangeGoal,
                rangeGoal => rangeGoal.OrderBy(node =>
                {
                    Vector2Int vec = mediator.GetPos(goal) - mediator.GetPos(node);
                    return vec.x * vec.x + vec.y * vec.y;
                }).First());

            var sortedRangeGoal = nearestGoals.OrderBy(rangeGoal =>
            {
                Vector2Int vec = mediator.GetPos(goal) - mediator.GetPos(rangeGoal.Value);
                return vec.x * vec.x + vec.y * vec.y;
            });

            List<Node> path = new List<Node>();

            foreach (KeyValuePair<List<int>, int> rangeGoal in sortedRangeGoal)
            {
                path = pathFinder.FindPath(start, rangeGoal.Value);

                bool isCorrectPath = path
                    .Select(node => node.Position - (Vector2)startPos)
                    .Select(v => v.x * v.x + v.y * v.y)
                    .All(d => d <= distance * distance);

                if (isCorrectPath)
                {
                    break;
                }
            }

            // ノードに変換して結果に追加
            return path.Select(node => node.Index).ToList();
        }

        private List<Node> CreateNodes(Graph graph)
        {
            // 座標からアルゴリズムで使用するノードを作成
            List<Node> nodes = new List<Node>(graph.NodeCount);

            for (int i = 0; i < graph.NodeCount; i++)
            {
                Vector2Int pos = mediator.GetPos(i);
                nodes.Add(new Node(i, new Vector2(pos.x, pos.y)));
            }

            return nodes;
        }
    }
}