using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class SimpleTestClimber : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;

    void Start()
    {
        Debug.Log("=== SCRIPT BAÞLADI ===");
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            Debug.LogError("NavMeshAgent bulunamadý!");
        else
            Debug.Log("NavMeshAgent bulundu OK");

        if (target == null)
        {
            GameObject targetObj = GameObject.Find("Navmesh Target");
            if (targetObj != null)
            {
                target = targetObj.transform;
                Debug.Log("Target otomatik bulundu: " + target.name);
            }
            else
                Debug.LogError("Target bulunamadý!");
        }
        else
        {
            Debug.Log("Target atanmýþ: " + target.name);
        }

        // Link sayýsýný kontrol et
        NavMeshLink[] links = FindObjectsOfType<NavMeshLink>();
        Debug.Log($"Toplam NavMeshLink sayýsý: {links.Length}");

        for (int i = 0; i < links.Length; i++)
        {
            Debug.Log($"Link {i}: {links[i].name}, Enabled: {links[i].enabled}, Active: {links[i].gameObject.activeInHierarchy}");
        }
    }

    void Update()
    {
        if (Time.frameCount % 120 == 0) // 2 saniyede bir
        {
            Debug.Log("=== UPDATE ÇALIÞIYOR ===");
            if (target != null)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                float height = target.position.y - transform.position.y;
                Debug.Log($"Mesafe: {dist:F1}, Yükseklik farký: {height:F1}");
                Debug.Log($"Agent pos: {transform.position}, Target pos: {target.position}");

            }
        }
    }
}