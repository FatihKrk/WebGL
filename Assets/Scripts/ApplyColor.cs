using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplyColor : MonoBehaviour
{
    [SerializeField] FlexibleColorPicker fcp;
    [SerializeField] GameObject flexibleColorPickerObject;
    [SerializeField] Image image;
    [SerializeField] MouseClick mouseClick;
    public List<GameObject> gameObjects = new List<GameObject>();

    public void ColorChangeButton()
    {
        image.color = fcp.color;
        if(mouseClick.multiSelect)
        {
            gameObjects = mouseClick.clickedItems;
            foreach(GameObject obj in gameObjects)
            {
                if(obj.transform.childCount > 0)
                {
                    MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
                    foreach(MeshRenderer mesh in meshRenderers)
                    {
                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        mesh.GetPropertyBlock(block);
                        block.SetColor("_Color", fcp.color);
                        mesh.SetPropertyBlock(block);
                    }
                }
                else
                {
                    MeshRenderer mesh = obj.GetComponentInChildren<MeshRenderer>();
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    mesh.GetPropertyBlock(block);
                    block.SetColor("_Color", fcp.color);
                    mesh.SetPropertyBlock(block);
                }
            }
        }
        else
        {
            if(mouseClick.currentObject.transform.childCount > 0)
            {
                MeshRenderer[] meshRenderers = mouseClick.currentObject.GetComponentsInChildren<MeshRenderer>();
                foreach(MeshRenderer mesh in meshRenderers)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    mesh.GetPropertyBlock(block);
                    block.SetColor("_Color", fcp.color);
                    mesh.SetPropertyBlock(block);
                }
            }
            else
            {
                MeshRenderer mesh = mouseClick.currentObject.GetComponentInChildren<MeshRenderer>();
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                mesh.GetPropertyBlock(block);
                block.SetColor("_Color", fcp.color);
                mesh.SetPropertyBlock(block);
            }
        }
        
    }

    public void EnableAndDisable()
    {
        if(flexibleColorPickerObject.activeSelf)
        {
            flexibleColorPickerObject.SetActive(false);
        } 
        else
        {
            flexibleColorPickerObject.SetActive(true);
            flexibleColorPickerObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);
        }
    }
}
