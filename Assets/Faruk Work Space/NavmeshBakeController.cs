using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshSurface))]
public class NavmeshBakeController : MonoBehaviour
{
    private NavMeshSurface surface;

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();

        // Objenin meshlerine göre world space'de ayarla
        FitSurfaceToWorldBounds();

        // Bake baþlat
        BakeNavMesh();
    }

    void FitSurfaceToWorldBounds()
    {
        // Objeye ve alt objelerine ait tüm MeshRenderer'larý al
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return;

        Bounds worldBounds = renderers[0].bounds;
        foreach (var rend in renderers)
            worldBounds.Encapsulate(rend.bounds);

        // World space'te bulunan bounds'ý local space'e çevir
        Vector3 worldCenter = worldBounds.center;
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);

        surface.center = localCenter;
        surface.size = worldBounds.size;

        // Ayrýca sadece kendi çocuklarýný topla
        surface.collectObjects = CollectObjects.Children;
    }

    void BakeNavMesh()
    {
#if UNITY_EDITOR
        surface.BuildNavMesh();
#else
        Debug.LogWarning("Runtime'da NavMesh bake desteklenmez.");
#endif
    }
}