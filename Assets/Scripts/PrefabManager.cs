using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

public class PrefabManager : MonoBehaviour
{
    VisualQueryManager visualQueryManager;
    GameObject first_Parent;
    Slider slider;
    TMP_Text objectText;
    private Dictionary<int, Color> colorDictionary = new Dictionary<int, Color>();

    void Start()
    {
        visualQueryManager = GameObject.FindGameObjectWithTag("Canvas").GetComponentInChildren<VisualQueryManager>();
        objectText = gameObject.GetComponentInChildren<TMP_Text>();
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        slider = gameObject.GetComponentInChildren<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    public Color GetColor(int id)
    {
        if (!colorDictionary.ContainsKey(id))
        {
            colorDictionary[id] = GenerateColor(id);
        }

        return colorDictionary[id];
    }

    private Color GenerateColor(int seed)
    {
        Random.InitState(seed);

        float hue = Random.Range(0f, 1f);   // Tüm renk spektrumu
        float saturation = Random.Range(0.5f, 1f); // Daha canlı renkler
        float value = Random.Range(0.5f, 1f); // Parlaklık

        return Color.HSVToRGB(hue, saturation, value);
    }

    void OnSliderValueChanged(float value)
    {
        OnOffObjects();
    }

    public void OnOffObjects()
    {
        visualQueryManager.loadingPanel.SetActive(true);
        objectText = gameObject.GetComponentInChildren<TMP_Text>();
        
        if(slider.value == 1)
        {
            if (gameObject.activeInHierarchy)
            {
                ResetColor();
            }
        }
        else if(slider.value == 0)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(ChangeColor());
            }          
        }
    }
    private Color grayColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.60f);
    IEnumerator ChangeColor()
    {
        int i = 0;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();

        foreach (var group in visualQueryManager.groupedData)
        {
            string groupName = group.Key;
            List<string> names = group.Value;
            Color groupColor = GetColor(i);
            if(objectText.text == groupName)
            {
                foreach (var name in names)
                {
                    Transform item = GetOrCacheItem(name);
                    if (item != null)
                    {
                        foreach (var renderer in item.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            renderer.GetPropertyBlock(block);
                            if(block.GetColor("_Color") == groupColor)
                            {
                                SetRendererColor(renderer, grayColor, block);
                                processedRenderers.Add(renderer);
                            }
                        }
                    }
                }
                break;
            }
            i++;
            yield return null;
        }
        visualQueryManager.loadingPanel.SetActive(false);
    }

    private void SetRendererColor(Renderer renderer, Color color, MaterialPropertyBlock block)
    {
        renderer.GetPropertyBlock(block);
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);
    }

    void ResetColor()
    {
        int i = 0;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();

        foreach (var group in visualQueryManager.groupedData)
        {
            string groupName = group.Key;
            List<string> names = group.Value;
            Color groupColor = GetColor(i);
            if(objectText.text == groupName)
            {
                foreach (var name in names)
                {
                    Transform item = GetOrCacheItem(name);
                    if (item != null)
                    {
                        foreach (var renderer in item.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            renderer.GetPropertyBlock(block);
                            SetRendererColor(renderer, groupColor, block);
                            processedRenderers.Add(renderer);
                        }
                    }
                }
                break;
            }
            i++;
        }
        visualQueryManager.loadingPanel.SetActive(false);
    }

    Dictionary<string, Transform> cachedItems = new Dictionary<string, Transform>();

    Transform GetOrCacheItem(string name)
    {
        if (!cachedItems.ContainsKey(name))
        {
            Transform item = Search(name);
            cachedItems[name] = item;
        }
        return cachedItems[name];
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
    
    public void InstantiateItems(GameObject text)
    {
        visualQueryManager.InstantiateItems(text);
    }
}
