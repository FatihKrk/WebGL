using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

[RequireComponent(typeof(NavMeshAgent))]
public class OffMeshLinkClimber : MonoBehaviour
{
    [Header("Tırmanış")]
    public float climbSpeed = 2.0f;
    public float endYOffset = 0.02f;

    [Header("Dar tüpte sürtünmeyi önle")]
    public bool disableCollidersDuringClimb = true;

    [Header("LINK ONAR / HAZIRLA")]
    public bool repairLinksOnStart = true;
    public float linkSnapRadius = 2.0f;
    public bool forceBidirectional = true;
    public bool forceActivate = true;
    public bool autoUpdatePositions = true;

    [Header("DEBUG")]
    public bool debugMode = true;

    NavMeshAgent agent;
    Coroutine climbing;
    Collider[] cachedCols;
    bool[] cachedEnabled;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;

        if (disableCollidersDuringClimb)
        {
            cachedCols = GetComponentsInChildren<Collider>(includeInactive: false);
            cachedEnabled = new bool[cachedCols.Length];
            for (int i = 0; i < cachedCols.Length; i++)
                cachedEnabled[i] = cachedCols[i].enabled;
        }

        if (debugMode) Debug.Log($"OffMeshLinkClimber initialized for {gameObject.name}");
    }

    void Start()
    {
        if (repairLinksOnStart)
            RepairAllLinks();
    }

    void Update()
    {
        if (agent.isOnOffMeshLink && climbing == null)
            climbing = StartCoroutine(TraverseLink());
    }

    IEnumerator TraverseLink()
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = data.startPos;
        Vector3 end = data.endPos + Vector3.up * endYOffset;

        if (disableCollidersDuringClimb && cachedCols != null)
            for (int i = 0; i < cachedCols.Length; i++) cachedCols[i].enabled = false;

        agent.isStopped = true;

        float length = Vector3.Distance(start, end);
        float estTime = Mathf.Max(0.1f, length / Mathf.Max(0.5f, climbSpeed));
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / estTime;
            transform.up = Vector3.up;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        try { agent.CompleteOffMeshLink(); } catch { }
        agent.Warp(end);
        agent.isStopped = false;

        if (disableCollidersDuringClimb && cachedCols != null)
            for (int i = 0; i < cachedCols.Length; i++) cachedCols[i].enabled = cachedEnabled[i];

        climbing = null;
    }

    void RepairAllLinks()
    {
        // OffMeshLink'ler
        var offLinks = FindObjectsOfType<OffMeshLink>(includeInactive: true);
        foreach (var link in offLinks)
        {
            if (!link.startTransform || !link.endTransform) continue;

            if (NavMesh.SamplePosition(link.startTransform.position, out var hs, linkSnapRadius, NavMesh.AllAreas))
                link.startTransform.position = hs.position + Vector3.up * 0.02f;

            if (NavMesh.SamplePosition(link.endTransform.position, out var he, linkSnapRadius, NavMesh.AllAreas))
                link.endTransform.position = he.position + Vector3.up * 0.02f;

            TrySetAgentTypeID(link, agent.agentTypeID);

            if (forceActivate) link.activated = true;
            if (forceBidirectional) link.biDirectional = true;
            link.autoUpdatePositions = autoUpdatePositions;
        }

        // NavMeshLink'ler
        var navLinks = FindObjectsOfType<NavMeshLink>(includeInactive: true);
        foreach (var link in navLinks)
        {
            if (link.agentTypeID != agent.agentTypeID)
                link.agentTypeID = agent.agentTypeID;

            link.bidirectional = forceBidirectional;   // doğru API
            link.autoUpdate = autoUpdatePositions;   // doğru API
            if (forceActivate && !link.enabled) link.enabled = true;

            // start/end local space — world snap
            Vector3 sWorld = link.transform.TransformPoint(link.startPoint);
            Vector3 eWorld = link.transform.TransformPoint(link.endPoint);
            if (NavMesh.SamplePosition(sWorld, out var hs, linkSnapRadius, NavMesh.AllAreas))
                link.startPoint = link.transform.InverseTransformPoint(hs.position);
            if (NavMesh.SamplePosition(eWorld, out var he, linkSnapRadius, NavMesh.AllAreas))
                link.endPoint = link.transform.InverseTransformPoint(he.position);

            link.UpdateLink();
        }
    }

    void TrySetAgentTypeID(OffMeshLink link, int typeId)
    {
        var f = link.GetType().GetField("agentTypeID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null) { f.SetValue(link, typeId); return; }
        var p = link.GetType().GetProperty("agentTypeID", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.CanWrite) { p.SetValue(link, typeId, null); return; }
    }
}
