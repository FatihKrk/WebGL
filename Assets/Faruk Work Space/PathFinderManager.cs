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

            // MeshCollider'ları devre dışı bırak
            MeshCollider[] colliders = original.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider col in colliders)
                col.enabled = false;

            Mesh mesh = mf.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            List<int> bottomTriangles = new List<int>();
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
                    bottomTriangles.Add(i);
                    if (avgZ < lowestZ)
                        lowestZ = avgZ;
                }
            }

            bool foundQuad = false;
            Vector3[] quadVertices = null;

            for (int i = 0; i < bottomTriangles.Count; i++)
            {
                int triA = bottomTriangles[i];
                int a0 = triangles[triA];
                int a1 = triangles[triA + 1];
                int a2 = triangles[triA + 2];

                for (int j = i + 1; j < bottomTriangles.Count; j++)
                {
                    int triB = bottomTriangles[j];
                    int b0 = triangles[triB];
                    int b1 = triangles[triB + 1];
                    int b2 = triangles[triB + 2];

                    int[] aVerts = { a0, a1, a2 };
                    int[] bVerts = { b0, b1, b2 };

                    HashSet<int> sharedVerts = new HashSet<int>();
                    foreach (int av in aVerts)
                        foreach (int bv in bVerts)
                            if (av == bv) sharedVerts.Add(av);

                    if (sharedVerts.Count == 2)
                    {
                        HashSet<int> allVerts = new HashSet<int>(aVerts);
                        allVerts.UnionWith(bVerts);

                        if (allVerts.Count == 4)
                        {
                            quadVertices = new Vector3[4];
                            int index = 0;
                            foreach (int vi in allVerts)
                                quadVertices[index++] = vertices[vi];

                            foundQuad = true;
                            break;
                        }
                    }
                }
                if (foundQuad) break;
            }

            if (!foundQuad)
            {
                Debug.LogWarning("Dörtgen taban yüzeyi bulunamadı: " + original.name);
                continue;
            }

            // Yüzeydeki noktalar localSpace, dünya uzayına çevirmeye gerek yok
            // Bu vertexlere göre prefab yerleştirilecek

            // Pozisyon
            Vector3 localCenter = (quadVertices[0] + quadVertices[1] + quadVertices[2] + quadVertices[3]) / 4f;

            // Yönler
            Vector3 xDir = (quadVertices[1] - quadVertices[0]).normalized;
            Vector3 zDir = (quadVertices[3] - quadVertices[0]).normalized;
            Vector3 yDir = Vector3.Cross(zDir, xDir).normalized;

            // Boyut
            float width = Vector3.Distance(quadVertices[0], quadVertices[1]);
            float depth = Vector3.Distance(quadVertices[0], quadVertices[3]);
            float height = 0.01f;

            // Instantiate
            GameObject newObj = Instantiate(targetPrefab);
            newObj.name = original.name + "_PlacedPrefab";
            newObj.layer = LayerMask.NameToLayer("Ground");

            // Yerleştirme
            newObj.transform.SetParent(original.transform, false); // önce localSpace içinde
            newObj.transform.localPosition = localCenter;
            newObj.transform.localRotation = Quaternion.LookRotation(zDir, yDir);
            float scaleFactor = 0.1f;
            newObj.transform.localScale = new Vector3(width * scaleFactor, height * scaleFactor, depth * scaleFactor);

            // Orijinal MeshRenderer'ı kapat
            rend.enabled = false;
        }
    }
}
