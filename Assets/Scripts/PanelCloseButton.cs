using UnityEngine;

public class PanelCloseButton : MonoBehaviour
{
    public GameObject panelToClose; // Kapatmak istediðin panel

    public void ClosePanel()
    {
        if (panelToClose != null)
            panelToClose.SetActive(false);
    }
}
