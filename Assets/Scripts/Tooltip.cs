using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public GameObject tooltipObject; // Tooltip UI objesi
    public TMP_Text tooltipText; // Tooltip'te gösterilecek metin
    public Canvas canvas; // Tooltip'in bağlı olduğu Canvas
    Vector2 tooltipOffset = new Vector2(10, -15); // Tooltip'in mouse'a göre ofseti
    private GraphicRaycaster raycaster;

    private void Start()
    {
        // Canvas üzerindeki GraphicRaycaster bileşenini al
        raycaster = canvas.GetComponent<GraphicRaycaster>();
    }

    private void Update()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        if (results.Count > 0)
        {
            GameObject hoveredObject = results[0].gameObject;

            if (hoveredObject.CompareTag("TooltipObject"))
            {
                if (tooltipText != null)
                    tooltipText.text = hoveredObject.name;

                tooltipObject.SetActive(true);
                tooltipObject.transform.SetAsLastSibling();

                RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
                RectTransform tooltipRectTransform = tooltipObject.GetComponent<RectTransform>();

                Vector2 localMousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    Input.mousePosition,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out localMousePosition
                );

                // Tooltip boyutu
                Vector2 tooltipSize = tooltipRectTransform.sizeDelta;

                // Canvas boyutu
                Vector2 canvasSize = canvasRectTransform.sizeDelta;

                // Ekran sınırlarını kontrol ederek akıllı pozisyonlama
                Vector2 adjustedOffset = tooltipOffset;

                tooltipRectTransform.anchoredPosition = localMousePosition + adjustedOffset;
            }
            else
            {
                tooltipObject.SetActive(false);
            }
        }
        else
        {
            tooltipObject.SetActive(false);
        }
    }
}
