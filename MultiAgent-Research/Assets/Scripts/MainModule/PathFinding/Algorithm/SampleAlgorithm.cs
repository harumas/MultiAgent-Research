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

        public SampleAlgorithm(Graph graph, GridType[,] grids, GridGraphMediator mediator)
        {
            this.grids = grids;
            this.mediator = mediator;

            pathFinder = new ConstrainedAStar(graph, CreateNodes(graph));
        }

        public List<(int agentIndex, List<int> path)> Solve(List<AgentContext> contexts)
        {
            var result = new List<(int agentIndex, List<int> path)>(contexts.Count);

            // エージェントの半分の目標地点を設定
            AssignHalfGoal(contexts);

            foreach (AgentContext context in contexts)
            {
                // 経路探索
                List<Node> path = pathFinder.FindPath(context.Position, context.Goal);

                // ノードに変換して結果に追加
                result.Add((context.AgentIndex, path.Select(node => node.Index).ToList()));
            }

            return result;
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

        private void AssignHalfGoal(List<AgentContext> contexts)
        {
            Vector2Int goal = mediator.GetPos(contexts[0].Goal);

            for (var i = 0; i < contexts.Count; i++)
            {
                Vector2Int agentPosition = mediator.GetPos(contexts[i].Position);
                Vector2Int diff = goal - agentPosition;
                
                // 中点を計算
                Vector2 bullet = new Vector2(agentPosition.x, agentPosition.y) + new Vector2(diff.x * 0.5f, diff.y * 0.5f);
                
                // グリッド上の座標に変換
                Vector2Int point = new Vector2Int((int)Math.Round(bullet.x), (int)Math.Round(bullet.y));
                Vector2Int result = goal;

                if ((grids[point.y, point.x] & GridType.Obstacle) == 0)
                {
                    result = point;
                }
                else
                {
                    Vector2Int? nearestGoal = FindNearestGoal(grids, point);

                    if (nearestGoal != null)
                    {
                        if (nearestGoal == agentPosition)
                        {
                            nearestGoal = goal;
                        }

                        result = nearestGoal.Value;
                    }
                }

                contexts[i] = new AgentContext(contexts[i].AgentIndex, contexts[i].Position, mediator.GetNode(result));
            }
        }

        // 幅優先探索で障害物を避け、最も近い通行可能なセルを見つける関数
        private Vector2Int? FindNearestGoal(GridType[,] grids, Vector2Int point)
        {
            int rows = grids.GetLength(0);
            int cols = grids.GetLength(1);

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
                if ((grids[p.y, p.x] & GridType.Obstacle) == 0)
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
    }
}