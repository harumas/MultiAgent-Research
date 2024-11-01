﻿using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    public class RangeGoalFinder
    {
        private readonly Graph graph;
        private readonly GridGraphMediator mediator;

        public RangeGoalFinder(Graph graph, GridGraphMediator mediator)
        {
            this.graph = graph;
            this.mediator = mediator;
        }

        public List<List<int>> GetRangeGoals(Vector2Int center, int radius)
        {
            List<List<int>> rangeGoals = new List<List<int>>();
            var circleNodes = CalculateCircleNodes(center, radius);

            while (circleNodes.Count > 0)
            {
                int first = circleNodes.First();
                var rangeGoal = new List<int> { first };
                rangeGoals.Add(rangeGoal);

                Queue<int> open = new Queue<int>();
                open.Enqueue(first);

                while (open.Count > 0)
                {
                    int current = open.Dequeue();
                    circleNodes.Remove(current);

                    foreach (int neighbor in graph.GetNextNodes(current))
                    {
                        if (circleNodes.Contains(neighbor))
                        {
                            rangeGoal.Add(neighbor);
                            open.Enqueue(neighbor);
                        }
                    }
                }
            }

            return rangeGoals;
        }

        private HashSet<int> CalculateCircleNodes(Vector2Int center, int radius)
        {
            HashSet<int> circlePoints = new HashSet<int>();

            Vector2Int pos = new Vector2Int(0, 0);
            int d = 0;

            d = 3 - 2 * radius;
            pos.y = radius;

            SetCirclePoint(circlePoints, center.x, radius + center.y);
            SetCirclePoint(circlePoints, center.x, -radius + center.y);
            SetCirclePoint(circlePoints, radius + center.x, center.y);
            SetCirclePoint(circlePoints, -radius + center.x, center.y);

            for (pos.x = 0; pos.x <= pos.y; pos.x++)
            {
                if (d < 0)
                {
                    d += 6 + 4 * pos.x;
                }
                else
                {
                    d += 10 + 4 * pos.x - 4 * pos.y--;
                }

                SetCirclePoint(circlePoints, pos.y + center.x, pos.x + center.y);
                SetCirclePoint(circlePoints, pos.x + center.x, pos.y + center.y);
                SetCirclePoint(circlePoints, -pos.x + center.x, pos.y + center.y);
                SetCirclePoint(circlePoints, -pos.y + center.x, pos.x + center.y);
                SetCirclePoint(circlePoints, -pos.y + center.x, -pos.x + center.y);
                SetCirclePoint(circlePoints, -pos.x + center.x, -pos.y + center.y);
                SetCirclePoint(circlePoints, pos.x + center.x, -pos.y + center.y);
                SetCirclePoint(circlePoints, pos.y + center.x, -pos.x + center.y);
            }

            return circlePoints;
        }

        private void SetCirclePoint(HashSet<int> circlePoints, int x, int y)
        {
            var pos = new Vector2Int(x, y);

            if (mediator.TryGetNode(pos, out int node))
            {
                circlePoints.Add(node);
            }
        }
    }
}