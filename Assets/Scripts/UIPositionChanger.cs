using UnityEngine;
using UnityEngine.EventSystems;

public class UIPositionChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Camera canvasCam;
    GameObject panel;
    private bool isDragging = false; // Mouse basılı tutma durumu
    private Vector3 offset; // Başlangıç tıklama pozisyonu

    void Start()
    {
        panel = gameObject;
    }

    // Bu fonksiyon, mouse tıklama olayı başladığında çağrılır
    public void OnPointerDown(PointerEventData eventData)
    {
        // Mouse pozisyonunu kaydedelim
        isDragging = true;
    }

    // Bu fonksiyon, mouse tıklama olayı bittiğinde çağrılır
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false; // Mouse bırakıldığında sürükleme durur
    }

    // Update fonksiyonunda sürekli olarak pozisyonu güncelle
    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePos = canvasCam.ScreenToWorldPoint(Input.mousePosition);
            panel.transform.position = mousePos;
            panel.transform.localPosition = new Vector3(panel.transform.localPosition.x, panel.transform.localPosition.y, -2.5f);
        }
    }
}
