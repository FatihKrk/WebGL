using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(NavMeshAgent))]
public class ForceSnapToNavmesh : MonoBehaviour
{
    [Header("Baþlangýç hedefi (opsiyonel)")]
    public Transform initialTarget;

    [Header("NavMesh arama")]
    public float rayDownFrom = 50f;     // Yukarýdan aþaðý ray baþlangýcý
    public float rayDownDist = 200f;   // Ray mesafesi
    public float spiralStep = 2f;     // Spiral artým (metre)
    public float spiralMax = 200f;   // Maks arama yarýçapý
    public float sampleRange = 1.0f;   // SamplePosition yarýçapý
    public float yOffset = 0.02f;  // Zeminden hafif yukarý

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Agent'ý kapalý baþlatýyoruz ki NavMesh dýþýnda da konumu deðiþtirebilelim
        agent.enabled = false;
    }

    void Start()
    {
        // 1) Mevcut konum civarýnda zemin dene
        Vector3 pos = transform.position;
        if (Physics.Raycast(pos + Vector3.up * rayDownFrom, Vector3.down, out var hit, rayDownDist))
            pos = hit.point;

        // 2) En yakýn NavMesh noktasýný ara (önce bulunduðun yerde)
        if (!NavMesh.SamplePosition(pos, out var nm, sampleRange, NavMesh.AllAreas))
        {
            // Spiral tarama: yarýçapý artýrýp çevrede tarýyoruz
            bool found = false;
            for (float r = spiralStep; r <= spiralMax && !found; r += spiralStep)
            {
                for (float a = 0f; a < 360f; a += 15f)
                {
                    float rad = a * Mathf.Deg2Rad;
                    Vector3 p = pos + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * r;
                    if (NavMesh.SamplePosition(p, out nm, sampleRange, NavMesh.AllAreas))
                    {
                        pos = nm.position;
                        found = true;
                        break;
                    }
                }
            }
        }
        else
        {
            pos = nm.position;
        }

        // 3) Agent KAPALI iken pozisyonu doðrudan set et (Warp deðil!)
        transform.position = pos + Vector3.up * yOffset;

        // 4) Þimdi agent'ý aç
        agent.enabled = true;

        // 5) Hedef varsa yürü
        if (initialTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(initialTarget.position);
        }
    }
}

