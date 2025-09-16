// OffMeshLinkTuning.cs
using UnityEngine;
using UnityEngine.AI;

public class OffMeshLinkTuning : MonoBehaviour
{
    [Tooltip("Link geçiþ maliyeti (yol hesabýnda sabit eklenir).")]
    public float linkCostOverride = 0.1f;   // çok ucuz
    public bool bidirectional = true;

    void Start()
    {
        int ladder = NavMesh.GetAreaFromName("Ladder");
        var links = FindObjectsOfType<OffMeshLink>(true);

        int tuned = 0;
        foreach (var l in links)
        {
            l.biDirectional = bidirectional;
            l.activated = true;
            if (ladder >= 0) l.area = ladder;        // link alanýný Ladder yap
            l.costOverride = linkCostOverride;      // maliyeti çok düþük yap
            tuned++;
        }
        Debug.Log($"[Links] Tuned {tuned} OffMeshLink. area=Ladder, cost={linkCostOverride}");
    }
}
