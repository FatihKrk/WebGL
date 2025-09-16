using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

[RequireComponent(typeof(NavMeshAgent))]
public class NavMeshLinkClimber : MonoBehaviour
{
    [Header("Týrmanýþ")]
    public float climbSpeed = 2.0f;
    public float endYOffset = 0.02f;

    [Header("Dar tüpte sürtünmeyi önle")]
    public bool disableCollidersDuringClimb = true;

    [Header("NAVMESHLÝNK ONAR / HAZIRLA")]
    public bool repairLinksOnStart = true;
    public float linkSnapRadius = 2.0f;
    public bool forceBidirectional = true;
    public bool forceActivate = true;
    public bool autoUpdatePositions = true;

    [Header("DEBUG")]
    public bool debugMode = true;

    [Header("Rota Bulma")]
    public float repathInterval = 0.5f; // Rota yenileme sýklýðý
    public float verticalThreshold = 1.0f; // Yükseklik farký eþiði

    NavMeshAgent agent;
    Coroutine climbing;
    Collider[] cachedCols;
    bool[] cachedEnabled;

    // Rota yönetimi
    private Transform target;
    private Coroutine pathfindingCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false; // Manuel link geçiþi

        if (disableCollidersDuringClimb)
        {
            cachedCols = GetComponentsInChildren<Collider>(includeInactive: false);
            cachedEnabled = new bool[cachedCols.Length];
            for (int i = 0; i < cachedCols.Length; i++)
                cachedEnabled[i] = cachedCols[i].enabled;
        }

