using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Silindir (merdiven) nesneleri için OffMeshLink'leri runtime'da otomatik kurar.
/// Aynı dosya adına yapıştırıp eskisini tamamen değiştirin.
/// </summary>
public class RuntimeLadderLinks : MonoBehaviour
{
    [Header("Hedef Nesneler")]
    [Tooltip("Bu layer'daki objeler merdiven olarak kabul edilir (örn: Climbable).")]
    public LayerMask ladderLayer;
    [Tooltip("İsimde bu anahtar geçiyorsa (örn: CYLINDER) merdiven say.")]
    public string nameMustContain = "CYLINDER";

    [Header("Arama & Örnekleme")]
    [Tooltip("Aşağı/üst NavMesh araması için yarıçap.")]
    public float sampleRadius = 2.0f;
    [Tooltip("Alt uç için zemin arama yüksekliği (aşağı-yukarı).")]
    public float bottomScanHeight = 1.5f;
    [Tooltip("Üst uç için taranacak maksimum yükseklik farkı.")]
    public float maxRiseScan = 8.0f;
    [Tooltip("Üst katta NavMesh bulmak için silindirin etrafında taranan halka yarıçapı.")]
    public float topRingRadius = 0.8f;
    [Tooltip("Üst halka etrafında taranacak açı adedi.")]
    public int topRingSamples = 16;
    [Tooltip("Alt-üst arasında minimum yükseklik farkı (merdiven saymak için).")]
    public float minRise = 1.0f;

    [Header("Link Ayarları")]
    public bool bidirectional = true;
    public bool autoUpdatePositions = true;
    [Tooltip("Link Area adı (Navigation → Areas).")]
    public string ladderAreaName = "Ladder";
    [Tooltip("Oluşan linkleri bir ebeveyn altında topla.")]
    public string parentName = "_Generated_LadderLinks";

    [Header("Agent Türü (opsiyonel)")]
    [Tooltip("Agent atarsan, linklerin AgentTypeID'si bununla eşitlenir.")]
    public NavMeshAgent referenceAgent;

    [Header("Debug")]
    public bool testOnly = false; // true ise link kurmaz, sadece noktaları çizer
    public bool verboseLog = true;

    void Start()
    {
        BuildAll();
    }

    public void BuildAll()
    {
        // Eski oluşturulanları temizle
        var old = GameObject.Find(parentName);
        if (old) DestroyImmediate(old);

        var root = new GameObject(parentName);
        var targets = FindCandidates();

        int areaLadder = NavMesh.GetAreaFromName(ladderAreaName);
        if (areaLadder < 0 && verboseLog)
            Debug.LogWarning($"[LadderLinks] Area '{ladderAreaName}' bulunamadı. Linkler default area ile oluşturulacak.");

        int built = 0, skipped = 0;

        foreach (var t in targets)
        {
            if (!TryMakeLinkForCylinder(t, root.transform, areaLadder))
                skipped++;
            else
                built++;
        }

        Debug.Log($"[LadderLinks] Done. Built: {built}, Skipped: {skipped}. (candidates: {targets.Count})");
    }

    List<Transform> FindCandidates()
    {
        var list = new List<Transform>();
        var all = FindObjectsOfType<Transform>(true);

        foreach (var tr in all)
        {
            if (!tr.gameObject.activeInHierarchy) continue;

            // Layer filtresi (opsiyonel)
            bool layerOk = (ladderLayer == 0) || (((1 << tr.gameObject.layer) & ladderLayer.value) != 0);

            // İsim filtresi
            bool nameOk = string.IsNullOrEmpty(nameMustContain) || tr.name.ToUpper().Contains(nameMustContain.ToUpper());

            if (layerOk && nameOk)
            {
                // Silindir olduğunu kabaca mesh/collider ile anlamaya çalış
                var col = tr.GetComponent<Collider>();
                var rend = tr.GetComponent<Renderer>();
                if (col == null && rend == null) continue;

                // Dikey obje şartı (yüksekliği genişliğinden bariz büyük)
                var bounds = (col != null ? col.bounds : rend.bounds);
                float h = bounds.size.y;
                float r = Mathf.Max(bounds.size.x, bounds.size.z);
                if (h > r * 1.5f) list.Add(tr);
            }
        }

        if (verboseLog) Debug.Log($"[LadderLinks] Candidates found: {list.Count}");
        return list;
    }

