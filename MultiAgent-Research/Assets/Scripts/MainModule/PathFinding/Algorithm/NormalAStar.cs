using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
     public class NormalAStar : ISolver
    {
        private readonly GridGraphMediator mediator;
        private readonly ConstrainedAStar pathFinder;

        public NormalAStar(Graph graph, GridGraphMediator mediator)
        {
            this.mediator = mediator;

            pathFinder = new ConstrainedAStar(graph, CreateNodes(graph));
        }

        public List<(int agentIndex, List<int> path)> Solve(List<AgentContext> contexts)
        {
            var result = new List<(int agentIndex, List<int> path)>(contexts.Count);

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

        public List<int> Solve(int start, int goal)
        {
            throw new System.NotImplementedException();
        }
    }
}