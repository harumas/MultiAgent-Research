using System;
using System.Collections.Generic;
using System.IO;
using PathFinding;

namespace Visualizer.MapEditor
{
    public class DigMaze : IMazeGenerator
    {
        private List<Cell> startCells;

        // 生成処理
        public GridType[,] Generate(int width, int height)
        {
            // 5未満のサイズや偶数では生成できない
            if (width < 5 || height < 5) throw new ArgumentOutOfRangeException();
            if (width % 2 == 0) width++;
            if (height % 2 == 0) height++;

            // 迷路情報を初期化
            var maze = new GridType[width, height];
            startCells = new List<Cell>();

            // 全てを壁で埋める
            // 穴掘り開始候補(x,yともに偶数)座標を保持しておく
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        maze[x, y] = GridType.Road; // 外壁は判定の為通路にしておく(最後に戻す)
                    }
                    else
                    {
                        maze[x, y] = GridType.Obstacle;
                    }
                }
            }

            // 穴掘り開始
            Dig(maze, 1, 1);

            // 外壁を戻す
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        maze[x, y] = GridType.Obstacle;
                    }
                }
            }

            return maze;
        }

        // 座標(x, y)に穴を掘る
        private void Dig(GridType[,] maze, int x, int y)
        {
            // 指定座標から掘れなくなるまで堀り続ける
            var rnd = new Random();
            while (true)
            {
                // 掘り進めることができる方向のリストを作成
                var directions = new List<Direction>();
                if (maze[x, y - 1] == GridType.Obstacle && maze[x, y - 2] == GridType.Obstacle)
                    directions.Add(Direction.Up);
                if (maze[x + 1, y] == GridType.Obstacle && maze[x + 2, y] == GridType.Obstacle)
                    directions.Add(Direction.Right);
                if (maze[x, y + 1] == GridType.Obstacle && maze[x, y + 2] == GridType.Obstacle)
                    directions.Add(Direction.Down);
                if (maze[x - 1, y] == GridType.Obstacle && maze[x - 2, y] == GridType.Obstacle)
                    directions.Add(Direction.Left);

                // 掘り進められない場合、ループを抜ける
                if (directions.Count == 0) break;

                // 指定座標を通路とし穴掘り候補座標から削除
                SetPath(maze, x, y);
                // 掘り進められる場合はランダムに方向を決めて掘り進める
                var dirIndex = rnd.Next(directions.Count);
                // 決まった方向に先2マス分を通路とする
                switch (directions[dirIndex])
                {
                    case Direction.Up:
                        SetPath(maze, x, --y);
                        SetPath(maze, x, --y);
                        break;
                    case Direction.Right:
                        SetPath(maze, ++x, y);
                        SetPath(maze, ++x, y);
                        break;
                    case Direction.Down:
                        SetPath(maze, x, ++y);
                        SetPath(maze, x, ++y);
                        break;
                    case Direction.Left:
                        SetPath(maze, --x, y);
                        SetPath(maze, --x, y);
                        break;
                }
            }

            // どこにも掘り進められない場合、穴掘り開始候補座標から掘りなおし
            // 候補座標が存在しないとき、穴掘り完了
            var cell = GetStartCell();
            if (cell != null)
            {
                Dig(maze, cell.X, cell.Y);
            }
        }

        // 座標を通路とする(穴掘り開始座標候補の場合は保持)
        private void SetPath(GridType[,] maze, int x, int y)
        {
            maze[x, y] = GridType.Road;
            if (x % 2 == 1 && y % 2 == 1)
            {
                // 穴掘り候補座標
                startCells.Add(new Cell() { X = x, Y = y });
            }
        }

        // 穴掘り開始位置をランダムに取得する
        private Cell GetStartCell()
        {
            if (startCells.Count == 0) return null;

            // ランダムに開始座標を取得する
            var rnd = new Random();
            var index = rnd.Next(startCells.Count);
            var cell = startCells[index];
            startCells.RemoveAt(index);

            return cell;
        }

        // セル情報
        private class Cell
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        // 方向
        private enum Direction
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }
    }
}