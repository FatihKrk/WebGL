using UnityEngine;
using UnityEngine.EventSystems;

public class DragImage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    MoveButtons moveButtons;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isDragging = false;

    public GameObject avatarPrefab; // Instantiate edilecek avatar prefab
    public LayerMask groundLayer; // Ground için layer seçimi

    private Camera uiCamera; // Canvas için kullanılan kamera
    private Vector3 originalPosition, instantiatePosition = new Vector3(1600, 100, 760); // Image'in orijinal pozisyonu
    GameObject emptyObject;

    void Awake()
    {
        GameObject cnvsObj = GameObject.Find("CanvasCamera");
        uiCamera = cnvsObj.GetComponent<Camera>();

        GameObject mvmObj = GameObject.Find("MovementsPanel");
        moveButtons = mvmObj.GetComponent<MoveButtons>();

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas not found! Make sure the Image is inside a proper Canvas.");
        }

        if (uiCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogError("No camera assigned to the canvas!");
        }
    }

    void Start()
    {
        // İlk pozisyonu kaydet
        originalPosition = rectTransform.localPosition;
        emptyObject = new GameObject("InstantiatePosition");
    }

    void Update()
    {
        // Mouse pozisyonu ile Ground Layer'daki objeleri kontrol et
        Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayer))
        {

            instantiatePosition = hitInfo.point; // Doğrudan raycast pozisyonu al
        }
        if (isDragging)
        {
            // Kamerayı sabit bir pozisyona ayarlamak isterseniz burayı değiştirebilirsiniz
            Camera.main.transform.position = new Vector3(1598f,130f,722f);
            Camera.main.transform.rotation = Quaternion.Euler(45, 0, 0);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
{
    isDragging = false;

    // Prefab oluştur
    Instantiate(avatarPrefab, instantiatePosition, Quaternion.identity);

    moveButtons.Avatar(); // Eğer başka bir işlem yapıyorsanız çağırabilirsiniz

    // UI elemanını orijinal pozisyonuna geri döndür
    rectTransform.localPosition = originalPosition;
}


    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null || uiCamera == null)
            return;

        // Mouse pozisyonunu Canvas koordinatlarına çevir
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
        RectTransform tooltipRectTransform = transform.GetComponent<RectTransform>();

        Vector2 anchoredPosition;
        // Mouse pozisyonunu Canvas'taki yerel koordinatlara dönüştür
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            Input.mousePosition,
            uiCamera,
            out anchoredPosition
        );

        // Pivot noktasına göre pozisyonu düzelt
        Vector2 pivotOffset = new Vector2(500, -200);

        // Yeni pozisyonu ayarla
        tooltipRectTransform.localPosition = anchoredPosition + pivotOffset;
    }
}
