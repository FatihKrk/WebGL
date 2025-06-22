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
    private MaterialPropertyBlock grayBlock;

    void Start()
    {
        visualQueryManager = GameObject.FindGameObjectWithTag("Canvas").GetComponentInChildren<VisualQueryManager>(true);
        objectText = gameObject.GetComponentInChildren<TMP_Text>(true);
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        slider = gameObject.GetComponentInChildren<Slider>(true);
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
        objectText = gameObject.GetComponentInChildren<TMP_Text>(true);
        
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
        grayBlock = new MaterialPropertyBlock();
        grayBlock.SetFloat("_UseOverrideColor", 1f);
        grayBlock.SetColor("_OverrideColor", grayColor);

        foreach (var group in visualQueryManager.groupedData)
        {
            string groupName = group.Key;
            List<string> names = group.Value;
            Color groupColor = GetColor(i);

            if (objectText.text == groupName)
            {
                foreach (var name in names)
                {
                    Transform item = Search(name);
                    if (item != null)
                    {
                        foreach (var renderer in item.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            visualQueryManager.changedBlocks[renderer] = grayBlock;
                            renderer.SetPropertyBlock(grayBlock);
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

    void ResetColor()
    {
        int i = 0;
        MaterialPropertyBlock groupblock = new MaterialPropertyBlock();
        groupblock = new MaterialPropertyBlock();
        groupblock.SetFloat("_UseOverrideColor", 1f);

        foreach (var group in visualQueryManager.groupedData)
        {
            string groupName = group.Key;
            List<string> names = group.Value;
            Color groupColor = GetColor(i);

            if (objectText.text == groupName)
            {
                foreach (var name in names)
                {
                    Transform item = Search(name);
                    if (item != null)
                    {
                        foreach (var renderer in item.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            groupblock.SetColor("_OverrideColor", groupColor);
                            visualQueryManager.changedBlocks[renderer] = groupblock;
                            renderer.SetPropertyBlock(groupblock);
                        }
                    }
                }
                break;
            }
            i++;
        }

        visualQueryManager.loadingPanel.SetActive(false);
    }

private void SetRendererOverrideColor(Renderer renderer, Color color, MaterialPropertyBlock block)
{
    renderer.GetPropertyBlock(block);
    block.SetFloat("_UseOverrideColor", 1f);
    block.SetColor("_OverrideColor", color);
    renderer.SetPropertyBlock(block);
}


    private Dictionary<string, List<Transform>> _nameCache;

    void BuildNameCache()
    {
        _nameCache = new Dictionary<string, List<Transform>>(System.StringComparer.OrdinalIgnoreCase);
        Transform[] allTransforms = first_Parent.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform t in allTransforms)
        {
            if (!_nameCache.ContainsKey(t.name))
            {
                _nameCache[t.name] = new List<Transform>();
            }
            _nameCache[t.name].Add(t);
        }
    }

    public Transform Search(string searchedText)
    {
        if (string.IsNullOrEmpty(searchedText)) return null;
        if (_nameCache == null) BuildNameCache(); // Cache yoksa yeniden oluştur
        if (_nameCache.TryGetValue(searchedText, out List<Transform> transforms) && transforms.Count > 0) {
            return transforms[0];
        }
        return null;
    }
    
    public void InstantiateItems(GameObject text)
    {
        visualQueryManager.InstantiateItems(text);
    }
}
