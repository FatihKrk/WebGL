using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class LadderLinkMaker : MonoBehaviour
{
    [Header("Neleri tara? (root objeler)")]
    public List<Transform> parentRoots = new List<Transform>();
    public NavMeshSurface surface; // Bake ettiðin surface'i buraya ver

    [Header("Arama ve yerleþim parametreleri")]
    public float searchUp = 6f;         // üst kat için dikey arama mesafesi
    public float searchDown = 1.5f;     // alt kat için dikey arama mesafesi
    public float sampleRadius = 1.0f;   // NavMesh.SamplePosition yarýçapý
    public float outward = 0.20f;       // halkaya doðru dýþarý itme (metre)
    public float linkWidth = 0.6f;      // link geniþliði
    public float costModifier = 0.2f;   // link maliyeti (daha ucuz = daha cazip)
    public string cylinderNameContains = "CYLINDER";

    int ladderArea;

    void Awake()
    {
        ladderArea = NavMesh.GetAreaFromName("Ladder"); // Navigation > Areas'ta eklediðin
        if (ladderArea < 0) Debug.LogWarning("[Ladder] 'Ladder' area bulunamadý.");
    }

    void Start() => Build();

    public void Build()
    {
        int total = 0, ok = 0, missTop = 0, missBottom = 0;

        foreach (var root in parentRoots)
        {
            if (!root) continue;

            foreach (var rend in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!rend.name.ToUpper().Contains(cylinderNameContains)) continue;

                total++;
                Vector3 center = rend.bounds.center;

                if (!TryFind(center, Vector3.down, searchDown, out var p0)) { missBottom++; continue; }
                if (!TryFind(center, Vector3.up, searchUp, out var p1)) { missTop++; continue; }

                // Halkaya otursun diye hafif dýþarý it
                Vector3 dir = (new Vector3(p0.x, 0, p0.z) - new Vector3(center.x, 0, center.z)).normalized;
                p0 += dir * outward;
                p1 += dir * outward;

                // Link objesi
                var go = new GameObject($"LadderLink_{rend.name}");
                go.transform.SetParent(rend.transform, false);

                var link = go.AddComponent<NavMeshLink>();
                if (surface) link.agentTypeID = surface.agentTypeID;   // bake ile ayný agent type
                link.startPoint = go.transform.InverseTransformPoint(p0);
                link.endPoint = go.transform.InverseTransformPoint(p1);
                link.width = linkWidth;
                link.bidirectional = true;
                link.costModifier = costModifier;
                if (ladderArea >= 0) link.area = ladderArea;
                link.UpdateLink();

                ok++;
                Debug.Log($"[Ladder] Link @ {rend.name} start:{p0} end:{p1}");
            }
        }

        Debug.Log($"[Ladder] {ok}/{total} link oluþturuldu. (alt yok:{missBottom}, üst yok:{missTop})");
    }

    // Dikey ray boyunca küçük adýmlarla NavMesh taramasý
    bool TryFind(Vector3 origin, Vector3 dir, float distance, out Vector3 hit)
    {
        const float step = 0.25f;
        int mask = NavMesh.AllAreas;

        for (float d = 0; d <= distance; d += step)
        {
            var probe = origin + dir * d;
            if (NavMesh.SamplePosition(probe, out var h, sampleRadius, mask))
            {
                // Yukarý bakarken alt katý seçmemek, aþaðý bakarken üst katý seçmemek için
                bool ok =
                    (dir.y > 0 && h.position.y >= probe.y - 0.1f) ||
                    (dir.y < 0 && h.position.y <= probe.y + 0.1f);

                if (ok) { hit = h.position; return true; }
            }
        }
        hit = default; return false;
    }
}
