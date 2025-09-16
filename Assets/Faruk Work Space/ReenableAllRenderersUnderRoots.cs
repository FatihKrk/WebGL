using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ReenableAllRenderersUnderRoots : MonoBehaviour
{
    public List<Transform> roots = new List<Transform>();
    public float delaySeconds = 6f;
    public bool enableColliders = true;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delaySeconds);
        foreach (var r in roots)
        {
            if (!r) continue;
            foreach (var ren in r.GetComponentsInChildren<Renderer>(true)) ren.enabled = true;
            if (enableColliders)
                foreach (var col in r.GetComponentsInChildren<Collider>(true)) col.enabled = true;
        }
        Debug.Log($"[PFM] Re-enabled renderers under {roots.Count} root(s).");
    }
}
