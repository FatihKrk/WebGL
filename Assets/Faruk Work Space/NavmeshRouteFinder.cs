using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

[RequireComponent(typeof(NavMeshAgent))]
public class NavmeshRouteFinder : MonoBehaviour
{
    [Header("Refs")]
    public Transform targetObject;
    public NavMeshAgent agent;

    [Header("Init")]
    public float initDelay = 0.5f;        // runtime bake/link için kısa bekleme
    public float initWarpRadius = 8f;     // başlangıçta NavMesh’e oturtma yarıçapı

    [Header("Repath")]
    public float repathInterval = 0.35f;  // daha çevik takip
    public float updateThreshold = 0.25f; // hedef bu kadar oynarsa repath

    [Header("Link seçimi")]
    public float ladderPickDeltaY = 0.4f; // hedef ajan’dan bu kadar yukarıdaysa link ara
    public float linkSearchRadius = 22f;  // ajanın etrafında link arama yarıçapı

    [Header("Hedefi yüzeye oturtma")]
    public float probeRadius = 1.5f;
    public float verticalStep = 1.0f;
    public int stepsDown = 2;
    public int stepsUp = 10;
    public float snapDistance = 1.0f;

    [Header("Güvenlik")]
    public float maxPathDistance = 250f;

    // Dahili
    NavMeshLink[] navLinks = System.Array.Empty<NavMeshLink>();
    Vector3 lastTargetPos, lastPickedOnMesh;
    float nextRepathTime;
    bool initialized;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!agent.enabled) agent.enabled = true;

        // NAVMESH LINK KULLANIYORUZ → otomatik geçiş açık olmalı
        agent.autoTraverseOffMeshLink = true;
        agent.isStopped = false;

        Debug.Log("[RouteFinder] Awake – NavMeshLink mode, autoTraverse=TRUE");
    }

    System.Collections.IEnumerator Start()
    {
        yield return new WaitForSeconds(initDelay);

        // Başlangıçta ajanı NavMesh’e oturt
        if (!IsOnMesh())
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, initWarpRadius, agent.areaMask))
                agent.Warp(hit.position);
        }

        // Linkleri önbelleğe al
        RefreshLinks();

        if (targetObject)
        {
            lastTargetPos = targetObject.position;
            Repath(force: true);
        }

        initialized = true;
        Debug.Log("[RouteFinder] Initialized");
    }

    void RefreshLinks()
    {
        navLinks = FindObjectsOfType<NavMeshLink>(includeInactive: true);
        Debug.Log($"[RouteFinder] Found {navLinks.Length} NavMeshLinks");
    }

    void Update()
    {
        if (!initialized || targetObject == null) return;
        if (!IsOnMesh()) return;

        if (Time.time >= nextRepathTime &&
            (targetObject.position - lastTargetPos).sqrMagnitude > updateThreshold * updateThreshold)
        {
            Repath(false);
        }

        // hedef çok yakınsa ajanı yüzeye oturt
        if (!agent.pathPending &&
            agent.pathStatus == NavMeshPathStatus.PathComplete &&
            agent.remainingDistance > 0f &&
            agent.remainingDistance <= snapDistance &&
            lastPickedOnMesh != Vector3.zero)
        {
            agent.Warp(lastPickedOnMesh);
        }
    }

    void Repath(bool force)
    {
        if (!IsOnMesh()) return;

        nextRepathTime = Time.time + repathInterval;
        lastTargetPos = targetObject.position;

        if (force || navLinks == null || navLinks.Length == 0)
            RefreshLinks();

        float dy = targetObject.position.y - agent.nextPosition.y;
        // Hedef bariz yukarıdaysa en yakın uygun NavMeshLink’i kullan
        if (dy > ladderPickDeltaY && TryRouteViaNearestNavLink(targetObject.position))
            return;

        // Aksi halde hedefe yükseklik bilinçli yaklaş
        SetDestinationSmart(targetObject.position);
    }

    bool TryRouteViaNearestNavLink(Vector3 goal)
    {
        if (navLinks == null || navLinks.Length == 0) return false;

        NavMeshLink best = null;
        float bestScore = float.MaxValue;
        Vector3 a = agent.nextPosition;

        foreach (var link in navLinks)
        {
            if (link == null || !link.enabled || !link.gameObject.activeInHierarchy) continue;
            // Agent type aynı olmalı
            if (link.agentTypeID != agent.agentTypeID) continue;

            // world koordinatlarına çevir
            Vector3 s = link.transform.TransformPoint(link.startPoint);
            Vector3 e = link.transform.TransformPoint(link.endPoint);

            // dikey fark anlamlı mı? (gerçekten yukarı taşıyan link)
            float climb = e.y - s.y;
            if (climb < 0.4f) continue;

            // ajana yakın mı?
            float dStart = Vector3.Distance(a, s);
            if (dStart > linkSearchRadius) continue;

            // ajandan link başlangıcına path var mı?
            if (!CanReach(a, s)) continue;

            // Skor: girişe yakınlık + end'den hedefe yakınlık + yükseklik uyumu
            float score = dStart
                        + Vector3.Distance(e, goal) * 0.25f
                        + Mathf.Abs(e.y - goal.y) * 0.3f;

            if (score < bestScore) { bestScore = score; best = link; }
        }

        if (best == null) return false;

        // KRİTİK: NavMeshLink'te ENDE hedef ver (Unity link üzerinden yolu kurar)
        Vector3 endWorld = best.transform.TransformPoint(best.endPoint);

        if (NavMesh.SamplePosition(endWorld, out var hit, 2.5f, agent.areaMask))
        {
            lastPickedOnMesh = hit.position;
            Debug.Log($"[RouteFinder] Using NavMeshLink '{best.name}' → end {hit.position}");
            return SafeSetDestination(hit.position);
        }
        return false;
    }

    bool SetDestinationSmart(Vector3 worldPos)
    {
        if (SetDestinationHeightAware(worldPos)) return true;

        // spiral örnekleme – hedef çevresi
        float[] rings = { 0.6f, 1.2f, 1.8f, 2.4f, 3.0f };
        int perRing = 12;

        for (int r = 0; r < rings.Length; r++)
        {
            float rad = rings[r];
            for (int i = -stepsDown; i <= stepsUp; i++)
            {
                float y = i * verticalStep;
                for (int k = 0; k < perRing; k++)
                {
                    float ang = (k / (float)perRing) * Mathf.PI * 2f;
                    Vector3 probe = worldPos + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * rad + Vector3.up * y;

                    if (NavMesh.SamplePosition(probe, out var hit, probeRadius, agent.areaMask) &&
                        CanReach(agent.nextPosition, hit.position))
                    {
                        lastPickedOnMesh = hit.position;
                        return SafeSetDestination(hit.position);
                    }
                }
            }
        }
        return false;
    }

    bool SetDestinationHeightAware(Vector3 worldPos)
    {
        // önce direkt nokta
        if (NavMesh.SamplePosition(worldPos, out var direct, probeRadius, agent.areaMask))
        {
            lastPickedOnMesh = direct.position;
            return SafeSetDestination(direct.position);
        }

        // yukarı/aşağı tarama
        Vector3 best = Vector3.zero; bool found = false; float bestDy = float.MaxValue;
        for (int i = -stepsDown; i <= stepsUp; i++)
        {
            Vector3 probe = worldPos + Vector3.up * (i * verticalStep);
            if (NavMesh.SamplePosition(probe, out var hit, probeRadius, agent.areaMask))
            {
                float dy = Mathf.Abs(hit.position.y - worldPos.y);
                if (dy < bestDy) { bestDy = dy; best = hit.position; found = true; }
            }
        }

        if (!found) return false;
        lastPickedOnMesh = best;
        return SafeSetDestination(best);
    }

    bool SafeSetDestination(Vector3 pos)
    {
        if (!IsOnMesh()) return false;

        // aşırı uzun mesafeleri ele
        if (Vector3.Distance(agent.nextPosition, pos) > maxPathDistance) return false;

        // önce path doğrula
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(agent.nextPosition, pos, agent.areaMask, path)) return false;
        if (path.status != NavMeshPathStatus.PathComplete) return false;

        agent.isStopped = false;
        bool ok = agent.SetDestination(pos);
        if (ok) Debug.Log($"[RouteFinder] SetDestination SUCCESS → {pos}");
        else Debug.LogWarning($"[RouteFinder] SetDestination FAILED → {pos}");
        return ok;
    }

    bool CanReach(Vector3 from, Vector3 to)
    {
        if (!NavMesh.SamplePosition(to, out var hit, 2.0f, agent.areaMask)) return false;
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(from, hit.position, agent.areaMask, path)) return false;
        return path.status == NavMeshPathStatus.PathComplete;
    }

    bool IsOnMesh()
    {
#if UNITY_2021_3_OR_NEWER
        return agent && agent.isOnNavMesh;
#else
        return NavMesh.SamplePosition(transform.position, out var _, 1f, agent.areaMask);
#endif
    }
}
