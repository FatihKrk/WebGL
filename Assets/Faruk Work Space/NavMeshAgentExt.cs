using UnityEngine;
using UnityEngine.AI;

public static class NavMeshAgentExt
{
    // Ajan aktif + NavMesh'te ise SetDestination yapar, deðilse sessizce false döner
    public static bool SafeSetDestination(this NavMeshAgent agent, Vector3 dest, float sampleRadius = 2f)
    {
        if (agent == null || !agent.isActiveAndEnabled) return false;
        if (!agent.enabled || !agent.isOnNavMesh) return false;

        if (NavMesh.SamplePosition(dest, out var hit, sampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            return true;
        }
        return false;
    }

    public static bool SafeStop(this NavMeshAgent agent)
    {
        if (agent == null || !agent.isActiveAndEnabled) return false;
        if (!agent.enabled || !agent.isOnNavMesh) return false;
        agent.isStopped = true;
        return true;
    }

    public static bool SafeResume(this NavMeshAgent agent)
    {
        if (agent == null || !agent.isActiveAndEnabled) return false;
        if (!agent.enabled || !agent.isOnNavMesh) return false;
        agent.isStopped = false;
        return true;
    }
}
