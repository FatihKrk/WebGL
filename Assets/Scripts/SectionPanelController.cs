using UnityEngine;

public class SectionPanelController : MonoBehaviour
{
    
    public GameObject sectionPanel;

    
    public void ToggleSectionPanel()
    {
        
        bool isActive = sectionPanel.activeSelf;
        // Tersi
        sectionPanel.SetActive(!isActive);
    }
}
