using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeHandleController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform targetPanel; // Boyutlandýrýlacak panelin RectTransform'u

    private Vector2 initialMousePosition;
    private Vector2 initialPanelSize;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetPanel == null)
        {
            Debug.LogWarning("Target panel not assigned!");
            return;
        }
        initialMousePosition = eventData.position;
        initialPanelSize = targetPanel.sizeDelta;
        Debug.Log("Resize handle pointer down");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetPanel == null) return;
        Vector2 delta = eventData.position - initialMousePosition;

        float newWidth = Mathf.Max(initialPanelSize.x + delta.x, 100f);
        float newHeight = Mathf.Max(initialPanelSize.y + delta.y, 100f); // Y ekseni için toplama yapýyoruz

        targetPanel.sizeDelta = new Vector2(newWidth, newHeight);
        Debug.Log("Resizing: New Size = " + newWidth + "x" + newHeight);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Resize handle pointer up");
    }
}
