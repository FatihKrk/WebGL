using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentBootstrap : MonoBehaviour
{
    public float searchRadius = 1000f;   // navmesh’i uzaktan da bulsun
    public float snapOffsetY = 0.05f;    // zemine hafif oturt

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Ajaný henüz baþlatma; önce zemini bulalým
        agent.enabled = false;

        // En yakýn navmesh noktasýný bul
        if (NavMesh.SamplePosition(transform.position, out var hit, searchRadius, NavMesh.AllAreas))
        {
            var p = hit.position;
            p.y += snapOffsetY;
            // Pozisyonu doðrudan koy
            transform.position = p;
        }

        // Þimdi ajaný aç
        agent.enabled = true;

        // Güvenlik: hala navmesh dýþýndaysa yakýn bir yere warp et
        if (!agent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out var hit2, searchRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit2.position + Vector3.up * snapOffsetY);
        }
    }
}
