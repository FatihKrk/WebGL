using UnityEngine;
using UnityEngine.AI;

public class AutoLadderLinks : MonoBehaviour
{
    [Header("Neyi merdiven sayalým?")]
    public string ladderTag = "Ladder";

    [Header("Link ayarlarý")]
    public bool bidirectional = true;
    public float margin = 0.05f;  // alt/üstten biraz içeriden örnekle
    public float snapRadius = 6f;     // NavMesh'e yapýþma yarýçapý
    public float topOffset = 0.6f;   // üst ucu yürünebilir yüzeyin üstüne taþý

    [Header("Opsiyonel: Ajan tipini buradan al (yarýçap için)")]
    public NavMeshAgent agentTypeSource;

    const string StartName = "__LinkStart";
    const string EndName = "__LinkEnd";

    void Start()
    {
        int ladderArea = NavMesh.GetAreaFromName("Ladder");
        int walkableArea = NavMesh.GetAreaFromName("Walkable");
        int walkableMask = (walkableArea >= 0) ? (1 << walkableArea) : NavMesh.AllAreas;

        float agentRadius = agentTypeSource ? agentTypeSource.radius : 0.4f; // makul varsayýlan

        foreach (var go in GameObject.FindGameObjectsWithTag(ladderTag))
        {
            var rend = go.GetComponent<Renderer>();
            if (!rend) continue;

            var b = rend.bounds;

            var startGuess = new Vector3(b.center.x, b.min.y + margin, b.center.z);
            var endGuess = new Vector3(b.center.x, b.max.y + topOffset, b.center.z);

            if (!NavMesh.SamplePosition(startGuess, out var hitA, snapRadius, walkableMask) ||
                !NavMesh.SamplePosition(endGuess, out var hitB, snapRadius, walkableMask))
            {
                continue; // bu silindir için link kurma
            }

            // START noktasýný silindirin merkezinden DIÞARI doðru, ajan yarýçapýna göre ittir
            Vector3 pillarCenter = b.center; pillarCenter.y = hitA.position.y;
            Vector3 pushDir = (hitA.position - pillarCenter).normalized;
            if (pushDir.sqrMagnitude < 0.001f) pushDir = Vector3.forward; // emniyet
            Vector3 startPos = hitA.position + pushDir * (agentRadius * 0.6f + 0.05f);

            // ÜST noktanýn tam tavan altýnda kalmasýný istemiyorsan biraz daha yukarý taþýyabilirsin:
            Vector3 endPos = hitB.position;

            var link = go.GetComponent<OffMeshLink>();
            if (!link) link = go.AddComponent<OffMeshLink>();

#pragma warning disable CS0618
            link.biDirectional = bidirectional;
            link.activated = true;
            if (ladderArea >= 0) link.area = ladderArea;
            link.costOverride = -1f; // area cost'u kullan
#pragma warning restore CS0618

            // start/end transformlarýný oluþtur/kullan
            var startTf = go.transform.Find(StartName);
            if (!startTf) { startTf = new GameObject(StartName).transform; startTf.SetParent(go.transform, true); }

            var endTf = go.transform.Find(EndName);
            if (!endTf) { endTf = new GameObject(EndName).transform; endTf.SetParent(go.transform, true); }

            startTf.position = startPos;
            endTf.position = endPos;

            link.startTransform = startTf;
            link.endTransform = endTf;
        }
    }
}
