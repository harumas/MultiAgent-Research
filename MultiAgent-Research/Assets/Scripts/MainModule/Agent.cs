using System.Collections.Generic;
using UnityEngine;

namespace MainModule
{
    public class Agent : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Renderer spriteRenderer;
        [SerializeField] private Color playerColor;
        [SerializeField] private Color enemyColor;

        public int Index { get; private set; }
        public Vector2Int Position => gridPositions[moveCount];

        private List<Vector2Int> gridPositions;
        private int moveCount;
        private Vector3 agentPosition;

        public void Initialize(int index, Vector2Int start)
        {
            Index = index;

            gameObject.name = $"Agent_{index}";
            transform.localPosition = GetAgentPos(start);
            SetWaypoints(new List<Vector2Int>(1) { start });

            if (index == -1)
            {
                spriteRenderer.material.color = playerColor;
            }
            else
            {
                spriteRenderer.material.color = enemyColor;
            }
        }

        public void SetWaypoints(List<Vector2Int> gridPositions)
        {
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