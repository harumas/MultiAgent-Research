using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Visualizer
{
    [BurstCompile]
    public class RiskMapConvertJob : IMapConvertJob<float>
    {
        private readonly Color32 maxRiskColor = new Color32(255, 0, 0, 255);
        private readonly Color32 minRiskColor = new Color32(0, 0, 255, 255);

        [BurstCompile]
        private unsafe struct WriteDataToTextureJob : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public float* MapData;
            [NativeDisableParallelForRestriction] public NativeArray<Color32> TextureData;
            public Color32 MaxRiskColor;
            public Color32 MinRiskColor;

            public void Execute(int i)
            {
                float t = MapData[i]; // 線形補間の割合 (0.0f から 1.0f)

                // 各チャンネルごとに線形補間を手動で行う
                byte r = (byte)(MinRiskColor.r + (MaxRiskColor.r - MinRiskColor.r) * t);
                byte g = (byte)(MinRiskColor.g + (MaxRiskColor.g - MinRiskColor.g) * t);
                byte b = (byte)(MinRiskColor.b + (MaxRiskColor.b - MinRiskColor.b) * t);

                // 補間された結果をdataに書き込む
                TextureData[i] = new Color32(r, g, b, 255);
            }
        }

        public unsafe JobHandle Schedule(float* mapData, NativeArray<Color32> textureData)
        {
            return new WriteDataToTextureJob()
            {
                MapData = mapData,
                TextureData = textureData,
                MaxRiskColor = maxRiskColor,
                MinRiskColor = minRiskColor
            }.Schedule(textureData.Length, 16);
        }
    }
}