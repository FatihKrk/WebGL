using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(800)] // Snapper vs. sonrası, Lock'tan önce
public class LadderCostBias : MonoBehaviour
{
    [Header("Ladder area cost (>=1 olmalı)")]
    [Min(1f)] public float ladderCost = 2f;

    [Header("Walkable cost (opsiyonel)")]
    [Min(1f)] public float walkableCost = 1f;

    void OnEnable() { Apply(); }
    void Start() { Apply(); }

    void Apply()
    {
        int lad = NavMesh.GetAreaFromName("Ladder");
        int wal = NavMesh.GetAreaFromName("Walkable");

        float lc = Mathf.Max(1f, ladderCost);
        float wc = Mathf.Max(1f, walkableCost);

        NavMesh.SetAreaCost(lad, lc);
        NavMesh.SetAreaCost(wal, wc);
        // Debug.Log($"[Areas] WalkableCost={wc} LadderCost={lc}");
    }
}
