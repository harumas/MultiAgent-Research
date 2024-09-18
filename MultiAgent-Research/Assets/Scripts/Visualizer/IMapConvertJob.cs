using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Visualizer
{
    public interface IMapConvertJob<T> where T : unmanaged
    {
        unsafe JobHandle Schedule(T* mapData, NativeArray<Color32> textureData);
    }
}