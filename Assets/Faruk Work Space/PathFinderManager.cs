using UnityEngine;

public class PathFinderManager : MonoBehaviour
{
    public GameObject parentRoot;

    void Start()
    {
        if (parentRoot == null)
            return;

        MeshRenderer[] meshRenderers = parentRoot.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer rend in meshRenderers)
        {
            GameObject original = rend.gameObject;

            MeshFilter originalMF = original.GetComponent<MeshFilter>();
            MeshCollider meshCollider = original.GetComponent<MeshCollider>();

            if (originalMF == null || originalMF.sharedMesh == null || meshCollider == null || meshCollider.sharedMesh == null)
                continue;

            Mesh colliderMesh = meshCollider.sharedMesh;
            Vector3[] localVertices = colliderMesh.vertices;

            // Yeni GameObject oluştur (sadece 1 kopya)
            GameObject copy = new GameObject(original.name + "_Copy");
            copy.transform.SetParent(original.transform, false); // Child yap
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            copy.transform.localScale = Vector3.one;

            // Mesh'i collider'dan al ama local vertexleri kullan
            Mesh newMesh = new Mesh();
            newMesh.vertices = localVertices;
            newMesh.triangles = colliderMesh.triangles;
            newMesh.RecalculateNormals();

            // MeshFilter ve MeshRenderer ekle
            MeshFilter copyMF = copy.AddComponent<MeshFilter>();
            copyMF.sharedMesh = newMesh;

            MeshRenderer copyMR = copy.AddComponent<MeshRenderer>();
            Material whiteMat = new Material(Shader.Find("Standard"));
            whiteMat.color = Color.white;
            copyMR.material = whiteMat;
        }
    }
}
