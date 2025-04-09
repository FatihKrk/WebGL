using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPrefab : MonoBehaviour
{
    VisualQueryManager visualQueryManager;
    MouseClick mouseClick;
    GameObject first_Parent, itemPanel;
    Transform selectedItem;
    Vector3 itemPos;
    Slider slider;
    private Dictionary<int, Color> colorDictionary = new Dictionary<int, Color>();
    void Start()
    {
        visualQueryManager = GameObject.FindGameObjectWithTag("Canvas").GetComponentInChildren<VisualQueryManager>();
        mouseClick = GameObject.Find("Main Camera").GetComponentInChildren<MouseClick>();
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        itemPanel = GameObject.Find("ByItemNamePanel");
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
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(OnOrOffItem());
        }
    }

    IEnumerator OnOrOffItem()
    {
        visualQueryManager.loadingPanel.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        visualQueryManager.ControlItemSlider();
        selectedItem = Search(GetComponentInChildren<TMP_Text>().text);
        if(selectedItem != null)
        {
            if(slider.value == 0)
            {
                OffItem();
            }
            else if(slider.value == 1)
            {
                OnItem();
            }
        }
    }

    void OnItem()
    {
        ResetColor();
    }

    void OffItem()
    {
        ChangeColor();
    }

    public void SelectItem()
    {
        selectedItem = Search(GetComponentInChildren<TMP_Text>().text);
        if(selectedItem != null)
        {
            mouseClick.currentObject = selectedItem.gameObject;
            mouseClick.FindItemPosition();
            StartCoroutine(mouseClick.FindParents(mouseClick.currentObject));
            StartCoroutine(mouseClick.Expand());
            if(mouseClick.attributes_Panel.activeSelf)
            {
                mouseClick.ShowAttributes();
            }
        }
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

    private Color grayColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.60f);

    void ChangeColor()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();
        Color groupColor = Color.grey;
        
        foreach(var group in visualQueryManager.groupColors)
        {
            if(itemPanel!= null)
            {
                if(group.Key == itemPanel.GetComponentInChildren<TMP_Text>().text)
                {
                    groupColor = group.Value;
                }
            }
        }

        if (selectedItem != null)
        {
            selectedItem.GetComponentInChildren<MeshRenderer>(true).GetPropertyBlock(block);
            foreach (var renderer in selectedItem.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.GetPropertyBlock(block);
                if(block.GetColor("_Color") == groupColor)
                {
                    SetRendererColor(renderer, grayColor, block);
                    processedRenderers.Add(renderer);
                }
            }
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
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();

        Color groupColor = Color.grey;
        
        foreach(var group in visualQueryManager.groupColors)
        {
            if(itemPanel != null)
            {
                if(group.Key == itemPanel.GetComponentInChildren<TMP_Text>().text)
                {
                    groupColor = group.Value;
                    
                    if(selectedItem.GetComponentInChildren<MeshRenderer>(true) != null)
                    {
                        MeshRenderer[] renderers = selectedItem.GetComponentsInChildren<MeshRenderer>(true);
                        foreach (var renderer in renderers)
                        {
                            processedRenderers.Add(renderer);
                            renderer.GetPropertyBlock(block);
                            block.SetColor("_Color", groupColor);
                            renderer.SetPropertyBlock(block);
                        }
                    }
                }
            } 
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

}
