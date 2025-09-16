using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

// Ajanı: (1) uygun Ladder linkinin START noktasına götürür,
// (2) linki geçerken OffMeshLink akışı çalışır,
// (3) çıkınca tekrar hedefe set eder. Rampaya "kısa" olsa bile kaçmaz.
[RequireComponent(typeof(NavMeshAgent))]
public class LadderDirector : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;                // Navmesh Target (Inspector'dan ver)
    public NavMeshAgent agent;              // Boşsa Awake'te bulunur

    [Header("Davranış")]
    public bool forceUseLadders = true;     // Yukarı/Aşağı farkı varsa merdiven zorunlu
    public float repathInterval = 0.25f;    // Kaç sn'de bir planlama
    public float arriveDistance = 0.35f;    // Link start’ta durma eşiği
    public float startHeightTolerance = 0.8f;// Başlangıç yüksekliği toleransı
    public float minVerticalGain = 1.0f;    // "Yukarı" sayılması için min fark
    public float minVerticalDrop = 0.8f;    // "Aşağı" sayılması için min fark
    public float searchRadius = 12f;        // Aday link arama yarıçapı (XZ)
    public float planBias = 0.6f;           // maliyet: d(agent→start) + bias*d(end→hedef)

    readonly List<NavMeshLink> _ladderLinks = new();
    NavMeshLink _activeLink;
    bool _wasOnLink;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        CollectLadderLinks();
        StartCoroutine(PlannerLoop());
    }

    void CollectLadderLinks()
    {
        _ladderLinks.Clear();
        int ladderArea = NavMesh.GetAreaFromName("Ladder");
        foreach (var link in FindObjectsOfType<NavMeshLink>(true))
        {
            if (!link || !link.isActiveAndEnabled) continue;
            if (ladderArea >= 0 && link.area != ladderArea) continue; // sadece Ladder area
            if (Mathf.Abs(link.endPoint.y - link.startPoint.y) > 0.2f) // net dikey fark olsun
                _ladderLinks.Add(link);
        }
        // Debug.Log($"[LadderDirector] Ladder link sayısı: {_ladderLinks.Count}");
    }

    IEnumerator PlannerLoop()
    {
        var wait = new WaitForSeconds(repathInterval);
        while (true)
        {
            Tick();
            yield return wait;
        }
    }

    void Tick()
    {
        if (!agent || !target) return;

        // Link üzerinde ise tırmanış/iniş animasyonu akmaya devam etsin.
        if (agent.isOnOffMeshLink)
        {
            _wasOnLink = true;
            return;
        }

        // Linkten yeni çıktıysak hedefe geri dön.
        if (_wasOnLink && !agent.isOnOffMeshLink)
        {
            _wasOnLink = false;
            _activeLink = null;
            agent.stoppingDistance = 0f;
            agent.SetDestination(target.position);
            return;
        }

        Vector3 aPos = agent.transform.position;
        Vector3 tPos = target.position;
        float dv = tPos.y - aPos.y;

        // Aynı kat gibi ise normal takip (merdiven zorunlu değil)
        if (!forceUseLadders || Mathf.Abs(dv) < 0.6f)
        {
            if (_activeLink == null && agent.destination != tPos)
                agent.SetDestination(tPos);
            return;
        }

        bool goingUp = dv > 0f;
        var best = ChooseBestLink(aPos, tPos, goingUp);

        if (best == null)
        {
            // Uygun merdiven yoksa son çare normal rota
            if (agent.destination != tPos) agent.SetDestination(tPos);
            return;
        }

        // Link yönünü hedefe uygunlaştır
        Vector3 s = best.transform.TransformPoint(best.startPoint);
        Vector3 e = best.transform.TransformPoint(best.endPoint);
        bool linkGoesUp = e.y > s.y;
        if ((goingUp && !linkGoesUp) || (!goingUp && linkGoesUp))
        {
            var tmp = s; s = e; e = tmp;
        }

        _activeLink = best;
        agent.stoppingDistance = arriveDistance;

        // Start'a kilitlen
        if ((aPos - s).sqrMagnitude > 0.04f)
            agent.SetDestination(s);
        else
            agent.SetDestination(s + (e - s).normalized * 0.1f); // girişe dürtme
    }

    NavMeshLink ChooseBestLink(Vector3 aPos, Vector3 tPos, bool goingUp)
    {
        float bestCost = float.PositiveInfinity;
        NavMeshLink best = null;

        Vector2 a2 = new Vector2(aPos.x, aPos.z);
        Vector2 t2 = new Vector2(tPos.x, tPos.z);

        foreach (var link in _ladderLinks)
        {
            if (!link) continue;
            Vector3 s = link.transform.TransformPoint(link.startPoint);
            Vector3 e = link.transform.TransformPoint(link.endPoint);

            float dy = e.y - s.y;
            if (goingUp && dy < minVerticalGain) continue;
            if (!goingUp && -dy < minVerticalDrop) continue;

            // Başlangıç katı ajana yakın olmalı
            float lowY = Mathf.Min(s.y, e.y);
            float highY = Mathf.Max(s.y, e.y);
            float aY = aPos.y;
            if (goingUp)
            {
                if (Mathf.Abs(lowY - aY) > startHeightTolerance) continue;
            }
            else
            {
                if (Mathf.Abs(highY - aY) > startHeightTolerance) continue;
            }

            // Çok uzak start/endlere bakma
            Vector2 s2 = new Vector2(s.x, s.z);
            Vector2 e2 = new Vector2(e.x, e.z);
            if ((a2 - s2).magnitude > searchRadius && (a2 - e2).magnitude > searchRadius)
                continue;

            // Maliyet fonksiyonu
            float cost = (a2 - s2).magnitude + planBias * (t2 - e2).magnitude;
            if (cost < bestCost)
            {
                bestCost = cost;
                best = link;
            }
        }
        return best;
    }
}
