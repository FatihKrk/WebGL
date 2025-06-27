using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour, ICanvasAware
{
    public GameObject tooltipObject; // Tooltip UI objesi
    public TMP_Text tooltipText; // Tooltip'te gösterilecek metin
    public Canvas canvas; // Tooltip'in bağlı olduğu Canvas
    public Vector2 tooltipOffset = new Vector2(20, -20); // Tooltip'in mouse'a göre ofseti
    private GraphicRaycaster raycaster;

    public void OnCanvasChanged(GameObject activeCanvas)
    {
        canvas = activeCanvas.GetComponent<Canvas>();
    }
    private void Start()
    {
        // Canvas üzerindeki GraphicRaycaster bileşenini al
        raycaster = canvas.GetComponent<GraphicRaycaster>();
    }

    private void Update()
    {
        // Mouse'un altında UI elemanı olup olmadığını kontrol et
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            // İlk UI elemanını al
            GameObject hoveredObject = results[0].gameObject;

            // Eğer objenin tag'i "TooltipObject" ise tooltip'i göster
            if (hoveredObject.CompareTag("TooltipObject"))
            {
                // Eğer bir metin alanı tanımlıysa, metni güncelle
                if (tooltipText != null)
                {
                    tooltipText.text = hoveredObject.name;
                }

                tooltipObject.SetActive(true);

                // Mouse pozisyonunu Canvas koordinatlarına çevir
                RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
                RectTransform tooltipRectTransform = tooltipObject.GetComponent<RectTransform>();

                Vector2 anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    Input.mousePosition,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out anchoredPosition
                );

                // Offset'i ekle ve pozisyonu ayarla
                tooltipRectTransform.anchoredPosition = anchoredPosition + tooltipOffset;
            }
            else
            {
                // Tooltip'i gizle
                tooltipObject.SetActive(false);
            }
        }
        else
        {
            // Tooltip'i gizle
            tooltipObject.SetActive(false);
        }
    }
}
