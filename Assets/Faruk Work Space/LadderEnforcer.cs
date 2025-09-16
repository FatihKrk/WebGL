using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(10)]
public class LadderEnforcer : MonoBehaviour
{
    public Transform target;
    [Header("Ne zaman devreye girsin?")]
    public float heightDeltaToForce = 1.0f; // hedef ile arandaki dikey fark
    public bool alsoForceDown = true;

    [Header("Arama ve yaklaþým")]
    public float searchRadius = 4.0f;       // merdiven arama yarýçapý
    public float arriveDistance = 0.35f;    // giriþte kabul mesafesi
    public float repathInterval = 0.25f;    // tekrar hedef belirleme

    NavMeshAgent agent;
    int ladderArea, walkableArea;
    float nextTick;
    bool forcing;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ladderArea = NavMesh.GetAreaFromName("Ladder");
        walkableArea = NavMesh.GetAreaFromName("Walkable");
    }

    void Update()
    {
        if (!agent || !target) return;
        if (Time.time < nextTick) return;
        nextTick = Time.time + repathInterval;

        float dy = target.position.y - transform.position.y;
        bool wantUp = dy > heightDeltaToForce;
        bool wantDn = dy < -heightDeltaToForce && alsoForceDown;

        if (!(wantUp || wantDn))
        {
            if (forcing) { agent.SetAreaCost(walkableArea, 1f); forcing = false; }
            return;
        }

        // Yakýndaki en iyi "Ladder" linki bul
        OffMeshLink best = null;
        float bestDist = float.MaxValue;
        Vector3 entry = default, exit = default;

        foreach (var l in FindObjectsOfType<OffMeshLink>())
        {
            if (!l.enabled || !l.activated) continue;
            if (l.area != ladderArea) continue;

            Vector3 a = l.startTransform ? l.startTransform.position : l.transform.position;
            Vector3 b = l.endTransform ? l.endTransform.position : l.transform.position;

            bool linkGoesUp = b.y > a.y;
            if (wantUp && !linkGoesUp) continue;
            if (wantDn && linkGoesUp) continue;

            float da = Vector3.Distance(transform.position, a);
            float db = Vector3.Distance(transform.position, b);
            float d = Mathf.Min(da, db);
            if (d > searchRadius) continue;

            if (d < bestDist)
            {
                bestDist = d; best = l;
                bool useA = da < db;
                entry = useA ? a : b;
                exit = useA ? b : a;
            }
        }

        if (best)
        {
            // Zorla: rampayý “pahalý” yap, linkin diðer ucunu hedefle
            agent.SetAreaCost(walkableArea, 100f);
            forcing = true;
            Vector3 goal = exit + Vector3.up * 0.02f;
            agent.SetDestination(goal);
        }
        else if (forcing)
        {
            // Yakýnda merdiven yok -> normale dön
            agent.SetAreaCost(walkableArea, 1f);
            forcing = false;
        }
    }
}
