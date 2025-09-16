using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentSafeBootstrapper))]
public class TargetChaser : MonoBehaviour
{
    public Transform target;
    public float repathInterval = 0.25f;

    NavMeshAgent agent;
    AgentSafeBootstrapper boot;
    float nextRepath;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        boot = GetComponent<AgentSafeBootstrapper>();
    }

    void OnEnable() => nextRepath = 0f;

    void Update()
    {
        if (!target) return;
        if (!boot.Ready) return;               // NavMesh'e oturmadan ASLA

        if (Time.time >= nextRepath)
        {
            agent.SafeSetDestination(target.position, 2f);
            nextRepath = Time.time + repathInterval;
        }
    }
}
