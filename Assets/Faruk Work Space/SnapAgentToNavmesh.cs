using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class SnapAgentToNavmesh : MonoBehaviour
{
    [Tooltip("Ajanýn etrafýnda NavMesh ararken kullanýlacak yarýçap (metre).")]
    public float sampleRadius = 5f;

    [Tooltip("NavMesh Sample için alan maskesi. Genelde AllAreas býrakýlýr.")]
    public int areaMask = NavMesh.AllAreas;

    [Tooltip("Yüzeye çok hafif yukarýdan oturtmak için ofset.")]
    public float yOffset = 0.02f;

    private NavMeshAgent agent;

    private IEnumerator Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Ajaný geçici olarak kapat ki konumla rahat oynayalým.
        agent.enabled = false;

        // Bir frame bekle (NavMesh bake edilmiþ/geometri enable olmuþ olsun).
        yield return null;

        Vector3 origin = transform.position;
        if (NavMesh.SamplePosition(origin, out NavMeshHit hit, sampleRadius, areaMask))
        {
            // Tam NavMesh noktasý
            Vector3 onNav = hit.position + Vector3.up * yOffset;

            // Ajaný oraya koy ve etkinleþtir
            transform.position = onNav;
            agent.enabled = true;

            // Güvenlik: Baþlatýrken herhangi bir path kalýntýsýný sil
            if (agent.isOnNavMesh)
                agent.ResetPath();
            else
                Debug.LogError($"[SnapAgentToNavmesh] Agent hâlâ NavMesh üzerinde deðil! Pos={transform.position}");
        }
        else
        {
            Debug.LogError(
                $"[SnapAgentToNavmesh] Yakýnda NavMesh yok! Pos={origin}, Radius={sampleRadius}. " +
                "NavMesh Surface'ý yeniden bake et ya da sampleRadius'u artýr.");
        }
    }
}
