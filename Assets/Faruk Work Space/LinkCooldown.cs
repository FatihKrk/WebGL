using UnityEngine;
using UnityEngine.AI;

public class LinkCooldown : MonoBehaviour
{
    public float cooldown = 0.8f;

    NavMeshAgent agent;
    int lastLinkId = -1;
    float coolUntil = -1f;

    void Awake() { agent = GetComponent<NavMeshAgent>(); }

    void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            var data = agent.currentOffMeshLinkData;
            if (Time.time < coolUntil && data.offMeshLink != null && data.offMeshLink.GetInstanceID() == lastLinkId)
            {
                // Ayný linke cooldown süresinde tekrar girildiyse zorla çýk
                agent.CompleteOffMeshLink();
                return;
            }
        }
        else
        {
            // Linkten yeni çýktýk mý?
            if (lastLinkId != -1 && Time.time >= coolUntil)
            {
                // boþta
            }
        }
    }

    public void MarkJustFinished(OffMeshLink link)
    {
        if (link == null) return;
        lastLinkId = link.GetInstanceID();
        coolUntil = Time.time + cooldown;
    }
}
