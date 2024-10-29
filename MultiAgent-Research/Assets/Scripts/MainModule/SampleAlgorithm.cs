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

        public List<int> Solve(int start, int goal)
        {
            return pathFinder.FindPath(start, goal).Select(node => node.Index).ToList();
        }
    }
}