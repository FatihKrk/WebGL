using UnityEngine;
using System.Collections;

public class ReenableTaggedRenderersAfterPFM : MonoBehaviour
{
    public string tagName = "Ladder";
    public float delaySeconds = 6f;   // PFM 5 sn gecikmeli, biz 6.sn'de açýyoruz
    public bool alsoEnableColliders = true;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delaySeconds);
        var objs = GameObject.FindGameObjectsWithTag(tagName);
        foreach (var go in objs)
        {
            var r = go.GetComponent<Renderer>();
            if (r) r.enabled = true;
            if (alsoEnableColliders)
                foreach (var col in go.GetComponentsInChildren<Collider>(true))
                    col.enabled = true;
        }
        Debug.Log($"[Ladder] Re-enabled {objs.Length} renderer(s).");
    }
}
