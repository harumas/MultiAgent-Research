using PathFinding;

namespace Visualizer.MapEditor
{
    public class EmptyMaze : IMazeGenerator
    {
        public GridType[,] Generate(int height, int width)
        {
            var maze = new GridType[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        maze[x, y] = GridType.Obstacle; // 外周はすべて壁
                    }
                    else
                    {
                        maze[x, y] = GridType.Road; // 外周以外は通路
                    }
                }
            }

            return maze;
        }
    }
}