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
        public (Agent player, List<Agent> agents) CreateAgents(MapData mapData)
        {
            var agentPoints = mapData.Agents;

            // 逃避エージェントの作成
            Agent player = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
            player.Initialize(-1, mapData.Goal);

            // 追従エージェントの作成
            List<Agent> agents = new List<Agent>(agentPoints.Count);

            for (var index = 0; index < agentPoints.Count; index++)
            {
                var point = agentPoints[index];

                // エージェントのオブジェクトを生成
                Agent moveAgent = Instantiate(agentPrefab, agentParent).GetComponent<Agent>();
                moveAgent.Initialize(index, point);

                agents.Add(moveAgent);
            }

            return (player, agents);
        }
    }
}