using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class LadderWaypointRouter : MonoBehaviour
{
    [Header("Zorlanacaðý hedef")]
    public Transform target;

    [Header("Ne zaman merdiven zorlanýr?")]
    [Tooltip("Hedef ile ajan arasýndaki düþey fark bu deðerden büyükse merdiven zorlanýr.")]
    public float heightDeltaToForce = 1.0f;

    [Header("Tekrar yol bulma")]
    public float repathInterval = 0.2f;
    public float arriveDistance = 0.35f;   // merdiven dibine varmýþ sayma eþiði

    NavMeshAgent agent;
    int areaLadder;
    float nextRepath;
    List<LadderNode> nodes = new List<LadderNode>();
    bool forcingToLadder;
    Vector3 forcedPoint;

    struct LadderNode
    {
        public OffMeshLink link;
        public Vector3 bottom; // düþük Y
        public Vector3 top;    // yüksek Y
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        areaLadder = NavMesh.GetAreaFromName("Ladder");
        RefreshNodes();
    }

    void OnEnable()
    {
        // sahnede linkler yeniden üretildiyse (Bake/Play baþý) güvene al
        RefreshNodes();
    }

    void RefreshNodes()
    {
        nodes.Clear();
        var links = FindObjectsOfType<OffMeshLink>(true);
        foreach (var l in links)
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy) continue;
            // Sadece Ladder alanýndakileri kullan (alan yoksa hepsini kabul et)
            if (areaLadder >= 0 && l.area != areaLadder) continue;

            var a = l.startTransform.position;
            var b = l.endTransform.position;
            LadderNode n = new LadderNode();
            if (a.y <= b.y) { n.bottom = a; n.top = b; }
            else { n.bottom = b; n.top = a; }
            n.link = l;

            // NavMesh'e oturt (küçük bir örnekleme)
            if (NavMesh.SamplePosition(n.bottom, out var hb, 1.0f, NavMesh.AllAreas))
                n.bottom = hb.position;
            if (NavMesh.SamplePosition(n.top, out var ht, 1.0f, NavMesh.AllAreas))
                n.top = ht.position;

            nodes.Add(n);
        }
    }

    void Update()
    {
        if (target == null || agent == null) return;

        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;
            Route();
        }
    }

    void Route()
    {
        float dy = target.position.y - agent.nextPosition.y;

        // Ajan OffMeshLink üzerindeyse, karýþma
        if (agent.isOnOffMeshLink) return;

        // Yukarý ZORLA
        if (dy > heightDeltaToForce)
        {
            var n = PickBestNode(upward: true);
            if (n != null)
            {
                forcedPoint = n.Value.bottom;
                forcingToLadder = true;
                agent.SetDestination(forcedPoint);
                return;
            }
        }
        // Aþaðý ZORLA
        else if (dy < -heightDeltaToForce)
        {
            var n = PickBestNode(upward: false);
            if (n != null)
            {
                forcedPoint = n.Value.top;
                forcingToLadder = true;
                agent.SetDestination(forcedPoint);
                return;
            }
        }

        // Zorlama yoksa normal hedefe dön
        forcingToLadder = false;
        agent.SetDestination(target.position);
    }

    LadderNode? PickBestNode(bool upward)
    {
        if (nodes.Count == 0) return null;

        // Basit fakat etkili: ulaþýlabilir olanlar içinden en düþük toplam maliyet
        // maliyet = ajan->giriþ XZ mesafesi + giriþ/çýkýþ sonrasý hedefe XZ mesafesi + dikey fark cezasý
        LadderNode? best = null;
        float bestScore = float.PositiveInfinity;

        foreach (var n in nodes)
        {
            Vector3 entry = upward ? n.bottom : n.top;
            Vector3 exit = upward ? n.top : n.bottom;

            // Hýzlý eriþilebilirlik testi
            if (!PathExists(agent.nextPosition, entry)) continue;

            float score =
                Horizontal(agent.nextPosition, entry) +
                Horizontal(exit, target.position) * 0.6f +        // merdivenden sonra hedefe yakýn olsun
                Mathf.Abs(target.position.y - exit.y) * 0.1f;     // hedef yüksekliðiyle uyum

            if (score < bestScore)
            {
                bestScore = score;
                best = n;
            }
        }
        return best;
    }

    bool PathExists(Vector3 from, Vector3 to)
    {
        var path = new NavMeshPath();
        return NavMesh.CalculatePath(from, to, agent.areaMask, path) && path.status == NavMeshPathStatus.PathComplete;
    }

    static float Horizontal(Vector3 a, Vector3 b)
    {
        a.y = b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // Merdiven dibine geldiysek hedefi tekrar asýl target yap
    void LateUpdate()
    {
        if (!forcingToLadder) return;
        if (Vector3.Distance(new Vector3(agent.nextPosition.x, 0, agent.nextPosition.z),
                             new Vector3(forcedPoint.x, 0, forcedPoint.z)) <= arriveDistance)
        {
            agent.SetDestination(target.position);
            // forcingToLadder flag'ini, linke bindiðinde Update döngüsü zaten tekrar ele alýr
        }
    }
}
