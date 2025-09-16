// NavAreaCostBoot.cs
using UnityEngine;
using UnityEngine.AI;

public class NavAreaCostBoot : MonoBehaviour
{
    [Header("Costs >= 1 olmalý")]
    public float walkableCost = 5f;   // rampalar pahalý
    public float ladderCost = 1f;   // merdiven/boru ucuz

    void Awake()
    {
        int aWalk = NavMesh.GetAreaFromName("Walkable");
        int aLad = NavMesh.GetAreaFromName("Ladder");

        if (aWalk >= 0) NavMesh.SetAreaCost(aWalk, Mathf.Max(1f, walkableCost));
        if (aLad >= 0) NavMesh.SetAreaCost(aLad, Mathf.Max(1f, ladderCost));

        Debug.Log($"[Areas] Walkable={NavMesh.GetAreaCost(aWalk)}  Ladder={NavMesh.GetAreaCost(aLad)}");
    }
}
