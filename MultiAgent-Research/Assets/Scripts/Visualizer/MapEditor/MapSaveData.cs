using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Visualizer.MapEditor
{
    [CreateAssetMenu(menuName = "MapData")]
    public class MapSaveData : ScriptableObject
    {
        [SerializeField] private Vector2Int start;
        [SerializeField] private Vector2Int goal;
        
        [Multiline(lines: 21)]
        [SerializeField]
        private string data;

        public string Data => data;
        public Vector2Int Start => start;
        public Vector2Int Goal => goal;

        public void SetData(string data)
        {
            this.data = data;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}