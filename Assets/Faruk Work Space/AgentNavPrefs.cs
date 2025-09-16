using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-8000)]
[RequireComponent(typeof(NavMeshAgent))]
public class AgentNavPrefs : MonoBehaviour
{
    public bool preferLadders = true;
    [Min(1f)] public float walkableCost = 5f;
    [Min(1f)] public float ladderCost = 1f;

    NavMeshAgent agent;

    IEnumerator Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // NavMesh’e oturana kadar bekle
        float t = 0f;
        while (!agent.isOnNavMesh && t < 3f) { t += Time.deltaTime; yield return null; }

        int aWalk = NavMesh.GetAreaFromName("Walkable");
        int aLadd = NavMesh.GetAreaFromName("Ladder");

        // Ladder alanını maskeye ekle (varsa)
        if (aLadd >= 0) agent.areaMask |= (1 << aLadd);

        if (preferLadders)
        {
            if (aWalk >= 0) agent.SetAreaCost(aWalk, Mathf.Max(1f, walkableCost));
            if (aLadd >= 0) agent.SetAreaCost(aLadd, Mathf.Max(1f, ladderCost));
        }
    }
}
