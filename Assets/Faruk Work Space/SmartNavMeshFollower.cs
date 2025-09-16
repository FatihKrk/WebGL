using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshAgent))]
public class SmartNavMeshFollower : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;

    [Header("NavMesh Ayarları")]
    public float updateInterval = 0.5f;
    public float verticalThreshold = 1.5f; // Yükseklik farkı eşiği
    public float linkPreferenceRadius = 5f; // Link arama mesafesi

    [Header("Debug")]
    public bool showDebug = true;
    public bool forceUseLadders = false; // Test için ladder'ları zorla kullan

    private NavMeshAgent agent;
    private float lastUpdateTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Hedef otomatik bul
        if (target == null)
        {
            GameObject targetObj = GameObject.Find("Navmesh Target");
            if (targetObj != null) target = targetObj.transform;
        }

        if (target == null)
        {
            Debug.LogError("Target bulunamadı! 'Navmesh Target' isimli obje var mı?");
            enabled = false;
            return;
        }

        // Agent ayarları kontrol et
        CheckAgentSettings();

        if (showDebug) Debug.Log($"SmartFollower başlatıldı. Hedef: {target.name}");
    }

    void Update()
    {
        if (target == null || !agent.enabled) return;

        // Belirli aralıklarla rota güncelle
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdatePath();
            lastUpdateTime = Time.time;
        }
    }

    void UpdatePath()
    {
        Vector3 targetPos = target.position;
        float heightDiff = targetPos.y - transform.position.y;
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        if (showDebug) Debug.Log($"Hedef mesafesi: {distanceToTarget:F1}m, yükseklik farkı: {heightDiff:F2}m");

        // Çok yakındaysa direct git
        if (distanceToTarget < 2f && Mathf.Abs(heightDiff) < 0.5f)
        {
            SetDestinationSafe(targetPos);
            return;
        }

        // Yükseklik farkı büyükse veya zorla link kullanıyorsak
        if (Mathf.Abs(heightDiff) > verticalThreshold || forceUseLadders)
        {
            if (TryFindBestLink(targetPos))
            {
                return;
            }
            else if (showDebug)
            {
                Debug.Log("Uygun link bulunamadı, normal rota deneniyor");
            }
        }

        // Normal rota
        SetDestinationSafe(targetPos);
    }

    bool TryFindBestLink(Vector3 targetPos)
    {
        NavMeshLink[] allLinks = FindObjectsOfType<NavMeshLink>();
        NavMeshLink bestLink = null;
        float bestScore = float.MaxValue;

        foreach (var link in allLinks)
        {
            if (!link.enabled) continue;

            // Link pozisyonları
            Vector3 startWorld = link.transform.TransformPoint(link.startPoint);
            Vector3 endWorld = link.transform.TransformPoint(link.endPoint);

            // Agent'a yakın olan ucu bul
            float distToStart = Vector3.Distance(transform.position, startWorld);
            float distToEnd = Vector3.Distance(transform.position, endWorld);

            Vector3 closerPoint = (distToStart < distToEnd) ? startWorld : endWorld;
            Vector3 farPoint = (distToStart < distToEnd) ? endWorld : startWorld;

            // Link üzerinden hedefe giden toplam mesafe
            float totalDist = Vector3.Distance(transform.position, closerPoint) +
                             Vector3.Distance(farPoint, targetPos);

            if (totalDist < bestScore && Vector3.Distance(transform.position, closerPoint) < linkPreferenceRadius)
            {
                bestScore = totalDist;
                bestLink = link;
            }
        }

        if (bestLink != null)
        {
            // En yakın link ucuna git
            Vector3 startWorld = bestLink.transform.TransformPoint(bestLink.startPoint);
            Vector3 endWorld = bestLink.transform.TransformPoint(bestLink.endPoint);

            float distToStart = Vector3.Distance(transform.position, startWorld);
            float distToEnd = Vector3.Distance(transform.position, endWorld);

            Vector3 linkEntry = (distToStart < distToEnd) ? startWorld : endWorld;

            if (showDebug)
            {
                Debug.Log($"En iyi link bulundu: {bestLink.name}, mesafe: {bestScore:F2}m");
                Debug.DrawLine(transform.position, linkEntry, Color.yellow, 2f);
            }

            return SetDestinationSafe(linkEntry);
        }

        return false;
    }

    bool SetDestinationSafe(Vector3 destination)
    {
        if (!agent.isOnNavMesh)
        {
            if (showDebug) Debug.LogWarning("Agent NavMesh üzerinde değil!");
            return false;
        }

        // Hedef pozisyonu NavMesh üzerinde mi kontrol et
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 5f, agent.areaMask))
        {
            agent.SetDestination(hit.position);

            if (showDebug) Debug.Log($"Destination ayarlandı: {hit.position}");
            return true;
        }
        else
        {
            if (showDebug) Debug.LogWarning($"Destination NavMesh'te bulunamadı: {destination}");
            return false;
        }
    }

    void CheckAgentSettings()
    {
        if (showDebug)
        {
            Debug.Log($"Agent Area Mask: {agent.areaMask}");
            Debug.Log($"Agent Type ID: {agent.agentTypeID}");
            Debug.Log($"Auto Traverse OffMesh: {agent.autoTraverseOffMeshLink}");
        }

        // GetAreaName() runtime'da yok → sadece indeks/cost logla (güvenli)
        for (int i = 0; i < 32; i++)
        {
            try
            {
                float cost = NavMesh.GetAreaCost(i);
                if (showDebug) Debug.Log($"Area {i}: Cost = {cost}");
            }
            catch (System.Exception ex)
            {
                if (showDebug) Debug.LogWarning($"Area {i} cost okunamadı: {ex.Message}");
            }
        }

        // "Ladder" alanı tanımlıysa indeksini isimden bul ve uyarı ver
        int ladderIndex = NavMesh.GetAreaFromName("Ladder");
        if (ladderIndex < 0) ladderIndex = NavMesh.GetAreaFromName("ladder");
        if (ladderIndex < 0) ladderIndex = NavMesh.GetAreaFromName("LADDER");

        if (ladderIndex >= 0)
        {
            try
            {
                float ladderCost = NavMesh.GetAreaCost(ladderIndex);
                if (ladderCost < 1f)
                {
                    Debug.LogWarning($"Ladder area cost çok düşük: {ladderCost}. En az 1 olmalı!");
                }
                else if (showDebug)
                {
                    Debug.Log($"Ladder (#{ladderIndex}) cost = {ladderCost}");
                }
            }
            catch (System.Exception ex)
            {
                if (showDebug) Debug.LogWarning($"Ladder alan cost okunamadı (#{ladderIndex}): {ex.Message}");
            }
        }
    }


    void OnDrawGizmos()
    {
        if (!showDebug || agent == null) return;

        // Agent
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one);

        // Hedef
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(target.position + Vector3.up * 0.5f, Vector3.one * 0.8f);

            // Yükseklik farkı çizgisi
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(transform.position.x, transform.position.y, transform.position.z),
                new Vector3(target.position.x, transform.position.y, target.position.z)
            );
            Gizmos.DrawLine(
                new Vector3(target.position.x, transform.position.y, target.position.z),
                target.position
            );
        }

        // NavMeshLink'leri göster
        NavMeshLink[] links = FindObjectsOfType<NavMeshLink>();
        foreach (var link in links)
        {
            if (!link.enabled) continue;

            Vector3 start = link.transform.TransformPoint(link.startPoint);
            Vector3 end = link.transform.TransformPoint(link.endPoint);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(start, 0.3f);
            Gizmos.DrawWireSphere(end, 0.3f);
            Gizmos.DrawLine(start, end);
        }

        // Mevcut path
        if (agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i] + Vector3.up * 0.1f, corners[i + 1] + Vector3.up * 0.1f);
            }
        }
    }
}