    bool TryMakeLinkForCylinder(Transform cyl, Transform parent, int areaLadder)
    {
        // Silindirin yaklaşık orta ve alt/üst referansları
        var hasBounds = TryGetBounds(cyl, out Bounds b);
        if (!hasBounds) return false;

        Vector3 center = b.center;
        float bottomY = b.min.y + 0.02f;
        float topY = b.max.y - 0.02f;

        // ALT UÇ → zemindeki NavMesh
        Vector3 bottomProbe = new Vector3(center.x, bottomY, center.z);
        if (!TrySampleAround(bottomProbe, sampleRadius, bottomScanHeight, 8, out var bottomHit))
        {
            if (verboseLog) Debug.Log($"[LadderLinks] Alt NavMesh bulunamadı: {cyl.name}");
            return false;
        }

        // ÜST UÇ → üst kattaki NavMesh
        // Silindirin yanına küçük bir halka koyup, farklı açılardan üst seviyede tarıyoruz
        Vector3 topBase = new Vector3(center.x, Mathf.Min(topY + maxRiseScan, center.y + maxRiseScan), center.z);
        if (!TryFindTopNavMesh(cyl, topBase, out var topHit))
        {
            if (verboseLog) Debug.Log($"[LadderLinks] Üst NavMesh bulunamadı: {cyl.name}");
            return false;
        }

        if ((topHit.position.y - bottomHit.position.y) < minRise)
        {
            if (verboseLog) Debug.Log($"[LadderLinks] Yükseklik farkı yetersiz ({(topHit.position.y - bottomHit.position.y):F2}m): {cyl.name}");
            return false;
        }

        // Test modunda sadece küre çiz
        if (testOnly)
        {
            DrawDot(bottomHit.position, Color.green);
            DrawDot(topHit.position, Color.red);
            return true;
        }

        // Link GameObject
        var linkRoot = new GameObject($"OffMeshLink_{cyl.name}");
        linkRoot.transform.SetParent(parent, worldPositionStays: true);
        linkRoot.transform.position = bottomHit.position;

        // Start/End child'ları
        var startGO = new GameObject("Start");
        startGO.transform.SetParent(linkRoot.transform, worldPositionStays: true);
        startGO.transform.position = bottomHit.position + Vector3.up * 0.02f;

        var endGO = new GameObject("End");
        endGO.transform.SetParent(linkRoot.transform, worldPositionStays: true);
        endGO.transform.position = topHit.position + Vector3.up * 0.02f;

        // OffMeshLink ekle
        var link = linkRoot.AddComponent<OffMeshLink>();
        link.startTransform = startGO.transform;
        link.endTransform = endGO.transform;
        link.biDirectional = bidirectional;
        link.activated = true;
        link.autoUpdatePositions = autoUpdatePositions;

        if (areaLadder >= 0) link.area = areaLadder;

        // AgentType eşitle (varsa)
        if (referenceAgent != null)
        {
            try
            {
                // Unity 2021+ için public olabilir
                var f = typeof(OffMeshLink).GetField("agentTypeID",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (f != null) f.SetValue(link, referenceAgent.agentTypeID);
                else
                {
                    var p = typeof(OffMeshLink).GetProperty("agentTypeID",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (p != null && p.CanWrite) p.SetValue(link, referenceAgent.agentTypeID, null);
                }
            }
            catch { /* sürüme göre değişebilir, sorun değil */ }
        }

        if (verboseLog)
        {
            Debug.Log($"[LadderLinks] Link OK → {cyl.name}  start={startGO.transform.position}  end={endGO.transform.position}");
        }

        return true;
    }

    bool TryGetBounds(Transform t, out Bounds b)
    {
        var col = t.GetComponent<Collider>();
        if (col != null) { b = col.bounds; return true; }
        var r = t.GetComponent<Renderer>();
        if (r != null) { b = r.bounds; return true; }
        b = default; return false;
    }

    /// <summary>
    /// Verilen noktada aşağı-yukarı küçük tarama ile NavMesh örnekler.
    /// </summary>
    bool TrySampleAround(Vector3 basePos, float radius, float vertical, int horSamples, out NavMeshHit hit)
    {
        // önce direkt
        if (NavMesh.SamplePosition(basePos, out hit, radius, NavMesh.AllAreas))
            return true;

        // halka taraması
        for (int i = 0; i < horSamples; i++)
        {
            float ang = (i / (float)horSamples) * Mathf.PI * 2f;
            Vector3 p = basePos + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * (radius * 0.8f);
            if (NavMesh.SamplePosition(p, out hit, radius, NavMesh.AllAreas))
                return true;
        }

        // dikey tarama
        for (int s = -2; s <= 2; s++)
        {
            Vector3 p = basePos + Vector3.up * (s * vertical * 0.5f);
            if (NavMesh.SamplePosition(p, out hit, radius, NavMesh.AllAreas))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Silindirin etrafında üst katta bir NavMesh noktası bulur.
    /// </summary>
    bool TryFindTopNavMesh(Transform cyl, Vector3 topBase, out NavMeshHit best)
    {
        best = default;
        float bestY = float.MinValue;

        // silindirin etrafında halka
        for (int i = 0; i < topRingSamples; i++)
        {
            float ang = (i / (float)topRingSamples) * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            Vector3 probe = cyl.position + dir * topRingRadius;
            probe.y = topBase.y;

            if (NavMesh.SamplePosition(probe, out var h, sampleRadius, NavMesh.AllAreas))
            {
                if (h.position.y > bestY) { bestY = h.position.y; best = h; }
            }
        }

        // artı: tam üzerinde de dene
        if (NavMesh.SamplePosition(topBase, out var mid, sampleRadius, NavMesh.AllAreas))
        {
            if (mid.position.y > bestY) { best = mid; bestY = mid.position.y; }
        }

        return bestY > float.MinValue;
    }

    void DrawDot(Vector3 pos, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.position = pos;
        g.transform.localScale = Vector3.one * 0.2f;
        var r = g.GetComponent<Renderer>();
        if (r) r.material.color = c;
        Destroy(g, 3f);
    }
}
