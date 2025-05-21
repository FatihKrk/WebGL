using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 offset;
    private bool isDragging = false; // S�r�kleme durumunu kontrol ediyoruz

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // RectTransform component'�n� al�yoruz
        canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroup component'�n� al�yoruz
    }

    // Panel ta��ma ba�lad���nda �a�r�l�r
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true; // S�r�kleme ba�lad���n� i�aret ediyoruz
        canvasGroup.alpha = 0.6f;  // Panel opakl���n� %60 yap�yoruz
        canvasGroup.blocksRaycasts = false; // Di�er UI ��eleriyle etkile�imi engelliyoruz
        offset = rectTransform.position - (Vector3)eventData.position; // Mouse'un konumu ile panelin fark�n� al�yoruz
    }

    // Panel ta��rken s�rekli �a�r�l�r
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging) // E�er s�r�kleme i�lemine ba�land�ysa
        {
            rectTransform.position = (Vector3)eventData.position + offset; // Panelin konumunu mouse'un pozisyonuna g�re de�i�tiriyoruz
        }
    }

    // Panel ta��mas� bitti�inde �a�r�l�r
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false; // S�r�kleme bitiyor
        canvasGroup.alpha = 1f;  // Opakl��� tekrar %100 yap�yoruz
        canvasGroup.blocksRaycasts = true;  // UI ��eleriyle etkile�imi tekrar a��yoruz
    }
}
