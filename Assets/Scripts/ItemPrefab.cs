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
    
    private Color grayColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.60f);
    private MaterialPropertyBlock grayBlock;

    void Start()
    {
        visualQueryManager = GameObject.FindGameObjectWithTag("Canvas").GetComponentInChildren<VisualQueryManager>();
        mouseClick = GameObject.Find("MainCamera").GetComponentInChildren<MouseClick>();
        first_Parent = GameObject.FindGameObjectWithTag("ParentObject");
        itemPanel = GameObject.Find("ByItemNamePanel");
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
        float hue = Random.Range(0f, 1f);
        float saturation = Random.Range(0.5f, 1f);
        float value = Random.Range(0.5f, 1f);
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

        if (selectedItem != null)
        {
            if (slider.value == 0)  OffItem();
            else if (slider.value == 1) OnItem();
        }
    }

    void OnItem() => ResetColor();
    void OffItem() => ChangeColor();

    public void SelectItem()
    {
        selectedItem = Search(GetComponentInChildren<TMP_Text>().text);
        if (selectedItem != null)
        {
            mouseClick.currentObject = selectedItem.gameObject;
            mouseClick.FindItemPosition();
            StartCoroutine(SelectItemRoutine());
        }
    }

    private IEnumerator SelectItemRoutine()
    {
        yield return StartCoroutine(mouseClick.FindParents(mouseClick.currentObject));
        yield return StartCoroutine(mouseClick.Expand());

        if (mouseClick.attributes_Panel.activeSelf)
            mouseClick.ShowAttributes();
    }


    public Transform Search(string searched_Text)
    {
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(first_Parent.transform);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.name.IndexOf(searched_Text, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return current;

            for (int i = current.childCount - 1; i >= 0; i--)
                stack.Push(current.GetChild(i));
        }
        return null;
    }

    void ChangeColor()
    {
        grayBlock = new MaterialPropertyBlock();
        grayBlock.SetFloat("_UseOverrideColor", 1f);
        grayBlock.SetColor("_OverrideColor", grayColor);
        Color groupColor = Color.grey;

        foreach (var group in visualQueryManager.groupColors)
        {
            if (itemPanel != null && group.Key == itemPanel.GetComponentInChildren<TMP_Text>().text)
            {
                groupColor = group.Value;
                break;
            }
        }

        if (selectedItem != null)
        {
            foreach (var renderer in selectedItem.GetComponentsInChildren<MeshRenderer>(true))
            {
                visualQueryManager.changedBlocks[renderer] = grayBlock;
                renderer.SetPropertyBlock(grayBlock);
            }
        }

        visualQueryManager.loadingPanel.SetActive(false);
    }

    void ResetColor()
    {
        MaterialPropertyBlock groupblock = new MaterialPropertyBlock();
        groupblock = new MaterialPropertyBlock();
        groupblock.SetFloat("_UseOverrideColor", 1f);
        
        HashSet<Renderer> processedRenderers = new HashSet<Renderer>();
        Color groupColor = Color.grey;

        foreach (var group in visualQueryManager.groupColors)
        {
            if (itemPanel != null && group.Key == itemPanel.GetComponentInChildren<TMP_Text>().text)
            {
                groupColor = group.Value;

                if (selectedItem != null)
                {
                    foreach (var renderer in selectedItem.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        groupblock.SetColor("_OverrideColor", groupColor);
                        visualQueryManager.changedBlocks[renderer] = groupblock;
                        renderer.SetPropertyBlock(groupblock);
                        processedRenderers.Add(renderer);
                    }
                }

                break;
            }
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
}
