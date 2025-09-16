using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HardChase : MonoBehaviour
{
    public Transform target;
    [Range(0.05f, 1f)] public float repathInterval = 0.2f;
    public float arriveDistance = 0.35f;

    [Header("NavMesh’e oturtma")]
    public float probeRadius = 60f;

    [Header("Link týrmanma")]
    public float climbSpeed = 2.2f;       // dikey ilerleme hýzý
    public float traverseSpeed = 1.6f;    // yatay ilerleme hýzý (gerekirse)
    public float postLinkSnap = 1.0f;     // link çýkýþýnda tekrar snap yarýçapý

    NavMeshAgent agent;
    float nextRepath;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false; // biz yönetiyoruz
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    void Start()
    {
        // En yakýn NavMesh’e oturt
        if (NavMesh.SamplePosition(transform.position, out var hit, probeRadius, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            Debug.LogWarning("[HardChase] Yakýnda NavMesh yok; probeRadius'i arttýr.", this);

        // Link dinleyicisi
        StartCoroutine(OffMeshLoop());
    }

    IEnumerator OffMeshLoop()
    {
        while (true)
        {
            if (agent.isOnOffMeshLink)
            {
                var data = agent.currentOffMeshLinkData;

                // Baþlangýç/bitis dünya noktalari
                Vector3 start = data.startPos;
                Vector3 end = data.endPos;

                // Ajaný link baþlangýcýna topla (küçük bir yaklaþma)
                while ((transform.position - start).sqrMagnitude > 0.02f)
                {
                    var step = (start - transform.position);
                    var dir = step.normalized;
                    agent.Move(dir * traverseSpeed * Time.deltaTime);
                    yield return null;
                }

                // Dikey/lineer týrmanma (basit ve saðlam)
                float t = 0f;
                float len = Vector3.Distance(start, end);
                float dur = Mathf.Max(0.01f, len / climbSpeed);

                var startRot = transform.rotation;
                var lookDir = (end - start); lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(lookDir);

                while (t < dur)
                {
                    t += Time.deltaTime;
                    float k = Mathf.Clamp01(t / dur);
                    transform.position = Vector3.Lerp(start, end, k);
                    yield return null;
                }

                agent.CompleteOffMeshLink();

                // Çýkýþta tekrar NavMesh’e yapýþtýr
                if (NavMesh.SamplePosition(transform.position, out var stick, postLinkSnap, NavMesh.AllAreas))
                    agent.Warp(stick.position);
            }

            yield return null;
        }
    }

    void Update()
    {
        if (!target) return;

        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;

            if ((target.position - transform.position).sqrMagnitude >
                arriveDistance * arriveDistance)
            {
                // Agent aktif deðilse bile hatasýz: isOnNavMesh kontrolü
                if (agent.isOnNavMesh) agent.SetDestination(target.position);
            }
        }
    }
}
