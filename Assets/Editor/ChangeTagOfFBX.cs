#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ChangeTagOfFBX : MonoBehaviour
{
    [MenuItem("Tools/Set Tag to RVMObjects for Scene Object")]
    static void SetTagForSceneObjects()
    {
        // Sahnedeki "TPO_PH1_Meg" adlı objeyi buluyoruz
        GameObject tpoObject = GameObject.FindGameObjectWithTag("ParentObject");

        if (tpoObject == null)
        {
            Debug.LogError("TPO_PH1_Meg adlı obje sahnede bulunamadı.");
            return;
        }

        // tpo ve onun alt objelerini topluca alıyoruz
        MeshRenderer[] allChildren = tpoObject.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var child in allChildren)
        {
            GameObject childObj = child.gameObject;

            // Tag'i "RVMObjects" olarak ayarlıyoruz
            childObj.tag = "RVMObjects";
        }

        Debug.Log("Sahnedeki objelerin tag'leri RVMObjects olarak ayarlandı.");
    }
}
#endif
