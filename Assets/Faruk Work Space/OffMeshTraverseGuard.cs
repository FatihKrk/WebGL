using UnityEngine;
using UnityEngine.AI;

public class OffMeshTraverseGuard : MonoBehaviour
{
    public float sameLinkCooldown = 0.6f;

    NavMeshAgent agent;
    OffMeshLink lastLink;
    float lastLinkTime;
    bool onLink;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // Çift yürütmeyi engelle
        agent.autoTraverseOffMeshLink = false;
    }

    void Update()
    {
        // Link üstündeyken yeniden path hesaplamasýný kapat (geri dönmesin)
        if (agent.isOnOffMeshLink)
        {
            if (!onLink)
            {
                onLink = true;
                agent.autoRepath = false;
            }
        }
        else if (onLink)
        {
            // Linkten yeni çýktýk
            onLink = false;
            agent.autoRepath = true;

            // Ayný linke hemen geri dönmeyi önlemek için kýsa cooldown
            if (agent.hasPath == false)
                agent.SetDestination(agent.transform.position); // navmesh state temiz

            if (lastLink != null) StartCoroutine(Cooldown(lastLink));
        }

        // Ayný linke üst üste girme kontrolü
        if (agent.isOnOffMeshLink)
        {
            var data = agent.currentOffMeshLinkData;
            var link = data.offMeshLink;
            if (link != null)
            {
                lastLink = link;
                lastLinkTime = Time.time;
            }
        }
    }

    System.Collections.IEnumerator Cooldown(OffMeshLink link)
    {
        if (link == null) yield break;
        link.activated = false;
        yield return new WaitForSeconds(sameLinkCooldown);
        link.activated = true;
    }
}
