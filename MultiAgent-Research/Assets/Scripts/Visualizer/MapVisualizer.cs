using UnityEngine;

namespace Visualizer
{
    public class MapVisualizer : MonoBehaviour
    {
        // テクスチャの表示サイズ
        [SerializeField] private Vector2 displaySize;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Texture2D currentTexture;

        public void Create(int width, int height)
        {
            // テクスチャを生成
            currentTexture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point
            };

            // テクスチャをスプライトに変換して表示
            if (currentTexture != null)
            {
                // テクスチャをスプライトに変換
                Sprite sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), new Vector2(0f, 0f), 1f);

                // SpriteRendererに設定
                spriteRenderer.sprite = sprite;

                // スプライトのサイズを指定サイズにスケール調整
                Vector2 textureSize = new Vector2(currentTexture.width, currentTexture.height);
                Vector3 scale = new Vector3(displaySize.x / textureSize.x, displaySize.y / textureSize.y, 1);
                transform.localScale = scale;
                transform.localPosition = new Vector3(-displaySize.x / 2f, -displaySize.y / 2f, 0);
            }
            else
            {
                Debug.LogError("テクスチャの生成に失敗しました");
            }
        }

        public unsafe void UpdateMap<T>(T[,] mapData, IMapConvertJob<T> mapConvertJob) where T : unmanaged
        {
            fixed (T* mapPtr = mapData)
            {
                var textureData = currentTexture.GetRawTextureData<Color32>();
                var jobHandle = mapConvertJob.Schedule(mapPtr, textureData);
                jobHandle.Complete();
            }

            currentTexture.Apply();
        }
    }
}