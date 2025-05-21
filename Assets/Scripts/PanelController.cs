using UnityEngine;

public class PanelController : MonoBehaviour
{
    public GameObject Panel;


    public void TogglePanel()
    {

        bool isActive = Panel.activeSelf;
        // Tersi
        Panel.SetActive(!isActive);
    }

}