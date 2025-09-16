using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

public class HeightAwareRouteFinder : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;

    [Header("Heuristics")]
    public float minClimbDeltaY = 0.3f;       // daha toleranslı
    public float repathInterval = 0.30f;
    public float maxEntrySearchRadius = 40f;
    public float linkLengthWeight = 0.35f;
    public float detourPenaltyPerMeter = 0.10f;

    [Header("Sampling")]
    public float sampleProbeRadius = 3.5f;

    [Header("Handoff")]
    [Tooltip("Entry'ye bu yarıçapta gelince hedefi tekrar 'target'a çeviririz ki ajan linke bassın.")]
    public float entryReachRadius = 0.6f;     // agent.stoppingDistance'dan büyük olmalı (örn 0.1)

    float nextRepathTime;
    int lastTriedLinkId = -1;
    float lastLinkTriedTime = -10f;
    const float linkRetryCooldown = 1.0f;

    NavMeshPath _path;

    // son seçilen giriş noktası (handoff için)
    Vector3 _entry = Vector3.positiveInfinity;
    bool _entryActive = false;

    void Awake()
    {
        if (!agent) agent = GetComponentInChildren<NavMeshAgent>();
        if (agent) sampleProbeRadius = Mathf.Max(sampleProbeRadius, agent.radius + 0.2f);
        _path = new NavMeshPath();
    }

    void Update()
    {
        if (!agent || !agent.enabled || target == null) return;

        // 1) Entry'ye çok yaklaştıysak hedefi hemen tekrar 'target'a çevir → path linki içersin
        if (_entryActive && !agent.isOnOffMeshLink)
        {
            float dxz = Vector2.Distance(
                new Vector2(agent.transform.position.x, agent.transform.position.z),
                new Vector2(_entry.x, _entry.z));

            if (dxz <= entryReachRadius * 1.1f)
            {
                // handoff: şimdi hedefi gerçek hedefe çeviriyoruz
                if (SetDestSafe(target.position))
                    Debug.Log("[RouteFinder] ↪ handoff: entry reached, pushing path to target (with link)");
                _entryActive = false;
            }
        }

        // tırmanma/iniş sırasında bozma
        if (agent.isOnOffMeshLink) return;

        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        Vector3 aPos = agent.transform.position;
        Vector3 tPos = target.position;

        // hedef aynı/alt kattaysa düz git
        if ((tPos.y - aPos.y) < minClimbDeltaY)
        {
            SetDestSafe(tPos);
            return;
        }

        // yukarı bağlantı seç
        if (PickBestUpwardLink(aPos, tPos, out Vector3 entry, out int linkId))
        {
            if (linkId == lastTriedLinkId && (Time.time - lastLinkTriedTime) < linkRetryCooldown) return;

            _entry = entry;
            _entryActive = true;

            Debug.Log($"[RouteFinder] ✔ chosen link={linkId} entry={entry}");
            if (SetDestSafe(entry))
            {
                lastTriedLinkId = linkId;
                lastLinkTriedTime = Time.time;
            }
        }
        else
        {
            Debug.Log("[RouteFinder] ✖ no upward link kept → straight to target");
            SetDestSafe(tPos);
            _entryActive = false;
        }
    }

    // ---- Aday seçimi: OffMeshLink + NavMeshLink (yukarı yönlü) ----
    bool PickBestUpwardLink(Vector3 agentPos, Vector3 targetPos, out Vector3 bestEntry, out int bestId)
    {
        bestEntry = Vector3.zero; bestId = -1;
        float bestCost = float.PositiveInfinity;
        Vector2 agentXZ = new Vector2(agentPos.x, agentPos.z);

        int cand = 0, kept = 0;

#pragma warning disable 0618
        foreach (var l in FindObjectsOfType<OffMeshLink>(false))
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy || !l.startTransform || !l.endTransform) continue;
            Vector3 s = l.startTransform.position, e = l.endTransform.position;
            if ((e.y - s.y) < 0.3f) continue; // yukarı değil
            cand++;
            if ((new Vector2(s.x, s.z) - agentXZ).sqrMagnitude > maxEntrySearchRadius * maxEntrySearchRadius) continue;
            Evaluate(agentPos, targetPos, s, e, l.GetInstanceID(), ref bestCost, ref bestEntry, ref bestId, ref kept);
        }