        if (debugMode) Debug.Log($"NavMeshLinkClimber initialized for {gameObject.name}");
    }

    void Start()
    {
        if (repairLinksOnStart)
            RepairAllNavMeshLinks();

        // Hedef bulma (örnek: "Target" tag'li obje)
        GameObject targetGO = GameObject.FindWithTag("Target");
        if (targetGO != null)
        {
            target = targetGO.transform;
            pathfindingCoroutine = StartCoroutine(PathfindingLoop());
        }
    }

    void Update()
    {
        // NavMeshLink üzerindeyse týrman
        if (agent.isOnOffMeshLink && climbing == null)
        {
            climbing = StartCoroutine(TraverseNavMeshLink());
        }
    }

    // Sürekli rota kontrolü
    IEnumerator PathfindingLoop()
    {
        while (target != null)
        {
            if (agent.enabled && !agent.isOnOffMeshLink && climbing == null)
            {
                SetDestinationSafely(target.position);
            }
            yield return new WaitForSeconds(repathInterval);
        }
    }

    // Güvenli hedef belirleme
    void SetDestinationSafely(Vector3 targetPos)
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            if (debugMode) Debug.LogWarning("Agent not ready for SetDestination");
            return;
        }

        // Hedef pozisyonu NavMesh üzerinde mi kontrol et
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, agent.areaMask))
        {
            // Yükseklik farkýný kontrol et
            float heightDiff = hit.position.y - transform.position.y;

            if (Mathf.Abs(heightDiff) > verticalThreshold)
            {
                if (debugMode) Debug.Log($"Height difference detected: {heightDiff:F2}m. Looking for vertical path.");

                // Merdiven/link arama sistemi burada çaðrýlabilir
                FindBestNavMeshLink(hit.position);
            }

            agent.SetDestination(hit.position);

            if (debugMode) Debug.Log($"Destination set to: {hit.position}");
        }
        else
        {
            if (debugMode) Debug.LogWarning($"Cannot sample position for: {targetPos}");
        }
    }

    // En uygun NavMeshLink'i bul
    void FindBestNavMeshLink(Vector3 targetPos)
    {
        NavMeshLink[] allLinks = FindObjectsOfType<NavMeshLink>();
        NavMeshLink bestLink = null;
        float closestDistance = float.MaxValue;

        foreach (var link in allLinks)
        {
            if (!link.enabled) continue;

            // Link'in start ve end pozisyonlarýný al
            Vector3 startWorld = link.transform.TransformPoint(link.startPoint);
            Vector3 endWorld = link.transform.TransformPoint(link.endPoint);

            // Agent'a en yakýn olan link'i bul
            float distToStart = Vector3.Distance(transform.position, startWorld);
            float distToEnd = Vector3.Distance(transform.position, endWorld);
            float minDist = Mathf.Min(distToStart, distToEnd);

            if (minDist < closestDistance)
            {
                closestDistance = minDist;
                bestLink = link;
            }
        }

        if (bestLink != null && debugMode)
        {
            Debug.Log($"Best link found at distance: {closestDistance:F2}m");
            Vector3 linkStart = bestLink.transform.TransformPoint(bestLink.startPoint);
            Debug.DrawLine(transform.position, linkStart, Color.yellow, 2f);
        }
    }

    IEnumerator TraverseNavMeshLink()
    {
        if (debugMode) Debug.Log("Starting NavMeshLink traversal");

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = data.startPos;
        Vector3 end = data.endPos + Vector3.up * endYOffset;

        // Collider'larý devre dýþý býrak
        if (disableCollidersDuringClimb && cachedCols != null)
        {
            for (int i = 0; i < cachedCols.Length; i++)
                cachedCols[i].enabled = false;
        }

        agent.isStopped = true;

        // Týrmanýþ animasyonu
        float length = Vector3.Distance(start, end);
        float estTime = Mathf.Max(0.1f, length / Mathf.Max(0.5f, climbSpeed));
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / estTime;
            transform.up = Vector3.up; // Rotasyonu sabitle
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Link geçiþini tamamla
        try
        {
            agent.CompleteOffMeshLink();
            if (debugMode) Debug.Log("OffMeshLink completed successfully");
        }
        catch (System.Exception e)
        {
            if (debugMode) Debug.LogError($"CompleteOffMeshLink failed: {e.Message}");
        }

        // Final pozisyon ayarla
        agent.Warp(end);
        agent.isStopped = false;

        // Collider'larý geri aç
        if (disableCollidersDuringClimb && cachedCols != null)
        {
            for (int i = 0; i < cachedCols.Length; i++)
                cachedCols[i].enabled = cachedEnabled[i];
        }

        climbing = null;

        if (debugMode) Debug.Log("NavMeshLink traversal completed");
    }

    void RepairAllNavMeshLinks()
    {
        var navLinks = FindObjectsOfType<NavMeshLink>(includeInactive: true);
        int repairedCount = 0;

        foreach (var link in navLinks)
        {
            // Agent type ID'yi eþitle
            if (link.agentTypeID != agent.agentTypeID)
                link.agentTypeID = agent.agentTypeID;

            // Ayarlarý zorla
            link.bidirectional = forceBidirectional;
            link.autoUpdate = autoUpdatePositions;

            if (forceActivate && !link.enabled)
                link.enabled = true;

            // Pozisyonlarý NavMesh'e snap et
            Vector3 sWorld = link.transform.TransformPoint(link.startPoint);
            Vector3 eWorld = link.transform.TransformPoint(link.endPoint);

            bool snappedStart = false, snappedEnd = false;

            if (NavMesh.SamplePosition(sWorld, out var hs, linkSnapRadius, NavMesh.AllAreas))
            {
                link.startPoint = link.transform.InverseTransformPoint(hs.position + Vector3.up * 0.02f);
                snappedStart = true;
            }

            if (NavMesh.SamplePosition(eWorld, out var he, linkSnapRadius, NavMesh.AllAreas))
            {
                link.endPoint = link.transform.InverseTransformPoint(he.position + Vector3.up * 0.02f);
                snappedEnd = true;
            }

            if (snappedStart || snappedEnd)
            {
                link.UpdateLink();
                repairedCount++;
            }
        }

        if (debugMode) Debug.Log($"Repaired {repairedCount} NavMeshLinks");
    }

    // Public method hedef deðiþtirmek için
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (pathfindingCoroutine != null)
            StopCoroutine(pathfindingCoroutine);

        if (target != null)
            pathfindingCoroutine = StartCoroutine(PathfindingLoop());
    }

    void OnDestroy()
    {
        if (pathfindingCoroutine != null)
            StopCoroutine(pathfindingCoroutine);
    }

    // Debug çizimleri
    void OnDrawGizmos()
    {
        if (!debugMode || agent == null) return;

        // Agent pozisyonu
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Hedef varsa çiz
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position, 0.3f);
            Gizmos.DrawLine(transform.position, target.position);
        }

        // Mevcut rota
        if (agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
                Gizmos.DrawLine(corners[i], corners[i + 1]);
        }
    }
}