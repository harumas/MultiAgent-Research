using System.Collections.Generic;
using PathFinding;
using UnityEngine;

namespace MainModule
{
    public class AgentFactory : MonoBehaviour
    {
        [SerializeField] private GameObject agentPrefab;
        [SerializeField] private Transform agentParent;
        

        /// <summary>
        /// Solverに渡すContextを作成します
        /// </summary>
        /// <returns></returns>
        public (Agent player, Agent enemy) CreateAgents(MapData mapData)
        {
            // 逃避エージェントの作成
            Agent player = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
            player.Initialize(true, mapData.Player);

            // 追従エージェントの作成
            Agent enemy = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
            enemy.Initialize(false, mapData.Enemy);

            return (player, enemy);
        }
    }
}