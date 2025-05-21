using UnityEngine;
using UnityEngine.EventSystems;

public class SubButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject subTooltip;         // Alt butonun kendi tooltip'i
    public GameObject parentTooltip;      // Ana butonun tooltip'i

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ana buton tooltip kapat
        if (parentTooltip != null)
            parentTooltip.SetActive(false);

        // Kendi tooltip’ini aç
        subTooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Kendi tooltip’ini kapat
        subTooltip.SetActive(false);
    }
}
