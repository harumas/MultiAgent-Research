using System.Collections.Generic;

namespace PathFinder.Core
{
    public interface ISolver
    {
        List<(int agentIndex, List<int> path)> Solve(List<AgentContext> contexts);
    }
}