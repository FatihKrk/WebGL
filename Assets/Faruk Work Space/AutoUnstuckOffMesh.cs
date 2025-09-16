using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LinkCooldown))]
public class AutoUnstuckOffMesh : MonoBehaviour
{
    public float timeout = 2f;

    NavMeshAgent agent;
    LinkCooldown cd;
    bool traversing;
    float startT;

    void Awake() { agent = GetComponent<NavMeshAgent>(); cd = GetComponent<LinkCooldown>(); }

    void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            if (!traversing) { traversing = true; startT = Time.time; }
            var data = agent.currentOffMeshLinkData;

            if (Time.time - startT > timeout)
            {
                // Zorla güvenli tamamla
                var target = data.endPos;
                agent.CompleteOffMeshLink();
                agent.Warp(target);                // nadiren gerekebilir
                cd.MarkJustFinished(data.offMeshLink);
                traversing = false;
            }
        }
        else if (traversing)
        {
            // Normal tamamlandý
            var data = agent.currentOffMeshLinkData;
            cd.MarkJustFinished(data.offMeshLink);
            traversing = false;
        }
    }
}
