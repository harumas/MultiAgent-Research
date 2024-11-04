using System.Collections.Generic;
using UnityEngine;
using Vector2Int = PathFinder.Core.Vector2Int;

namespace MainModule
{
    public class Agent : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Renderer spriteRenderer;
        [SerializeField] private Color playerColor;
        [SerializeField] private Color enemyColor;

        public Vector2Int Position => gridPositions[moveCount];

        private List<Vector2Int> gridPositions;
        private int moveCount;
        private Vector3 agentPosition;

        public void Initialize(bool isPlayer, Vector2Int start)
        {
            gameObject.name = $"Agent_{isPlayer}";
            transform.localPosition = GetAgentPos(start);
            SetWaypoints(new List<Vector2Int>(1) { start });

            spriteRenderer.material.color = isPlayer ? playerColor : enemyColor;
        }

        public void SetWaypoints(List<Vector2Int> gridPositions)
        {
            if (gridPositions.Count == 0)
            {
                return;
            }
            
            this.gridPositions = gridPositions;
            moveCount = 0;
            agentPosition = GetAgentPos(gridPositions[moveCount]);
        }

        private void Update()
        {
            bool isDirMove = Input.GetKeyDown(KeyCode.LeftArrow) ||
                             Input.GetKeyDown(KeyCode.RightArrow) ||
                             Input.GetKeyDown(KeyCode.UpArrow) ||
                             Input.GetKeyDown(KeyCode.DownArrow);
            //Enterキーを押したら進む
            if (Input.GetKeyDown(KeyCode.Return) || isDirMove)
            {
                if (gridPositions == null)
                {
                    return;
                }

                //進みきったら終了
                if (moveCount + 1 >= gridPositions.Count)
                {
                    return;
                }

                moveCount++;

                agentPosition = GetAgentPos(gridPositions[moveCount]);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DestroyImmediate(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (gridPositions == null)
            {
                return;
            }

            if (gridPositions != null && moveCount >= gridPositions.Count)
            {
                return;
            }

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, agentPosition, Time.fixedDeltaTime * speed);
        }

        private Vector3 GetAgentPos(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x, gridPos.y, 0f);
        }
    }
}