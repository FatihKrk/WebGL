using UnityEngine;
using UnityEngine.AI;

public class OnLinkGuard : MonoBehaviour
{
    public MonoBehaviour chaser; // TargetChaser (veya takip eden script)
    NavMeshAgent agent;
    bool savedRepath, frozen;

    void Awake() { agent = GetComponent<NavMeshAgent>(); }

    void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            if (!frozen)
            {
                if (chaser && chaser.enabled) chaser.enabled = false; // SetDestination spam yok
                savedRepath = agent.autoRepath; agent.autoRepath = false;
                frozen = true;
            }
        }
        else if (frozen)
        {
            if (chaser && !chaser.enabled) chaser.enabled = true;
            agent.autoRepath = savedRepath;
            frozen = false;
        }
    }
}
