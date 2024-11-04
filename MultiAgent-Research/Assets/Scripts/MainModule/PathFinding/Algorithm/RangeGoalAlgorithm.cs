using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    public class RangeGoalAlgorithm : ISolver
    {
        private readonly GridGraphMediator mediator;
        private readonly ConstrainedAStar pathFinder;
        private readonly RangeGoalFinder rangeGoalFinder;

        public HashSet<int> CorrectGoals;
        public HashSet<int> IncorrectGoals;

        public RangeGoalAlgorithm(Graph graph, GridGraphMediator mediator)
        {
            this.mediator = mediator;

            pathFinder = new ConstrainedAStar(graph, CreateNodes(graph));
            rangeGoalFinder = new RangeGoalFinder(graph, mediator);
        }

        public List<int> Solve(int start, int goal)
        {
            Vector2Int startPos = mediator.GetPos(start);
            Vector2Int goalPos = mediator.GetPos(goal);

            const float alpha = 0.5f;
            Vector2Int diff = startPos - goalPos;

            // ゴールからスタートの距離 * alphaの長さをゴールの範囲とする
            double distance = Math.Sqrt(diff.x * diff.x + diff.y * diff.y) * alpha;
            int radius = (int)Math.Round(distance);

            // 範囲ゴールを取得
            List<HashSet<int>> rangeGoals = rangeGoalFinder.GetRangeGoals(goalPos, radius);

            // 到達可能な範囲ゴール
            HashSet<int> correctRangeGoal = new HashSet<int>();

            foreach (HashSet<int> rangeGoal in rangeGoals)
            {
                // 範囲ゴールの方向を求める
                Vector2Int target = default;
                foreach (int index in rangeGoal)
                {
                    target += mediator.GetPos(index);
                }

                // ゴールから範囲ゴールまでの経路を求める
                List<Node> path = pathFinder.FindPath(goal, rangeGoal, target);

                // 経路が範囲ゴールの半径に収まっているか
                bool isCorrectPath = path
                    .Select(node => node.Position - (Vector2)goalPos)
                    .Select(v => v.x * v.x + v.y * v.y)
                    .All(d => d <= radius * radius);

                // 収まっていたら到達可能な範囲ゴールに含める
                if (isCorrectPath)
                {
                    correctRangeGoal.UnionWith(rangeGoal);
                }
            }

            CorrectGoals = correctRangeGoal;

            HashSet<int> rangeGoalSet = rangeGoals.SelectMany(item => item).ToHashSet();
            rangeGoalSet.ExceptWith(correctRangeGoal);
            IncorrectGoals = rangeGoalSet;

            // 到達可能な範囲ゴールに対して経路探索
            List<Node> result = pathFinder.FindPath(start, correctRangeGoal, mediator.GetPos(goal));

            // ノードに変換して結果に追加
            return result.Select(node => node.Index).ToList();
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