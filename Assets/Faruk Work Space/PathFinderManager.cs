using System.Collections.Generic;
using UnityEngine;

public class PathFinderManager : MonoBehaviour
{
    public GameObject parentRoot;
    public GameObject targetPrefab;

    void Start()
    {
        if (parentRoot == null || targetPrefab == null) return;

        MeshRenderer[] meshRenderers = parentRoot.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer rend in meshRenderers)
        {
            GameObject original = rend.gameObject;
            MeshFilter mf = original.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            // MeshCollider'ları kapat
            MeshCollider[] colliders = original.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider col in colliders) col.enabled = false;

            Mesh mesh = mf.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // 1) Tabandaki uygun üçgenleri bul (eğim ve en düşük Z)
            List<int> bottomTris = new List<int>();
            float thresholdAngle = 60f;
            float lowestZ = float.MaxValue;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                float angleToBack = Vector3.Angle(normal, Vector3.back);
                float avgZ = (v0.z + v1.z + v2.z) / 3f;

                if (angleToBack < thresholdAngle && avgZ < lowestZ + 0.1f)
                {
                    bottomTris.Add(i);
                    if (avgZ < lowestZ) lowestZ = avgZ;
                }
            }

            if (bottomTris.Count == 0)
            {
                Debug.LogWarning("Taban yüzeyi bulunamadı: " + original.name);
                continue;
            }

            // 2) Tabanın sınır kenarlarını bul (konkav şekiller için sağlam)
            Dictionary<(int, int), int> edgeUse = new Dictionary<(int, int), int>();
            foreach (int ti in bottomTris)
            {
                int a = triangles[ti];
                int b = triangles[ti + 1];
                int c = triangles[ti + 2];
                AddEdge(edgeUse, a, b);
                AddEdge(edgeUse, b, c);
                AddEdge(edgeUse, c, a);
            }

            List<(int, int)> borderEdges = new List<(int, int)>();
            foreach (var kv in edgeUse)
                if (kv.Value == 1) borderEdges.Add(kv.Key);

            if (borderEdges.Count == 0)
            {
                Debug.LogWarning("Taban sınırı çıkarılamadı: " + original.name);
                continue;
            }

            // 3) Sınır komşulukları ve halka oluştur
            Dictionary<int, List<int>> adj = new Dictionary<int, List<int>>();
            foreach (var e in borderEdges)
            {
                int i0 = e.Item1, i1 = e.Item2;
                if (!adj.ContainsKey(i0)) adj[i0] = new List<int>();
                if (!adj.ContainsKey(i1)) adj[i1] = new List<int>();
                adj[i0].Add(i1);
                adj[i1].Add(i0);
            }

            List<int> loop = BuildLoop(adj);
            if (loop == null || loop.Count < 3)
            {
                Debug.LogWarning("Sınır halkası oluşturulamadı: " + original.name);
                continue;
            }

            // 4) Local space'deki sınır noktaları
            List<Vector3> borderLocal = new List<Vector3>(loop.Count);
            foreach (int vi in loop) borderLocal.Add(vertices[vi]);

            // 5) Taban düzlemini hesapla (yaklaşık normal)
            Vector3 approxNormal = Vector3.Cross(borderLocal[1] - borderLocal[0], borderLocal[2] - borderLocal[0]).normalized;
            Vector3 yDir = approxNormal;

            // 6) Tabana göre eksenler (xDir: sınırdaki ilk kenar doğrultusu, zDir: xDir'e dik)
            Vector3 xDir = (borderLocal[1] - borderLocal[0]).normalized;
            Vector3 zDir = Vector3.Cross(yDir, xDir).normalized;

            // 7) Sınır noktalarını xz düzlemine projekte et (2D koordinatlar)
            List<Vector2> border2D = new List<Vector2>();
            foreach (var p in borderLocal)
            {
                Vector3 r = p - borderLocal[0];
                float x = Vector3.Dot(r, xDir);
                float z = Vector3.Dot(r, zDir);
                border2D.Add(new Vector2(x, z));
            }

            // 8) 2D noktaların AABB'sini (minimum bounding box) hesapla
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (var p in border2D)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minZ) minZ = p.y;
                if (p.y > maxZ) maxZ = p.y;
            }

            Vector2 center2D = new Vector2((minX + maxX) / 2f, (minZ + maxZ) / 2f);
            float width = maxX - minX;
            float depth = maxZ - minZ;

            // 9) 2D merkezden tekrar 3D pozisyona dönüştür
            Vector3 localCenter = borderLocal[0] + xDir * center2D.x + zDir * center2D.y;

            // 10) Prefab instantiate ve ayarla
            GameObject newObj = Instantiate(targetPrefab);
            newObj.name = original.name + "_PlacedPrefab";
            newObj.layer = LayerMask.NameToLayer("Ground");

            newObj.transform.SetParent(original.transform, false);
            newObj.transform.localPosition = localCenter;
            newObj.transform.localRotation = Quaternion.LookRotation(zDir, yDir);

            // Yüzü ters çevir (aşağı değil yukarı baksın)
            newObj.transform.localRotation *= Quaternion.Euler(180f, 0f, 0f);

            float height = 0.01f;
            float scaleFactor = 0.1f;
            newObj.transform.localScale = new Vector3(width * scaleFactor, height * scaleFactor, depth * scaleFactor);

            // Orijinal mesh renderer kapat
            rend.enabled = false;
        }
    }

    // Yardımcılar
    private static void AddEdge(Dictionary<(int, int), int> dict, int a, int b)
    {
        if (a > b) { int t = a; a = b; b = t; }
        var key = (a, b);
        dict.TryGetValue(key, out int cnt);
        dict[key] = cnt + 1;
    }

    private static List<int> BuildLoop(Dictionary<int, List<int>> adj)
    {
        int start = int.MaxValue;
        foreach (var k in adj.Keys) if (k < start) start = k;

        List<int> loop = new List<int>();
        int prev = -1;
        int curr = start;

        int safety = 0, maxSteps = adj.Count * 4 + 8;

        while (safety++ < maxSteps)
        {
            loop.Add(curr);
            List<int> nb = adj[curr];

            int next = -1;
            for (int i = 0; i < nb.Count; i++)
            {
                int c = nb[i];
                if (c != prev) { next = c; break; }
            }
            if (next == -1) break;
            if (next == start) break;

            prev = curr;
            curr = next;
        }

        return loop.Count >= 3 ? loop : null;
    }
}
