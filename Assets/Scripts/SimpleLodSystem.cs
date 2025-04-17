using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLodSystem : MonoBehaviour
{
    GameObject first_Parent, user;
    Vector3 centerPos = new Vector3(202,107,1505);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        user = GameObject.FindGameObjectWithTag("MainCamera");
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        InvokeRepeating(nameof(CheckDistanceAndToggle), 0f, 0.5f); // Yarım saniyede bir kontrol
        if (first_Parent == null) return;

        foreach (MeshRenderer renderer in first_Parent.GetComponentsInChildren<MeshRenderer>(true))
        {
            Bounds bounds = renderer.bounds;
            float volume = bounds.size.x * bounds.size.y * bounds.size.z;

            if (volume < 0.01)
            {
                GameObject obj = renderer.gameObject;
                if (!cachedItems.Contains(obj))
                {
                    cachedItems.Add(obj);
                }
            }
        }
    }

    void CheckDistanceAndToggle()
    {
        float distanceToCenter = Vector3.Distance(user.transform.position, centerPos);

        if (distanceToCenter >= 20f && !isFar)
        {
            isFar = true;
            StartCoroutine(SetRenderersActive(false));
        }
        else if (distanceToCenter < 20f && isFar)
        {
            isFar = false;
            StartCoroutine(SetRenderersActive(true));
        }
    }

    // Bu flag her iki durumu kontrol eder
    private bool isFar = false;

    IEnumerator SetRenderersActive(bool state)
    {
        int chunkSize = 250; // Her frame'de 20 renderer aktif/pasif et
        int count = 0;

        foreach (GameObject item in cachedItems)
        {
            foreach (var renderer in item.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.enabled = state;
                count++;

                if (count >= chunkSize)
                {
                    count = 0;
                    yield return null; // Bir frame bekle
                }
            }
        }
    }



    List<GameObject> cachedItems = new List<GameObject>();

    GameObject GetOrCacheItem(string name)
    {
        Transform item = Search(name);
        if (item == null)
        {
            return null;
        }
        return item.gameObject;
    }


    public Transform Search(string searched_Text)
    {
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(first_Parent.transform);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            // Objeyi adıyla karşılaştırıyoruz
            if (current.name.IndexOf(searched_Text, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return current; // Objeyi bulduktan sonra hemen geri dön
            }

            // İlk olarak child'ları stack'e tersten ekleyerek önce ilk child'ın işlenmesini sağlıyoruz
            for (int i = current.childCount - 1; i >= 0; i--)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return null; // Eğer obje bulunamazsa null döner
    }
}
