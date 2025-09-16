using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class ManualLinkClimber : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target; // Inspector'da atayabilirsin; boşsa "Navmesh Target" otomatik bulunur

    [Header("Link / Algılama")]
    public float detectionRadius = 20f;     // Ajan çevresinde link arama yarıçapı
    public float heightThreshold = 0.4f;    // Hedef bundan yukarıdaysa link ara
    public float linkApproachDistance = 0.25f; // Link alt ucuna yaklaşma eşiği

    [Header("Tırmanış")]
    public float climbSpeed = 2.5f;         // m/sn (lerp süresi = mesafe / hız)
    public bool debugMode = true;

    [Header("Path / NavMesh")]
    public float pathUpdateInterval = 0.25f; // Takip ve yaklaşım güncelleme hızı
    public float sampleRadius = 2.0f;        // SamplePosition yarıçapı

    private NavMeshAgent agent;
    private bool isClimbing = false;
    private bool isApproachingLink = false;
    private NavMeshLink currentLink;
    private Vector3 targetLinkPoint;         // NavMesh'te alt ucun snaplenmiş hali
    private float lastPathUpdate;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // NavMeshLink kullanıyoruz, tırmanışı biz yöneteceğiz:
        agent.autoTraverseOffMeshLink = false;

        if (target == null)
        {
            var t = GameObject.Find("Navmesh Target");
            if (t) target = t.transform;
        }

        if (debugMode)
        {
            Debug.Log("=== ManualLinkClimber ===");
            var links = FindObjectsOfType<NavMeshLink>(includeInactive: true);
            Debug.Log($"Scene NavMeshLink count: {links.Length}");
            foreach (var l in links)
                Debug.Log($"Link: {l.name}, enabled={l.enabled}, active={l.gameObject.activeInHierarchy}");
        }
    }

    void Update()
    {
        if (target == null || isClimbing) return;

        // belirli aralıklarla path güncelle
        if (Time.time - lastPathUpdate > pathUpdateInterval)
        {
            UpdateMovement();
            lastPathUpdate = Time.time;
        }

        // yaklaşım tetikleme: agent hedefine ulaştı mı?
        if (isApproachingLink && currentLink != null)
        {
            if (!agent.pathPending &&
                (agent.remainingDistance <= linkApproachDistance || Vector3.Distance(transform.position, targetLinkPoint) <= linkApproachDistance + 0.05f))
            {
                if (debugMode) Debug.Log($"[Climber] Link alt ucuna ulaştı → tırman!");
                StartCoroutine(ClimbLink(currentLink));
            }
        }
    }

    void UpdateMovement()
    {
        if (isApproachingLink) return;

        float heightDiff = target.position.y - transform.position.y;

        // hedef bariz yukarıdaysa link ara
        if (heightDiff > heightThreshold)
        {
            var best = FindBestClimbingLink();
            if (best != null)
            {
                SetLinkApproach(best);
                return;
            }
        }

        // normal takip (aynı katta vs.)
        FollowOnGround();
    }

    NavMeshLink FindBestClimbingLink()
    {
        var links = FindObjectsOfType<NavMeshLink>(includeInactive: false);
        NavMeshLink best = null;
        float bestScore = float.MaxValue;
        Vector3 a = agent.nextPosition;

        foreach (var link in links)
        {
            if (link == null || !link.enabled || !link.gameObject.activeInHierarchy) continue;
            if (link.agentTypeID != agent.agentTypeID) continue;

            // world koordinatları
            Vector3 s = link.transform.TransformPoint(link.startPoint);
            Vector3 e = link.transform.TransformPoint(link.endPoint);

            // aşağı/ yukarı ucunu ayır
            Vector3 lower = s.y <= e.y ? s : e;
            Vector3 upper = s.y <= e.y ? e : s;

            // gerçek bir yükselti sağlıyor mu?
            if (upper.y - lower.y < heightThreshold) continue;

            // ajana yakın mı?
            float dLower = Vector3.Distance(a, lower);
            if (dLower > detectionRadius) continue;

            // lower'ı navmesh'e snaple (aksi takdirde path başarısız)
            if (!NavMesh.SamplePosition(lower, out var lowerHit, sampleRadius, NavMesh.AllAreas))
                continue;

            // ajandan lower'a komple path var mı?
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(a, lowerHit.position, NavMesh.AllAreas, path)) continue;
            if (path.status != NavMeshPathStatus.PathComplete) continue;

            // skor: girişe yakınlık + üst uçtan hedefe yakınlık + yükseklik uyumu
            float score = dLower + Vector3.Distance(upper, target.position) * 0.25f + Mathf.Abs(upper.y - target.position.y) * 0.3f;

            if (score < bestScore)
            {
                bestScore = score;
                best = link;
            }
        }

        if (debugMode) Debug.Log(best ? $"[Climber] En iyi NavMeshLink: {best.name}" : "[Climber] Uygun NavMeshLink yok");
        return best;
    }

    void SetLinkApproach(NavMeshLink link)
    {
        // world start/end
        Vector3 s = link.transform.TransformPoint(link.startPoint);
        Vector3 e = link.transform.TransformPoint(link.endPoint);

        // alt ucu belirle
        Vector3 lower = s.y <= e.y ? s : e;

        // navmesh'e snaple
        if (!NavMesh.SamplePosition(lower, out var hit, sampleRadius, NavMesh.AllAreas))
            return;

        targetLinkPoint = hit.position;
        currentLink = link;
        isApproachingLink = true;

        agent.stoppingDistance = linkApproachDistance * 0.5f;
        agent.isStopped = false;
        agent.SetDestination(targetLinkPoint);

        if (debugMode) Debug.Log($"[Climber] Link'e yaklaş: {link.name} → {targetLinkPoint}");
    }

    void FollowOnGround()
    {
        if (agent.pathPending) return;

        // hedefi navmesh'e snaple
        if (NavMesh.SamplePosition(target.position, out var hit, 3f, NavMesh.AllAreas))
        {
            // çok yakınsa gerek yok
            if (Vector3.Distance(agent.nextPosition, hit.position) < 0.2f) return;

            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(agent.nextPosition, hit.position, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                agent.isStopped = false;
                agent.SetDestination(hit.position);
            }
        }
    }

    IEnumerator ClimbLink(NavMeshLink link)
    {
        isClimbing = true;
        isApproachingLink = false;

        // world start/end ve yön
        Vector3 s = link.transform.TransformPoint(link.startPoint);
        Vector3 e = link.transform.TransformPoint(link.endPoint);
        Vector3 lower = s.y <= e.y ? s : e;
        Vector3 upper = s.y <= e.y ? e : s;

        if (debugMode) Debug.Log($"[Climber] Tırmanış: {lower} -> {upper}");

        // Agent'i kapat, pozisyonu biz yönetelim
        agent.isStopped = true;
        agent.enabled = false;

        // alt uca yumuşak yaklaş (0.2m hata toleransı)
        float t = 0f;
        Vector3 startPos = transform.position;
        float approachTime = Mathf.Clamp(Vector3.Distance(startPos, lower) / 3f, 0.05f, 0.6f);
        while (t < 1f)
        {
            t += Time.deltaTime / approachTime;
            transform.position = Vector3.Lerp(startPos, lower, t);
            yield return null;
        }

        // tırmanış
        float climbDist = Vector3.Distance(lower, upper);
        float climbTime = Mathf.Max(0.1f, climbDist / Mathf.Max(0.2f, climbSpeed));
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / climbTime;
            // dikey eksende daha akıcı
            float sCurve = Mathf.SmoothStep(0f, 1f, t);
            Vector3 pos = Vector3.Lerp(lower, upper, sCurve);
            transform.position = pos;
            yield return null;
        }

        // üst ucu navmesh'e snaple, agent'i yeniden aç ve hedefe devam et
        Vector3 final = upper;
        if (NavMesh.SamplePosition(upper, out var upHit, 2.0f, NavMesh.AllAreas))
            final = upHit.position;

        agent.enabled = true;
        agent.Warp(final);
        agent.isStopped = false;

        // hemen hedeften devam et
        if (target != null && NavMesh.SamplePosition(target.position, out var goal, 3f, NavMesh.AllAreas))
        {
            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(agent.nextPosition, goal.position, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(goal.position);
            }
        }

        currentLink = null;
        isClimbing = false;
        if (debugMode) Debug.Log("[Climber] Tırmanış tamamlandı");
        yield return null;
    }

    void OnDrawGizmosSelected()
    {
        // arama yarıçapı
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (currentLink != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetLinkPoint, 0.25f);
        }

        // sahnedeki linkleri çiz
        var links = FindObjectsOfType<NavMeshLink>();
        foreach (var link in links)
        {
            if (!link.enabled) continue;
            Vector3 s = link.transform.TransformPoint(link.startPoint);
            Vector3 e = link.transform.TransformPoint(link.endPoint);
            Gizmos.color = (e.y > s.y) ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(s, 0.18f);
            Gizmos.DrawWireSphere(e, 0.18f);
            Gizmos.DrawLine(s, e);
        }
    }
}
