using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class VisualQueryManager : MonoBehaviour
{
    [SerializeField] MoveButtons moveButtons;
    [SerializeField] GameObject groupPrefab, groupPanelContent, itemPrefab, itemPanelContent;
    public GameObject mainPanel, groupPanel, itemPanel;
    public GameObject loadingPanel;
    [SerializeField ]TMP_Text groupText;
    GameObject first_Parent;
    TMP_Text objectText;
    Slider slider, itemPanelSlider, groupPanelSlider;
    public Material newMaterial;
    private Dictionary<int, Color> colorDictionary = new Dictionary<int, Color>();
    private string apiUrl = "https://m3muhendislik.com/api/sortMegByTag.php";

    [System.Serializable]
    public class ApiResponse
    {
        public List<string> types;
        public List<string> names;
    }

    private Dictionary<string, List<Transform>> _nameCache;

    void Awake()
    {
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        BuildNameCache();
    }

    void Start()
    {
        StartCoroutine(GetData());
        itemPanelSlider = itemPanel.GetComponentInChildren<Slider>(true);
        itemPanelSlider.onValueChanged.AddListener(ChangeItemColors);
        groupPanelSlider = groupPanel.GetComponentInChildren<Slider>(true);
        slider = mainPanel.GetComponentInChildren<Slider>(true);
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

    public void sortByTags()
    {
        loadingPanel.SetActive(true);
        moveButtons.visualQuery = true;
        StartCoroutine(InstantiateGroups());
    }

    public Dictionary<string, Color> groupColors = new Dictionary<string, Color>();
    List<string> validTypes = new List<string> { "EQUI", "PIPE", "VALV", "INST", "PCOM", "STRV", "VTWA", "VFWAY" };
    public Dictionary<string, List<string>> groupedData = new Dictionary<string, List<string>>();

    IEnumerator GetData()
    {
        loadingPanel.SetActive(true);
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();        

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            
            // JSON verisini çözümle
            ApiResponse data = JsonUtility.FromJson<ApiResponse>(jsonResponse);

            for (int j = 0; j < data.types.Count; j++)
            {
                string type = data.types[j];
                string name = data.names[j];

                if (validTypes.Contains(type))
                {
                    if (!groupedData.ContainsKey(type))
                    {
                        groupedData[type] = new List<string>();
                    }
                    groupedData[type].Add(name);
                }
            }
        }
        else
        {
            Debug.LogError("Hata: " + request.error);
        }
        BuildTransformGroupCache();
    }

    IEnumerator InstantiateGroups()
    {

        TMP_Text[] groupTexts = groupPrefab.GetComponentsInChildren<TMP_Text>();

        // UI'ye toplam grup sayısını yaz
        groupText.text = groupedData.Count.ToString() + " groups";
            
        int i = 0;
        foreach (var group in groupedData)
        {
            string groupName = group.Key;  // Grubun adı (örneğin, "EQUI")
            List<string> names = group.Value;  // O gruptaki name'ler

            groupTexts[0].text = groupName;
            groupTexts[1].text = names.Count + " items";

            // Renk ayarla
            Image[] images = groupPrefab.GetComponentsInChildren<Image>();
            Color groupColor = GetColor(i);
            images[images.Length - 1].color = groupColor;

            // Renk bilgisini kaydet
            if (!groupColors.ContainsKey(groupName))
            {
                groupColors.Add(groupName, groupColor);
            }

            i++;
            Instantiate(groupPrefab, groupPanelContent.transform);

            yield return null;
        }
    }


    private void OnSliderValueChanged(float value)
    {
        slider.onValueChanged.RemoveAllListeners(); // Tüm dinleyicileri kaldır
        loadingPanel.SetActive(true);

        sortByTags();
        ChangeColorFonk();
        slider.value = slider.minValue; // Değeri sıfırla
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    IEnumerator InstantiateItems()
    {
        TMP_Text[] itemTexts = itemPanel.GetComponentsInChildren<TMP_Text>();
        Slider[] sliders = groupPanelContent.GetComponentsInChildren<Slider>();

        int i = 0;
        int counter = 0; // Eklenen sayaç

        // Dinamik olarak her type grubuna erişim
        foreach (var group in groupedData)
        {
            string groupName = group.Key;
            List<string> names = group.Value;

            if (objectText.text == groupName)
            {
                itemTexts[0].text = groupName;
                itemTexts[1].text = names.Count.ToString() + " items";

                foreach (var name in names)
                {
                    if (Search(name) != null)
                    {
                        TMP_Text text = itemPrefab.GetComponentInChildren<TMP_Text>();
                        text.text = name;

                        if (sliders[i].value == 0)
                        {
                            itemPrefab.GetComponentInChildren<Slider>().value = 0;
                        }
                        else
                        {
                            itemPrefab.GetComponentInChildren<Slider>().value = 1;
                        }

                        Instantiate(itemPrefab, itemPanelContent.transform);

                        counter++;
                        if (counter % 100 == 0)
                        {
                            yield return null;
                        }
                    }
                }

                Image[] images = itemPanel.GetComponentsInChildren<Image>();
                images[images.Length - 1].color = GetColor(i);
                groupPanel.SetActive(false);
                itemPanel.SetActive(true);
                ControlItemSlider();
                break;
            }
            i++;
        }

        loadingPanel.SetActive(false);
    }


    public void InstantiateItems(GameObject textObj)
    {
        loadingPanel.SetActive(true);
        objectText = textObj.GetComponentInChildren<TMP_Text>();
        StartCoroutine(InstantiateItems());
    }

    public void DestroyItemChildren()
    {
        // Parent'ın tüm çocuklarını döngüyle yok et
        foreach (Transform child in itemPanelContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void DestroyGroupChildren()
    {
        // Parent'ın tüm çocuklarını döngüyle yok et
        foreach (Transform child in groupPanelContent.transform)
        {
            Destroy(child.gameObject);
        }
    }  
    private Color grayColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.60f);

    public void ChangeColorFonk()
    {
        loadingPanel.SetActive(true);
        StartCoroutine(InitializeColorSystem());
    }

    public Dictionary<Renderer, MaterialPropertyBlock> originalBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
    public Dictionary<Renderer, MaterialPropertyBlock> changedBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
    private Dictionary<string, MaterialPropertyBlock> colorBlocks = new Dictionary<string, MaterialPropertyBlock>();
    private HashSet<string> allGroupNames;
    private MaterialPropertyBlock grayBlock;

    IEnumerator InitializeColorSystem()
    {
        // Tüm renderer'ların orijinal değerlerini kaydet
        var allRenderers = first_Parent.GetComponentsInChildren<MeshRenderer>(true);
        allGroupNames = new HashSet<string>(groupedData.SelectMany(g => g.Value));

        // Gri blok ve grup renk bloklarını oluştur
        grayBlock = new MaterialPropertyBlock();
        grayBlock.SetFloat("_UseOverrideColor", 1f);
        grayBlock.SetColor("_OverrideColor", grayColor);
        
        for (int i = 0; i < groupedData.Count; i++)
        {
            var group = groupedData.ElementAt(i);
            var block = new MaterialPropertyBlock();
            block.SetFloat("_UseOverrideColor", 1f);
            block.SetColor("_OverrideColor", GetColor(i));
            colorBlocks[group.Key] = block;
        }

        yield return null;
        StartCoroutine(ChangeColorWithClipping());
    }

    IEnumerator ChangeColorWithClipping()
    {
        loadingPanel.SetActive(true);

        var allRenderers = first_Parent.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var renderer in allRenderers)
        {
            if (transformToGroupMap.ContainsKey(renderer.transform))
                continue;

            if (!originalBlocks.ContainsKey(renderer))
            {
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                originalBlocks[renderer] = block;
            }
            renderer.SetPropertyBlock(grayBlock);
        }

        foreach (var kvp in transformToGroupMap)
        {
            Transform t = kvp.Key;
            string groupName = kvp.Value;

            if (!colorBlocks.TryGetValue(groupName, out var colorBlock))
                continue;

            MeshRenderer[] renderers = t.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var renderer in renderers)
            {
                // Eğer renderer'ın transform'u başka bir grubun ana objesi olarak geçiyorsa, atla
                if (transformToGroupMap.ContainsKey(renderer.transform) && renderer.transform != t)
                    continue;

                if (!originalBlocks.ContainsKey(renderer))
                {
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    originalBlocks[renderer] = block;
                }

                renderer.SetPropertyBlock(colorBlock);

                if (!changedBlocks.ContainsKey(renderer))
                {
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    changedBlocks[renderer] = block;
                }
            }
        }
        yield return null;
        loadingPanel.SetActive(false);
    }


    Dictionary<Transform, string> transformToGroupMap = new Dictionary<Transform, string>();

    void BuildTransformGroupCache()
    {
        foreach (var group in groupedData)
        {
            string groupName = group.Key;
            foreach (var objectName in group.Value)
            {
                Transform objTransform = Search(objectName);
                if (objTransform == null) continue;

                // Objeyi ve alt tüm çocuklarını gruba ata
                AssignGroupRecursive(objTransform, groupName);
            }
        }
    }

    void AssignGroupRecursive(Transform t, string groupName)
    {
        if (!transformToGroupMap.ContainsKey(t))
            transformToGroupMap[t] = groupName;

        loadingPanel.SetActive(false);
    }

    public void ResetColorsWithClipping()
    {
        loadingPanel.SetActive(true);
        
        foreach (var renderer in originalBlocks.Keys)
        {
            var block = new MaterialPropertyBlock();
            block.Clear(); // override'ları temizle
            renderer.SetPropertyBlock(block);
        }
        originalBlocks.Clear();
        colorBlocks.Clear();
        loadingPanel.SetActive(false);
    }

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


    public void ChangeGroupsColor()
    {
        if (groupPanelSlider.value == 0)
        {
            foreach (Slider slider in groupPanelContent.GetComponentsInChildren<Slider>())
            {
                slider.value = 0;
            }
        }
        if (groupPanelSlider.value == 1)
        {
            foreach (Slider slider in groupPanelContent.GetComponentsInChildren<Slider>())
            {
                slider.value = 1;
            }
        }
    }


    private void ChangeItemColors(float value)
    {
        if(itemPanelSlider.gameObject.activeInHierarchy)
        {
            if(itemPanelSlider.value == 1)
            {
                foreach(Slider slider in itemPanelContent.GetComponentsInChildren<Slider>())
                {
                    slider.value = 1;
                }
            }
            if(itemPanelSlider.value == 0)
            {
                foreach(Slider slider in itemPanelContent.GetComponentsInChildren<Slider>())
                {
                    slider.value = 0;
                }
            }
        }
    }

    public void ControlItemSlider()
    {
        bool allOpen = true;
        foreach(Slider slider in itemPanelContent.GetComponentsInChildren<Slider>())
        {
            if(slider.value == 0)
            {
                allOpen = false;
                break;
            }
        }
        
        if(allOpen)
        {
            TMP_Text[] groupNames = groupPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var groupName in groupNames)
            {
                if(itemPanel.GetComponentInChildren<TMP_Text>().text == groupName.text)
                {
                    groupName.transform.parent.GetComponentInChildren<Slider>(true).value = 1;
                }
            }
            itemPanelSlider.SetValueWithoutNotify(1);
        }
        else
        {
            TMP_Text[] groupNames = groupPanel.GetComponentsInChildren<TMP_Text>();
            foreach (var groupName in groupNames)
            {
                if(itemPanel.GetComponentInChildren<TMP_Text>().text == groupName.text)
                {
                    groupName.transform.parent.GetComponentInChildren<Slider>(true).value = 0;
                }
            }
            itemPanelSlider.SetValueWithoutNotify(0);
        } 
    }
}
