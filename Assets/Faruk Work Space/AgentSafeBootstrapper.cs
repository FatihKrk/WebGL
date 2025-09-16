using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentSafeBootstrapper : MonoBehaviour
{
    [Header("NavMesh'e oturtma")]
    public float searchRadius = 50f;
    public float yOffset = 0.02f;

    [Header("Opsiyonel: ilk hedef")]
    public Transform initialTarget;

    [Header("Hazýr olunca açýlacaklar")]
    public Behaviour[] enableAfterReady;

    public bool Ready { get; private set; }

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Ajan daha oturmadan hareket etmesin
        agent.isStopped = true;
        agent.enabled = false;

        // Diðer AI scriptlerini kapat
        if (enableAfterReady != null)
            foreach (var b in enableAfterReady) if (b) b.enabled = false;

        Ready = false;
    }

    void OnEnable() => StartCoroutine(Bootstrap());

    System.Collections.IEnumerator Bootstrap()
    {
        // Bake vs. için 1–2 frame bekle
        yield return null;
        yield return null;

        // Yakýna NavMesh noktasý tara
        var probe = transform.position + Vector3.up * yOffset;

        if (NavMesh.SamplePosition(probe, out var hit, searchRadius, NavMesh.AllAreas))
        {
            // *** SIRA ÖNEMLÝ ***
            // 1) Agent kapalýyken transform'u NavMesh noktasýna taþý
            transform.position = hit.position;

            // 2) Þimdi agent'ý aç
            agent.enabled = true;

            // 3) Güvenli olsun diye bir de Warp (artýk NavMesh üstündeyiz)
            agent.Warp(hit.position);

            agent.isStopped = false;
            Ready = true;

            if (initialTarget != null)
                agent.SafeSetDestination(initialTarget.position, 2f);

            if (enableAfterReady != null)
                foreach (var b in enableAfterReady) if (b) b.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[Bootstrap] Yakýnda NavMesh bulunamadý (r={searchRadius}). " +
                             "Manager > Bake ve/veya radiusu büyüt.");
            // Ready=false kalýr; diðer scriptler çalýþmaz ve hata da vermez.
        }
    }
}
