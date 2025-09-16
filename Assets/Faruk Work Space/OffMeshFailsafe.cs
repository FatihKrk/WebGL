using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class OffMeshFailsafe : MonoBehaviour
{
    public float timeout = 2f;
    NavMeshAgent agent; bool traversing; float t0;

    void Awake() { agent = GetComponent<NavMeshAgent>(); }

    void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            if (!traversing) { traversing = true; t0 = Time.time; }
            if (Time.time - t0 > timeout)
            {
                var data = agent.currentOffMeshLinkData;
                var end = data.endPos;
                agent.CompleteOffMeshLink();       // otomatik bitir
                agent.Warp(end);                   // nadiren gerekir, garanti olsun
                traversing = false;
            }
        }
        else traversing = false;
    }
}
