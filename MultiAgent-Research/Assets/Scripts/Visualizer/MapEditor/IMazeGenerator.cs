using PathFinding;

namespace Visualizer.MapEditor
{
    public enum MazeType
    {
        Empty,
        Bar,
        Dig
    }

    public interface IMazeGenerator
    {
        GridType[,] Generate(int height, int width);
    }
}