using UnityEngine;
using UnityEngine.AI;

public class OffMeshLinkDebugger : MonoBehaviour
{
    NavMeshAgent agent;

    void Awake() { agent = GetComponent<NavMeshAgent>(); }

    void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            var data = agent.currentOffMeshLinkData;
            // En basit geçiþ: doðrudan uca “warp”
            agent.Warp(data.endPos);
            agent.CompleteOffMeshLink();
        }
    }
}
