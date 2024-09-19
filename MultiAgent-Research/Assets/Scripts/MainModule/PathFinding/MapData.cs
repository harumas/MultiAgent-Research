using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding
{
    [Flags]
    public enum GridType
    {
        Road = 1,
        Obstacle = 2,
        Path = 4
    }

    public class MapData
    {
        public readonly int Height;
        public readonly int Width;
        public readonly int PassableCount;
        public readonly IReadOnlyList<Vector2Int> Agents;
        public readonly Vector2Int Goal;
        public readonly GridType[,] Grids;

        public MapData(int height, int width, int passableCount, IReadOnlyList<Vector2Int> agents, Vector2Int goal, GridType[,] grids)
        {
            Height = height;
            Width = width;
            PassableCount = passableCount;
            Agents = agents;
            Goal = goal;
            Grids = grids;
        }

        public bool IsValid()
        {
            var agents = Agents;
            bool isUniqueStarts = !agents.GroupBy(p => p).SelectMany(g => g.Skip(1)).Any();

            if (!isUniqueStarts)
            {
                throw new ArgumentException("スタートのデータが重複しています。");
            }

            return true;
        }
    }

}