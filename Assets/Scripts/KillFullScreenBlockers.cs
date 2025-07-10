#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class KillFullScreenBlockers : MonoBehaviour
{
    [ContextMenu("Disable Full-Screen Raycasters")]
    void Fix()
    {
        foreach (Image img in GetComponentsInChildren<Image>(true))
        {
            Rect r = img.rectTransform.rect;
            if (r.width > 1000 && r.height > 600 && img.raycastTarget)
            {
                img.raycastTarget = false;
                Debug.Log("Raycast kapatıldı ➜ " + img.name, img);
            }
        }
        foreach (CanvasGroup cg in GetComponentsInChildren<CanvasGroup>(true))
        {
            if (cg.blocksRaycasts && cg.GetComponent<Image>() == null)   // genelde şeffaf panel
            {
                cg.blocksRaycasts = false;
                Debug.Log("CanvasGroup blocksRaycasts kapatıldı ➜ " + cg.name, cg);
            }
        }
    }
}
#endif