#pragma warning restore 0618

        foreach (var l in FindObjectsOfType<NavMeshLink>(false))
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy) continue;

            Vector3 a = l.transform.TransformPoint(l.startPoint);
            Vector3 b = l.transform.TransformPoint(l.endPoint);

            bool abUp = (b.y - a.y) >= 0.3f;
            bool baUp = l.bidirectional && (a.y - b.y) >= 0.3f;

            if (abUp && (new Vector2(a.x, a.z) - agentXZ).sqrMagnitude <= maxEntrySearchRadius * maxEntrySearchRadius)
                Evaluate(agentPos, targetPos, a, b, l.GetInstanceID(), ref bestCost, ref bestEntry, ref bestId, ref kept);

            if (baUp && (new Vector2(b.x, b.z) - agentXZ).sqrMagnitude <= maxEntrySearchRadius * maxEntrySearchRadius)
                Evaluate(agentPos, targetPos, b, a, l.GetInstanceID(), ref bestCost, ref bestEntry, ref bestId, ref kept);

            if (abUp || baUp) cand++;
        }

        Debug.Log($"[RouteFinder] candidates={cand} kept={kept} best={(bestId != -1)}");
        return bestId != -1;
    }

    void Evaluate(Vector3 agentPos, Vector3 targetPos, Vector3 start, Vector3 end, int id,
                  ref float bestCost, ref Vector3 bestEntry, ref int bestId, ref int keptCount)
    {
        if (!TrySnap(ref start) || !TrySnap(ref end)) return;

        var any = new NavMeshQueryFilter { areaMask = ~0, agentTypeID = agent ? agent.agentTypeID : 0 };

        float distAtoStart = PathOrApprox(agentPos, start, any);
        float distEndToTarget = PathOrApprox(end, targetPos, any);
        float linkLen = Vector3.Distance(start, end);

        float cost = distAtoStart + distEndToTarget + linkLen * linkLengthWeight;
        cost += Mathf.Abs((targetPos.y - agentPos.y) - (end.y - agentPos.y)) * 0.25f;
        cost += (distAtoStart + distEndToTarget) * detourPenaltyPerMeter;

        keptCount++;
        if (cost < bestCost) { bestCost = cost; bestEntry = start; bestId = id; }
    }

    bool TrySnap(ref Vector3 p)
    {
        float[] rs = { sampleProbeRadius, 2.5f, 4.0f, 6.0f };
        foreach (var r in rs)
            if (NavMesh.SamplePosition(p, out var hit, r, ~0)) { p = hit.position; return true; }
        return false;
    }

    float PathOrApprox(Vector3 from, Vector3 to, NavMeshQueryFilter filter)
    {
        if (NavMesh.CalculatePath(from, to, filter, _path) && _path.status != NavMeshPathStatus.PathInvalid)
        {
            float len = 0f; var c = _path.corners;
            for (int i = 1; i < c.Length; i++) len += Vector3.Distance(c[i - 1], c[i]);
            return len;
        }
        return Vector3.Distance(from, to) * 1.5f;
    }

    bool SetDestSafe(Vector3 pos)
    {
        if (!agent || !agent.enabled || !agent.gameObject.activeInHierarchy) return false;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(agent.transform.position, out var hit0, sampleProbeRadius, ~0))
                agent.Warp(hit0.position);
            else return false;
        }

        if (NavMesh.SamplePosition(pos, out var hit, sampleProbeRadius, ~0))
        {
            agent.ResetPath();
            return agent.SetDestination(hit.position);
        }
        return false;
    }
}
