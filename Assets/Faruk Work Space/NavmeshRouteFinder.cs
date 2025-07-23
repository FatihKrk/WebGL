using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavmeshRouteFinder : MonoBehaviour
{
    public Transform targetObject; // Takip edilecek obje
    private NavMeshAgent m_Agent;
    private Vector3 lastTargetPosition;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();

        if (targetObject != null)
            lastTargetPosition = targetObject.position;
    }

    void Update()
    {
        if (targetObject == null)
            return;

        // Hedef pozisyon yeterince deðiþmiþse ve NavMesh üzerinde ise
        if (Vector3.Distance(targetObject.position, lastTargetPosition) > 0.05f)
        {
            NavMeshHit hit;

            // Hedef pozisyon yakýnlarýnda geçerli bir NavMesh noktasý varsa (1 birimlik yarýçapta)
            if (NavMesh.SamplePosition(targetObject.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                // SetDestination sadece NavMesh üzerinden gidilebilen yerlerde çalýþýr
                m_Agent.SetDestination(hit.position);
                lastTargetPosition = targetObject.position;
            }
            else
            {
                // Hedef NavMesh dýþýnda, gidilmez
                // Debug.Log("Hedef NavMesh dýþýnda.");
            }
        }
    }
}
