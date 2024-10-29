using System;
using System.Collections.Generic;
using UnityEngine;

namespace Visualizer.MapEditor
{
    [Flags]
    public enum GridType
    {
        Road = 1,
        Obstacle = 2,
        Path = 4,
        Circle = 8,
    }

    public class MapData
    {
        public readonly int Height;
        public readonly int Width;
        public readonly int PassableCount;

        public readonly Vector2Int Player;
        public readonly Vector2Int Enemy;
        public readonly GridType[,] Grids;

        public MapData(
            int height,
            int width,
            int passableCount,
            Vector2Int start,
            Vector2Int goal,
            GridType[,] grids
        )
        {
            Height = height;
            Width = width;
            Player = start;
            Enemy = goal;
            PassableCount = passableCount;
            Grids = grids;
        }
    }

    public class MapDataManager : MonoBehaviour
    {
        [SerializeField] private int defaultHeight;
        [SerializeField] private int defaultWidth;
        [SerializeField] private MapSaveData mapData;

        private MapData currentMapData;
        public MapData CurrentMapData => currentMapData;

        public MapData Load()
        {
            GridType[,] mapIds;
            int width, height;

            if (mapData.Data.Length == 0)
            {
                mapIds = GetDefaultMapData(defaultHeight, defaultWidth);
                width = defaultWidth;
                height = defaultHeight;
                Save(mapIds);
            }
            else
            {
                mapIds = ParseMapData();
                height = mapIds.GetLength(0);
                width = mapIds.GetLength(1);
            }

            int passableCount = 0;

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isPassable = mapIds[z, x] == GridType.Road;

                    if (isPassable)
                    {
                        passableCount++;
                    }
                }
            }

            currentMapData = new MapData(width, height, passableCount, mapData.Start, mapData.Goal, mapIds);
            

            return currentMapData;
        }

        public void Save(GridType[,] mapIds)
        {
            string[] data = mapData.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int width = data.Length != 0 ? data[0].Length : defaultWidth;
            int height = data.Length != 0 ? data.Length : defaultHeight;

            char[] str = new char[width * height + height];
            Array.Fill(str, '.');

            for (int i = 0; i < mapIds.GetLength(0); i++)
            {
                int index = 0;
                for (int j = 0; j < mapIds.GetLength(1); j++)
                {
                    GridType v = mapIds[i, j];
                    index = i * width + j + i;
                    str[index] = v == GridType.Road ? '.' : '*';
                }

                str[index + 1] = '\n';
            }

            string result = new string(str);
            mapData.SetData(result);
        }

        private GridType[,] ParseMapData()
        {
            string[] data = mapData.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            int width = data.Length != 0 ? data[0].Length : defaultWidth;
            int height = data.Length != 0 ? data.Length : defaultHeight;

            GridType[,] mapIds = new GridType[height, width];

            for (var i = 0; i < height; i++)
            {
                var h = data[i];

                for (var j = 0; j < width; j++)
                {
                    mapIds[i, j] = h[j] == '.' ? GridType.Road : GridType.Obstacle;
                }
            }

            return mapIds;
        }

        private GridType[,] GetDefaultMapData(int height, int width)
        {
            var map = new GridType[height, width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    map[y, x] = GridType.Road;
                }
            }

            return map;
        }
    }
}