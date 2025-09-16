using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LadderOnlyWhenClimbing : MonoBehaviour
{
    public Transform target;
    [Tooltip("Hedef ile arandaki düþey fark bu deðeri aþarsa týrmanma moduna gir")]
    public float heightDelta = 1.0f;
    public string rampAreaName = "Ramp";
    public float repathInterval = 0.2f;

    NavMeshAgent agent;
    int originalMask;
    int rampBit;
    bool rampDisabled;
    float nextRepath;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalMask = agent.areaMask;
        int rampArea = NavMesh.GetAreaFromName(rampAreaName);
        rampBit = (rampArea >= 0) ? (1 << rampArea) : 0;
    }

    void OnDisable() { if (agent) agent.areaMask = originalMask; rampDisabled = false; }

    void Update()
    {
        if (!target) return;

        bool needClimbUp = (target.position.y - agent.nextPosition.y) > heightDelta;
        bool needClimbDown = (agent.nextPosition.y - target.position.y) > heightDelta;

        // Týrmanýþta rampalarý kapat
        if ((needClimbUp || needClimbDown))
        {
            if (!rampDisabled && rampBit != 0)
            {
                agent.areaMask = originalMask & ~rampBit;
                rampDisabled = true;
                // Repath’i sýklaþtýr
                agent.autoRepath = true;
                nextRepath = 0f;
                Debug.Log("[LadderOnly] Rampa alaný maskeden çýkarýldý.");
            }
        }
        else if (rampDisabled && !agent.isOnOffMeshLink)
        {
            // Normal moda dön
            agent.areaMask = originalMask;
            rampDisabled = false;
            Debug.Log("[LadderOnly] Rampa alaný geri açýldý.");
        }

        // Hedefi canlý tut
        if (Time.time >= nextRepath)
        {
            agent.SetDestination(target.position);
            nextRepath = Time.time + repathInterval;
        }
    }
}
