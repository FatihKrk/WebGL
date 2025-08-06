using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

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
        StartCoroutine(DelayedBake());
    }

    IEnumerator DelayedBake()
    {
        yield return new WaitForSeconds(5f); // ⏱️ 1 saniye bekle

        MarkOnlyGroundObjects();
        FitBoundsToGroundObjects();
        BakeNavMesh();
    }

    void MarkOnlyGroundObjects()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj == this.gameObject) continue;

            if (!obj.TryGetComponent(out NavMeshModifier modifier))
                modifier = obj.AddComponent<NavMeshModifier>();

            if (obj.layer == LayerMask.NameToLayer("Ground"))
                modifier.ignoreFromBuild = false;
            else
                modifier.ignoreFromBuild = true;
        }

        surface.collectObjects = CollectObjects.All;
    }

    void FitBoundsToGroundObjects()
    {
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
        List<MeshRenderer> groundRenderers = new List<MeshRenderer>();

        foreach (MeshRenderer rend in allRenderers)
        {
            if (rend.gameObject.layer == LayerMask.NameToLayer("Ground"))
                groundRenderers.Add(rend);
        }

        if (groundRenderers.Count == 0) return;

        Bounds bounds = groundRenderers[0].bounds;
        foreach (MeshRenderer rend in groundRenderers)
            bounds.Encapsulate(rend.bounds);

        Vector3 worldCenter = bounds.center;
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);

        surface.center = localCenter;
        surface.size = bounds.size;
    }

    void BakeNavMesh()
    {
#if UNITY_EDITOR
        surface.RemoveData();      // Önceki NavMeshData'yı sil
        surface.BuildNavMesh();    // Tek bir tane NavMeshData oluştur
#else
        Debug.LogWarning("Runtime'da NavMesh bake edilmez.");
#endif
    }
}
