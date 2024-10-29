namespace PathFinder.Core
{
    public readonly struct AgentContext
    {
        public readonly int Position;
        public readonly int Goal;

        public AgentContext(int position, int goal)
        {
            Position = position;
            Goal = goal;
        }
    }
}