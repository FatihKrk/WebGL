using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

[DefaultExecutionOrder(100)]
public class OffMeshLinkSnapper : MonoBehaviour
{
    public int waitFrames = 3;
    public float snapRadius = 2.5f;
    public float[] extraRadii = new float[] { 4f, 6f, 8f, 12f };

    [Tooltip("Raycast ile aþaðý/yukarý yüzey arar")]
    public float raycastMax = 10f;

    IEnumerator Start()
    {
        for (int i = 0; i < waitFrames; i++) yield return null;
        SnapAll();
    }

    void SnapAll()
    {
        int offOk = 0, offFail = 0, navOk = 0, navFail = 0;

        // ---- Eski OffMeshLink ----
#pragma warning disable 0618
        foreach (var l in FindObjectsOfType<OffMeshLink>(false))
        {
            if (!l.enabled || !l.gameObject.activeInHierarchy || !l.startTransform || !l.endTransform) continue;

            Vector3 s = l.startTransform.position;
            Vector3 e = l.endTransform.position;

            bool sOk = ForceSnap(ref s);
            bool eOk = ForceSnap(ref e);

            if (sOk && eOk)
            {
                l.startTransform.position = s;
                l.endTransform.position = e;
                offOk++;
            }
            else
            {
                offFail++;
                Debug.LogWarning($"[Snapper] OffMeshLink snap FAIL: {l.name} (s:{sOk} e:{eOk})");
            }
        }
#pragma warning restore 0618

        // ---- Yeni NavMeshLink ----
        foreach (var nl in FindObjectsOfType<NavMeshLink>(false))
        {
            if (!nl.enabled || !nl.gameObject.activeInHierarchy) continue;

            Vector3 s = nl.transform.TransformPoint(nl.startPoint);
            Vector3 e = nl.transform.TransformPoint(nl.endPoint);

            bool sOk = ForceSnap(ref s);
            bool eOk = ForceSnap(ref e);

            if (sOk && eOk)
            {
                nl.startPoint = nl.transform.InverseTransformPoint(s);
                nl.endPoint = nl.transform.InverseTransformPoint(e);
                nl.UpdateLink();
                navOk++;
            }
            else
            {
                navFail++;
                Debug.LogWarning($"[Snapper] NavMeshLink snap FAIL: {nl.name} (s:{sOk} e:{eOk})");
            }
        }

        Debug.Log($"[Snapper] Off ok={offOk} fail={offFail} | Nav ok={navOk} fail={navFail}");
    }

    bool ForceSnap(ref Vector3 p)
    {
        // 1) Direkt SamplePosition
        if (NavMesh.SamplePosition(p, out var hit, snapRadius, NavMesh.AllAreas)) { p = hit.position; return true; }
        foreach (var r in extraRadii)
            if (NavMesh.SamplePosition(p, out hit, r, NavMesh.AllAreas)) { p = hit.position; return true; }

        // 2) Aþaðý doðru raycast (mesh collider’a)
        if (Physics.Raycast(new Vector3(p.x, p.y + raycastMax, p.z), Vector3.down, out RaycastHit rh, raycastMax * 2f))
        {
            Vector3 proj = rh.point + Vector3.up * 0.05f;
            if (NavMesh.SamplePosition(proj, out hit, snapRadius, NavMesh.AllAreas)) { p = hit.position; return true; }
        }

        // 3) Yukarý ve aþaðý yönde adýmlý tarama
        for (float dy = 0.5f; dy <= raycastMax; dy += 0.5f)
        {
            Vector3 up = p + Vector3.up * dy;
            if (NavMesh.SamplePosition(up, out hit, snapRadius, NavMesh.AllAreas)) { p = hit.position; return true; }

            Vector3 down = p + Vector3.down * dy;
            if (NavMesh.SamplePosition(down, out hit, snapRadius, NavMesh.AllAreas)) { p = hit.position; return true; }
        }

        return false;
    }
}
