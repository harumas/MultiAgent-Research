using MainModule;
using PathFinding;
using UnityEngine;

namespace Visualizer.MapEditor
{
    public class MapEditor : MonoBehaviour
    {
        [SerializeField] private MapDataManager mapDataManager;
        [SerializeField] private Transform scaler;

        private bool isEditing;
        private MapData mapData;

        private void Update()
        {
            SwitchEditState();

            if (isEditing && TryGetPaint(out Vector2Int position, out GridType gridType))
            {
                mapData.Grids[position.y, position.x] = gridType;
            }
        }

        private void SwitchEditState()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isEditing = !isEditing;

                if (isEditing)
                {
                    mapData = mapDataManager.CurrentMapData;
                    Debug.Log("Begin Edit");
                }
                else
                {
                    mapDataManager.Save(mapData.Grids);
                    Debug.Log("End Edit");
                }
            }
        }

        private bool TryGetPaint(out Vector2Int position, out GridType gridType)
        {
            bool isRoad = Input.GetMouseButton(0);
            bool isObstacle = Input.GetMouseButton(1);

            if (isRoad || isObstacle)
            {
                Camera camera = Camera.main;
                Vector3 mousePos = Input.mousePosition;

                // スクリーン座標をワールド座標に変換
                Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camera.nearClipPlane));

                // ワールド座標をテクスチャ座標に変換
                position = WorldToTextureCoordinate(worldPos);

                // テクスチャ座標が範囲内かチェック
                bool isWithinBounds = position.x >= 0 && position.x < mapData.Width && position.y >= 0 && position.y < mapData.Height;

                gridType = isRoad ? GridType.Road : GridType.Obstacle;
                return isWithinBounds;
            }

            position = Vector2Int.zero;
            gridType = GridType.Obstacle;
            return false;
        }

        // ワールド座標をテクスチャ座標に変換する関数
        Vector2Int WorldToTextureCoordinate(Vector3 worldPos)
        {
            // オブジェクトの左下を基準にする
            Vector3 localPos = scaler.InverseTransformPoint(worldPos);

            // UV座標をテクスチャ座標に変換
            int x = Mathf.FloorToInt(localPos.x);
            int y = Mathf.FloorToInt(localPos.y);

            return new Vector2Int(x, y);
        }
    }
}