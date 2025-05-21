using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolSelectButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Icon Settings")]
    public Sprite normalIcon;     
    public Sprite selectedIcon;   

    [Header("Color Settings")]
    public Color normalColor = Color.white;   
    public Color selectedColor = new Color32(33, 150, 243, 255);

    private Image buttonImage;
    private ToolManager toolManager;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
            buttonImage = GetComponentInChildren<Image>();

        
        toolManager = FindObjectOfType<ToolManager>();

        SetSelected(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (toolManager != null)
        {
            
            toolManager.SetActiveButton(this);
        }
    }

    
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            
            if (selectedIcon != null)
                buttonImage.sprite = selectedIcon;
            buttonImage.color = selectedColor;
        }
        else
        {
           
            if (normalIcon != null)
                buttonImage.sprite = normalIcon;
            buttonImage.color = normalColor;
        }
    }
}
