using UnityEngine;
using UnityEngine.EventSystems;

public class SectionButtonTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipSection; // Ana butonun tooltip'i

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipSection.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipSection.SetActive(false);
    }
}
