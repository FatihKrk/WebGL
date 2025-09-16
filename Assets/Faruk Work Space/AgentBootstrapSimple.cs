using System.Reflection;                // reflection için
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;              // NavMeshSurface

[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentBootstrapSimple : MonoBehaviour
{
    [Header("Sahnedeki NavMesh Surface'i sürükle")]
    public NavMeshSurface surface;

    [Header("Takip edilecek hedef")]
    public Transform target;

    [Header("NavMesh bekleme & konumlama")]
    public float waitNavmeshTimeout = 10f;
    public float searchRadius = 50f;
    public float ySnap = 0.02f;

    NavMeshAgent agent;
    NavMeshPath tmpPath;
    float repathTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        tmpPath = new NavMeshPath();
        agent.enabled = false;
        agent.isStopped = true;
    }

    void OnEnable() => StartCoroutine(Bootstrap());

    System.Collections.IEnumerator Bootstrap()
    {
        // 0) Surface yoksa sahneden bul
        if (surface == null)
            surface = FindObjectOfType<NavMeshSurface>();

        // 1) Gerekirse runtime bake et
        bool needBake = (NavMesh.CalculateTriangulation().vertices.Length == 0) ||
                        (surface != null && surface.navMeshData == null);

        if (surface != null && needBake)
        {
            Debug.Log("[Bootstrap] NavMesh yok, runtime bake baþlýyor…");
            TryBuildSurface(surface);    // <-- ASYNC varsa reflection ile çaðýr, yoksa BuildNavMesh
        }

        // 2) NavMesh oluþana kadar bekle (en çok waitNavmeshTimeout)
        float t = 0f;
        while (NavMesh.CalculateTriangulation().vertices.Length == 0 && t < waitNavmeshTimeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (NavMesh.CalculateTriangulation().vertices.Length == 0)
        {
            Debug.LogError("[Bootstrap] Süre bitti: NavMesh hâlâ yok. Surface Layer Mask / geometry / slope ayarlarýný kontrol et.");
            yield break;
        }

        // 3) AgentType eþitle
        if (surface != null)
            agent.agentTypeID = surface.agentTypeID;

        // 4) Ajaný geçerli yüzeye yapýþtýr
        Vector3 pos = transform.position;
        if (NavMesh.SamplePosition(pos, out var hit, searchRadius, NavMesh.AllAreas))
            pos = hit.position + Vector3.up * ySnap;

        agent.enabled = true;

        if (!agent.Warp(pos))
        {
            Debug.LogError("[Bootstrap] Warp baþarýsýz. AgentType ile Surface AgentType eþleþmiyor olabilir.");
            yield break;
        }

        agent.isStopped = false;

        // 5) Hedef varsa yürüt ve periyodik repath yap
        if (target != null)
            SetDestinationSafe(target.position);

        Debug.Log("[Bootstrap] Agent hazýr ve NavMesh üzerinde.");
    }

    void Update()
    {
        if (!agent.enabled || agent.isStopped || target == null) return;

        repathTimer += Time.deltaTime;
        if (repathTimer >= 0.25f)
        {
            repathTimer = 0f;
            SetDestinationSafe(target.position);
        }
    }

    void SetDestinationSafe(Vector3 dest)
    {
        if (NavMesh.SamplePosition(dest, out var hit, searchRadius, NavMesh.AllAreas))
            dest = hit.position;

        if (NavMesh.CalculatePath(agent.transform.position, dest, NavMesh.AllAreas, tmpPath) &&
            tmpPath.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(tmpPath);
        }
        else
        {
            agent.SetDestination(dest);
        }
    }

    // ---- Yardýmcý: BuildNavMeshAsync varsa reflection ile çaðýr; yoksa BuildNavMesh kullan ----
    static void TryBuildSurface(NavMeshSurface s)
    {
        MethodInfo asyncMethod = typeof(NavMeshSurface).GetMethod(
            "BuildNavMeshAsync",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (asyncMethod != null)
        {
            asyncMethod.Invoke(s, null);
        }
        else
        {
            s.BuildNavMesh();
        }
    }
}
