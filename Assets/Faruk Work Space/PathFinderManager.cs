using UnityEngine;

public class PathFinderManager : MonoBehaviour
{
    public GameObject parentRoot;
    public GameObject markerPrefab;
    public float markerThickness = 0.1f;

    void Start()
    {
        if (parentRoot == null || markerPrefab == null)
        {
            Debug.LogWarning("Parent root veya marker prefab atanmadı.");
            return;
        }

        MeshRenderer[] meshRenderers = parentRoot.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer rend in meshRenderers)
        {
            Bounds bounds = rend.bounds;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            // Alt 4 köşe
            Vector3[] localBottomCorners = new Vector3[4];
            localBottomCorners[0] = new Vector3(-extents.x, -extents.y, -extents.z); // sol arka
            localBottomCorners[1] = new Vector3(extents.x, -extents.y, -extents.z);  // sağ arka
            localBottomCorners[2] = new Vector3(-extents.x, -extents.y, extents.z);  // sol ön
            localBottomCorners[3] = new Vector3(extents.x, -extents.y, extents.z);   // sağ ön

            // World space'e çevir
            Vector3[] worldCorners = new Vector3[4];
            for (int i = 0; i < 4; i++)
                worldCorners[i] = rend.transform.TransformPoint(localBottomCorners[i]);

            // 2 ana yön seç (ön-arka ve sağ-sol)
            Vector3 forwardEdge = worldCorners[2] - worldCorners[0];  // sol ön - sol arka
            Vector3 sideEdge = worldCorners[1] - worldCorners[0];     // sağ arka - sol arka

            float forwardSlope = Mathf.Atan2(forwardEdge.y, forwardEdge.z) * Mathf.Rad2Deg;
            float sideSlope = Mathf.Atan2(sideEdge.y, sideEdge.x) * Mathf.Rad2Deg;

            float slopeMagnitude = Mathf.Max(Mathf.Abs(forwardSlope), Mathf.Abs(sideSlope));

            bool isSloped = slopeMagnitude > 1f; // 1 dereceden büyükse eğimlidir diyelim

            if (isSloped)
            {
                Debug.Log($"✅ Eğim var: {rend.name} | Açı: {slopeMagnitude:F2}°");

                // Eğim yönü için world yüzey normalini hesapla
                Vector3 cross = Vector3.Cross(forwardEdge.normalized, sideEdge.normalized);
                Vector3 surfaceNormal = cross.normalized;

                // Eğim yönü: düz yukarıdan ne kadar sapmış
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.up - surfaceNormal, Vector3.up).normalized;

                // Eğim açısını bul
                float slopeAngle = Vector3.Angle(Vector3.up, surfaceNormal);

                // Quaternion rotasyon oluştur (slope yönüne doğru eğilsin)
                Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, slopeDirection), surfaceNormal);

                // Instantiate et
                GameObject marker = Instantiate(markerPrefab, bounds.center, rotation, rend.transform);
                marker.transform.localScale = new Vector3(bounds.size.x, markerThickness, bounds.size.z);
            }
            else
            {
                Debug.Log($"❌ Eğim yok: {rend.name}");

                // Düzse düz yerleştir
                GameObject marker = Instantiate(markerPrefab, bounds.center, Quaternion.identity, rend.transform);
                marker.transform.localScale = new Vector3(bounds.size.x, markerThickness, bounds.size.z);
            }
        }
    }
}
