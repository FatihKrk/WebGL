// SmartLadderRouter.cs
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SmartLadderRouter : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;

    [Header("Planlama")]
    [Tooltip("Kaç saniyede bir rota karþýlaþtýrmasý yapýlýr.")]
    public float replanInterval = 0.25f;

    [Tooltip("Ajan etrafýnda merdiven linki arama yarýçapý (metre).")]
    public float ladderSearchRadius = 15f;

    [Tooltip("Merdiven (OffMeshLink) üzerinden 'metre baþýna' eklenen maliyet faktörü.")]
    public float linkMetersCost = 0.20f;

    private NavMeshAgent _agent;

    // durum
    private OffMeshLink _chosenLink;
    private bool _headingToLadder;
    private WaitForSeconds _wait;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        // Traversal'ý biz yapacaðýz (OffMeshLinkClimber).
        _agent.autoTraverseOffMeshLink = false;
        _wait = new WaitForSeconds(replanInterval);
    }

    void OnEnable()
    {
        StartCoroutine(ReplanLoop());
    }

    IEnumerator ReplanLoop()
    {
        while (enabled)
        {
            TryPlan();
            yield return _wait;
        }
    }

    void TryPlan()
    {
        if (target == null) return;

        // Merdivene gidiyorken linki geçtiysek asýl hedefe dön.
        if (_headingToLadder && _chosenLink != null)
        {
            Vector3 end = _chosenLink.endTransform != null ? _chosenLink.endTransform.position : _chosenLink.transform.position;
            if (NearXZ(transform.position, end, 1.5f))
            {
                _agent.SetDestination(target.position);
                _headingToLadder = false;
                _chosenLink = null;
                return;
            }
        }

        // 1) Düz rotanýn maliyeti
        float costDirect = PathLength(transform.position, target.position);
        if (float.IsInfinity(costDirect)) return;

        // 2) Yakýndaki en iyi merdiven üzerinden maliyet
        OffMeshLink best;
        float costViaLadder = EstimateViaLadder(out best);

        // 3) Karþýlaþtýr ve hedef koy
        if (best != null && costViaLadder + 0.01f < costDirect)
        {
            _chosenLink = best;
            _headingToLadder = true;

            Vector3 start = best.startTransform != null ? best.startTransform.position : best.transform.position;
            _agent.SetDestination(start); // önce linkin baþýna git
        }
        else if (!_headingToLadder)
        {
            _agent.SetDestination(target.position); // rampadan devam
        }
    }

    // Yakýndaki OffMeshLink'leri gezip: (agent->start) + (link uzunluðu * faktör) + (end->target) toplar.
    float EstimateViaLadder(out OffMeshLink best)
    {
        best = null;
        float bestCost = Mathf.Infinity;

        // Sahnedeki tüm linkler (aktif olanlar)
        var links = FindObjectsOfType<OffMeshLink>(true);
        Vector3 p = transform.position;

        foreach (var l in links)
        {
            if (l == null || !l.enabled || !l.gameObject.activeInHierarchy) continue;

            Vector3 s = l.startTransform != null ? l.startTransform.position : l.transform.position;
            Vector3 e = l.endTransform != null ? l.endTransform.position : l.transform.position;

            // Merdiven saymak için dikey fark anlamlý olsun
            if (Mathf.Abs(e.y - s.y) < 0.5f) continue;

            // Çok uzak linkleri ele
            if (DistXZ(p, s) > ladderSearchRadius) continue;

            float toStart = PathLength(p, s);
            if (float.IsInfinity(toStart)) continue;

            // Linkin 'metresine' maliyet uygula (dikey/çapraz uzunluk)
            float linkCost = Vector3.Distance(s, e) * Mathf.Max(0f, linkMetersCost);

            float toGoal = PathLength(e, target.position);
            if (float.IsInfinity(toGoal)) continue;

            float total = toStart + linkCost + toGoal;
            if (total < bestCost)
            {
                bestCost = total;
                best = l;
            }
        }

        return bestCost;
    }

    // NavMesh path uzunluðu (metre). Hesaplanamazsa sonsuz döner.
    float PathLength(Vector3 from, Vector3 to)
    {
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path) ||
            path.status == NavMeshPathStatus.PathInvalid ||
            path.corners == null || path.corners.Length < 2)
        {
            return Mathf.Infinity;
        }

        float len = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            len += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return len;
    }

    // -------- yardýmcýlar --------
    static bool NearXZ(Vector3 a, Vector3 b, float maxDist)
    {
        return DistXZ(a, b) <= maxDist;
    }

    static float DistXZ(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
