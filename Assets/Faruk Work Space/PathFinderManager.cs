using System.Collections.Generic;
using UnityEngine;

public class PathFinderManager : MonoBehaviour
{
    public GameObject parentRoot;

    void Start()
    {
        if (parentRoot == null) return;

        MeshRenderer[] meshRenderers = parentRoot.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer rend in meshRenderers)
        {
            GameObject original = rend.gameObject;
            MeshFilter mf = original.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;

            // --- BURASI EKLENDİ ---
            // Orijinal objenin ve varsa tüm child objelerin MeshCollider'larını kapat
            MeshCollider[] colliders = original.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider col in colliders)
            {
                col.enabled = false;
            }
            // --- BURASI BİTTİ ---

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

                    int sharedCount = 0;
                    int[] aVerts = { a0, a1, a2 };
                    int[] bVerts = { b0, b1, b2 };

                    HashSet<int> sharedVerts = new HashSet<int>();

                    foreach (int aV in aVerts)
                    {
                        foreach (int bV in bVerts)
                        {
                            if (aV == bV)
                            {
                                sharedCount++;
                                sharedVerts.Add(aV);
                            }
                        }
                    }

                    if (sharedCount == 2)
                    {
                        HashSet<int> quadVertIndices = new HashSet<int>(aVerts);
                        quadVertIndices.UnionWith(bVerts);

                        if (quadVertIndices.Count == 4)
                        {
                            quadVertices = new Vector3[4];
                            int index = 0;
                            foreach (int vi in quadVertIndices)
                            {
                                quadVertices[index++] = vertices[vi];
                            }
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

            Vector3[] prismVertices = new Vector3[8];
            for (int i = 0; i < 4; i++)
            {
                prismVertices[i] = quadVertices[i];
                prismVertices[i + 4] = quadVertices[i] + new Vector3(0, 0, 0.01f);
            }

            int[] prismTriangles = new int[]
            {
                0, 1, 2,
                2, 3, 0,

                4, 6, 5,
                6, 4, 7,

                0, 4, 5,
                5, 1, 0,

                1, 5, 6,
                6, 2, 1,

                2, 6, 7,
                7, 3, 2,

                3, 7, 4,
                4, 0, 3
            };

            Mesh prismMesh = new Mesh();
            prismMesh.vertices = prismVertices;
            prismMesh.triangles = prismTriangles;
            prismMesh.RecalculateNormals();

            GameObject copy = new GameObject(original.name + "_BottomPrism");
            copy.transform.SetParent(original.transform, false);

            copy.layer = LayerMask.NameToLayer("Ground");

            MeshFilter copyMF = copy.AddComponent<MeshFilter>();
            copyMF.mesh = prismMesh;

            MeshRenderer copyMR = copy.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.white;
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            copyMR.material = mat;

            MeshCollider copyCollider = copy.AddComponent<MeshCollider>();
            copyCollider.sharedMesh = prismMesh;
            copyCollider.convex = true;

            rend.enabled = false; // Orijinal objeyi kapatmak istersen burayı açabilirsin
        }
    }
}
