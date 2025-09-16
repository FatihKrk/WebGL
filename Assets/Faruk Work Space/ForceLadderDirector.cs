using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(50)]
public class ForceLadderDirector : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;

    [Header("Ne zaman merdiveni zorla?")]
    public float floorDelta = 1.0f;      // hedef ile yükseklik farký > ise merdiven
    public float arriveDist = 0.35f;     // link giriþine varmýþ sayma mesafesi
    public float refresh = 0.10f;        // karar yenileme aralýðý (sn)

    int ladderArea;
    List<OffMeshLink> ladderLinks = new List<OffMeshLink>();
    OffMeshLink current;

    enum State { Free, ToStart, Crossing, ToEnd }
    State state = State.Free;
    float nextTick;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        ladderArea = NavMesh.GetAreaFromName("Ladder");

        // Ajaný biraz çevik yap
        agent.speed = Mathf.Max(agent.speed, 3.5f);
        agent.acceleration = Mathf.Max(agent.acceleration, 12f);
        agent.angularSpeed = Mathf.Max(agent.angularSpeed, 120f);
        agent.autoBraking = false;
    }

    void OnEnable()
    {
        CacheLinks();
        ReleaseBias();
    }

    void CacheLinks()
    {
        ladderLinks.Clear();
        foreach (var l in FindObjectsOfType<OffMeshLink>(true))
            if (l && l.activated && l.area == ladderArea)
                ladderLinks.Add(l);
    }

    void Update()
    {
        if (!agent || !target) return;
        if (Time.time < nextTick) return;
        nextTick = Time.time + refresh;

        if (agent.isOnOffMeshLink) { state = State.Crossing; return; }

        float dz = Mathf.Abs(target.position.y - agent.transform.position.y);

        switch (state)
        {
            case State.Free:
                if (dz > floorDelta)
                {
                    var best = ChooseBestLink(target.position.y, agent.transform.position);
                    if (best != null)
                    {
                        current = best;
                        Vector3 start = GetStartPoint(best, goingUp: agent.transform.position.y < target.position.y);
                        BiasForLadder();                        // Walkable pahalý, Ladder ucuz
                        agent.SetDestination(start);
                        state = State.ToStart;
                    }
                }
                else
                {
                    ReleaseBias();
                    agent.SetDestination(target.position);
                }
                break;

            case State.ToStart:
                if (Vector3.Distance(agent.transform.position, agent.destination) <= arriveDist)
                    NudgeTowards(agent, agent.destination);     // Link giriþine küçük itme

                if (agent.isOnOffMeshLink) state = State.Crossing;
                if (dz <= floorDelta) { ReleaseBias(); state = State.Free; }
                break;

            case State.Crossing:
                // Link üzerindeyken Climber devrede; geçiþ bitince ToEnd'e
                if (!agent.isOnOffMeshLink)
                {
                    if (current)
                    {
                        var end = GetEndPoint(current, goingUp: agent.transform.position.y < target.position.y);
                        Vector3 push = (target.position - end); push.y = 0;
                        agent.SetDestination(end + push.normalized * 0.5f);
                    }
                    state = State.ToEnd;
                }
                break;

            case State.ToEnd:
                if (Vector3.Distance(agent.transform.position, agent.destination) <= arriveDist)
                {
                    ReleaseBias();
                    state = State.Free;
                }
                break;
        }
    }

    void BiasForLadder()
    {
        // Walkable'ý pahalý yap, Ladder'ý çok ucuz yap.
        NavMesh.SetAreaCost(NavMesh.GetAreaFromName("Walkable"), 50f);
        NavMesh.SetAreaCost(ladderArea, 0.05f);
    }

    void ReleaseBias()
    {
        NavMesh.SetAreaCost(NavMesh.GetAreaFromName("Walkable"), 1f);
        NavMesh.SetAreaCost(ladderArea, 1f);
    }

    OffMeshLink ChooseBestLink(float targetY, Vector3 from)
    {
        OffMeshLink best = null;
        float bestScore = float.PositiveInfinity;

        foreach (var l in ladderLinks)
        {
            if (!l || !l.activated) continue;

            Vector3 a = (l.startTransform != null) ? l.startTransform.position : l.transform.position;
            Vector3 b = (l.endTransform != null) ? l.endTransform.position : l.transform.position;
            Vector3 bot = a.y < b.y ? a : b;
            Vector3 top = a.y < b.y ? b : a;

            float floorMatch = Mathf.Abs(top.y - targetY);     // hedef katýna yakýnlýk
            float d = Vector3.Distance(from, bot);             // ajana yakýnlýk
            float score = floorMatch * 10f + d;                // aðýrlýklandýrýlmýþ skor

            if (score < bestScore) { bestScore = score; best = l; }
        }
        return best;
    }

    Vector3 GetStartPoint(OffMeshLink l, bool goingUp)
    {
        Vector3 a = (l.startTransform != null) ? l.startTransform.position : l.transform.position;
        Vector3 b = (l.endTransform != null) ? l.endTransform.position : l.transform.position;
        Vector3 bot = a.y < b.y ? a : b;
        Vector3 top = a.y < b.y ? b : a;
        return goingUp ? bot : top;   // çýkýyorsak alt uca, iniyorsak üst uca
    }

    Vector3 GetEndPoint(OffMeshLink l, bool goingUp)
    {
        Vector3 a = (l.startTransform != null) ? l.startTransform.position : l.transform.position;
        Vector3 b = (l.endTransform != null) ? l.endTransform.position : l.transform.position;
        Vector3 bot = a.y < b.y ? a : b;
        Vector3 top = a.y < b.y ? b : a;
        return goingUp ? top : bot;
    }

    static void NudgeTowards(NavMeshAgent ag, Vector3 pos)
    {
        var dir = pos - ag.transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f) ag.Move(dir.normalized * 0.10f);
    }
}

