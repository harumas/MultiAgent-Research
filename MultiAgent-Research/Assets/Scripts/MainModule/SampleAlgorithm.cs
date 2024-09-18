using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace MainModule
{
    public class SampleAlgorithm : ISolver
    {
        private readonly ConstrainedAStar pathFinder;

        public SampleAlgorithm(Graph graph, List<Node> nodes)
        {
            pathFinder = new ConstrainedAStar(graph, nodes);
        }

        public List<(int agentIndex, List<int> path)> Solve(List<AgentContext> contexts)
        {
            var result = new List<(int agentIndex, List<int> path)>(contexts.Count);
            
            foreach (AgentContext context in contexts)
            {
                result.Add((context.AgentIndex, pathFinder.FindPath(context.Position, context.Goal).Select(node => node.Index).ToList())); 
            }

            return result;
        }
    }
}