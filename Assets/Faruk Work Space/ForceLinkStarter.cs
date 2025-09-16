using UnityEngine;
using UnityEngine.AI;

public class ForceLinkStarter : MonoBehaviour
{
    public float triggerDistance = 1.0f;   // link startýna bu kadar yaklaþýnca
    public float nudge = 0.05f;            // çok küçük yukarý itme
    NavMeshAgent agent;

    void Awake() { agent = GetComponent<NavMeshAgent>(); }

    void Update()
    {
        if (!agent || !agent.enabled) return;
        if (agent.isOnOffMeshLink) return; // zaten linkte

        // En yakýn etkin OffMeshLink'i bul
        OffMeshLink nearest = null;
        float best = Mathf.Infinity;
        var links = FindObjectsOfType<OffMeshLink>(true);
        Vector3 p = transform.position;

        foreach (var l in links)
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy) continue;
            if (!l.activated) continue;
            Vector3 s = l.startTransform.position;
            Vector3 e = l.endTransform.position;
            // yukarý/asaðý baðlantý olsun, önemli deðil
            float d = Vector3.Distance(p, s);
            if (d < best) { best = d; nearest = l; }
        }

        if (nearest == null) return;

        // Yeterince yakýnýz ama bir türlü linke giremiyoruz: hafifçe link baþlangýcýna it
        if (best <= triggerDistance)
        {
            Vector3 start = nearest.startTransform.position;
            // start'ý çok az yukarý kaldýr ki navmesh'e gömülmesin
            start.y += nudge;
            agent.Warp(start);
            // Agent linki algýlasýn diye küçük bir repath
            agent.SetDestination(agent.destination);
        }
    }
}
