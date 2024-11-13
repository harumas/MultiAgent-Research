using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PathFinding;
using Random = UnityEngine.Random;

namespace Visualizer.MapEditor
{
    public class BarMaze : IMazeGenerator
    {
        public GridType[,] Generate(int width, int height)
        {
            // 5未満のサイズでは生成できない
            if (height < 5 || width < 5)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (width % 2 == 0)
            {
                width++;
            }

            if (height % 2 == 0)
            {
                height++;
            }

            // 指定サイズで生成し外周を壁にする
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


            // 棒を立て、倒す
            var rnd = new System.Random();
            for (int x = 2; x < width - 1; x += 2)
            {
                for (int y = 2; y < height - 1; y += 2)
                {
                    maze[x, y] = GridType.Obstacle; // 棒を立てる

                    // 倒せるまで繰り返す
                    while (true)
                    {
                        // 1行目のみ上に倒せる
                        int direction;
                        if (y == 2)
                        {
                            direction = rnd.Next(4);
                        }
                        else
                        {
                            direction = rnd.Next(3);
                        }

                        // 棒を倒す方向を決める
                        int wallX = x;
                        int wallY = y;
                        switch (direction)
                        {
                            case 0: // 右
                                wallX++;
                                break;
                            case 1: // 下
                                wallY++;
                                break;
                            case 2: // 左
                                wallX--;
                                break;
                            case 3: // 上
                                wallY--;
                                break;
                        }

                        // 壁じゃない場合のみ倒して終了
                        if (maze[wallX, wallY] != GridType.Obstacle)
                        {
                            maze[wallX, wallY] = GridType.Obstacle;
                            break;
                        }
                    }
                }
            }

            return maze;
        }
    }
}