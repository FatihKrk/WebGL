using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HardLadderForce : MonoBehaviour
{
    public Transform target;          // Navmesh Target
    public float triggerRadius = 1.6f; // Ajana en yakýn merdiveni algýlama yarýçapý (yatay)
    public float minUpDelta = 0.5f;    // Yukarý zorlamak için min. yükseklik farký
    public float minDownDelta = 0.5f;  // Aþaðý zorlamak için min. yükseklik farký
    public float climbSpeed = 2.0f;    // Týrmanma hýzý (m/sn)
    public string ladderAreaName = "Ladder";

    NavMeshAgent agent;
    OffMeshLink[] links;
    int ladderArea;
    bool busy;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ladderArea = NavMesh.GetAreaFromName(ladderAreaName);
        links = FindObjectsOfType<OffMeshLink>(true); // sahnedeki tüm linkler
    }

    void Update()
    {
        if (busy || target == null) return;

        float dy = target.position.y - transform.position.y;
        if (Mathf.Abs(dy) < (dy > 0 ? minUpDelta : minDownDelta)) return;

        // Ajana en uygun (yakýn) Ladder linkini bul
        OffMeshLink best = null;
        float bestScore = float.PositiveInfinity;
        Vector3 p = transform.position;
        foreach (var l in links)
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy) continue;
            if (ladderArea >= 0 && l.area != ladderArea) continue;

            Vector3 a = l.startTransform.position;
            Vector3 b = l.endTransform.position;
            bool upLink = b.y > a.y;

            // Yön uygun mu?
            if (dy > 0 && !upLink) continue;   // yukarý gitmek istiyoruz ama link aþaðý
            if (dy < 0 && upLink) continue;    // aþaðý gitmek istiyoruz ama link yukarý

            Vector3 start = dy > 0 ? a : b; // bize yakýn baþlangýç ucu
            float horiz = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(start.x, start.z));
            if (horiz > triggerRadius) continue;

            float score = horiz + Mathf.Abs(start.y - p.y) * 0.25f;
            if (score < bestScore) { bestScore = score; best = l; }
        }

        if (best != null)
            StartCoroutine(Climb(best, dy > 0));
    }

    IEnumerator Climb(OffMeshLink link, bool goingUp)
    {
        busy = true;

        Vector3 a = link.startTransform.position;
        Vector3 b = link.endTransform.position;
        Vector3 start = goingUp ? a : b;
        Vector3 end = goingUp ? b : a;

        // Ajan kontrolünü býrak ve tam link baþlangýcýna al
        agent.ResetPath();
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;
        transform.position = start;

        // Basit doðrusal týrmanma (link boyunca)
        float dist = Vector3.Distance(start, end);
        float dur = Mathf.Max(0.01f, dist / Mathf.Max(0.1f, climbSpeed));
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Ajaný link sonuna “teslim et” ve hedefe devam et
        agent.Warp(end);
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;
        if (target != null) agent.SetDestination(target.position);

        busy = false;
    }
}
