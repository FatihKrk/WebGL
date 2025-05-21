using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

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

    void Start()
    {
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        CacheAllObjects();
        StartCoroutine(GetData());
        itemPanelSlider = itemPanel.GetComponentInChildren<Slider>();
        itemPanelSlider.onValueChanged.AddListener(ChangeItemColors);
        groupPanelSlider = groupPanel.GetComponentInChildren<Slider>();
        slider = mainPanel.GetComponentInChildren<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void CacheAllObjects()
    {
        foreach (MeshRenderer renderer in first_Parent.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (!cachedItems.ContainsKey(renderer.name))
            {
                cachedItems[renderer.name] = renderer.transform;
            }
            if (!originalColors.ContainsKey(renderer))
            {
                originalColors[renderer] = renderer.material.color;
            }
        }
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
        loadingPanel.SetActive(false);
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
    Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    private Color grayColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.60f);

    public void ChangeColorFonk()
    {
        loadingPanel.SetActive(true);
        StartCoroutine(ChangeColor());
    }

    IEnumerator ChangeColor()
    {
        int i = 0;
        int batchSize = 0;
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();

        // İlk başta tüm renderer'ları cache'leyelim
        MeshRenderer[] allFirstParentRenderers = first_Parent.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var group in groupedData)
        {
            string groupName = group.Key;
            List<string> names = group.Value;
            Color groupColor = GetColor(i);

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
                        batchSize++;

                        if (batchSize >= 250)
                        {
                            batchSize = 0;
                            yield return null;  // İşlem arası vererek yükü dağıtıyoruz
                        }
                    }
                }
            }
            i++;
        }

        // İlk parent'teki işlenmemiş renderer'ları griye çevir
        foreach (var renderer in allFirstParentRenderers)
        {
            if (!processedRenderers.Contains(renderer))
            {
                renderer.GetPropertyBlock(block);
                SetRendererColor(renderer, grayColor, block);
            }

            batchSize++;
            if (batchSize >= 250)
            {
                batchSize = 0;
                yield return null;  // İşlem arası vererek yükü dağıtıyoruz
            }
        }

        // UI slider değerlerini sıfırlama işlemi
        foreach (Slider slider in groupPanel.GetComponentsInChildren<Slider>())
        {
            slider.value = 1;
        }

        // Yükleme panelini gizleme
        loadingPanel.SetActive(false);
    }

    private void SetRendererColor(Renderer renderer, Color color, MaterialPropertyBlock block)
    {
        renderer.GetPropertyBlock(block);
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);
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

    // Renkleri eski haline döndüren fonksiyon
    public void ResetColors()
    {
        loadingPanel.SetActive(true);
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        foreach (var kvp in originalColors)
        {
            Renderer renderer = kvp.Key;
            Color originalColor = kvp.Value;

            renderer.GetPropertyBlock(block);
            block.SetColor("_Color", originalColor);
            renderer.SetPropertyBlock(block);
        }

        foreach (Slider slider in groupPanelContent.GetComponentsInChildren<Slider>())
        {
            slider.value = 0;
        }

        loadingPanel.SetActive(false);
    }


    public void ChangeGroupsColor()
    {
        if(groupPanelSlider.value == 0)
        {
            foreach(Slider slider in groupPanelContent.GetComponentsInChildren<Slider>())
            {
                slider.value = 0;
            }
        }
        if(groupPanelSlider.value == 1)
        {
            foreach(Slider slider in groupPanelContent.GetComponentsInChildren<Slider>())
            {
                slider.value = 1;
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
