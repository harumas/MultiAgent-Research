using PathFinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Visualizer
{
    [BurstCompile]
    public class ObstacleMapConvertJob : IMapConvertJob<GridType>
    {
        private static readonly Color32 obstacleColor = new Color32(58, 58, 58, 255);
        private static readonly Color32 roadColor = new Color32(255, 255, 255, 255);
        private static readonly Color32 pathColor = new Color32(148, 255, 172, 255);
        private static readonly Color32 correctCircleColor = new Color32(255, 79, 106, 255);
        private static readonly Color32 incorrectCircleColor = new Color32(163, 163, 156, 255);

        [BurstCompile]
        private unsafe struct ObstacleMapToTextureJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public GridType* MapData;
            [NativeDisableParallelForRestriction] public NativeArray<Color32> TextureData;
            public Color32 MaxRiskColor;
            public Color32 MinRiskColor;

            public void Execute(int i)
            {
                GridType id = MapData[i];

                if (HasBitFlag(id, GridType.Path))
                {
                    TextureData[i] = pathColor;
                }
                else if (HasBitFlag(id, GridType.CorrectCircle))
                {
                    TextureData[i] = correctCircleColor;
                }
                else if (HasBitFlag(id, GridType.IncorrectCircle))
                {
                    TextureData[i] = incorrectCircleColor;
                }
                else if (HasBitFlag(id, GridType.Obstacle))
                {
                    TextureData[i] = obstacleColor;
                }
                else if (HasBitFlag(id, GridType.Road))
                {
                    TextureData[i] = roadColor;
                }
            }

            private bool HasBitFlag(GridType value, GridType flag) => (value & flag) == flag;
        }

        public unsafe JobHandle Schedule(GridType* mapData, NativeArray<Color32> textureData)
        {
            return new ObstacleMapToTextureJob()
            {
                MapData = mapData,
                TextureData = textureData,
            }.Schedule(textureData.Length, 16);
        }
    }
}