using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class NavmeshSanity : MonoBehaviour
{
    public NavMeshSurface surface;        // Manager'daki Surface'i sürükle
    public NavMeshAgent agent;            // Agent'ý sürükle
    public float sampleRadius = 50f;

    void Start()
    {
        if (!surface)
        {
            surface = FindObjectOfType<NavMeshSurface>();
            Debug.Log("[Sanity] Surface atanmadý, sahneden buldum: " + (surface ? surface.name : "YOK"));
        }

        // 1) NavMesh var mý?
        var tri = NavMesh.CalculateTriangulation();
        Debug.Log("[Sanity] Triangulation vertices: " + tri.vertices.Length);

        if (tri.vertices.Length == 0 && surface != null)
        {
            Debug.LogWarning("[Sanity] NavMesh YOK. Surface.BuildNavMesh() çaðýrýyorum…");
            surface.BuildNavMesh(); // asenkron yoksa senkron build
            tri = NavMesh.CalculateTriangulation();
            Debug.Log("[Sanity] Build sonrasý triangulation vertices: " + tri.vertices.Length);
        }

        // 2) Agent ile eþleþme
        if (agent)
        {
            if (surface) agent.agentTypeID = surface.agentTypeID;

            var pos = agent.transform.position;
            if (NavMesh.SamplePosition(pos, out var hit, sampleRadius, NavMesh.AllAreas))
            {
                // güvenle warp + býrak
                agent.enabled = true;
                if (agent.Warp(hit.position + Vector3.up * 0.02f))
                {
                    agent.isStopped = false;
                    Debug.Log("[Sanity] Agent NavMesh'e oturdu.");
                }
                else
                {
                    Debug.LogError("[Sanity] Warp BAÞARISIZ. AgentType ile Surface eþleþmiyor olabilir.");
                }
            }
            else
            {
                Debug.LogError("[Sanity] SamplePosition baþarýsýz. Agent zemine çok uzakta ya da Surface layer'ý toplamýyor.");
            }
        }
        else
        {
            Debug.LogWarning("[Sanity] Agent atanmadý.");
        }
    }
}
