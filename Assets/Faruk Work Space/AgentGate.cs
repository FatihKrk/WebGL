using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentGate : MonoBehaviour
{
    [Header("Optional: Ýlk hedef")]
    public Transform initialTarget;        // Navmesh Target
    [Header("Oturma ayarlarý")]
    public float searchRadius = 8f;        // En yakýn NavMesh arama yarýçapý
    public float yOffset = 0.02f;     // Zemin üstüne hafif kaldýr

    NavMeshAgent agent;
    Behaviour[] siblings;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Agent üzerindeki diðer komponentleri geçici kapat (Agent + bu script hariç)
        siblings = GetComponents<Behaviour>();
        foreach (var b in siblings)
            if (b && b != agent && b != this)
                b.enabled = false;
    }

    void OnEnable()
    {
        StartCoroutine(Bootstrap());
    }

    System.Collections.IEnumerator Bootstrap()
    {
        // 1) Agent'ý kapat, pozisyonu düzelt
        agent.enabled = false;

        // Yukarýdan aþaðý raycast ile dünya zeminini bulmayý dene
        Vector3 p = transform.position;
        if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out var hit, 200f))
            p = hit.point + Vector3.up * yOffset;

        // 2) En yakýn NavMesh noktasýna "snap" et
        if (NavMesh.SamplePosition(p, out var nmHit, searchRadius, NavMesh.AllAreas))
            p = nmHit.position + Vector3.up * yOffset;

        // 3) Agent'ý aç ve o noktaya Warp et
        agent.enabled = true;
        agent.Warp(p);

        // 4) Gerçekten NavMesh üzerindeyiz mi? Kýsa bir süre bekle & doðrula
        float t = 0f;
        while (!agent.isOnNavMesh && (t += Time.deltaTime) < 2f)
            yield return null;

        // 5) Artýk güvenli: diðer komponentleri yeniden aç
        foreach (var b in siblings)
            if (b && b != agent && b != this)
                b.enabled = true;

        // 6) Hedef verilmiþse yürü
        if (initialTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(initialTarget.position);
        }
    }
}
