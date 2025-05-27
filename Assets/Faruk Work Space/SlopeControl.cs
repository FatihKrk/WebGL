using UnityEngine;

public class SlopeControl : MonoBehaviour
{
    void Start()
    {
        // 4. MeshCollider verileri
        MeshCollider collider = GetComponent<MeshCollider>();
        Mesh colliderMesh = collider.sharedMesh;

        if (colliderMesh == null)
        {
            Debug.LogWarning("MeshCollider içinde mesh yok!");
        }
        else
        {
            Debug.Log($"[MeshCollider] Vertex Sayısı: {colliderMesh.vertexCount}");
            for (int i = 0; i < colliderMesh.vertexCount; i++)
            {
                Vector3 local = colliderMesh.vertices[i];
                Vector3 world = transform.TransformPoint(local);
                Debug.Log($"[MeshCollider] Vertex {i}: Local = {local}, World = {world}");
            }
        }
    }
}
