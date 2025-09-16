using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class AutoRampArea : MonoBehaviour
{
    [Header("Neleri tara? (senin LadderLinkMaker ile ayný kökler)")]
    public List<Transform> roots;              // Örn: /GEG-CIV_STRU_2_AccessRoute, /PH2_AccessRoute (Hierarchy’de olanlar)
    [Tooltip("Ýsminde bu parçalar geçen objeler rampadýr")]
    public string[] nameContains = { "AccessRoute" };

    [Header("NavMesh")]
    public NavMeshSurface surface;             // Manager’daki NavMeshSurface’ý sürükle
    [Range(1, 10000)] public float rampCost = 250f; // Rampa aþýrý pahalý
    public string rampAreaName = "Ramp";
    public bool includeInactive = true;
    public bool log = true;

    void Start() { Apply(); }

    public void Apply()
    {
        if (surface == null) surface = FindObjectOfType<NavMeshSurface>();
        int rampArea = NavMesh.GetAreaFromName(rampAreaName);
        if (rampArea < 0) { Debug.LogError($"[RampTagger] '{rampAreaName}' area’sýný AI Navigation > Areas’dan ekle."); return; }

        int count = 0;
        foreach (var root in roots.Where(r => r != null))
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive);
            foreach (var r in renderers)
            {
                string n = r.gameObject.name;
                if (!nameContains.Any(s => n.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)) continue;

                var mod = r.GetComponent<NavMeshModifier>();
                if (mod == null) mod = r.gameObject.AddComponent<NavMeshModifier>();
                mod.overrideArea = true;
                mod.area = rampArea;
                mod.ignoreFromBuild = false;
                count++;
            }
        }

        // Rampa maliyetini yükselt
        NavMesh.SetAreaCost(rampArea, rampCost);

        // Tek seferlik hýzlý rebake
        if (surface != null) surface.BuildNavMesh();

        if (log) Debug.Log($"[RampTagger] {count} ramp nesnesi '{rampAreaName}' alanýna etiketlendi. Cost={rampCost}");
    }
}
