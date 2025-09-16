using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(9000)]  // diğerleri set etse bile en sonda basar
public class NavAreaCostLock : MonoBehaviour
{
    [Min(1f)] public float walkableCost = 5f;
    [Min(1f)] public float ladderCost = 1f;

    int walkId, ladId;
    float stopTime;

    void OnEnable()
    {
        walkId = NavMesh.GetAreaFromName("Walkable");
        ladId = NavMesh.GetAreaFromName("Ladder");
        stopTime = Time.time + 2f; // 2 sn kilitle
        Apply();
    }

    void Update()
    {
        if (Time.time > stopTime) return;
        Apply(); // ilk 2 sn başka script bozarsa düzelt
    }

    void Apply()
    {
        if (walkId >= 0) NavMesh.SetAreaCost(walkId, Mathf.Max(1f, walkableCost));
        if (ladId >= 0) NavMesh.SetAreaCost(ladId, Mathf.Max(1f, ladderCost));
    }
}
