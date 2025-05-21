using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 offset;
    private bool isDragging = false; // Sürükleme durumunu kontrol ediyoruz

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // RectTransform component'ýný alýyoruz
        canvasGroup = GetComponent<CanvasGroup>(); // CanvasGroup component'ýný alýyoruz
    }

    // Panel taþýma baþladýðýnda çaðrýlýr
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true; // Sürükleme baþladýðýný iþaret ediyoruz
        canvasGroup.alpha = 0.6f;  // Panel opaklýðýný %60 yapýyoruz
        canvasGroup.blocksRaycasts = false; // Diðer UI öðeleriyle etkileþimi engelliyoruz
        offset = rectTransform.position - (Vector3)eventData.position; // Mouse'un konumu ile panelin farkýný alýyoruz
    }

    // Panel taþýrken sürekli çaðrýlýr
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging) // Eðer sürükleme iþlemine baþlandýysa
        {
            rectTransform.position = (Vector3)eventData.position + offset; // Panelin konumunu mouse'un pozisyonuna göre deðiþtiriyoruz
        }
    }

    // Panel taþýmasý bittiðinde çaðrýlýr
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false; // Sürükleme bitiyor
        canvasGroup.alpha = 1f;  // Opaklýðý tekrar %100 yapýyoruz
        canvasGroup.blocksRaycasts = true;  // UI öðeleriyle etkileþimi tekrar açýyoruz
    }
}
